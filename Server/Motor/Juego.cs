using System;
using System.Linq;
using System.Threading;
using Proyecto_MonopoTEC.Compartido;
using Proyecto_MonopoTEC.Server.Red;
using Proyecto_MonopoTEC.Server.Modelo;
using Proyecto_MonopoTEC.Server.Estructuras;
using Proyecto_MonopoTEC.Server.Hardware;
using Proyecto_MonopoTEC.Server.Persistencia;

namespace Proyecto_MonopoTEC.Server.Motor
{
    /// <summary>
    /// Representa el juego.
    /// </summary>
    public class Juego
    {
        private Tablero _tableroJuego;
        private ManejadorAcciones _manejadorAcciones;

        private Servidor _server;
        private RFIDDriver _driver;
        private readonly RegistroTransacciones _registro;

        private Jugador? jugador1;
        private Jugador? jugador2;
        private Jugador? jugador3;
        private Jugador? jugador4;

        private string[] UIDs = new string[4];

        private Jugador? jugadorActual;


        private ColaCircular? ColaTurnos;
        private int Turno;

        private Dado? dado;
        private readonly ManualResetEventSlim _accionEvent = new ManualResetEventSlim(false);
        private readonly object _accionLock = new object();
        private int _accionJugadorId = -1;
        private string _accionPendiente = "";
        private string _valorPendiente = "";
        private string _accionEsperada = "";
        private int _jugadorDesbloqueadoId = -1;
        private int _partidaIniciada;

        public Juego(Servidor server, RFIDDriver driver, RegistroTransacciones registro)
        {
            _server = server;
            _driver = driver;
            _registro = registro;

            _tableroJuego = new Tablero();
            _tableroJuego.Inicializar();

            _manejadorAcciones = new ManejadorAcciones(_registro, _driver, _server, _tableroJuego, EsperarAccion);
        }


        /// <summary>
        /// Recibe la accion de un jugador.
        /// </summary>
        /// <param name="jugadorId"></param>
        /// <param name="accion"></param>
        /// <param name="valor"></param>
        public void RecibirAccion(int jugadorId, string accion, string valor)
        {
            if (jugadorActual == null || jugadorId != jugadorActual.ID)
            {
                _server.EnviarMensaje(Protocolo.ErrorAccion, new { mensaje = "No es el turno de ese jugador." });
                return;
            }

            if (accion == "desbloquear")
            {
                if (!VerificarJugador(jugadorId))
                {
                    _server.EnviarMensaje(Protocolo.ErrorAccion, new { mensaje = "No se pudo verificar el UID del jugador." });
                    return;
                }

                _jugadorDesbloqueadoId = jugadorId;
                _server.EnviarMensaje(Protocolo.DesbloquearTurno, new { jugadorId });
                return;
            }

            if (_jugadorDesbloqueadoId != jugadorId)
            {
                _server.EnviarMensaje(Protocolo.ErrorAccion, new { mensaje = "Primero verifica tu tarjeta RFID." });
                return;
            }


            lock (_accionLock)
            {
                string accionNormalizada = accion.Trim();
                string accionEsperadaNormalizada = _accionEsperada.Trim();

                bool accionValida = _accionEsperada switch
                {
                    "turno" => accionNormalizada is "dado" or "venta" or "propiedades" or "1" or "2" or "3",
                    "compra" => accionNormalizada is "comprar" or "rechazar" or "1" or "2",
                    "venta" => int.TryParse(valor, out _) || valor == "venta",
                    "continuar" => accionNormalizada == "continuar",
                    _ => accionNormalizada == accionEsperadaNormalizada
                };

                if (!accionValida)
                {
                    _server.EnviarMensaje(Protocolo.ErrorAccion, new { mensaje = "La acción no corresponde al estado actual del turno." });
                    return;
                }
            }

            lock (_accionLock)
            {
                _accionJugadorId = jugadorId;
                _accionPendiente = accion;
                _valorPendiente = valor;
                _accionEvent.Set();
            }
        }



        /// <summary>
        /// Espera la accion de un jugador.
        /// </summary>
        /// <param name="jugador"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        private string EsperarAccion(Jugador jugador, string tipo)
        {
            lock (_accionLock)
            {
                _accionEsperada = tipo;
                _accionEvent.Reset();
            }

            _server.EnviarMensaje(Protocolo.AccionesTurno, new { id = jugador.ID, tipo });

            while (true)
            {
                _accionEvent.Wait();

                lock (_accionLock)
                {
                    if (_accionJugadorId != jugador.ID)
                    {
                        _accionEvent.Reset();
                        continue;
                    }

                    string accion = tipo == "venta" ? _valorPendiente : _accionPendiente;
                    _accionEvent.Reset();
                    _accionJugadorId = -1;
                    _accionPendiente = "";
                    _valorPendiente = "";
                    _accionEsperada = "";
                    return accion;
                }
            }
        }



        /// <summary>
        /// Método empleado para inicializar un jugador. 
        /// </summary>
        /// <returns>Retorna el objeto jugador inicializado.</returns>
        private Jugador InicializarJugador(int id, string UID, string nombre, Casilla posicion)
        {
            return new Jugador(id, UID, nombre, posicion);
        }



        /// <summary>
        /// Envía el historial de transacciones.
        /// </summary>
        public void EnviarHistorialTransacciones()
        {
            Transaccion[] transacciones = _registro.VerListaTransacciones();
            _server.EnviarMensaje(Protocolo.Transacciones, new
            {
                transacciones = transacciones.Select(t => t.ConvertirTexto()).ToArray()
            });
        }


        /// <summary>
        /// Envía la transaccion más reciente.
        /// </summary>
        public void EnviarUltimaTransaccion()
        {
            Transaccion? transaccion = _registro.VerUltimaTransaccion();
            if (transaccion == null)
            {
                _server.EnviarMensaje(Protocolo.UltimaTransaccion, new { transaccion = (object?)null });
                return;
            }

            _server.EnviarMensaje(Protocolo.UltimaTransaccion, new { transaccion = transaccion.ConvertirTexto() });
        }


        /// <summary>
        /// Envía la primera transaccion (la más antigua).
        /// </summary>
        public void EnviarPrimeraTransaccion()
        {
            Transaccion? transaccion = _registro.VerPrimeraTransaccion();
            if (transaccion == null)
            {
                _server.EnviarMensaje(Protocolo.PrimeraTransaccion, new { transaccion = (object?)null });
                return;
            }

            _server.EnviarMensaje(Protocolo.PrimeraTransaccion, new { transaccion = transaccion.ConvertirTexto() });
        }


        /// <summary>
        ///  Envía las transacciones de un jugador.
        /// </summary>
        /// <param name="nombreJugador"></param>
        public void EnviarTransaccionesPorJugador(string nombreJugador)
        {
            Transaccion[] transacciones = _registro.VerTransaccionesPorJugador(nombreJugador);
            _server.EnviarMensaje(Protocolo.TransaccionesPorJugador, new
            {
                jugador = nombreJugador,
                transacciones = transacciones.Select(t => t.ConvertirTexto()).ToArray()
            });
        }


        /// <summary>
        /// Envía las transacciones por tipo.
        /// </summary>
        /// <param name="tipo"></param>
        public void EnviarTransaccionesPorTipo(string tipo)
        {
            string tipoSolicitado = string.IsNullOrWhiteSpace(tipo) ? string.Empty : tipo.Trim();
            Transaccion[] transacciones;
            string tipoRespuesta = tipoSolicitado;

            if (Enum.TryParse<TipoTransaccion>(tipoSolicitado, true, out TipoTransaccion tipoTransaccion))
            {
                transacciones = _registro.VerTransaccionesPorTipo(tipoTransaccion);
                tipoRespuesta = tipoTransaccion.ToString();
            }
            else if (tipoSolicitado.Equals("Compra", StringComparison.OrdinalIgnoreCase) ||
                     tipoSolicitado.Equals("Pago", StringComparison.OrdinalIgnoreCase) ||
                     tipoSolicitado.Equals("Evento", StringComparison.OrdinalIgnoreCase))
            {
                transacciones = _registro.VerTransaccionesPorTipo(tipoSolicitado);
                tipoRespuesta = tipoSolicitado;
            }
            else
            {
                transacciones = _registro.VerTransaccionesPorTipo(tipoSolicitado);
                tipoRespuesta = tipoSolicitado;
            }

            _server.EnviarMensaje(Protocolo.TransaccionesPorTipo, new
            {
                tipo = tipoRespuesta,
                transacciones = transacciones.Select(t => t.ConvertirTexto()).ToArray()
            });
        }



        /// <summary>
        /// Metodo para autenticar un  nuevo jugador.
        /// Revisa si ya existe mediante una lista de UIDs
        /// Se puede simplificar con lista de Jugador pero rompería el sistema de colas
        /// </summary>
        /// <param name="id"></param>
        /// <param name="nombre"></param>
        /// <returns></returns>
        public bool AutenticarJugador(int id, string nombre)
        {
            if (Volatile.Read(ref _partidaIniciada) == 1)
            {
                _server.EnviarMensaje(Protocolo.RegistrarJugador, new { id = -1, error = "La partida ya comenzó." });
                return false;
            }

            // Revisar que la ID sea válida
            if (id >= 0 && id <= 3)
            {
                if (string.IsNullOrWhiteSpace(nombre) || nombre.Length > 12)
                {
                    _server.EnviarMensaje(Protocolo.RegistrarJugador, new { id = -1, error = "El nombre debe tener entre 1 y 12 caracteres." });
                    return false;
                }

                // Hacer la lectura del UID
                string UID = _driver.ReadUID("PASAR JUGADOR " + (id + 1));
                if (UIDs.Contains(UID))
                {
                    // Si ya existe, enviar una ID inválida, luego se maneja en el Cliente
                    _server.EnviarMensaje(Protocolo.RegistrarJugador, new { id = -1, error = "Ese jugador ya existe" });
                    return false;
                }
                else
                {
                    UIDs[id] = UID;
                }

                if (id == 0)
                {
                    jugador1 = InicializarJugador(id, UID, nombre, _tableroJuego.Head!.Data!);

                    _server.EnviarMensaje(Protocolo.RegistrarJugador, new { id = jugador1.ID, nombre = jugador1.Nombre });
                }

                if (id == 1)
                {
                    jugador2 = InicializarJugador(id, UID, nombre, _tableroJuego.Head!.Data!);

                    _server.EnviarMensaje(Protocolo.RegistrarJugador, new { id = jugador2.ID, nombre = jugador2.Nombre });
                }

                if (id == 2)
                {
                    jugador3 = InicializarJugador(id, UID, nombre, _tableroJuego.Head!.Data!);

                    _server.EnviarMensaje(Protocolo.RegistrarJugador, new { id = jugador3.ID, nombre = jugador3.Nombre });
                }

                if (id == 3)
                {
                    jugador4 = InicializarJugador(id, UID, nombre, _tableroJuego.Head!.Data!);

                    _server.EnviarMensaje(Protocolo.RegistrarJugador, new { id = jugador4.ID, nombre = jugador4.Nombre });
                }

                _server.EnviarMensaje(Protocolo.RegistrarJugador, new
                {
                    id,
                    nombre
                });

                return true;
            }

            return false;
        }


        /// <summary>
        /// Verifica si un jugador ya se encuentra registrado.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool VerificarJugador(int id)
        {

#pragma warning disable CS0162
            // DEBUG TEMPORAL PARA SALTAR AUTENTICACIÓN
            if (Config.Skip)
            {
                return true;
            }

            if (id == 0)
            {
                if (jugador1 == null || string.IsNullOrWhiteSpace(jugador1.UID))
                    return false;
                _driver.ReadUID("VERIF JUGADOR " + (id + 1), jugador1.UID);
                return true;
            }

            if (id == 1)
            {
                if (jugador2 == null || string.IsNullOrWhiteSpace(jugador2.UID))
                    return false;
                _driver.ReadUID("VERIF JUGADOR " + (id + 1), jugador2.UID);
                return true;
            }

            if (id == 2)
            {
                if (jugador3 == null || string.IsNullOrWhiteSpace(jugador3.UID))
                    return false;
                _driver.ReadUID("VERIF JUGADOR " + (id + 1), jugador3.UID);
                return true;
            }

            if (id == 3)
            {
                if (jugador4 == null || string.IsNullOrWhiteSpace(jugador4.UID))
                    return false;
                _driver.ReadUID("VERIF JUGADOR " + (id + 1), jugador4.UID);
                return true;
            }

            return false;
        }



        /// <summary>
        /// Método empleado para inicializar la cola de turnos. 
        /// </summary>
        /// <returns>Retorna la cola de turnos inicializada.</returns>
        private ColaCircular InicializarTurnos(Jugador jugador1, Jugador jugador2, Jugador jugador3, Jugador jugador4)
        {
            ColaCircular cola = new ColaCircular();
            cola.Enqueue(jugador1);
            cola.Enqueue(jugador2);
            cola.Enqueue(jugador3);
            cola.Enqueue(jugador4);
            return cola;
        }


        /// <summary>
        /// Método empleado para preparar el turno de un jugador.
        /// </summary>
        /// <returns></returns>
        private AccionCasilla PrepararTurno()
        {
            bool sigueEnTurno = true;
            while (sigueEnTurno)
            {
                if (jugadorActual!.EnCarcel)
                {
                    Console.WriteLine($"{jugadorActual.Nombre} está en la carcel, pierde el turno.");

                    _server.EnviarMensaje(Protocolo.JugadorCarcel, new { mensaje = $"{jugadorActual.Nombre} está en la carcel, pierde el turno." });
                    jugadorActual.ReducirCondena();

                    if (jugadorActual.TurnosCarcel == 0)
                    {
                        jugadorActual.SalirCarcel();
                        Console.WriteLine($"{jugadorActual.Nombre} saldrá en el siguiente turno.");

                        _server.EnviarMensaje(Protocolo.JugadorCarcel, new { jugadorId = jugadorActual.ID, mensaje = $"{jugadorActual.Nombre} saldrá en el siguiente turno." });
                    }
                    else
                    {
                        Console.WriteLine($"A {jugadorActual.Nombre} le que quedan {jugadorActual.TurnosCarcel} turnos en carcel.");

                        _server.EnviarMensaje(Protocolo.JugadorCarcel, new { mensaje = $"A {jugadorActual.Nombre} le quedan {jugadorActual.TurnosCarcel} turnos en carcel." });
                    }
                    return AccionCasilla.SinAccion;
                }

                else if (jugadorActual.TurnosPerdidos != 0)
                {
                    Console.WriteLine($"{jugadorActual.Nombre} pierde el turno.");

                    _server.EnviarMensaje(Protocolo.TurnoPerdido, new { jugadorId = jugadorActual.ID, turnosRestantes = jugadorActual.TurnosPerdidos, mensaje = $"{jugadorActual.Nombre} pierde el turno." });

                    Console.WriteLine($"{jugadorActual.Nombre} perderá {jugadorActual.TurnosPerdidos} más para volver a jugar.");

                    jugadorActual.ReducirTurnoPerdido();
                    return AccionCasilla.SinAccion;
                }

                Console.WriteLine("[Juego] Esperando acciones.");

                string opcion = EsperarAccion(jugadorActual, "turno");

                switch (opcion)
                {
                    case "1":
                    case "dado":
                        sigueEnTurno = false;
                        break;
                    case "2":
                    case "venta":
                        _manejadorAcciones.AccionVenderPropiedad(jugadorActual, Turno);
                        Console.WriteLine("[Juego] Esperando acciones.");
                        EsperarAccion(jugadorActual, "continuar");

                        break;
                }
            }

            int resultadoDado1 = dado!.Lanzar();
            int resultadoDado2 = dado!.Lanzar();

            Thread.Sleep(1200);

            Console.WriteLine("El jugador " + jugadorActual!.Nombre + " ha lanzado el dado y obtuvo: " + resultadoDado1 + " y " + resultadoDado2);

            _server.EnviarMensaje(Protocolo.DadosLanzados, new { jugadorId = jugadorActual.ID, dado1 = resultadoDado1, dado2 = resultadoDado2, resultado = resultadoDado1 + resultadoDado2 });

            Thread.Sleep(1200);

            bool pasoPorSalida;
            Casilla casillaJugadorActual = _tableroJuego.AvanzarJugador(jugadorActual, resultadoDado1 + resultadoDado2, out pasoPorSalida);

            Console.WriteLine("El jugador " + jugadorActual.Nombre + " se ha movido a la casilla: " + casillaJugadorActual.Nombre);

            _server.EnviarMensaje(Protocolo.JugadorMovido, new
            {
                jugadorId = jugadorActual.ID,
                casilla = casillaJugadorActual.Nombre,
                posicion = casillaJugadorActual.ID,
                pasoPorSalida
            }
            );

            if (pasoPorSalida)
            {
                _manejadorAcciones.DarPremio(jugadorActual, Turno);
                Console.WriteLine($"Jugador {jugadorActual.Nombre} pasó por Salida y recibió premio.");
            }

            Console.WriteLine("[Juego] Esperando acciones.");
            EsperarAccion(jugadorActual, "continuar");

            return casillaJugadorActual.DevolverAccion(jugadorActual);
        }


        /// <summary>
        /// Inicia el juego.
        /// </summary>
        public void IniciarJuego()
        {
            if (Volatile.Read(ref _partidaIniciada) == 1)
            {
                _server.EnviarMensaje(Protocolo.IniciarJuego, new { error = "La partida ya estaba en curso." });

                _server.EnviarMensaje(Protocolo.CerrarJuego, new { });

                Console.WriteLine("[Juego] Conexión cerrada manualmente.");
                Environment.Exit(0);
                return;
            }

            if (jugador1 == null || jugador2 == null || jugador3 == null || jugador4 == null || jugador1.UID == "" || jugador2.UID == "" || jugador3.UID == "" || jugador4.UID == "")
            {
                Console.WriteLine("[Juego] Jugadores insuficientes para comenzar.");

                _server.EnviarMensaje(Protocolo.IniciarJuego, new { error = "Jugadores insuficientes para comenzar." });
                return;
            }
            else
            {
                if (Interlocked.CompareExchange(ref _partidaIniciada, 1, 0) != 0)
                {
                    _server.EnviarMensaje(Protocolo.IniciarJuego, new { error = "La partida ya estba en curso." });

                    _server.EnviarMensaje(Protocolo.CerrarJuego, new { });

                    Console.WriteLine("[Juego] Conexión cerrada manualmente.");
                    Environment.Exit(0);
                    return;
                }

                Console.WriteLine("[Juego] Iniciando juego...");

                ColaTurnos = InicializarTurnos(jugador1, jugador2, jugador3, jugador4);
                Turno = 0;
                jugadorActual = ColaTurnos.Peek();
                dado = new Dado();

                IniciarPartida();
            }
        }



        /// <summary>
        /// Inicia la partida.
        /// </summary>
        public void IniciarPartida()
        {

            ColaTurnos = InicializarTurnos(jugador1!, jugador2!, jugador3!, jugador4!);
            Turno = 0;


            Console.WriteLine("El juego ha comenzado.");

            _server.EnviarMensaje(Protocolo.IniciarJuego, new { });

            while (true)
            {
                Console.WriteLine("El jugador actual es: " + jugadorActual?.Nombre);

                _server.EnviarMensaje(Protocolo.InicioTurno, new
                {
                    jugadorId = jugadorActual?.ID,
                    turno = Turno
                }
                );


                AccionCasilla accion = PrepararTurno();
                Console.WriteLine(accion);

                bool sigueEnJuego = _manejadorAcciones.EjecutarAccion(accion, jugadorActual!, Turno);
                if (sigueEnJuego)
                {
                    Console.WriteLine("Fin del turno.");

                    _server.EnviarMensaje(Protocolo.FinTurno, new { jugadorId = jugadorActual!.ID, turno = Turno });

                    ColaTurnos.Advance();
                }
                else
                {
                    Console.WriteLine("Se eliminó al jugador" + jugadorActual!.Nombre);

                    _server.EnviarMensaje(Protocolo.JugadorEliminado, new
                    {
                        jugadorId = jugadorActual.ID,
                        mensaje = "Se eliminó al jugador" + jugadorActual.Nombre
                    }
                    );

                    ColaTurnos.Dequeue();
                }
                Turno++;

                if (ColaTurnos.Size == 1)
                {
                    Console.WriteLine($"Ganó jugador {ColaTurnos.Peek().Nombre}");

                    _server.EnviarMensaje(Protocolo.Ganador, new { jugadorId = ColaTurnos.Peek().ID });

                    _server.EnviarMensaje(Protocolo.TerminarJuego, new
                    {
                        ganadorId = ColaTurnos.Peek().ID,
                        jugadores = new[] { jugador1, jugador2, jugador3, jugador4 }
                            .Where(jugador => jugador != null)
                            .OrderByDescending(jugador => jugador!.Saldo)
                            .Select(jugador => new { jugador = jugador!.Nombre, saldo = jugador.Saldo })
                    }
                    );

                    break;
                }
                else if (Turno == 1000)
                {
                    Console.WriteLine("Se ha alcanzado el turno 1000.");

                    var jugadoresActivos = new[] { jugador1, jugador2, jugador3, jugador4 }
                        .Where(jugador => jugador != null)
                        .OrderByDescending(jugador => jugador!.Saldo)
                        .ToArray();

                    var ganadorFinal = jugadoresActivos.FirstOrDefault();

                    _server.EnviarMensaje(Protocolo.TerminarJuego, new
                    {
                        ganadorId = ganadorFinal?.ID ?? -1,
                        ganador = ganadorFinal?.Nombre ?? "Nadie",
                        jugadores = jugadoresActivos
                            .Select(jugador => new { jugador = jugador!.Nombre, saldo = jugador.Saldo })
                    });

                    if (ganadorFinal != null)
                    {
                        _server.EnviarMensaje(Protocolo.Ganador, new { jugadorId = ganadorFinal.ID, ganador = ganadorFinal.Nombre });
                    }

                    break;
                }

                jugadorActual = ColaTurnos.Peek();
                _jugadorDesbloqueadoId = -1;
            }
        }
    }
}
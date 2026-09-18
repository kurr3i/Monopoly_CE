using System;
using Proyecto_MonopoTEC.Compartido;
using Proyecto_MonopoTEC.Server.Red;
using Proyecto_MonopoTEC.Server.Modelo;
using Proyecto_MonopoTEC.Server.Estructuras;
using Proyecto_MonopoTEC.Server.Hardware;
using Server.Persistencia;

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
        private readonly RegistroPartida _logger;

        private Jugador jugador1;
        private Jugador jugador2;
        private Jugador jugador3;
        private Jugador jugador4;

        private string[] UIDs = new string[4];

        private Jugador jugadorActual;

        private ColaCircular ColaTurnos;
        private int Turno;

        private Dado dado;

        public Juego(Servidor server, RFIDDriver driver, RegistroPartida logger)
        {
            _server = server;
            _driver = driver;
            _logger = logger;

            _tableroJuego = new Tablero();
            _tableroJuego.Inicializar();

            _manejadorAcciones = new ManejadorAcciones(driver, _tableroJuego);
        }

        private Jugador InicializarJugador(int id, string UID, string nombre, Casilla posicion)
        {
            return new Jugador(id, UID, nombre, posicion);
        }

        public bool AutenticarJugador(int id, string nombre)
        {
            if (id >= 0 && id <= 3)
            {
                string UID = _driver.ReadUID("PASAR J" + (id + 1));
                if (UIDs.Contains(UID))
                {
                    _server.EnviarMensaje(new Mensaje(Protocolo.AutenticarJugador, new { id = -1, error = "Ese jugador ya existe" }));
                    _logger.GuardarRegistro($"Intento de registro duplicado para UID: {UID}", "Juego");
                    return false;
                }
                else
                {
                    UIDs[id] = UID;
                }

                if (id == 0)
                {
                    jugador1 = InicializarJugador(id, UID, nombre, _tableroJuego.Head.Data);
                    _server.EnviarMensaje(new Mensaje(Protocolo.AutenticarJugador, new { id = jugador1.ID, nombre = jugador1.Nombre }));
                }

                if (id == 1)
                {
                    jugador2 = InicializarJugador(id, UID, nombre, _tableroJuego.Head.Data);
                    _server.EnviarMensaje(new Mensaje(Protocolo.AutenticarJugador, new { id = jugador2.ID, nombre = jugador2.Nombre }));
                }

                if (id == 2)
                {
                    jugador3 = InicializarJugador(id, UID, nombre, _tableroJuego.Head.Data);
                    _server.EnviarMensaje(new Mensaje(Protocolo.AutenticarJugador, new { id = jugador3.ID, nombre = jugador3.Nombre }));
                }

                if (id == 3)
                {
                    jugador4 = InicializarJugador(id, UID, nombre, _tableroJuego.Head.Data);
                    _server.EnviarMensaje(new Mensaje(Protocolo.AutenticarJugador, new { id = jugador4.ID, nombre = jugador4.Nombre }));
                }

                _logger.GuardarRegistro($"Jugador {nombre} (ID: {id}, UID: {UID}) autenticado exitosamente.", "Juego");
                return true;
            }

            return false;
        }

        public bool VerificarJugador(int id)
        {
            if (id == 0)
            {
                _driver.ReadUID("PASAR J" + (id + 1), jugador1.UID);
                return true;
            }

            if (id == 1)
            {
                _driver.ReadUID("PASAR J" + (id + 1), jugador2.UID);
                return true;
            }

            if (id == 2)
            {
                _driver.ReadUID("PASAR J" + (id + 1), jugador3.UID);
                return true;
            }

            if (id == 3)
            {
                _driver.ReadUID("PASAR J" + (id + 1), jugador4.UID);
                return true;
            }

            return false;
        }

        public void PruebaConexion(Mensaje mensaje)
        {
            if (mensaje.Comando == Protocolo.Prueba)
            {
                Console.WriteLine("[Juego] Recibido: " + mensaje.Contenido);
                _logger.GuardarRegistro($"Prueba de conexión recibida: {mensaje.Contenido}", "Juego");

                _server.EnviarMensaje(new Mensaje(Protocolo.Prueba,
                new
                {
                    prueba1 = "prueba1",
                    prueba2 = "prueba2"
                }
                ));
            }
        }

        private ColaCircular InicializarTurnos(Jugador jugador1, Jugador jugador2, Jugador jugador3, Jugador jugador4)
        {
            ColaCircular cola = new ColaCircular();
            cola.Enqueue(jugador1);
            cola.Enqueue(jugador2);
            cola.Enqueue(jugador3);
            cola.Enqueue(jugador4);
            return cola;
        }

        private AccionCasilla PrepararTurno()
        {
            bool sigueEnTurno = true;
            while (sigueEnTurno)
            {
                if (jugadorActual.EnCarcel)
                {
                    Console.WriteLine("El jugador está en la carcel");
                    _logger.GuardarRegistro($"El jugador {jugadorActual.Nombre} está en la cárcel.", "Juego");
                    jugadorActual.ReducirCondena();
                    if (jugadorActual.TurnosCarcel == 0)
                    {
                        jugadorActual.SalirCarcel();
                    }
                    else
                    {
                        Console.WriteLine($"Quedan {jugadorActual.TurnosCarcel} turnos en carcel");
                    }
                    return AccionCasilla.SinAccion;
                }
                else if (jugadorActual.TurnosPerdidos != 0)
                {
                    jugadorActual.ReducirTurnoPerdido();
                    return AccionCasilla.SinAccion;
                }

                Console.Clear();
                Console.WriteLine("Elige una opción:\n1. Lanzar el dado\n2. Vender propiedad\n3. Ver propiedades");
                string opcion = ValidarOpcionJugador(Console.ReadLine());

                switch (opcion)
                {
                    case "1":
                        sigueEnTurno = false;
                        break;
                    case "2":
                        _manejadorAcciones.AccionVenderPropiedad(jugadorActual, Turno);
                        Console.WriteLine("Presiona Enter para continuar al siguiente turno...");
                        Console.ReadLine();
                        break;
                    case "3":
                        Console.WriteLine("Sus propiedades son:");
                        jugadorActual.PropiedadesAdquiridas.Display();
                        Console.WriteLine("Presiona Enter para continuar al siguiente turno...");
                        Console.ReadLine();
                        break;
                }
            }

            int resultadoDado1 = dado.Lanzar();
            int resultadoDado2 = dado.Lanzar();
            int totalDado = resultadoDado1 + resultadoDado2;

            Console.WriteLine("El jugador " + jugadorActual.Nombre + " ha lanzado el dado y obtuvo: " + resultadoDado1 + " y " + resultadoDado2);
            Console.WriteLine("Eso suma: " + totalDado);
            _logger.GuardarRegistro($"Jugador {jugadorActual.Nombre} lanzó dados: {resultadoDado1} y {resultadoDado2} (Total: {totalDado})", "Juego");

            bool pasoPorSalida;
            Casilla casillaJugadorActual = _tableroJuego.AvanzarJugador(jugadorActual, totalDado, out pasoPorSalida);
            Console.WriteLine("El jugador " + jugadorActual.Nombre + " se ha movido a la casilla: " + casillaJugadorActual.Nombre);
            _logger.GuardarRegistro($"Jugador {jugadorActual.Nombre} avanzó a la casilla: {casillaJugadorActual.Nombre}", "Juego");

            if (pasoPorSalida)
            {
                _manejadorAcciones.DarPremio(jugadorActual, Turno);
                _logger.GuardarRegistro($"Jugador {jugadorActual.Nombre} pasó por Salida y recibió premio.", "Juego");
            }

            Console.WriteLine("Presiona Enter para continuar...");
            Console.ReadLine();

            return casillaJugadorActual.DevolverAccion(jugadorActual);
        }

        private string ValidarOpcionJugador(string opcion)
        {
            while (opcion != "1" && opcion != "2" && opcion != "3")
            {
                Console.WriteLine("Opción inválida. Por favor, elige una opción válida.");
                opcion = Console.ReadLine();
            }
            return opcion;
        }

        public void IniciarJuego()
        {
            if (jugador1 == null || jugador2 == null || jugador3 == null || jugador4 == null || jugador1.UID == "" || jugador2.UID == "" || jugador3.UID == "" || jugador4.UID == "")
            {
                Console.WriteLine("[Juego] Jugadores insuficientes para comenzar.");
                _logger.GuardarRegistro("Intento de iniciar juego con jugadores insuficientes.", "Juego");

                _server.EnviarMensaje(new Mensaje(Protocolo.IniciarJuego, new { error = "Jugadores insuficientes para comenzar." }));
                return;
            }
            else
            {
                Console.WriteLine("[Juego] Iniciando juego...");
                _logger.GuardarRegistro("Iniciando juego...", "Juego");

                ColaTurnos = InicializarTurnos(jugador1, jugador2, jugador3, jugador4);
                Turno = 0;
                jugadorActual = ColaTurnos.Peek();
                dado = new Dado();

                IniciarPartida();
            }
        }

        public void IniciarPartida()
        {
            ColaTurnos = InicializarTurnos(jugador1, jugador2, jugador3, jugador4);
            Turno = 0;

            Console.WriteLine("El juego ha comenzado. El jugador actual es: " + jugadorActual.Nombre);
            _logger.GuardarRegistro($"El juego ha comenzado. Primer turno para: {jugadorActual.Nombre}", "Juego");

            _server.EnviarMensaje(new Mensaje(Protocolo.IniciarJuego, new { jugadorActual = jugadorActual.Nombre }));

            while (true)
            {
                AccionCasilla accion = PrepararTurno();
                Console.WriteLine(accion);

                bool sigueEnJuego = _manejadorAcciones.EjecutarAccion(accion, jugadorActual, Turno);
                if (sigueEnJuego)
                {
                    ColaTurnos.Advance();
                }
                else
                {
                    Console.WriteLine("Se eliminó al jugador" + jugadorActual);
                    _logger.GuardarRegistro($"Se eliminó al jugador {jugadorActual.Nombre}", "Juego");
                    ColaTurnos.Dequeue();
                }
                Console.WriteLine("Presiona Enter para continuar al siguiente turno...");
                Turno++;

                if (ColaTurnos.Size == 1)
                {
                    Console.WriteLine($"Ganó jugador {ColaTurnos.Peek().Nombre}");
                    _logger.GuardarRegistro($"Ganó el jugador {ColaTurnos.Peek().Nombre}", "Juego");
                    break;
                }
                else if (Turno == 100)
                {
                    _logger.GuardarRegistro("Se alcanzó el límite de 100 turnos.", "Juego");
                }

                jugadorActual = ColaTurnos.Peek();
                Console.ReadLine();
            }
        }
    }
}
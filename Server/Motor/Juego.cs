using System;
using Proyecto_MonopoTEC.Compartido;
using Proyecto_MonopoTEC.Server.Red;
using Proyecto_MonopoTEC.Server.Modelo;
using Proyecto_MonopoTEC.Server.Estructuras;
using Proyecto_MonopoTEC.Server.Hardware;

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

        private Jugador jugador1;
        private Jugador jugador2;
        private Jugador jugador3;
        private Jugador jugador4;

        private string[] UIDs = new string[4];

        private Jugador jugadorActual;


        private ColaCircular ColaTurnos;
        private int Turno;

        private Dado dado;

        /// <summary>
        /// Inicializa una nueva instancia de la clase Juego.
        /// </summary>
        /// <param name="puertoArduino">Nombre del puerto serial utilizado para comunicarse con el Arduino.</param>
        public Juego(Servidor server, RFIDDriver driver)
        {
            _server = server;
            _driver = driver;

            _tableroJuego = new Tablero();
            _tableroJuego.Inicializar();

            _manejadorAcciones = new ManejadorAcciones(driver, _tableroJuego, _server);
        }

        /// <summary>
        /// Método empleado para inicializar un jugador. 
        /// </summary>
        /// <returns>Retorna el objeto jugador inicializado.</returns>
        private Jugador InicializarJugador(int id, string UID, string nombre, Casilla posicion) // Nota: Este método posteriormente se debe modificar para que se inicialice el jugador con los ingresos del cliente y el arduino.
        {
            return new Jugador(id, UID, nombre, posicion);
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

            // Revisar que la ID sea válida
            if (id >= 0 && id <= 3)
            {
                // Hacer la lectura del UID
                string UID = _driver.ReadUID("PASAR J" + (id + 1));
                if (UIDs.Contains(UID))
                {
                    // Si ya existe, enviar una ID inválida, luego se maneja en el Cliente
                    _server.EnviarMensaje(new Mensaje(Protocolo.AutenticarJugador, new { id = -1, error = "Ese jugador ya existe" }));
                    return false;
                }
                else
                {
                    // Si no existe, agregarlo a la lista
                    UIDs[id] = UID;
                }

                // Inicializar el jugador según la ID y responder

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


                _server.EnviarMensaje(new Mensaje(Protocolo.Prueba,
                new
                {
                    prueba1 = "prueba1",
                    prueba2 = "prueba2"
                }
                ));


            }
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

        private AccionCasilla PrepararTurno()
        {
            bool sigueEnTurno = true;
            while (sigueEnTurno)
            {
                if (jugadorActual.EnCarcel)
                {
                    Console.Clear();
                    Console.WriteLine($"{jugadorActual.Nombre} está en la carcel, pierde el turno.");
                    _server.EnviarMensaje(new Mensaje(Protocolo.EnviarProtocolo, new { mensaje = $"{jugadorActual.Nombre} está en la carcel, pierde el turno." })); // *****

                    Console.WriteLine("Presiona Enter para continuar...");
                    _server.EnviarMensaje(new Mensaje(Protocolo.EnviarProtocolo, new { mensaje = "Presiona Enter para continuar..." })); // *****
                    Console.Read();

                    jugadorActual.ReducirCondena();

                    if (jugadorActual.TurnosCarcel == 0)
                    {
                        jugadorActual.SalirCarcel();
                        Console.WriteLine("Saldrá en el siguiente turno.");
                        _server.EnviarMensaje(new Mensaje(Protocolo.EnviarProtocolo, new { mensaje = "Saldrá en el siguiente turno." })); // *****

                        Console.WriteLine("Presiona Enter para continuar...");
                        _server.EnviarMensaje(new Mensaje(Protocolo.EnviarProtocolo, new { mensaje = "Presiona Enter para continuar..." })); // *****
                        Console.Read();
                    }
                    else
                    {
                        Console.WriteLine($"Le quedan {jugadorActual.TurnosCarcel} turnos en carcel.");
                        _server.EnviarMensaje(new Mensaje(Protocolo.EnviarProtocolo, new { mensaje = $"Le quedan {jugadorActual.TurnosCarcel} turnos en carcel." })); // *****

                        Console.WriteLine("Presiona Enter para continuar...");
                        _server.EnviarMensaje(new Mensaje(Protocolo.EnviarProtocolo, new { mensaje = "Presiona Enter para continuar..." })); // *****
                        Console.Read();
                    }
                    return AccionCasilla.SinAccion;
                }

                else if (jugadorActual.TurnosPerdidos != 0)
                {
                    Console.WriteLine($"{jugadorActual.Nombre} pierde el turno.");
                    _server.EnviarMensaje(new Mensaje(Protocolo.EnviarProtocolo, new { mensaje = $"{jugadorActual.Nombre} pierde el turno." })); // *****

                    Console.WriteLine($"Perderá {jugadorActual.TurnosPerdidos} más para volver a jugar.");
                    _server.EnviarMensaje(new Mensaje(Protocolo.EnviarProtocolo, new { mensaje = $"Perderá {jugadorActual.TurnosPerdidos} más para volver a jugar." })); // *****

                    Console.WriteLine("Presiona Enter para continuar...");
                    _server.EnviarMensaje(new Mensaje(Protocolo.EnviarProtocolo, new { mensaje = "Presiona Enter para continuar..." })); // *****
                    Console.Read();

                    jugadorActual.ReducirTurnoPerdido();
                    return AccionCasilla.SinAccion;
                }

                Console.Clear();
                Console.WriteLine($"{jugadorActual.Nombre}, su saldo es de {jugadorActual.Saldo}");
                _server.EnviarMensaje(new Mensaje(Protocolo.EnviarProtocolo, new { mensaje = $"{jugadorActual.Nombre}, su slado es de {jugadorActual.Saldo}" })); // *****

                Console.WriteLine("Elige una opción:\n1. Lanzar el dado\n2. Vender propiedad\n3. Ver propiedades");
                _server.EnviarMensaje(new Mensaje(Protocolo.EnviarProtocolo, new { mensaje = "Elige una opción:\n1. Lanzar el dado\n2. Vender propiedad\n3. Ver propiedades" })); // *****

                string opcion = ValidarOpcionJugador(Console.ReadLine());

                switch (opcion)
                {
                    case "1":
                        sigueEnTurno = false;
                        break;
                    case "2":
                        _manejadorAcciones.AccionVenderPropiedad(jugadorActual, Turno);
                        Console.WriteLine("Presiona Enter para continuar...");
                        _server.EnviarMensaje(new Mensaje(Protocolo.EnviarProtocolo, new { mensaje = "Presiona Enter para continuar..." })); // *****
                        Console.ReadLine(); // Esperar a que el jugador presione Enter antes de continuar

                        break;
                    case "3":
                        Console.WriteLine("Sus propiedades son:");
                        jugadorActual.PropiedadesAdquiridas.Display(_server);
                        // Falta mostrar las propiedades se podría modificar display para que acepte al servidor como parametro. *****

                        Console.WriteLine("Presiona Enter para continuar...");
                        _server.EnviarMensaje(new Mensaje(Protocolo.EnviarProtocolo, new { mensaje = "Presiona Enter para continuar..." })); // *****
                        Console.ReadLine(); // Esperar a que el jugador presione Enter antes de continuar

                        break;
                }
            }

            int resultadoDado1 = dado.Lanzar();
            int resultadoDado2 = dado.Lanzar();

            Console.WriteLine("El jugador " + jugadorActual.Nombre + " ha lanzado el dado y obtuvo: " + resultadoDado1 + " y " + resultadoDado2);
            _server.EnviarMensaje(new Mensaje(Protocolo.EnviarProtocolo, new { mensaje = "El jugador " + jugadorActual.Nombre + " ha lanzado el dado y obtuvo: " + resultadoDado1 + " y " + resultadoDado2 })); // *****

            Console.WriteLine("Eso suma: " + (resultadoDado1 + resultadoDado2));
            _server.EnviarMensaje(new Mensaje(Protocolo.EnviarProtocolo, new { mensaje = "Eso suma: " + (resultadoDado1 + resultadoDado2) })); // *****

            bool pasoPorSalida;
            Casilla casillaJugadorActual = _tableroJuego.AvanzarJugador(jugadorActual, resultadoDado1 + resultadoDado2, out pasoPorSalida);

            Console.WriteLine("El jugador " + jugadorActual.Nombre + " se ha movido a la casilla: " + casillaJugadorActual.Nombre);
            _server.EnviarMensaje(new Mensaje(Protocolo.EnviarProtocolo, new { mensaje = "El jugador " + jugadorActual.Nombre + " se ha movido a la casilla: " + casillaJugadorActual.Nombre })); // *****

            if (pasoPorSalida)
            {
                _manejadorAcciones.DarPremio(jugadorActual, Turno);
            }

            Console.WriteLine("Presiona Enter para continuar...");
            _server.EnviarMensaje(new Mensaje(Protocolo.EnviarProtocolo, new { mensaje = "Presiona Enter para continuar..." })); // *****


            Console.ReadLine(); // Esperar a que el jugador presione Enter antes de continuar

            return casillaJugadorActual.DevolverAccion(jugadorActual);

        }


        // Falta comentar esto.
        private string ValidarOpcionJugador(string opcion)
        {
            while (opcion != "1" && opcion != "2" && opcion != "3")
            {
                Console.WriteLine("Opción inválida. Por favor, elige una opción válida.");
                _server.EnviarMensaje(new Mensaje(Protocolo.EnviarProtocolo, new { mensaje = "Opción inválida. Por favor, elige una opción válida." })); // *****


                opcion = Console.ReadLine();
            }
            return opcion;
        }



        public void IniciarJuego()
        {
            if (jugador1 == null || jugador2 == null || jugador3 == null || jugador4 == null || jugador1.UID == "" || jugador2.UID == "" || jugador3.UID == "" || jugador4.UID == "")
            {
                Console.WriteLine("[Juego] Jugadores insuficientes para comenzar.");

                _server.EnviarMensaje(new Mensaje(Protocolo.IniciarJuego, new { error = "Jugadores insuficientes para comenzar." }));
                return;
            }
            else
            {
                Console.WriteLine("[Juego] Iniciando juego...");

                ColaTurnos = InicializarTurnos(jugador1, jugador2, jugador3, jugador4);
                Turno = 0;

                jugadorActual = ColaTurnos.Peek();

                dado = new Dado();

                IniciarPartida();
            }
        }



        // Falta comentar esto.
        public void IniciarPartida()
        {

            ColaTurnos = InicializarTurnos(jugador1, jugador2, jugador3, jugador4);
            Turno = 0;


            Console.WriteLine("El juego ha comenzado.");
            _server.EnviarMensaje(new Mensaje(Protocolo.EnviarProtocolo, new { mensaje = "El juego ha comenzado." })); // *****


            // Ejemplo de contexto para el Cliente
            _server.EnviarMensaje(new Mensaje(Protocolo.IniciarJuego, new { jugadorActual = jugadorActual.Nombre }));

            while (true) // Bucle infinito para el juego
            {
                AccionCasilla accion = PrepararTurno();
                Console.WriteLine(accion);

                bool sigueEnJuego = _manejadorAcciones.EjecutarAccion(accion, jugadorActual, Turno);
                if (sigueEnJuego)
                {
                    Console.WriteLine($"Fin del turno de {jugadorActual.Nombre}, con un saldo de {jugadorActual.Saldo}");
                    _server.EnviarMensaje(new Mensaje(Protocolo.EnviarProtocolo, new { mensaje = $"Fin del turno de {jugadorActual.Nombre}, con un saldo de {jugadorActual.Saldo}" })); // *****
                    ColaTurnos.Advance();
                }
                else
                {
                    Console.WriteLine("Se eliminó al jugador" + jugadorActual.Nombre);
                    _server.EnviarMensaje(new Mensaje(Protocolo.EnviarProtocolo, new { mensaje = "Se eliminó al jugador" + jugadorActual.Nombre })); // *****
                    ColaTurnos.Dequeue();
                }
                Turno++;

                if (ColaTurnos.Size == 1)
                {
                    Console.WriteLine($"Ganó jugador {ColaTurnos.Peek().Nombre}");
                    _server.EnviarMensaje(new Mensaje(Protocolo.EnviarProtocolo, new { mensaje = $"Ganó jugador {ColaTurnos.Peek().Nombre}" })); // *****
                    break;
                }
                else if (Turno == 100)
                {
                    //Se debe evaluar quien tiene más dinero y valor en propiedades
                }

                jugadorActual = ColaTurnos.Peek();

                Console.WriteLine("Presiona Enter para continuar...");
                _server.EnviarMensaje(new Mensaje(Protocolo.EnviarProtocolo, new { mensaje = "Presiona Enter para continuar..." })); // *****
                Console.ReadLine(); // Esperar a que el jugador presione Enter antes de continuar
            }


        }
    }
}
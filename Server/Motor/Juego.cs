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

            _manejadorAcciones = new ManejadorAcciones(driver, _tableroJuego);

            dado = new Dado();
        }

        /// <summary>
        /// Método empleado para inicializar un jugador. 
        /// </summary>
        /// <returns>Retorna el objeto jugador inicializado.</returns>
        private Jugador InicializarJugador(int id, string UID, string nombre, Casilla posicion) // Nota: Este método posteriormente se debe modificar para que se inicialice el jugador con los ingresos del cliente y el arduino.
        {
            return new Jugador(id, UID, nombre, posicion);
        }



        public bool AutenticarJugador(int id, string nombre)
        {
            if (id == 0)
            {
                string UID = _driver.ReadUID("AUTH_0");
                jugador1 = InicializarJugador(id, UID, nombre, _tableroJuego.Head.Data);

                _server.EnviarMensaje(new Mensaje(Protocolo.AutenticarJugador, new { id = 0 }));
            }

            return true;
        }

        public bool VerificarJugador(int id)
        {
            if (id == 0)
            {
                _driver.ReadUID("AUTH_0", jugador1.UID);
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
                    Console.WriteLine("El jugador está en la carcel");
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
                        Console.ReadLine(); // Esperar a que el jugador presione Enter antes de continuar

                        break;
                    case "3":
                        Console.WriteLine("Sus propiedades son:");
                        jugadorActual.PropiedadesAdquiridas.Display();
                        Console.WriteLine("Presiona Enter para continuar al siguiente turno...");
                        Console.ReadLine(); // Esperar a que el jugador presione Enter antes de continuar

                        break;
                }
            }

            int resultadoDado1 = dado.Lanzar();
            int resultadoDado2 = dado.Lanzar();

            Console.WriteLine("El jugador " + jugadorActual.Nombre + " ha lanzado el dado y obtuvo: " + resultadoDado1 + " y " + resultadoDado2);
            Console.WriteLine("Eso suma: " + (resultadoDado1 + resultadoDado2));

            bool pasoPorSalida;
            Casilla casillaJugadorActual = _tableroJuego.AvanzarJugador(jugadorActual, resultadoDado1 + resultadoDado2, out pasoPorSalida);
            Console.WriteLine("El jugador " + jugadorActual.Nombre + " se ha movido a la casilla: " + casillaJugadorActual.Nombre);
            if (pasoPorSalida)
            {
                _manejadorAcciones.DarPremio(jugadorActual, Turno);
            }

            Console.WriteLine("Presiona Enter para continuar...");
            Console.ReadLine(); // Esperar a que el jugador presione Enter antes de continuar

            return casillaJugadorActual.DevolverAccion(jugadorActual);

        }


        // Falta comentar esto.
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
            Console.WriteLine("[Juego] Iniciando juego.");
            Console.WriteLine("[Juego] Esperando jugadores...");
            // Me falta añadir el lobby antes de comenzar la partida
        }



        // Falta comentar esto.
        public void IniciarPartida()
        {
            ColaTurnos = InicializarTurnos(jugador1, jugador2, jugador3, jugador4);
            Turno = 0;

            jugadorActual = ColaTurnos.Peek();

            Console.WriteLine("El juego ha comenzado. El jugador actual es: " + jugadorActual.Nombre);

            while (true) // Bucle infinito para el juego
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
                    ColaTurnos.Dequeue();
                }
                Console.WriteLine("Presiona Enter para continuar al siguiente turno...");
                Turno++;

                if (ColaTurnos.Size == 1)
                {
                    Console.WriteLine($"Ganó jugador {ColaTurnos.Peek().Nombre}");
                    break;
                }
                else if (Turno == 100)
                {
                    //Se debe evaluar quien tiene más dinero y valor en propiedades
                }

                jugadorActual = ColaTurnos.Peek();

                Console.ReadLine(); // Esperar a que el jugador presione Enter antes de continuar
            }


        }
    }
}
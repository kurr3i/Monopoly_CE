using System;

/// <summary>
/// Representa el juego.
/// </summary>
public class Juego
{
    private Tablero _tableroJuego;
    private ManejadorAcciones _manejadorAcciones;

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
    public Juego(string puertoArduino)
    {
        _tableroJuego = new Tablero();
        _tableroJuego.Inicializar();

        _manejadorAcciones = new ManejadorAcciones(puertoArduino, _tableroJuego);

        jugador1 = InicializarJugador(123, "Josué", _tableroJuego.Head.Data);
        jugador2 = InicializarJugador(456, "Joshua", _tableroJuego.Head.Data);
        jugador3 = InicializarJugador(789, "Kevin", _tableroJuego.Head.Data);
        jugador4 = InicializarJugador(101, "Ignacio", _tableroJuego.Head.Data);

        ColaTurnos = InicializarTurnos(jugador1, jugador2, jugador3, jugador4);
        Turno = 0;

        jugadorActual = ColaTurnos.Peek();

        dado = new Dado();
    }

    /// <summary>
    /// Método empleado para inicializar un jugador. 
    /// </summary>
    /// <returns>Retorna el objeto jugador inicializado.</returns>
    private Jugador InicializarJugador(int id, string nombre, Casilla posicion) // Nota: Este método posteriormente se debe modificar para que se inicialice el jugador con los ingresos del cliente y el arduino.
    {
        return new Jugador(id, nombre, posicion);
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
                    _manejadorAcciones.AccionVenderPropiedad(jugadorActual,Turno);
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

    

    // Falta comentar esto.
    public void IniciarJuego()
    {
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

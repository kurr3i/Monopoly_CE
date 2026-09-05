using System;



public enum SubtipoCasillaEspecial
{
    Salida,
    Carcel,
    PaqueDiversiones,
    VayaCarcel
}

/// <summary>
/// Representa el juego.
/// </summary>
public class Juego
{
    private Tablero TableroJuego;
    private ManejadorAcciones manejadorAcciones;

    private Jugador jugador1;
    private Jugador jugador2;
    private Jugador jugador3;
    private Jugador jugador4;
    private Jugador jugadorActual;
     

    private ColaCircular ColaTurnos;

    private Dado dado;

    public Juego()
    {
        TableroJuego = new Tablero();
        TableroJuego.Inicializar();

        manejadorAcciones = new ManejadorAcciones();

        jugador1 = InicializarJugador(123, "Josué", TableroJuego.Head.Data);
        jugador2 = InicializarJugador(456, "Joshua", TableroJuego.Head.Data);
        jugador3 = InicializarJugador(789, "Kevin", TableroJuego.Head.Data);
        jugador4 = InicializarJugador(101, "Ignacio", TableroJuego.Head.Data);

        ColaTurnos = InicializarTurnos(jugador1, jugador2, jugador3, jugador4);

        jugadorActual = ColaTurnos.Peek();

        dado = new Dado();
    }

    /// <summary>
    /// Método empleado para inicializar un jugador. 
    /// </summary>
    /// <returns>Retorna el objeto jugador inicializado.</returns>
    private Jugador InicializarJugador(int ID, string Nombre, Casilla Posicion) // Nota: Este método posteriormente se debe modificar para que se inicialice el jugador con los ingresos del cliente y el arduino.
    {
        return new Jugador(ID, Nombre, Posicion);
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
            Console.Clear();
            Console.WriteLine("Elige una opción:\n1. Lanzar el dado\n2. Vender propiedad\n3. Ver propiedades");
            string opcion = ValidarOpcionJugador(Console.ReadLine());

            switch (opcion)
            {
                case "1":
                    sigueEnTurno = false; 
                    break;
                case "2":
                    Console.WriteLine("Por ahora no se ha implementado lo de la venta");
                    Console.WriteLine("Presiona Enter para continuar al siguiente turno...");
                    Console.ReadLine(); // Esperar a que el jugador presione Enter antes de continuar

                    break;
                case "3":
                    Console.WriteLine("Por ahora no se ha implementado lo de ver propiedades");
                    Console.WriteLine("Presiona Enter para continuar al siguiente turno...");
                    Console.ReadLine(); // Esperar a que el jugador presione Enter antes de continuar

                    break;
            }
        }

        int resultadoDado1 = dado.Lanzar();
        int resultadoDado2 = dado.Lanzar();

        Console.WriteLine("El jugador " + jugadorActual.Nombre + " ha lanzado el dado y obtuvo: " + resultadoDado1 + " y " + resultadoDado2);
        Console.WriteLine("Eso suma: " + (resultadoDado1 + resultadoDado2));


        Casilla CasillaJugadorActual = TableroJuego.MoverJugador(jugadorActual, resultadoDado1 + resultadoDado2);
        Console.WriteLine("El jugador " + jugadorActual.Nombre + " se ha movido a la casilla: " + CasillaJugadorActual.Nombre);

        Console.WriteLine("Presiona Enter para continuar...");
        Console.ReadLine(); // Esperar a que el jugador presione Enter antes de continuar

        return CasillaJugadorActual.DevolverAccion(jugadorActual);
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

            bool sigueEnJuego = manejadorAcciones.EjecutarAccion(jugadorActual.Posicion,accion,jugadorActual);
            if (sigueEnJuego)
            {
                ColaTurnos.Advance();
                jugadorActual = ColaTurnos.Peek();
            }
            else
            {
                Console.WriteLine("Se eliminó al jugador" + jugadorActual);
                ColaTurnos.Dequeue();
            }
            Console.WriteLine("Presiona Enter para continuar al siguiente turno...");
            Console.ReadLine(); // Esperar a que el jugador presione Enter antes de continuar
        }


    }
}

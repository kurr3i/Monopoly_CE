using System;

/// <summary>
/// Representa el juego.
/// </summary>
public class Juego
{
    private Tablero TableroJuego;

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

        cola.Add(jugador1);
        cola.Add(jugador2);
        cola.Add(jugador3);
        cola.Add(jugador4);

        return cola;
    }


    private string ValidarOpcionJugador(string opcion)
    {
        while (opcion != "1" && opcion != "2")
        {
            Console.WriteLine("Opción inválida. Por favor, elige una opción válida.");
            opcion = Console.ReadLine();
        }
        return opcion;
    }
    public void IniciarJuego()
    {
        Console.WriteLine("El juego ha comenzado. El jugador actual es: " + jugadorActual.Nombre);

        while (true) // Bucle infinito para el juego
        {
            Console.Clear();

            Console.WriteLine("Elige una opción: \n1. Lanzar el dado\n2. Vender propiedad");
            string opcion = ValidarOpcionJugador(Console.ReadLine());

            switch (opcion)
            {
                case "1":
                    int resultadoDado1 = dado.Lanzar();
                    int resultadoDado2 = dado.Lanzar();
                    Console.WriteLine("El jugador " + jugadorActual.Nombre + " ha lanzado el dado y obtuvo: " + resultadoDado1 + " y " + resultadoDado2);
                    Console.WriteLine("Eso suma: " + (resultadoDado1 + resultadoDado2));
                    // Aquí se puede agregar la lógica para mover al jugador en el tablero según el resultado del dado
                    // Avanzar al siguiente jugador
                    break;
                case "2":
                    Console.WriteLine("Por ahora no se ha implementado lo de la venta");
                    break; // Salir del bucle y terminar el juego
            }

            Console.WriteLine("Presiona Enter para continuar al siguiente turno...");
            Console.ReadLine(); // Esperar a que el jugador presione Enter antes de continuar



        }
    }
}

using System;

/// <summary>
/// Representa el juego.
/// </summary>
public class Juego
{
    private Tablero TableroJuego;

    private Casilla casilla1;
    private Casilla casilla2;
    private Casilla casilla3;

    private Jugador jugador1;
    private Jugador jugador2;

    private Dado dado1;
    private Dado dado2;

    public Juego()
    {
        TableroJuego = new Tablero();

        casilla1 = new Casilla(1, "Casilla 1");
        casilla2 = new Casilla(2, "Casilla 2");
        casilla3 = new Casilla(3, "Casilla 3");

        TableroJuego.Add(casilla1);
        TableroJuego.Add(casilla2);
        TableroJuego.Add(casilla3);

        dado1 = new Dado();
        dado2 = new Dado();

        jugador1 = new Jugador(123, "Josué", casilla1);
        jugador2 = new Jugador(456, "Juan", casilla1);

    }

    public void IniciarJuego()
    {
        // Lógica para iniciar el juego
        Console.WriteLine("El juego ha comenzado.");

        while (jugador1.Saldo > 0)
        {
            Console.Clear();

            Console.WriteLine("Turno de " + jugador1.Nombre);
            Console.WriteLine("Saldo actual: " + jugador1.Saldo);
            Console.WriteLine("Presiona 1 para lanzar el dado...");

            int input = Convert.ToInt32(Console.ReadLine());
            if (input == 1)
            {
                int resultado1 = dado1.Lanzar();
                int resultado2 = dado2.Lanzar();

                int movimiento = resultado1 + resultado2;

                Console.WriteLine("Has lanzado un " + resultado1 + " y un " + resultado2 + ". Total: " + movimiento);

                Node nodoActual = TableroJuego.Head;

                for (int i = 0; i < movimiento; i++)
                {
                    nodoActual = nodoActual.Next;
                }

                jugador1.PosicionActual = (Casilla)nodoActual.Data;
                Console.WriteLine("Te has movido a la casilla: " + jugador1.PosicionActual.Nombre);
            }
            else
            {
                Console.WriteLine("Entrada no válida. Por favor, presiona 1 para lanzar el dado.");
            }

            jugador1.Saldo -= 200;

            Console.WriteLine("Presione Enter para continuar al siguiente turno...");
            Console.ReadLine();
        }
    }
}

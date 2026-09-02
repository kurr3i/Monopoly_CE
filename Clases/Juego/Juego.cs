using System;

/// <summary>
/// Representa el juego.
/// </summary>
public class Juego
{
	private Tablero TableroJuego; 

	private Casilla Casilla1;
	private Casilla Casilla2;
	private Casilla Casilla3;

    private Jugador Jugador1;
    private Jugador Jugador2;

    public Juego()
    {
        TableroJuego = new Tablero();

        Casilla1 = new Casilla(1, "Casilla 1");
        Casilla2 = new Casilla(2, "Casilla 2");
        Casilla3 = new Casilla(3, "Casilla 3");

        Jugador1 = new Jugador(123, "Josué", Casilla1);
        Jugador2 = new Jugador(456, "Juan", Casilla1);

    }

}

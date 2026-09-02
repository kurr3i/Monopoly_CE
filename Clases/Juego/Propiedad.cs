using System;

/// <summary>
/// Representa a una casilla propiedad dentro del tablero del juego.
/// </summary>
public class Propiedad : Casilla
{
	public int PrecioCompra { get; set; }
	public int Alquiler { get; set; }
	public Jugador Propietario { get; set; } 

    public Propiedad(int ID, string Nombre, int PrecioCompra, int Alquiler) : base(ID, Nombre)
	{
		this.PrecioCompra = PrecioCompra;
		this.Alquiler = Alquiler;
		this.Propietario = null;
	}
}

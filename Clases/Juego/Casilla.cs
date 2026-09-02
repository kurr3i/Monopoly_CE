using System;

/// <summary>
/// Representa a una casilla dentro del tablero del juego.
/// </summary>
public class Casilla
{
	public int ID { get; set; }
    public string Nombre { get; set; }
	// Pendiente: Agregar más propiedades que compartiran Propiedad, CasillaEvento y CasillaEspecial

    public Casilla(int ID, string Nombre)
	{
		this.ID = ID;
		this.Nombre = Nombre;
	}
}

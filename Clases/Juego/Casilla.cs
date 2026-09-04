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

    /// <summary>
    /// Método que modificaran las clases hijas para devolver la acción que se debe realizar al caer en la casilla.
    /// </summary>
	/// <param name="jugador">El jugador que ha caído en la casilla.</param>
    /// <returns>La acción que se debe realizar.</returns>
    public virtual AccionCasilla DevolverAccion(Jugador jugador)
	{
		return AccionCasilla.SinAccion;
	}
}

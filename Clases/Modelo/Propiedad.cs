using System;

/// <summary>
/// Representa a una casilla propiedad dentro del tablero del juego.
/// </summary>
public class Propiedad : Casilla
{
	public int PrecioCompra { get; set; }
	public int Alquiler { get; set; }
	public Jugador Propietario { get; set; } 

    public Propiedad(int id, string nombre, int precioCompra, int alquiler) : base(id, nombre)
	{
		this.PrecioCompra = precioCompra;
		this.Alquiler = alquiler;
		this.Propietario = null;
	}

    /// <summary>
    /// Determina la acción que se debe realizar al caer en la casilla propiedad según su propietario.
    /// </summary>
    /// <param name="jugador">El jugador que ha caído en la casilla.</param>
    /// <returns>La acción que se debe realizar.</returns>
    public override AccionCasilla DevolverAccion(Jugador jugador)
    {
        if (Propietario == jugador)
        {
            return AccionCasilla.SinAccion; // El jugador es el propietario de la propiedad, no se realiza ninguna acción.
        }
        else if (Propietario == null)
        {
            return AccionCasilla.PermitirComprar; // La propiedad no tiene propietario, se puede comprar.
        }
        else
        {
            return AccionCasilla.CobrarAlquiler; // La propiedad tiene un propietario diferente al jugador, se debe pagar alquiler.
        }
    }
}

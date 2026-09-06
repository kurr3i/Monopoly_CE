
/// <summary>
/// Representa a una casilla dentro del tablero del juego.
/// </summary>
public class Casilla
{
	public int ID { get; private set; }
    public string Nombre { get; private set; }

    public Casilla(int id, string nombre)
	{
		this.ID = id;
		this.Nombre = nombre;
	}

    /// <summary>
    /// Método que las clases hijas pueden sobrescribir para determinar la acción que se debe realizar al caer en la casilla.
    /// </summary>
	/// <param name="jugador">El jugador que ha caído en la casilla.</param>
    /// <returns>La acción que se debe realizar.</returns>
    public virtual AccionCasilla DevolverAccion(Jugador jugador)
	{
		return AccionCasilla.SinAccion;
	}
}

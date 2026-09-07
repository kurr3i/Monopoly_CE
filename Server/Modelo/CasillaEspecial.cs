
public enum SubtipoCasillaEspecial
{
    Salida,
    Carcel,
    ParqueDiversiones,
    VayaCarcel
}
/// <summary>
/// Representa a una casilla especial dentro del tablero del juego.
/// </summary>
public class CasillaEspecial : Casilla
{
    public SubtipoCasillaEspecial Subtipo { get; private set; }
    public CasillaEspecial(int id, string nombre, SubtipoCasillaEspecial subtipo) : base(id, nombre)
    {
        this.Subtipo = subtipo;
    }

    /// <summary>
    /// Determina la acción que se debe realizar al caer en la casilla especial según su subtipo.
    /// </summary>
    /// <param name="jugador">El jugador que ha caído en la casilla.</param>    
    /// <returns>La acción que se debe realizar.</returns>
    public override AccionCasilla DevolverAccion(Jugador jugador)
    {
        if (Subtipo == SubtipoCasillaEspecial.VayaCarcel)
        {
            return AccionCasilla.MandarCarcel;
        }

        return AccionCasilla.SinAccion; // En caso de que no sea la subcasilla VayaCarcel

    }
}

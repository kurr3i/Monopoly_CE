
public class CasillaEvento : Casilla
{
    public CasillaEvento(int id, string nombre) : base(id, nombre)
    {
    }

    /// <summary>
    /// Determina la acción que se debe realizar al caer en la casilla de evento.
    /// </summary>
    /// <param name="jugador">El jugador que ha caído en la casilla.</param>
    /// <returns>La acción que se debe realizar.</returns>
    public override AccionCasilla DevolverAccion(Jugador jugador)
    {
        return AccionCasilla.DarCarta;
    }
}

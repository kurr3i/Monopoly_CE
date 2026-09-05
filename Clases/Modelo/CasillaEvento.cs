using System;

public class CasillaEvento : Casilla
{
    public CasillaEvento(int ID, string Nombre) : base(ID, Nombre)
    {
    }

    /// <summary>
    /// Método que devuelve la acción que se debe realizar al caer en la casilla evento.
    /// </summary>
    /// <param name="jugador">El jugador que ha caído en la casilla.</param>
    /// <returns>La acción que se debe realizar.</returns>
    public override AccionCasilla DevolverAccion(Jugador jugador)
    {
        return AccionCasilla.DarCarta;
    }
}

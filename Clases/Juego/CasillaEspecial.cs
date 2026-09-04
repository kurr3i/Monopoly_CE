using System;

public class CasillaEspecial : Casilla
{
    public SubtipoCasillaEspecial Subtipo { get; private set; }
    public CasillaEspecial(int ID, string Nombre, SubtipoCasillaEspecial Subtipo) : base(ID, Nombre)
    {
        this.Subtipo = Subtipo;
    }

    /// <summary>
    /// Método que devuelve la acción que se debe realizar al caer en la casilla especial, dependiendo de su subtipo.
    /// </summary>
    /// <param name="jugador">El jugador que ha caído en la casilla.</param>    
    /// <returns>La acción que se debe realizar.</returns>
    public override AccionCasilla DevolverAccion(Jugador jugador)
    {
        if (Subtipo == SubtipoCasillaEspecial.VayaCarcel)
        {
            return AccionCasilla.MandarCarcel;
        }
        else
        {
            return AccionCasilla.SinAccion;
        }
    }
}

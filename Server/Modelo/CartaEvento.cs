using System;

public enum TipoCartaEvento
{
	GanoColones,
	PerdioColones,
	Avanzar,
	Retroceder,
	PerderTurno,
	AvanzarSalida,
	AvanzarParque,
	VayaCarcel
}

/// <summary>
/// Representa a una carta de evento de la baraja de cartas evento.
/// </summary>
public class CartaEvento
{
    public TipoCartaEvento Tipo { get; private set; }
    public string Descripcion { get; private set; }
	public int Valor { get; private set; }

    public CartaEvento(TipoCartaEvento tipo, int valor, string descripcion)
	{
		this.Tipo = tipo;
		this.Valor = valor;
		this.Descripcion = descripcion;
	}
}

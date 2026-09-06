using System;

/// <summary>
/// Representa el tipo de transacción.
/// </summary>
public enum TipoTransaccion
{
    CompraPropiedad,
    VentaPropiedad,
    PagoAlquiler,
    PagoBanco,
    PagoEntreJugadores,
    GananciaEvento,
    PerdidaEvento,
    PremioPorInicio
}

/// <summary>
/// Representa una transacción económica.
/// </summary>
public class Transaccion
{
	public int ID {  get; private set; }
	public DateTime FechaHora { get; private set; }
	public int NumeroTurno { get; private set; }
    public TipoTransaccion Tipo {  get; private set; }
    public Jugador JugadorOrigen {  get; private set; }
    public Jugador JugadorDestino { get; private set; }
    public int Monto { get; private set; }
    public string Descripcion { get; private set; }

    /// <summary>
    /// Inicializa una nueva transacción económica.
    /// </summary>
    /// <param name="id">Identificador de la transacción.</param>
    /// <param name="numeroTurno">Número de turno en el que se realizó la transacción.</param>
    /// <param name="tipo">Tipo de transacción realizada.</param>
    /// <param name="jugadorOrigen">Jugador que origina la transacción.</param>
    /// <param name="jugadorDestino">Jugador que recibe el monto de la transacción.</param>
    /// <param name="monto">Monto de la transacción.</param>
    /// <param name="descripcion">Descripción de la transacción.</param>
    public Transaccion(int id, int numeroTurno, TipoTransaccion tipo, Jugador jugadorOrigen, Jugador jugadorDestino, int monto, string descripcion)
	{
        this.ID = id;
        this.FechaHora = DateTime.Now;
        this.NumeroTurno = numeroTurno;
        this.Tipo = tipo;
        this.JugadorOrigen = jugadorOrigen;
        this.JugadorDestino = jugadorDestino;
        this.Monto = monto;
        this.Descripcion = descripcion;
	}
}

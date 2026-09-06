
/// <summary>
/// Representa el banco del juego y se encarga de las operaciones económicas.
/// </summary>
public class Banco
{
    /// <summary>
    /// Gestiona la comunicación con el lector RFID mediante Arduino.
    /// </summary>
    private  readonly RFIDDriver _rfidDriver;

    /// <summary>
    /// Inicializa una nueva instancia de la clase Banco.
    /// </summary>
    /// <param name="puertoArduino">Nombre del puerto serial utilizado para comunicarse con el Arduino.</param>
    public Banco(string puertoArduino)
    {
        _rfidDriver = new RFIDDriver(puertoArduino);
    }

    /// <summary>
    /// Verifica si un jugador tiene saldo suficiente para realizar un pago.
    /// </summary>
    /// <param name="jugadorEvaluar">El jugador que se va a evaluar.</param>
    /// <param name="montoPagar">El monto con el que se va a evaluar si puede o no pagar.</param>
    /// <returns>Booleano para saber si puede o no pagar.</returns>
    public bool PuedePagar (Jugador jugadorEvaluar, int montoPagar)
	{
        return jugadorEvaluar.Saldo >= montoPagar; // Booleano según la comparacion del saldo y el monto
	}

    /// <summary>
    /// Realiza un pago del jugador al banco.
    /// </summary>
    /// <param name="jugadorOrigen">El jugador que va a pagar.</param>
    /// <param name="montoPagar">El monto que debe pagar.</param>
    public void PagarAlBanco(Jugador jugadorOrigen, int montoPagar)
    {
        _rfidDriver.ReadUID("PAGUE", jugadorOrigen.UID);
        jugadorOrigen.DisminuirSaldo(montoPagar); // Solo le restamos el monto
    }

    /// <summary>
    /// Realiza un pago de un jugador a otro.
    /// </summary>
    /// <param name="jugadorOrigen">El jugador que va a pagar.</param>
    /// <param name="jugadorDestino">El jugador que va a recibir el pago.</param>
    /// <param name="montoPagar">El monto que debe pagar el jugador de origen.</param>
    public void PagarAlJugador(Jugador jugadorOrigen, Jugador jugadorDestino, int montoPagar)
    {
        _rfidDriver.ReadUID("PAGUE", jugadorOrigen.UID);
        jugadorOrigen.DisminuirSaldo(montoPagar); // Le quitamos el monto que debe pagar al jugador de origen
        jugadorDestino.AumentarSaldo(montoPagar); // Le damos el monto al jugador destino
    }

    /// <summary>
    /// Registra la compra de una propiedad por parte de un jugador.
    /// </summary>
    /// <param name="jugadorCompra">El jugador que va a comprar la propiedad.</param>
    /// <param name="propiedadCompra">La propiedad que va a comprar.</param>
    public void ComprarPropiedad(Jugador jugadorCompra, Propiedad propiedadCompra)
    {
        _rfidDriver.ReadUID("PAGUE", jugadorCompra.UID);
        jugadorCompra.DisminuirSaldo(propiedadCompra.PrecioCompra);
        propiedadCompra.Propietario = jugadorCompra;
        jugadorCompra.PropiedadesAdquiridas.Add(propiedadCompra);
    }

    /// <summary>
    /// Registra la venta de una propiedad por parte de un jugador.
    /// </summary>
    /// <param name="jugadorVenta">El jugador que va a realizar la venta de la propiedad.</param>
    /// <param name="propiedadVenta">La propiedad que va a ser vendida.</param>
    public void VenderPropiedad(Jugador jugadorVenta, Propiedad propiedadVenta)
    {
        jugadorVenta.AumentarSaldo(propiedadVenta.PrecioCompra);
        propiedadVenta.Propietario = null;
        jugadorVenta.PropiedadesAdquiridas.Remove(propiedadVenta);
    }
}

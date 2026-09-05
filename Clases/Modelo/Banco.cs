using System;

/// <summary>
/// Representa el banco del juego, se encarga de toda modificación económica
/// </summary>
public class Banco
{
    /// <summary>
    /// Método que verifica si un jugador puede pagar (propiedades, impuestos de las tarjetas, salida de la carcel o renta).
    /// </summary>
    /// <param name="jugadorEvaluar">El jugador que se va a evaluar.</param>
    /// <param name="montoPagar">El monto con el que se va a evaluar si puede o no pagar.</param>
    /// <returns>Booleano para saber si puede o no pagar.</returns>
    public bool PuedePagar (Jugador jugadorEvaluar, int montoPagar)
	{
        if (jugadorEvaluar.Saldo < montoPagar)
        {
            return false; // No puede pagar
        }
        else
        {
            return true; // Si puede pagar
        }
	}

    /// <summary>
    /// Método para realizar un pago al banco.
    /// </summary>
    /// <param name="jugadorOrigen">El jugador que va a pagar.</param>
    /// <param name="montoPagar">El monto que debe pagar.</param>
    public void PagarAlBanco(Jugador jugadorOrigen, int montoPagar)
    {
        //Aquí falta la conexión con el Arduino
        jugadorOrigen.Saldo -= montoPagar; // Solo le restamos el monto
    }

    /// <summary>
    /// Método para realizar un pago entre jugadores.
    /// </summary>
    /// <param name="jugadorOrigen">El jugador que va a pagar.</param>
    /// <param name="jugadorDestino">El jugador que va a recibir el pago.</param>
    /// <param name="montoPagar">El monto que debe pagar el jugador de origen.</param>
    public void PagarAlJugador(Jugador jugadorOrigen, Jugador jugadorDestino, int montoPagar)
    {
        //Aquí falta la conexión con el Arduino
        jugadorOrigen.Saldo -= montoPagar; // Le quitamos el monto que debe pagar al jugador de origen
        jugadorDestino.Saldo += montoPagar; // Le damos el monto al jugador destino
    }

    /// <summary>
    /// Método para que un jugador compre una propiedad.
    /// </summary>
    /// <param name="jugadorCompra">El jugador que va a comprar la propiedad.</param>
    /// <param name="propiedadCompra">La propiedad que va a comprar.</param>
    public void ComprarPropiedad(Jugador jugadorCompra, Propiedad propiedadCompra)
    {
        //Aquí falta la conexión con el Arduino
        jugadorCompra.Saldo -= propiedadCompra.PrecioCompra;
        propiedadCompra.Propietario = jugadorCompra;
        jugadorCompra.PropiedadesAdquiridas.Add(propiedadCompra);
    }

    /// <summary>
    /// Método para que un jugador venda una propiedad.
    /// </summary>
    /// <param name="jugadorVenta">El jugador que va a realizar la venta de la propiedad.</param>
    /// <param name="propiedadVenta">La propiedad que va a ser vendida.</param>
    public void VenderPropiedad(Jugador jugadorVenta, Propiedad propiedadVenta)
    {
        jugadorVenta.Saldo += propiedadVenta.PrecioCompra;
        propiedadVenta.Propietario = null;
        jugadorVenta.PropiedadesAdquiridas.Remove(propiedadVenta);
    }
}

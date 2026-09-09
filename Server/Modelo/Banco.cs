using Proyecto_MonopoTEC.Server.Hardware;
using Proyecto_MonopoTEC.Server.Estructuras;

namespace Proyecto_MonopoTEC.Server.Modelo
{

    /// <summary>
    /// Representa el banco del juego y se encarga de las operaciones económicas.
    /// </summary>
    public class Banco
    {
        /// <summary>
        /// Gestiona la comunicación con el lector RFID mediante Arduino.
        /// </summary>
        private readonly RFIDDriver _rfidDriver;

        /// <summary>
        /// Lista de transacciones de la partida actual.
        /// </summary>
        private readonly ListaTransacciones _listaTransaccionesPartida;

        /// <summary>
        /// El ID que tendrá la siguiente transacción.
        /// </summary>
        private int siguienteIdTransaccion = 1;

        /// <summary>
        /// Inicializa una nueva instancia de la clase Banco.
        /// </summary>
        /// <param name="driver">Nombre del puerto serial utilizado para comunicarse con el Arduino.</param>
        public Banco(RFIDDriver driver)
        {
            this._rfidDriver = driver;
            this._listaTransaccionesPartida = new ListaTransacciones();
        }

        /// <summary>
        /// Verifica si un jugador tiene saldo suficiente para realizar un pago.
        /// </summary>
        /// <param name="jugadorEvaluar">El jugador que se va a evaluar.</param>
        /// <param name="montoPagar">El monto con el que se va a evaluar si puede o no pagar.</param>
        /// <returns>Booleano para saber si puede o no pagar.</returns>
        public bool PuedePagar(Jugador jugadorEvaluar, int montoPagar)
        {
            return jugadorEvaluar.Saldo >= montoPagar; // Booleano según la comparacion del saldo y el monto
        }

        /// <summary>
        /// Realiza un pago del jugador al banco.
        /// </summary>
        /// <param name="jugadorOrigen">El jugador que va a pagar.</param>
        /// <param name="montoPagar">El monto que debe pagar.</param>
        /// <param name="numeroTurno">El turno actual de la partida.</param>
        public void PagarAlBanco(Jugador jugadorOrigen, int montoPagar, int numeroTurno)
        {
            _rfidDriver.ReadUID("PAGUE", jugadorOrigen.UID); // Solicitamos la logica del lector de tarjeta

            jugadorOrigen.DisminuirSaldo(montoPagar); // Solo le restamos el monto

            Transaccion nuevaTransaccion = new Transaccion( // Generamos una nueva transacción
                siguienteIdTransaccion,
                numeroTurno,
                TipoTransaccion.PagoBanco,
                jugadorOrigen,
                null,
                montoPagar,
                $"El jugador {jugadorOrigen.Nombre} le paga al banco un monto de {montoPagar} colones."
                );

            _listaTransaccionesPartida.Add(nuevaTransaccion);

            siguienteIdTransaccion++;
        }

        /// <summary>
        /// Realiza un pago de un jugador a otro.
        /// </summary>
        /// <param name="jugadorOrigen">El jugador que va a pagar.</param>
        /// <param name="jugadorDestino">El jugador que va a recibir el pago.</param>
        /// <param name="montoPagar">El monto que debe pagar el jugador de origen.</param>
        /// <param name="numeroTurno">El turno actual de la partida.</param>
        public void PagarAlquiler(Jugador jugadorOrigen, Jugador jugadorDestino, int montoPagar, int numeroTurno)
        {
            _rfidDriver.ReadUID("PAGUE", jugadorOrigen.UID); // Solicitamos la logica del lector de tarjeta

            jugadorOrigen.DisminuirSaldo(montoPagar); // Le quitamos el monto que debe pagar al jugador de origen
            jugadorDestino.AumentarSaldo(montoPagar); // Le damos el monto al jugador destino

            Transaccion nuevaTransaccion = new Transaccion( // Generamos una nueva transacción
                siguienteIdTransaccion,
                numeroTurno,
                TipoTransaccion.PagoAlquiler,
                jugadorOrigen,
                jugadorDestino,
                montoPagar,
                $"El jugador {jugadorOrigen.Nombre} le paga de alquiler a {jugadorDestino.Nombre} un monto de {montoPagar} colones."
                );

            _listaTransaccionesPartida.Add(nuevaTransaccion);

            siguienteIdTransaccion++;
        }

        /// <summary>
        /// Realiza un pago de un jugador a otro.
        /// </summary>
        /// <param name="jugadorOrigen">El jugador que va a pagar.</param>
        /// <param name="jugadorDestino">El jugador que va a recibir el pago.</param>
        /// <param name="montoPagar">El monto que debe pagar el jugador de origen.</param>
        /// <param name="numeroTurno">El turno actual de la partida.</param>
        public void PagarAlJugador(Jugador jugadorOrigen, Jugador jugadorDestino, int montoPagar, int numeroTurno)
        {
            _rfidDriver.ReadUID("PAGUE", jugadorOrigen.UID); // Solicitamos la logica del lector de tarjeta

            jugadorOrigen.DisminuirSaldo(montoPagar); // Le quitamos el monto que debe pagar al jugador de origen
            jugadorDestino.AumentarSaldo(montoPagar); // Le damos el monto al jugador destino

            Transaccion nuevaTransaccion = new Transaccion( // Generamos una nueva transacción
                siguienteIdTransaccion,
                numeroTurno,
                TipoTransaccion.PagoEntreJugadores,
                jugadorOrigen,
                jugadorDestino,
                montoPagar,
                $"El jugador {jugadorOrigen.Nombre} le paga a {jugadorDestino.Nombre} un monto de {montoPagar} colones."
                );

            _listaTransaccionesPartida.Add(nuevaTransaccion);

            siguienteIdTransaccion++;
        }

        /// <summary>
        /// Registra la compra de una propiedad por parte de un jugador.
        /// </summary>
        /// <param name="jugadorCompra">El jugador que va a comprar la propiedad.</param>
        /// <param name="propiedadCompra">La propiedad que va a comprar.</param>
        /// <param name="numeroTurno">El turno actual de la partida.</param>
        public void ComprarPropiedad(Jugador jugadorCompra, Propiedad propiedadCompra, int numeroTurno)
        {
            _rfidDriver.ReadUID("PAGUE", jugadorCompra.UID); // Solicitamos la logica del lector de tarjeta

            jugadorCompra.DisminuirSaldo(propiedadCompra.PrecioCompra);

            propiedadCompra.Propietario = jugadorCompra;
            jugadorCompra.PropiedadesAdquiridas.Add(propiedadCompra);

            Transaccion nuevaTransaccion = new Transaccion( // Generamos una nueva transacción
            siguienteIdTransaccion,
            numeroTurno,
            TipoTransaccion.CompraPropiedad,
            jugadorCompra,
            null,
            propiedadCompra.PrecioCompra,
            $"El jugador {jugadorCompra.Nombre} compra al banco la propiedad {propiedadCompra.Nombre} por un monto de {propiedadCompra.PrecioCompra} colones."
            );

            _listaTransaccionesPartida.Add(nuevaTransaccion);

            siguienteIdTransaccion++;
        }

        /// <summary>
        /// Registra la venta de una propiedad por parte de un jugador.
        /// </summary>
        /// <param name="jugadorVenta">El jugador que va a realizar la venta de la propiedad.</param>
        /// <param name="propiedadVenta">La propiedad que va a ser vendida.</param>
        /// <param name="numeroTurno">El turno actual de la partida.</param>
        public void VenderPropiedad(Jugador jugadorVenta, Propiedad propiedadVenta, int numeroTurno)
        {
            jugadorVenta.AumentarSaldo(propiedadVenta.PrecioCompra);
            propiedadVenta.Propietario = null;
            jugadorVenta.PropiedadesAdquiridas.Remove(propiedadVenta);

            Transaccion nuevaTransaccion = new Transaccion( // Generamos una nueva transacción
            siguienteIdTransaccion,
            numeroTurno,
            TipoTransaccion.VentaPropiedad,
            null,
            jugadorVenta,
            propiedadVenta.PrecioCompra,
            $"El banco le compra a {jugadorVenta.Nombre} la propiedad {propiedadVenta.Nombre} por un monto de {propiedadVenta.PrecioCompra} colones."
            );

            _listaTransaccionesPartida.Add(nuevaTransaccion);

            siguienteIdTransaccion++;
        }

        /// <summary>
        /// Registra la ganancia de un jugador por un evento.
        /// </summary>
        /// <param name="jugadorGanancia">El jugador que obtiene la ganancia.</param>
        /// <param name="ganancia">La ganancia del evento.</param>
        /// <param name="numeroTurno">El turno actual de la partida.</param>
        public void GananciaPorEvento(Jugador jugadorGanancia, int ganancia, int numeroTurno)
        {
            jugadorGanancia.AumentarSaldo(ganancia);

            Transaccion nuevaTransaccion = new Transaccion( // Generamos una nueva transacción
            siguienteIdTransaccion,
            numeroTurno,
            TipoTransaccion.GananciaEvento,
            null,
            jugadorGanancia,
            ganancia,
            $"El banco le paga a {jugadorGanancia.Nombre} por un evento un monto de {ganancia} colones."
            );

            _listaTransaccionesPartida.Add(nuevaTransaccion);

            siguienteIdTransaccion++;
        }

        /// <summary>
        /// Registra la pérdida de un jugador por un evento.
        /// </summary>
        /// <param name="jugadorPerdida">El jugador que sufre la pérdida.</param>
        /// <param name="perdida">La pérdida del evento.</param>
        /// <param name="numeroTurno">El turno actual de la partida.</param>
        public void PerdidaPorEvento(Jugador jugadorPerdida, int perdida, int numeroTurno)
        {
            _rfidDriver.ReadUID("PAGUE", jugadorPerdida.UID); // Solicitamos la logica del lector de tarjeta

            jugadorPerdida.DisminuirSaldo(perdida);

            Transaccion nuevaTransaccion = new Transaccion( // Generamos una nueva transacción
            siguienteIdTransaccion,
            numeroTurno,
            TipoTransaccion.PerdidaEvento,
            jugadorPerdida,
            null,
            perdida,
            $"El jugador {jugadorPerdida.Nombre} le paga al banco por un evento un monto de {perdida} colones."
            );

            _listaTransaccionesPartida.Add(nuevaTransaccion);

            siguienteIdTransaccion++;
        }

        /// <summary>
        /// Registra la ganancia de un jugador por pasar por el inicio.
        /// </summary>
        /// <param name="jugadorPremio">El jugador que obtiene la ganancia.</param>
        /// <param name="numeroTurno">El turno actual de la partida.</param>
        public void PremioPorInicio(Jugador jugadorPremio, int numeroTurno)
        {
            const int premioInicio = 400;

            jugadorPremio.AumentarSaldo(premioInicio);

            Transaccion nuevaTransaccion = new Transaccion( // Generamos una nueva transacción
            siguienteIdTransaccion,
            numeroTurno,
            TipoTransaccion.PremioPorInicio,
            null,
            jugadorPremio,
            premioInicio,
            $"El banco le paga a {jugadorPremio.Nombre} por pasar por el inicio un monto de {premioInicio} colones."
            );

            _listaTransaccionesPartida.Add(nuevaTransaccion);

            siguienteIdTransaccion++;
        }

    }
}
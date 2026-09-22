using System;
using System.Collections.Generic;
using Proyecto_MonopoTEC.Server.Hardware;
using Proyecto_MonopoTEC.Server.Modelo;
using Proyecto_MonopoTEC.Server.Estructuras;
using Proyecto_MonopoTEC.Compartido;
using Proyecto_MonopoTEC.Server.Red;
using Proyecto_MonopoTEC.Server.Persistencia;

namespace Proyecto_MonopoTEC.Server.Motor
{

    /// <summary>
    /// Representa la acción que puede devolver la casilla en la que cae un jugador.
    /// </summary>
    public enum AccionCasilla
    {
        SinAccion,
        PermitirComprar,
        CobrarAlquiler,
        DarCarta,
        MandarCarcel
    }

    /// <summary>
    /// Se encarga de ejecutar las acciones correspondientes a las casillas del tablero.
    /// </summary>
    public class ManejadorAcciones
    {
        /// <summary>
        /// El registro de partida.
        /// </summary>
        private readonly RegistroTransacciones _registro;

        /// <summary>
        /// El Server.
        /// </summary>
        private readonly Servidor _server;

        /// <summary>
        /// El banco del juego.
        /// </summary>
        private readonly Banco _bancoJuego;

        /// <summary>
        /// El tablero del juego.
        /// </summary>
        private readonly Tablero _tableroJuego;

        /// <summary>
        /// La baraja de cartas de evento.
        /// </summary>
        private readonly ColaCartasEvento _barajaCartas;

        /// <summary>
        /// Función para esperar una accion.
        /// </summary>
        private readonly Func<Jugador, string, string> _esperarAccion;

        /// <summary>
        /// Constructor del manejador de acciones.
        /// </summary>
        /// <param name="driver">Nombre del puerto serial utilizado para comunicarse con el Arduino.</param>
        /// <param name="tableroJuego">El tablero del juego.</param>
        public ManejadorAcciones(RegistroTransacciones registro, RFIDDriver driver, Servidor server, Tablero tableroJuego, Func<Jugador, string, string> esperarAccion)
        {
            this._bancoJuego = new Banco(driver, registro);
            this._tableroJuego = tableroJuego;
            this._registro = registro;
            this._server = server;
            this._esperarAccion = esperarAccion;
            this._barajaCartas = new ColaCartasEvento();
            _barajaCartas.Inicializar();
        }

        /// <summary>
        /// Ejecuta la acción correspondiente al tipo de acción que devuelve la casilla.
        /// </summary>
        /// <param name="accion">La acción que se va a ejecutar.</param>
        /// <param name="jugadorActual">El jugador en el turno actual.</param>
        /// <param name="numeroTurno">El turno actual de la partida.</param>
        /// <returns>Booleano que representa si el jugador sigue o no en el juego.</returns>
        public bool EjecutarAccion(AccionCasilla accion, Jugador jugadorActual, int numeroTurno)
        {
            Console.Clear();

            switch (accion)
            {
                // Caso sin accion
                case AccionCasilla.SinAccion:
                    return true; // El jugador sigue en el juego.


                // Caso de compra
                case AccionCasilla.PermitirComprar:

                    Propiedad propiedadComprar = (Propiedad)jugadorActual.Posicion; // Hacemos cast para poder tratarla como una propiedad

                    bool puedeComprar = _bancoJuego.PuedePagar(jugadorActual, propiedadComprar.PrecioCompra); // Evaluamos si puede pagar

                    if (puedeComprar)
                    {

                        Console.WriteLine($"{jugadorActual.Nombre}, desea Comprar {propiedadComprar.Nombre} con un precio de {propiedadComprar.PrecioCompra}?");

                        _server.EnviarMensaje(Protocolo.CompraPropiedad, new
                        {
                            jugadorId = jugadorActual.ID,
                            propiedad = propiedadComprar.Nombre,
                            precio = propiedadComprar.PrecioCompra
                        }
                        );

                        Console.WriteLine("[ManejadorAcciones] Esperando respuesta de compra.");
                        string decision = ValidarDecisionCompra(jugadorActual, _esperarAccion(jugadorActual, "compra"));

                        if (decision == "1")
                        {

                            _server.EnviarMensaje(Protocolo.PagoRFID, new
                            {
                                jugadorId = jugadorActual.ID,
                                jugador = jugadorActual.Nombre,
                                monto = propiedadComprar.PrecioCompra,
                                concepto = "Compra de propiedad"
                            }
                            );

                            Transaccion transaccionCompra = _bancoJuego.ComprarPropiedad(jugadorActual, propiedadComprar, numeroTurno); // Ejecutamos la compra


                            Console.WriteLine($"{jugadorActual.Nombre} compró la propiedad {propiedadComprar.Nombre}.");

                            _server.EnviarMensaje(Protocolo.UltimaTransaccion, new { transaccion = transaccionCompra.ConvertirTexto() });

                            _server.EnviarMensaje(Protocolo.PropiedadComprada, new
                            {
                                jugadorId = jugadorActual.ID,
                                propiedad = propiedadComprar.Nombre,
                                precio = propiedadComprar.PrecioCompra,
                                saldo = jugadorActual.Saldo
                            }
                                );

                        }
                        else
                        {

                            Console.WriteLine($"{jugadorActual.Nombre} no compró la propiedad {propiedadComprar.Nombre}.");

                        }


                    }
                    else
                    {

                        Console.WriteLine($"{jugadorActual.Nombre} no puede comprar la propiedad {propiedadComprar.Nombre}.");

                    }
                    return true;


                // Caso de cobro de alquiler
                case AccionCasilla.CobrarAlquiler:
                    Propiedad propiedadAlquilar = (Propiedad)jugadorActual.Posicion; // Hacemos cast para poder tratarla como una propiedad

                    bool puedePagar = _bancoJuego.PuedePagar(jugadorActual, propiedadAlquilar.Alquiler); // Evaluamos si puede pagar
                    if (puedePagar)
                    {

                        _server.EnviarMensaje(Protocolo.PagoRFID, new
                        {
                            jugadorId = jugadorActual.ID,
                            jugador = jugadorActual.Nombre,
                            monto = propiedadAlquilar.Alquiler,
                            concepto = "Pago de alquiler"
                        }
                        );

                        Transaccion transaccionAlquiler = _bancoJuego.PagarAlquiler(jugadorActual, propiedadAlquilar.Propietario, propiedadAlquilar.Alquiler, numeroTurno); // Se realizá el cobro del alquiler


                        Console.WriteLine($"Por caer en {propiedadAlquilar.Nombre}, {jugadorActual.Nombre} le paga {propiedadAlquilar.Alquiler} colones de alquiler a {propiedadAlquilar.Propietario.Nombre}.");

                        _server.EnviarMensaje(Protocolo.UltimaTransaccion, new { transaccion = transaccionAlquiler.ConvertirTexto() });

                        _server.EnviarMensaje(Protocolo.AlquilerPagado, new
                        {
                            jugadorId = jugadorActual.ID,
                            propietarioId = propiedadAlquilar.Propietario.ID,
                            propiedad = propiedadAlquilar.Nombre,
                            monto = propiedadAlquilar.Alquiler,
                            saldo = jugadorActual.Saldo,
                            saldoPropietario = propiedadAlquilar.Propietario.Saldo
                        });

                        return true; // Sigue en juego
                    }

                    Console.WriteLine($"{jugadorActual.Nombre} no puede pagar {propiedadAlquilar.Alquiler} colones de alquiler a {propiedadAlquilar.Propietario.Nombre}, entra en bancarrota.");

                    _server.EnviarMensaje(Protocolo.Bancarrota, new
                    {
                        jugadorId = jugadorActual.ID,
                        monto = propiedadAlquilar.Alquiler
                    }
                    );

                    return false; // Sino entra en bancarrota



                // Caso de dar carta
                case AccionCasilla.DarCarta:
                    CartaEvento cartaSacada = _barajaCartas.Peek();


                    bool sigueEnJuego = EjecutarAccionCarta(cartaSacada, jugadorActual, numeroTurno);

                    _barajaCartas.Advance(); // La carta vuelve al final 

                    return sigueEnJuego;



                // Caso de mandar a la carcel
                case AccionCasilla.MandarCarcel:

                    _tableroJuego.MoverJugadorACarcel(jugadorActual);

                    jugadorActual.EntrarCarcel();
                    Console.WriteLine($"{jugadorActual.Nombre} entra en la carcel.");

                    _server.EnviarMensaje(Protocolo.JugadorMovido, new
                    {
                        jugadorId = jugadorActual.ID,
                        casilla = jugadorActual.Posicion.Nombre,
                        posicion = jugadorActual.Posicion.ID,
                        pasoPorSalida = false
                    }
                    );

                    _server.EnviarMensaje(Protocolo.JugadorCarcel, new
                    {
                        jugadorId = jugadorActual.ID,
                        mensaje = $"{jugadorActual.Nombre} entra en la carcel."
                    }
                    );

                    return true;

                default:
                    Console.WriteLine("Acción desconocida.");
                    return true;
            }
        }

        /// <summary>
        /// Ejecuta la acción correspondiente al tipo de carta que saca el jugador.
        /// </summary>
        /// <param name="cartaEvento">La carta que sacó el jugador.</param>
        /// <param name="jugadorActual">El jugador que sacó la carta.</param>
        /// <param name="numeroTurno">El turno actual de la partida.</param>
        /// <returns>Booleano que representa si el jugador sigue o no en el juego.</returns>
        public bool EjecutarAccionCarta(CartaEvento cartaEvento, Jugador jugadorActual, int numeroTurno)
        {
            Console.Clear();

            TipoCartaEvento tipo = cartaEvento.Tipo;

            switch (tipo)
            {

                // Caso de carta de ganancia
                case TipoCartaEvento.GanoColones:
                    {
                        Transaccion transaccionGananciaEvento = _bancoJuego.GananciaPorEvento(jugadorActual, cartaEvento.Valor, numeroTurno); // Le damos la ganancia

                        Console.WriteLine(cartaEvento.Descripcion);

                        _server.EnviarMensaje(Protocolo.CartaEvento, new
                        {
                            jugadorId = jugadorActual.ID,
                            tipo = tipo.ToString(),
                            descripcion = cartaEvento.Descripcion,
                            valor = cartaEvento.Valor,
                            saldo = jugadorActual.Saldo
                        }
                        );

                        _server.EnviarMensaje(Protocolo.UltimaTransaccion, new { transaccion = transaccionGananciaEvento.ConvertirTexto() });

                        return true; // El jugador sigue en el juego.
                    }


                // Caso de carta de pérdida
                case TipoCartaEvento.PerdioColones:
                    {

                        bool puedePagar = _bancoJuego.PuedePagar(jugadorActual, cartaEvento.Valor); // Se verifica que pueda pagar o no

                        Console.WriteLine(cartaEvento.Descripcion);

                        _server.EnviarMensaje(Protocolo.CartaEvento, new
                        {
                            jugadorId = jugadorActual.ID,
                            tipo = tipo.ToString(),
                            descripcion = cartaEvento.Descripcion,
                            valor = cartaEvento.Valor,
                            saldo = jugadorActual.Saldo
                        }
                        );

                        if (puedePagar)
                        {
                            _server.EnviarMensaje(Protocolo.PagoRFID, new
                            {
                                jugadorId = jugadorActual.ID,
                                jugador = jugadorActual.Nombre,
                                monto = cartaEvento.Valor,
                                concepto = "Pago de carta de evento"
                            }
                            );

                            Transaccion transaccionPerdidaEvento = _bancoJuego.PerdidaPorEvento(jugadorActual, cartaEvento.Valor, numeroTurno); // Le rebajamos la perdida

                            _server.EnviarMensaje(Protocolo.UltimaTransaccion, new { transaccion = transaccionPerdidaEvento.ConvertirTexto() });
                            return true;
                        }
                    }

                    Console.WriteLine($"{jugadorActual.Nombre} no puede pagar {cartaEvento.Valor} colones, entra en bancarrota.");

                    _server.EnviarMensaje(Protocolo.Bancarrota, new
                    {
                        jugadorId = jugadorActual.ID,
                        monto = cartaEvento.Valor
                    }
                    );

                    return false;


                // Caso de carta de avanzar
                case TipoCartaEvento.Avanzar:
                    {
                        bool pasoPorSalida;
                        Casilla casillaObjetivo = _tableroJuego.AvanzarJugador(jugadorActual, cartaEvento.Valor, out pasoPorSalida); // Se avanza

                        Console.WriteLine(cartaEvento.Descripcion);

                        _server.EnviarMensaje(Protocolo.JugadorMovido, new
                        {
                            jugadorId = jugadorActual.ID,
                            casilla = casillaObjetivo.Nombre,
                            posicion = casillaObjetivo.ID,
                            pasoPorSalida
                        }
                        );

                        _server.EnviarMensaje(Protocolo.CartaEvento, new
                        {
                            jugadorId = jugadorActual.ID,
                            tipo = tipo.ToString(),
                            descripcion = cartaEvento.Descripcion,
                            valor = cartaEvento.Valor,
                            saldo = jugadorActual.Saldo
                        }
                        );

                        if (pasoPorSalida)
                        {
                            Transaccion transaccionPremio = _bancoJuego.PremioPorInicio(jugadorActual, numeroTurno);
                            _server.EnviarMensaje(Protocolo.UltimaTransaccion, new { transaccion = transaccionPremio.ConvertirTexto() });
                        }


                        AccionCasilla nuevaAccion = _tableroJuego.ObtenerAccion(jugadorActual.Posicion, jugadorActual);
                        bool sigueEnJuego = EjecutarAccion(nuevaAccion, jugadorActual, numeroTurno); // Se ejecuta la nueva acción

                        return sigueEnJuego;
                    }


                // Caso de carta de retroceder
                case TipoCartaEvento.Retroceder:
                    {
                        Casilla casillaObjetivo = _tableroJuego.RetrocederJugador(jugadorActual, cartaEvento.Valor); // Se retocede

                        Console.WriteLine(cartaEvento.Descripcion);

                        _server.EnviarMensaje(Protocolo.JugadorMovido, new
                        {
                            jugadorId = jugadorActual.ID,
                            casilla = casillaObjetivo.Nombre,
                            posicion = casillaObjetivo.ID,
                            pasoPorSalida = false
                        }
                        );

                        _server.EnviarMensaje(Protocolo.CartaEvento, new
                        {
                            jugadorId = jugadorActual.ID,
                            tipo = tipo.ToString(),
                            descripcion = cartaEvento.Descripcion,
                            valor = cartaEvento.Valor,
                            saldo = jugadorActual.Saldo
                        }
                        );

                        AccionCasilla nuevaAccion = _tableroJuego.ObtenerAccion(jugadorActual.Posicion, jugadorActual);
                        bool sigueEnJuego = EjecutarAccion(nuevaAccion, jugadorActual, numeroTurno); // Se ejecuta la nueva acción

                        return sigueEnJuego;
                    }


                // Caso de carta de perder turno
                case TipoCartaEvento.PerderTurno:
                    {
                        jugadorActual.PerderTurno(cartaEvento.Valor);

                        Console.WriteLine(cartaEvento.Descripcion);

                        _server.EnviarMensaje(Protocolo.CartaEvento, new
                        {
                            jugadorId = jugadorActual.ID,
                            tipo = tipo.ToString(),
                            descripcion = cartaEvento.Descripcion,
                            valor = cartaEvento.Valor,
                            saldo = jugadorActual.Saldo
                        }
                        );

                        return true;
                    }


                // Caso de carta de avanzar a la salida
                case TipoCartaEvento.AvanzarSalida:
                    {
                        _tableroJuego.MoverJugadorASalida(jugadorActual);
                        Transaccion transaccionSalida = _bancoJuego.PremioPorInicio(jugadorActual, numeroTurno);

                        Console.WriteLine(cartaEvento.Descripcion);

                        _server.EnviarMensaje(Protocolo.JugadorMovido, new
                        {
                            jugadorId = jugadorActual.ID,
                            casilla = jugadorActual.Posicion.Nombre,
                            posicion = jugadorActual.Posicion.ID,
                            pasoPorSalida = false
                        }
                        );

                        _server.EnviarMensaje(Protocolo.UltimaTransaccion, new { transaccion = transaccionSalida.ConvertirTexto() });

                        _server.EnviarMensaje(Protocolo.CartaEvento, new
                        {
                            jugadorId = jugadorActual.ID,
                            tipo = tipo.ToString(),
                            descripcion = cartaEvento.Descripcion,
                            valor = cartaEvento.Valor,
                            saldo = jugadorActual.Saldo
                        }
                        );

                        return true;
                    }


                // Caso de carta de avanzar al parque
                case TipoCartaEvento.AvanzarParque:
                    {
                        bool pasoPorSalida;

                        _tableroJuego.MoverJugadorAParque(jugadorActual, out pasoPorSalida); // Se mueve el jugador al parque y se obtiene si se pasó por salida

                        Console.WriteLine(cartaEvento.Descripcion);

                        _server.EnviarMensaje(Protocolo.JugadorMovido, new
                        {
                            jugadorId = jugadorActual.ID,
                            casilla = jugadorActual.Posicion.Nombre,
                            posicion = jugadorActual.Posicion.ID,
                            pasoPorSalida
                        }
                        );

                        _server.EnviarMensaje(Protocolo.CartaEvento, new
                        {
                            jugadorId = jugadorActual.ID,
                            tipo = tipo.ToString(),
                            descripcion = cartaEvento.Descripcion,
                            valor = cartaEvento.Valor,
                            saldo = jugadorActual.Saldo
                        }
                        );

                        if (pasoPorSalida)
                        {
                            _bancoJuego.PremioPorInicio(jugadorActual, numeroTurno);
                        }
                        return true;
                    }


                // Caso de carta de vaya a la carcel
                case TipoCartaEvento.VayaCarcel:
                    {
                        _tableroJuego.MoverJugadorACarcel(jugadorActual);
                        jugadorActual.EntrarCarcel();

                        Console.WriteLine(cartaEvento.Descripcion);

                        _server.EnviarMensaje(Protocolo.JugadorMovido, new
                        {
                            jugadorId = jugadorActual.ID,
                            casilla = jugadorActual.Posicion.Nombre,
                            posicion = jugadorActual.Posicion.ID,
                            pasoPorSalida = false
                        }
                        );

                        _server.EnviarMensaje(Protocolo.CartaEvento, new
                        {
                            jugadorId = jugadorActual.ID,
                            tipo = tipo.ToString(),
                            descripcion = cartaEvento.Descripcion,
                            valor = cartaEvento.Valor,
                            saldo = jugadorActual.Saldo
                        }
                        );

                        return true;
                    }
                default:
                    Console.WriteLine("Acción desconocida.");
                    return true;
            }
        }

        /// <summary>
        /// Valida si la desición de compra del jugador se encuentra entre las opciones disponibles.
        /// </summary>
        /// <param name="opcion">La opción del jugador.</param>
        /// <returns>Una opción valida</returns>
        private string ValidarDecisionCompra(Jugador jugador, string opcion)
        {
            while (opcion != "1" && opcion != "2" && opcion != "comprar" && opcion != "rechazar")
            {
                Console.WriteLine("Opción inválida. Por favor, elige una opción válida.");

                _server.EnviarMensaje(Protocolo.ErrorAccion, new { mensaje = "Opción inválida. Por favor, elige una opción válida." });
                opcion = _esperarAccion(jugador, "compra");
            }
            return opcion == "comprar" ? "1" : opcion == "rechazar" ? "2" : opcion;
        }

        /// <summary>
        /// Valida si el índice de venta se encuentra dentro de las opciones disponibles.
        /// </summary>
        /// <param name="limiteIndice">El limite superior que tendrá el indice.</param>
        /// <param name="indice">La opción del jugador.</param>
        /// <returns>Un indice valido.</returns>
        private Propiedad ValidarPropiedadVenta(Jugador jugador, string indice)
        {
            int indicePropiedad;
            string opcion = indice;

            while (true)
            {
                if (int.TryParse(opcion, out indicePropiedad))
                {
                    Casilla? propiedadPorId = jugador.PropiedadesAdquiridas.GetById(indicePropiedad);
                    if (propiedadPorId is Propiedad propiedad)
                        return propiedad;

                    if (indicePropiedad >= 1 && indicePropiedad <= jugador.PropiedadesAdquiridas.Size)
                        return (Propiedad)jugador.PropiedadesAdquiridas.GetAt(indicePropiedad);
                }

                Console.WriteLine("Opción inválida. Ingrese un número válido.");

                _server.EnviarMensaje(Protocolo.ErrorAccion, new { mensaje = "Indica el índice de la lista o el ID de la propiedad." });
                opcion = _esperarAccion(jugador, "venta");
            }
        }

        /// <summary>
        /// Se encarga de realizar la acción de venta de un jugador.
        /// </summary>
        /// <param name="jugadorVenta">El jugador que desea vender.</param>
        /// <param name="numeroTurno">El turno actual de la partida.</param>
        public void AccionVenderPropiedad(Jugador jugadorVenta, int numeroTurno)
        {
            if (jugadorVenta.PropiedadesAdquiridas.Size == 0)
            {
                Console.WriteLine("Sin Propiedades para vender.");

                _server.EnviarMensaje(Protocolo.VentaPropiedad, new
                {
                    mensaje = "Sin propiedades para vender."
                });
                return;
            }

            List<object> propiedadesDisponibles = new List<object>();
            NodeCasilla? nodoActual = jugadorVenta.PropiedadesAdquiridas.Head;

            while (nodoActual != null)
            {
                if (nodoActual.Data is Propiedad propiedad)
                {
                    propiedadesDisponibles.Add(new
                    {
                        id = propiedad.ID,
                        nombre = propiedad.Nombre,
                        precio = propiedad.PrecioCompra
                    });
                }

                nodoActual = nodoActual.Next;
            }

            Console.WriteLine("[ManejadorAcciones] Esperando selección de propiedad.");

            _server.EnviarMensaje(Protocolo.VentaPropiedad, new
            {
                jugadorId = jugadorVenta.ID,
                propiedades = propiedadesDisponibles,
                cantidad = propiedadesDisponibles.Count
            });

            Propiedad casillaVenta = ValidarPropiedadVenta(jugadorVenta, _esperarAccion(jugadorVenta, "venta"));

            Transaccion transaccionVenta = _bancoJuego.VenderPropiedad(jugadorVenta, casillaVenta, numeroTurno);

            _server.EnviarMensaje(Protocolo.UltimaTransaccion, new { transaccion = transaccionVenta.ConvertirTexto() });

            _server.EnviarMensaje(Protocolo.PropiedadVendida, new
            {
                jugadorId = jugadorVenta.ID,
                propiedad = casillaVenta.Nombre,
                precio = casillaVenta.PrecioCompra,
                saldo = jugadorVenta.Saldo
            });
        }

        /// <summary>
        /// Se encarga de darle el premio por pasar en el inicio al jugador.
        /// </summary>
        /// <param name="jugador">El que recibirá el premio.</param>
        /// <param name="numeroTurno">El turno actual de la partida.</param>
        public void DarPremio(Jugador jugador, int numeroTurno)
        {
            Transaccion transaccionPremio = _bancoJuego.PremioPorInicio(jugador, numeroTurno);
            Console.WriteLine("Ganas 450 colones por pasar por el inicio");

            _server.EnviarMensaje(Protocolo.UltimaTransaccion, new { transaccion = transaccionPremio.ConvertirTexto() });
            _server.EnviarMensaje(Protocolo.PremioSalida, new
            {
                jugadorId = jugador.ID,
                monto = 450,
                saldo = jugador.Saldo,
                descripcion = $"El banco le paga a {jugador.Nombre} por pasar por el inicio un monto de 450 colones."
            }
            );
        }
    }
}
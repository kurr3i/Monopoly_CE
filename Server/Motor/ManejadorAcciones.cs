using System;

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
    /// Constructor del manejador de acciones.
    /// </summary>
    /// <param name="puertoArduino">Nombre del puerto serial utilizado para comunicarse con el Arduino.</param>
    /// <param name="tableroJuego">El tablero del juego.</param>
    public ManejadorAcciones(string puertoArduino, Tablero tableroJuego)
        {
        this._bancoJuego = new Banco(puertoArduino);
        this._tableroJuego = tableroJuego;
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
            case AccionCasilla.SinAccion:
                Console.WriteLine("Fin del turno.");
                return true; // El jugador sigue en el juego.

            case AccionCasilla.PermitirComprar:

                Propiedad propiedadComprar = (Propiedad)jugadorActual.Posicion; // Hacemos cast para poder tratarla como una propiedad

                bool puedeComprar = _bancoJuego.PuedePagar(jugadorActual, propiedadComprar.PrecioCompra); // Evaluamos si puede pagar
                if (puedeComprar)
                {
                    Console.WriteLine("Desea Comprar " + propiedadComprar.Nombre + "Con un precio de " + propiedadComprar.PrecioCompra);
                    Console.WriteLine("1. Sí\n2. No");
                    string decision = ValidarDecisionCompra(Console.ReadLine());

                    if (decision == "1")
                    {
                        _bancoJuego.ComprarPropiedad(jugadorActual, propiedadComprar, numeroTurno); // Ejecutamos la compra
                    }
                }
                Console.WriteLine("Fin del turno.");
                return true;

            case AccionCasilla.CobrarAlquiler:
                Propiedad propiedadAlquilar = (Propiedad)jugadorActual.Posicion; // Hacemos cast para poder tratarla como una propiedad

                bool puedePagar = _bancoJuego.PuedePagar(jugadorActual, propiedadAlquilar.Alquiler); // Evaluamos si puede pagar
                if (puedePagar)
                {
                    _bancoJuego.PagarAlquiler(jugadorActual, propiedadAlquilar.Propietario, propiedadAlquilar.Alquiler, numeroTurno); // Se realizá el cobro del alquiler
                    Console.WriteLine("Fin del turno.");
                    return true; // Sigue en juego
                }
                Console.WriteLine("Fin del turno.");
                return false; // Sino entra en bancarrota

            case AccionCasilla.DarCarta:
                CartaEvento cartaSacada = _barajaCartas.Peek();
                

                bool sigueEnJuego = EjecutarAccionCarta(cartaSacada, jugadorActual, numeroTurno);

                _barajaCartas.Advance(); // La carta vuelve al final 

                Console.WriteLine("Fin del turno.");
                return sigueEnJuego;

            case AccionCasilla.MandarCarcel:

                _tableroJuego.MoverJugadorACarcel(jugadorActual);

                jugadorActual.EntrarCarcel();

                Console.WriteLine("Fin del turno.");
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
            case TipoCartaEvento.GanoColones:
                {
                    _bancoJuego.GananciaPorEvento(jugadorActual, cartaEvento.Valor, numeroTurno); // Le damos la ganancia
                    return true; // El jugador sigue en el juego.
                }
            case TipoCartaEvento.PerdioColones:
                {
                    bool puedePagar = _bancoJuego.PuedePagar(jugadorActual, cartaEvento.Valor); // Se verifica que pueda pagar o no

                    if (puedePagar)
                    {
                        _bancoJuego.PerdidaPorEvento(jugadorActual, cartaEvento.Valor, numeroTurno); // Le rebajamos la perdida
                        return true;
                    }
                }
                return false;

            case TipoCartaEvento.Avanzar:
                {
                    bool pasoPorSalida;
                    _tableroJuego.AvanzarJugador(jugadorActual, cartaEvento.Valor, out pasoPorSalida); // Se avanza
                    
                    if (pasoPorSalida)
                    {
                        _bancoJuego.PremioPorInicio(jugadorActual, numeroTurno);
                    }

                    AccionCasilla nuevaAccion = _tableroJuego.ObtenerAccion(jugadorActual.Posicion, jugadorActual);
                    bool sigueEnJuego = EjecutarAccion(nuevaAccion, jugadorActual, numeroTurno); // Se ejecuta la nueva acción

                    return sigueEnJuego;
                }
                
            case TipoCartaEvento.Retroceder:
                {
                    _tableroJuego.RetrocederJugador(jugadorActual, cartaEvento.Valor); // Se retocede

                    AccionCasilla nuevaAccion = _tableroJuego.ObtenerAccion(jugadorActual.Posicion, jugadorActual);
                    bool sigueEnJuego = EjecutarAccion(nuevaAccion, jugadorActual, numeroTurno); // Se ejecuta la nueva acción

                    return sigueEnJuego;
                }

            case TipoCartaEvento.PerderTurno:
                {
                    jugadorActual.PerderTurno(cartaEvento.Valor);
                    return true;
                }
            case TipoCartaEvento.AvanzarSalida:
                { 
                    _tableroJuego.MoverJugadorASalida(jugadorActual);
                    _bancoJuego.PremioPorInicio(jugadorActual, numeroTurno);

                    return true;
                }
            case TipoCartaEvento.AvanzarParque:
                {
                    bool pasoPorSalida;

                    _tableroJuego.MoverJugadorAParque(jugadorActual, out pasoPorSalida); // Se mueve el jugador al parque y se obtiene si se pasó por salida

                    if (pasoPorSalida)
                    {
                        _bancoJuego.PremioPorInicio(jugadorActual, numeroTurno);
                    }
                    return true;
                }
            case TipoCartaEvento.VayaCarcel:
                {
                    _tableroJuego.MoverJugadorACarcel(jugadorActual);

                    jugadorActual.EntrarCarcel();
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
    private string ValidarDecisionCompra(string opcion)
    {
        while (opcion != "1" && opcion != "2")
        {
            Console.WriteLine("Opión inválida. Por favor, elige una opción válida.");
            opcion = Console.ReadLine();
        }
        return opcion;
    }

    /// <summary>
    /// Valida si el índice de venta se encuentra dentro de las opciones disponibles.
    /// </summary>
    /// <param name="limiteIndice">El limite superior que tendrá el indice.</param>
    /// <param name="indice">La opción del jugador.</param>
    /// <returns>Un indice valido.</returns>
    private int ValidarIndiceVenta(int limiteIndice, string indice)
    {
        int indicePropiedad;
        string opcion = indice;

        while (!int.TryParse(opcion, out indicePropiedad) || indicePropiedad < 1 || indicePropiedad > limiteIndice)
        {
            Console.WriteLine("Opción inválida. Ingrese un número válido.");
            opcion = Console.ReadLine();
        }

        return indicePropiedad;
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
            Console.WriteLine("Sin Propiedades para vender");
        }
        else
        {
            jugadorVenta.PropiedadesAdquiridas.Display(); // Mostramos las propiedades

            Console.WriteLine("Ingrese cuál Propiedad desea vender:");
            int indicePropiedadVender = ValidarIndiceVenta(jugadorVenta.PropiedadesAdquiridas.Size, Console.ReadLine()); // Evaluamos el ingreso

            Propiedad casillaVenta = (Propiedad)jugadorVenta.PropiedadesAdquiridas.GetAt(indicePropiedadVender); // Obtenemos la Propiedad que se va a vender y hacemos cast

            _bancoJuego.VenderPropiedad(jugadorVenta, casillaVenta, numeroTurno);
        }  

    }

    /// <summary>
    /// Se encarga de darle el premio por pasar en el inicio al jugador.
    /// </summary>
    /// <param name="jugador">El que recibirá el premio.</param>
    /// <param name="numeroTurno">El turno actual de la partida.</param>
    public void DarPremio(Jugador jugador, int numeroTurno)
    {
        _bancoJuego.PremioPorInicio(jugador, numeroTurno);
    }
}

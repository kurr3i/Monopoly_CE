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
/// Será el encargado de ejecutar acciones internar y externas
/// </summary>
public class ManejadorAcciones
{
    Banco bancoJuego;

    public ManejadorAcciones()
        {
        this.bancoJuego = new Banco();
        }

    public bool EjecutarAccion(Casilla casillaActual, AccionCasilla accion, Jugador jugadorActual)
    {
        Console.Clear();
        switch (accion)
        {
            case AccionCasilla.SinAccion:
                Console.WriteLine("Fin del turno.");
                return true; // El jugador sigue en el juego.
                break;
            case AccionCasilla.PermitirComprar:
                Propiedad propiedadComprar = (Propiedad)jugadorActual.Posicion;
                // Evaluamos con el banco si se puede comprar
                Console.WriteLine("Desea Comprar " + propiedadComprar.Nombre + "Con un precio de " + propiedadComprar.PrecioCompra);
                Console.WriteLine("1. Sí\n2. No");

                string desicion = ValidarDesicionJugador(Console.ReadLine());

                if (desicion == "1")
                {
                    bancoJuego.ComprarPropiedad(jugadorActual, propiedadComprar); // Esto debería hacerlo el banco
                    return true;
                }
                else
                {
                    return true;
                }
                break;
            case AccionCasilla.CobrarAlquiler:
                Propiedad propiedadAlquilar = (Propiedad)jugadorActual.Posicion;
                //Evaluacion del banco
                jugadorActual.Saldo -= propiedadAlquilar.Alquiler; // Por ahora solo esto
                return true;
                break;
            case AccionCasilla.DarCarta:
                Console.WriteLine("El jugador recibe una carta de evento."); // Por ahora solo esto
                return true;
                break;
            case AccionCasilla.MandarCarcel:
                Console.WriteLine("El jugador es enviado a la cárcel."); // Hay que hacer que el tablero envíe a la carcel
                return true;
                break;
            default:
                Console.WriteLine("Acción desconocida.");
                return true;
                break;
        }
    }

        // Falta comentar esto.
    private string ValidarDesicionJugador(string opcion)
    {
        while (opcion != "1" && opcion != "2")
        {
            Console.WriteLine("Opión inválida. Por favor, elige una opción válida.");
            opcion = Console.ReadLine();
        }
        return opcion;
    }

    public void AccionVenderPropiedad(Jugador jugadorVenta)
    {

    }
}

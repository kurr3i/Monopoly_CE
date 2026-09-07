
/// <summary>
/// Representa una cola circular doblemente enlazada de la baraja de cartas de evento.
/// </summary>
public class ColaCartasEvento
{
    public NodeCartaEvento Head { get; private set; }
    public NodeCartaEvento Tail { get; private set; }
    public int Size { get; private set; }

    /// <summary>
    /// Agrega un nuevo nodo al final de la cola circular.
    /// </summary>
    /// <param name="cartaEvento">Carta de evento que almacenará el nodo.</param>
    public void Enqueue(CartaEvento cartaEvento)
    {
        NodeCartaEvento newNode = new NodeCartaEvento { Data = cartaEvento };

        if (Size == 0) // Si la cola está vacía, el nuevo nodo será tanto la cabeza como la cola (tail)
        {
            Head = newNode;
            Tail = newNode;

            Head.Next = Head; // Conectamos la cabeza con ella misma
            Head.Previous = Head;
        }
        else // Si la cola no está vacía, agregamos el nuevo nodo al final y lo conectamos con el primero
        {
            Tail.Next = newNode; // El siguiente nodo de la cola (tail) actual será el nuevo nodo
            newNode.Previous = Tail; // El nodo anterior del nuevo nodo será la cola (tail) actual
            Tail = newNode; // Actualizamos la cola (tail) para que sea el nuevo nodo

            Head.Previous = Tail; // Conectamos la cabeza con la nueva cola (tail)
            Tail.Next = Head; // Conectamos la nueva cola (tail) con la cabeza 
        }   

        Size++;
    }

    /// <summary>
    /// Elimina el primer nodo de la cola circular.
    /// </summary>
    /// <returns>La carta de evento eliminada de la cabeza de la cola.</returns>
    public CartaEvento Dequeue()
    {
        CartaEvento cartaEliminada = Head.Data;

        if (Size == 1)
        {
            Head = null;
            Tail = null;
        }
        else
        {
                Head = Head.Next; // Avanzamos la cabeza al siguiente nodo
                Head.Previous = Tail; // Actualizamos el nodo anterior de la nueva cabeza para que apunte a la cola (tail)
                Tail.Next = Head; // Actualizamos el siguiente nodo de la cola (tail) para que apunte a la nueva cabeza
        }
        
        Size--;
        return cartaEliminada; // Retornamos la carta eliminada
    }

    /// <summary>
    /// Elimina la primer carta de evento de la cola y la agrega al final.
    /// </summary>
    public void Advance()
    {
        CartaEvento cartaEliminada = Dequeue(); // Guardamos la carta eliminada de la cabeza de la cola
        Enqueue(cartaEliminada); // Agregamos un nuevo nodo al final de la cola con la carta eliminada
    }

    /// <summary>
    /// Devuelve la carta de evento de la cabeza de la cola sin eliminarla.
    /// </summary>
    /// <returns>La carta de evento en la cabeza de la cola.</returns>
    public CartaEvento Peek()
    {
        return Head.Data;
    }

    /// <summary>
    /// Inicializa la baraja de cartas de evento.
    /// </summary>
    public void Inicializar()
    {
        CartaEvento hallazgo = new CartaEvento(
            200,
            TipoCartaEvento.GanoColones, 
            "El hallazgo en el pantalón de manta. " +
            "Te pusiste un pantalón que tenías " +
            "guardado desde los últimos zapateos " +
            "en las Fiestas de Palmares y " +
            "encontraste doscientos colones"
            );
        Enqueue(hallazgo);

        CartaEvento platina = new CartaEvento(
            150,
            TipoCartaEvento.PerdioColones,
            "El impuesto de la platina. Caíste " +
            "en un hueco enorme en la carretera " +
            "hacia Alajuela y terminaste estallando " +
            "una llanta y doblando el aro de tu carro." +
            " Tuviste que llamar a la grúa de emergencia." +
            " Paga 150 colones."
            );
        Enqueue(platina);

        CartaEvento vivazo = new CartaEvento(
            0,
            TipoCartaEvento.VayaCarcel,
            "Por pasarse de \"vivazo\". Un oficial " +
            "de Tránsito te paró por circular con la " +
            "licencia vencida. En lugar de aceptar la " +
            "multa, le ofreciste \"para los frescos\". " +
            "Te cayó la ley por cohecho. Vaya directo " +
            "a la cárcel."
            );
        Enqueue(vivazo);

        CartaEvento lumaca = new CartaEvento(
            3,
            TipoCartaEvento.Avanzar,
            "¡Chofer, chofer más velocidad!. " +
            "Te tocó viajar en una de las unidades " +
            "nuevas de Lumaca con un chofer que manejaba " +
            "como si estuviera clasificando para la Fórmula " +
            "1. Esquivó todos los huecos de Ochomogo en " +
            "tiempo récord. Avanza 3 casillas."
            );
        Enqueue(lumaca);

        CartaEvento ebais = new CartaEvento(
        1,
        TipoCartaEvento.PerderTurno,
        "¡El calvario de las filas! Fuiste a sacar una " +
        "cita al Ebais desde la madrugada para que te " +
        "revisaran una dolencia, pero el sistema se cayó " +
        "a nivel nacional y te tocó esperar de pie en" +
        " una fila interminable que le daba la vuelta " +
        "a toda la cuadra. Pierde 1 turno."
        );
        Enqueue(ebais);

        CartaEvento ride = new CartaEvento(
            0,
            TipoCartaEvento.AvanzarSalida,
            "¡El \"Raid\" de la salvación! Te quedaste a pie " +
            "en medio del colapso vial de San José y la meta " +
            "se ve lejísimos. De la nada, apareció un compa en " +
            "una ZS Miedo rugiendo el motor y te ofreció " +
            "llevarte esquivando carros y subiéndose a las " +
            "aceras. Agarrate fuerte porque vas directo al final." +
            " Avanza de inmediato hasta la casilla de Salida."
            );
        Enqueue(ride);

        CartaEvento zurqui = new CartaEvento(
            4,
            TipoCartaEvento.Retroceder,
            "¡Derrumbe en el Zurquí! Ibas hacia Limón " +
            "y un derrumbe cerró la pista. La única " +
            "opción para avanzar es desviarse y dar un " +
            "tremendo rodeo por la catarata de La Paz y Vara Blanca," +
            " perdiendo muchísimo tiempo en las curvas. " +
            "Retrocede 3 casillas."
            );
        Enqueue(zurqui);

        CartaEvento pase = new CartaEvento(
            0,
            TipoCartaEvento.AvanzarParque,
            "¡Te ganaste el pase especial! " +
            "En la oficina hicieron una rifa " +
            "y te pegaste las entradas con todo incluido." +
            " Te salvaste de pagar alquileres ajenos " +
            "y te vas a comer churros. Avanza al Parque " +
            "de Diversiones y descansa gratis."
            );
        Enqueue(pase);
    }
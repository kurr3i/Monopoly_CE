
/// <summary>
/// Representa una cola circular doblemente enlazada de jugadores.
/// </summary>
public class ColaCircular
{
    public NodeJugador Head { get; private set; }
    public NodeJugador Tail { get; private set; }
    public int Size { get; private set; }

    /// <summary>
    /// Agrega un nuevo nodo al final de la cola circular.
    /// </summary>
    /// <param name="jugador">Jugador que almacenará el nodo.</param>
    public void Enqueue(Jugador jugador)
    {
        NodeJugador newNode = new NodeJugador { Data = jugador };

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
    /// <returns>El jugador eliminado de la cabeza de la cola.</returns>
    public Jugador Dequeue()
    {
        Jugador jugadorEliminado = Head.Data;

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
        return jugadorEliminado; // Retornamos el jugador eliminado
    }

    /// <summary>
    /// Elimina el primer jugador de la cola y lo agrega al final.
    /// </summary>
    public void Advance()
    {
        Jugador jugadorEliminado = Dequeue(); // Guardamos el jugador eliminado de la cabeza de la cola
        Enqueue(jugadorEliminado); // Agregamos un nuevo nodo al final de la cola con el jugador eliminado
    }

    /// <summary>
    /// Devuelve el jugador en la cabeza de la cola sin eliminarlo.
    /// </summary>
    /// <returns>El jugador en la cabeza de la cola.</returns>
    public Jugador Peek()
    {
        return Head.Data; // Retorna el jugador en la cabeza de la cola
    }
}
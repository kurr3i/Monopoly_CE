using System;

public class ColaCircular
{
    public NodeJugador Head { get; private set; }
    public NodeJugador Tail { get; private set; }
    public int Size { get; private set; }

    /// <summary>
    /// Agrega un nuevo nodo al final de la cola circular.
    /// </summary>
    /// <param name="jugador">jugador que almacenará el nodo.</param>
    public void Add(Jugador jugador)
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
    /// Mueve el primer nodo de la cola al final.
    /// </summary>
    public void Advance()
    {
        Head = Head.Next; // Avanzamos la cabeza al siguiente nodo
        Tail = Tail.Next; // La cola (tail) pasa a ser el siguiente nodo también (anterior cabeza), manteniendo la circularidad
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
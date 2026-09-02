using System;

/// <summary>
/// Representa el tablero del juego como una lista circular doblemente enlazada.
/// </summary>
public class Tablero
{
    public Node Head { get; private set; }
    public Node Tail { get; private set; }
    public int Size { get; private set; }

    /// <summary>
    /// Agrega un nuevo nodo al final de la lista circular doblemente enlazada.
    /// </summary>
    /// <param name="casilla">Dato que almacenará el nodo.</param>
    public void Add(Casilla casilla)
    {
        Node newNode = new Node { Data = casilla };

        if (Size == 0) // Si la lista está vacía, el nuevo nodo será tanto la cabeza como la cola
        {
            Head = newNode;
            Tail = newNode;

            Head.Next = Head; // Conectamos la cabeza con ella misma
            Head.Previous = Head;
        }
        else // Si la lista no está vacía, agregamos el nuevo nodo al final y lo conectamos con el primero
        {
            Tail.Next = newNode; // El siguiente nodo de la cola actual será el nuevo nodo
            newNode.Previous = Tail; // El nodo anterior del nuevo nodo será la cola actual
            Tail = newNode; // Actualizamos la cola para que sea el nuevo nodo

            Head.Previous = Tail; // Conectamos la cabeza con la nueva cola
            Tail.Next = Head; // Conectamos la nueva cola con la cabeza
        }

        Size++;
    }
}

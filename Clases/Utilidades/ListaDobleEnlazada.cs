using System;

/// <summary>
/// Representa a una lista doblemente enlazada.
/// </summary>
public class ListaDobleEnlazada
{
    public Node Head { get; private set; }
    public Node Tail { get; private set; }
    public int Size { get; private set; }

    /// <summary>
    /// Agrega un nuevo nodo al final de la lista doblemente enlazada.
    /// </summary>
    /// <param name="data">Dato que almacenará el nodo.</param>
    public void Add(object data)
    {
        Node newNode = new Node { Data = data };

        if (Size == 0) // Si la lista está vacía, el nuevo nodo será tanto la cabeza como la cola
        {
            Head = newNode;
            Tail = newNode;
        }
        else // Si la lista no está vacía, agregamos el nuevo nodo al final
        {
            Tail.Next = newNode; // El siguiente nodo de la cola actual será el nuevo nodo
            newNode.Previous = Tail; // El nodo anterior del nuevo nodo será la cola actual
            Tail = newNode; // Actualizamos la cola para que sea el nuevo nodo
        }

        Size++;
    }
}

using System;

/// <summary>
/// Representa a una lista doblemente enlazada.
/// </summary>
public class ListaDobleEnlazada
{
    public NodeCasilla Head { get; private set; }
    public NodeCasilla Tail { get; private set; }
    public int Size { get; private set; }

    /// <summary>
    /// Agrega un nuevo nodo al final de la lista doblemente enlazada.
    /// </summary>
    /// <param name="data">Dato que almacenará el nodo.</param>
    public void Add(Casilla data)
    {
        NodeCasilla newNode = new NodeCasilla { Data = data };

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

    /// <summary>
    /// Eliminar un nodo de la lista.
    /// </summary>
    /// <param name="data">Dato que comparará para encontrar el nodo a eliminar.</param>
    public void Remove(Casilla data)
    {
        NodeCasilla nodeActual = Head;

        if (Size == 1)
        {
            Head = null;
            Tail = null;
            Size--;
        }
        else
        {
            while (nodeActual.Data != data)
            {
                nodeActual = nodeActual.Next; // Buscamos el nodo que guarda el dato (Casilla)
            }

            if (nodeActual == Head) 
            {
                Head = Head.Next; // Actualizamos la cabeza
                Head.Previous = Tail; // El previous de la nueva cabeza será la cola
                Tail.Next = Head; // El next de la cola será la nueva cabeza
            }
            else if (nodeActual == Tail) 
            {
                Tail = Tail.Previous; // Actualizamos la cola
                Tail.Next = Head; // El next de la nueva cola será la cabeza
                Head.Previous = Tail; // El previous de la cabeza será la nueva cola
            }
            else // En caso de que el elemento esté entre la cabeza y la cola
            {
                nodeActual.Next.Previous = nodeActual.Previous; // El previous del siguiente nodo será el previos del nodo actual
                nodeActual.Previous.Next = nodeActual.Next; // El next del anterior nodo sera el next del nodo actual
            }

            Size--;
        }
    }
}

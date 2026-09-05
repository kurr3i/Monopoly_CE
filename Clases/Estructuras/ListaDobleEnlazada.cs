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
                nodeActual = nodeActual.Next;
            }

            if (nodeActual == Head)
            {
                Head = Head.Next;
                Head.Previous = null;
            }
            else if (nodeActual == Tail)
            {
                Tail = Tail.Previous;
                Tail.Next = null;
            }
            else
            {
                nodeActual.Next.Previous = nodeActual.Previous;
                nodeActual.Previous.Next = nodeActual.Next;
            }

            Size--;
        }
    }

        /// <summary>
        /// Recorre la lista y muestra en pantalla el nombre de la casilla de cada nodo.
        /// </summary>
     public void Display()
     {
        {

            NodeCasilla nodeActual = Head;
            int indice = 0;
            if (Size == 0)
            {
                Console.WriteLine("No hay propiedades que mostrar");
                return;
            }
            else
            {
                do
                {
                    Console.WriteLine("\n" + indice + "." + nodeActual.Data.Nombre);
                    nodeActual = nodeActual.Next;
                }while (nodeActual != null);
                
            }

            
        }
     }
        
}

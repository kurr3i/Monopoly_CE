using Proyecto_MonopoTEC.Server.Modelo;
using Proyecto_MonopoTEC.Server.Red;
using Proyecto_MonopoTEC.Compartido;

namespace Proyecto_MonopoTEC.Server.Estructuras
{

    /// <summary>
    /// Representa a una lista doblemente enlazada.
    /// </summary>
    public class ListaDobleEnlazada
    {
        public NodeCasilla? Head { get; private set; }
        public NodeCasilla? Tail { get; private set; }
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
                Tail!.Next = newNode; // El siguiente nodo de la cola actual será el nuevo nodo
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
            NodeCasilla nodeActual = Head!;

            if (Size == 1)
            {
                Head = null;
                Tail = null;
                Size--;
            }
            else
            {
                while (nodeActual!.Data != data)
                {
                    nodeActual = nodeActual.Next!;
                }

                if (nodeActual == Head)
                {
                    Head = Head.Next;
                    Head!.Previous = null!;
                }
                else if (nodeActual == Tail)
                {
                    Tail = Tail.Previous;
                    Tail!.Next = null!;
                }
                else
                {
                    nodeActual.Next!.Previous = nodeActual.Previous;
                    nodeActual.Previous!.Next = nodeActual.Next;
                }

                Size--;
            }
        }

        /// <summary>
        /// Recorre la lista y devuelve la casilla correspondiente al índice seleccionado.
        /// </summary>
        /// <param name="indicePropiedad">Índice de la propiedad que se desea obtener.</param>
        public Casilla GetAt(int indicePropiedad)
        {
            int indiceActual = 1;
            NodeCasilla nodoCasillaActual = Head!;

            while (indiceActual < indicePropiedad)
            {
                indiceActual++;
                nodoCasillaActual = nodoCasillaActual.Next!;
            }
            return nodoCasillaActual.Data!;

        }

        public Casilla? GetById(int id)
        {
            NodeCasilla? nodoActual = Head;

            while (nodoActual != null)
            {
                if (nodoActual.Data?.ID == id)
                    return nodoActual.Data;

                nodoActual = nodoActual.Next;
            }

            return null;
        }



    }
}

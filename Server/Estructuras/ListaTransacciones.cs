using System;

using Proyecto_MonopoTEC.Server.Modelo;

namespace Proyecto_MonopoTEC.Server.Estructuras
{

    /// <summary>
    /// Representa a una lista doblemente enlazada para las transacciones.
    /// </summary>
    public class ListaTransacciones
    {
        public NodeTransaccion Head { get; private set; }
        public NodeTransaccion Tail { get; private set; }
        public int Size { get; private set; }

        /// <summary>
        /// Agrega un nuevo nodo al final de la lista doblemente enlazada.
        /// </summary>
        /// <param name="data">Dato que almacenará el nodo.</param>
        public void Add(Transaccion data)
        {
            NodeTransaccion newNode = new NodeTransaccion { Data = data };

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
        /// Recorre la lista desde la cabeza hasta la cola.
        /// </summary>
        public void RecorrerDesdeInicio()
        {
            NodeTransaccion nodeActual = Head;

            if (Size == 0)
            {
                Console.WriteLine("Sin Transacciones que recorrer"); // Depurar
            }
            else
            {
                do
                {
                    Console.WriteLine(nodeActual.Data.ID); // Depurar
                    nodeActual = nodeActual.Next; // Avanza a la siguiente

                } while (nodeActual != null);
            }

        }

        /// <summary>
        /// Recorre la lista desde la cola hasta la cabeza.
        /// </summary>
        public void RecorrerDesdeFinal()
        {
            NodeTransaccion nodeActual = Tail;

            if (Size == 0)
            {
                Console.WriteLine("Sin Transacciones que recorrer"); // Depurar
            }
            else
            {
                do
                {
                    Console.WriteLine(nodeActual.Data.ID); // Depurar
                    nodeActual = nodeActual.Previous; // Avanza a la siguiente

                } while (nodeActual != null);
            }


        }

        /// <summary>
        /// Recorre la lista y devuelve la primera transacción correspondiente al jugador indicado.
        /// </summary>
        /// <param name="jugadorTransaccion">Jugador que realizó la transacción.</param>
        public Transaccion BuscarPorJugador(Jugador jugadorTransaccion)
        {
            NodeTransaccion nodoTransaccionActual = Head;

            if (nodoTransaccionActual == null)
            {
                Console.WriteLine("No hay transacciones");
                return null;
            }

            while (nodoTransaccionActual != null)
            {
                if (nodoTransaccionActual.Data.JugadorOrigen == jugadorTransaccion)
                {
                    return nodoTransaccionActual.Data;
                }
                nodoTransaccionActual = nodoTransaccionActual.Next;
            }

            return null;
        }

        /// <summary>
        /// Recorre la lista y devuelve la primera transacción correspondiente al tipo indicado.
        /// </summary>
        /// <param name="tipoTransaccion">El tipo de transaccion que se desea buscar.</param>
        public Transaccion BuscarPorTipo(TipoTransaccion tipoTransaccion)
        {
            NodeTransaccion nodoTransaccionActual = Head;

            if (nodoTransaccionActual == null)
            {
                Console.WriteLine("No hay transacciones");
                return null;
            }

            while (nodoTransaccionActual != null)
            {
                if (nodoTransaccionActual.Data.Tipo == tipoTransaccion)
                {
                    return nodoTransaccionActual.Data;
                }
                nodoTransaccionActual = nodoTransaccionActual.Next;
            }

            return null;
        }

        /// <summary>
        /// Recorre la lista y muestra en pantalla la información de cada transacción.
        /// </summary>
        public void Display()
        {
            NodeTransaccion nodeActual = Head;
            int indice = 1;

            if (Size == 0)
            {
                Console.WriteLine("No hay transacciones que mostrar");
            }
            else
            {

                do
                {
                    string origen;
                    string destino;

                    if (nodeActual.Data.JugadorOrigen != null)
                    {
                        origen = nodeActual.Data.JugadorOrigen.Nombre;
                    }
                    else
                    {
                        origen = "Banco";
                    }

                    if (nodeActual.Data.JugadorDestino != null)
                    {
                        destino = nodeActual.Data.JugadorDestino.Nombre;
                    }
                    else
                    {
                        destino = "Banco";
                    }

                    Console.WriteLine($"Transacción número {indice}");
                    Console.WriteLine($"ID: {nodeActual.Data.ID}");
                    Console.WriteLine($"Fecha y hora: {nodeActual.Data.FechaHora}");
                    Console.WriteLine($"Turno: {nodeActual.Data.NumeroTurno}");
                    Console.WriteLine($"Tipo: {nodeActual.Data.Tipo}");
                    Console.WriteLine($"Origen: {origen}");
                    Console.WriteLine($"Destino: {destino}");
                    Console.WriteLine($"Monto: {nodeActual.Data.Monto}");
                    Console.WriteLine($"Descripcion: {nodeActual.Data.Descripcion}");

                    nodeActual = nodeActual.Next; // Avanza a la siguiente
                    indice++;
                } while (nodeActual != null);
            }
        }
    }
}
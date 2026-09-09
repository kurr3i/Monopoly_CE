
using Proyecto_MonopoTEC.Server.Modelo;

namespace Proyecto_MonopoTEC.Server.Estructuras
{
	/// <summary>
	/// Representa a un nodo dentro de la lista doblemente enlazada para las transacciones.
	/// </summary>
	public class NodeTransaccion
	{
		public Transaccion Data { get; set; }
		public NodeTransaccion Next { get; set; }
		public NodeTransaccion Previous { get; set; }
	}
}
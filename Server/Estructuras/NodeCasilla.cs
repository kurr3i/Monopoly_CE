using Proyecto_MonopoTEC.Server.Modelo;

namespace Proyecto_MonopoTEC.Server.Estructuras
{
	/// <summary>
	/// Representa a un nodo dentro de la lista doblemente enlazada para las propiedades del jugador y las casillas del tablero.
	/// </summary>
	public class NodeCasilla
	{
		public Casilla Data { get; set; }
		public NodeCasilla Next { get; set; }
		public NodeCasilla Previous { get; set; }
	}
}
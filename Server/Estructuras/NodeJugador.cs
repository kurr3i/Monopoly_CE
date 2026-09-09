using Proyecto_MonopoTEC.Server.Modelo;

namespace Proyecto_MonopoTEC.Server.Estructuras
{
	/// <summary>
	/// Representa a un nodo dentro de la cola circular para los jugadores.
	/// </summary>
	public class NodeJugador
	{
		public Jugador Data { get; set; }
		public NodeJugador Next { get; set; }
		public NodeJugador Previous { get; set; }
	}

}
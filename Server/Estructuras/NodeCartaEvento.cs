
/// <summary>
/// Representa a un nodo dentro de la cola de cartas de evento que contiene una carta de evento de la baraja.
/// </summary>
public class NodeCartaEvento
{
	public CartaEvento Data { get; set; }
	public NodeCartaEvento Next { get; set; }
	public NodeCartaEvento Previous { get; set; }
}

using System.Net.Sockets;

namespace Proyecto_MonopoTEC.Server.Red
{
    /// <summary>
    /// Conexión TCP persistente de un jugador, con su propio lock de escritura.
    /// </summary>
    public class ConexionJugador
    {
        public string JugadorId { get; }
        public TcpClient Cliente { get; }
        public NetworkStream Stream { get; }
        public SemaphoreSlim EscrituraLock { get; }

        public ConexionJugador(string jugadorId, TcpClient cliente, NetworkStream stream, SemaphoreSlim escrituraLock)
        {
            JugadorId = jugadorId;
            Cliente = cliente;
            Stream = stream;
            EscrituraLock = escrituraLock;
        }
    }
}

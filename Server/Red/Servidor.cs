using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;

namespace Proyecto_MonopoTEC.Server.Red
{
    /// <summary>
    /// Servidor TCP del juego. Mantiene una conexión persistente por
    /// jugador, serializa las mutaciones de estado en una cola de
    /// comandos, y hace broadcast a todos cuando algo cambia.
    /// </summary>
    public class Servidor
    {
        private const int PUERTO = 5000;

        private readonly GestorConexiones _gestorConexiones = new();
        private readonly ConcurrentDictionary<string, int> _saldos = new();
        private readonly Channel<Func<Task>> _comandos = Channel.CreateUnbounded<Func<Task>>();
        private readonly Random _random = new();

        
    }
}
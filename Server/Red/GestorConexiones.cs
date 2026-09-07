using System.Collections.Concurrent;

namespace Proyecto_MonopoTEC.Server.Red
{
    /// <summary>
    /// Mantiene las conexiones activas de los jugadores y permite enviarles mensajes.
    /// </summary>
    public class GestorConexiones
    {
        private readonly ConcurrentDictionary<string, ConexionJugador> _conexiones = new();

        /// <summary>Registra la conexión de un jugador.</summary>
        public void Agregar(ConexionJugador conexion)
        {
            _conexiones[conexion.JugadorId] = conexion;
        }

        /// <summary>Elimina la conexión de un jugador.</summary>
        public void Remover(string jugadorId)
        {
            _conexiones.TryRemove(jugadorId, out _);
        }

        /// <summary>Indica si un jugadorId ya tiene una conexión activa.</summary>
        public bool EstaConectado(string jugadorId)
        {
            return _conexiones.ContainsKey(jugadorId);
        }

        /// <summary>Envía el mismo mensaje a todas las conexiones activas.</summary>
        public async Task BroadcastAsync(Mensaje mensaje)
        {
            foreach (ConexionJugador conexion in _conexiones.Values)
            {
                try
                {
                    await MensajeIO.EnviarAsync(conexion.Stream, mensaje, conexion.EscrituraLock);
                }
                catch (IOException)
                {
                }
            }
        }
    }
}
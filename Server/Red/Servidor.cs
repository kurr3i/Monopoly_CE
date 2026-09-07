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

        /// <summary>Arranca el listener TCP y el consumidor de comandos. No retorna mientras el servidor esté activo.</summary>
        public async Task IniciarAsync()
        {
            _ = Task.Run(ProcesarComandosAsync);

            TcpListener listener = new TcpListener(IPAddress.Any, PUERTO);
            listener.Start();

            Console.WriteLine($"Servidor iniciado en puerto {PUERTO}");
            Console.WriteLine("Esperando clientes...");

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                _ = Task.Run(() => ManejarClienteAsync(client));
            }
        }

        /// <summary>Maneja el ciclo de vida completo de la conexión de un jugador.</summary>
        private async Task ManejarClienteAsync(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            SemaphoreSlim escrituraLock = new(1, 1);

            string? jugadorId = null;

            try
            {
                Mensaje? primerMensaje = await MensajeIO.RecibirAsync(reader);

                if (primerMensaje == null || primerMensaje.Accion != Acciones.Conectar)
                {
                    Console.WriteLine("Conexión rechazada: el primer mensaje no fue CONECTAR.");
                    client.Close();
                    return;
                }

                jugadorId = ValidarOAsignarJugadorId(primerMensaje.JugadorId);

                ConexionJugador conexion = new ConexionJugador(jugadorId, client, stream, escrituraLock);
                _gestorConexiones.Agregar(conexion);

                await _comandos.Writer.WriteAsync(async () =>
                {
                    if (!_saldos.ContainsKey(jugadorId))
                        _saldos[jugadorId] = 1500;

                    Console.WriteLine($"Jugador conectado: {jugadorId} (saldo: {_saldos[jugadorId]})");

                    await MensajeIO.EnviarAsync(stream, new Mensaje
                    {
                        Accion = Acciones.Conectado,
                        JugadorId = jugadorId,
                        Datos = new { jugadorId, saldo = _saldos[jugadorId] }
                    }, escrituraLock);

                    await _gestorConexiones.BroadcastAsync(new Mensaje
                    {
                        Accion = Acciones.JugadorConectado,
                        JugadorId = jugadorId,
                        Datos = new { jugadorId }
                    });
                });

                while (true)
                {
                    Mensaje? mensaje = await MensajeIO.RecibirAsync(reader);

                    if (mensaje == null)
                        break;

                    await ProcesarMensajeAsync(mensaje, jugadorId, stream, escrituraLock);
                }
            }
            catch (IOException)
            {
            }
            finally
            {
                if (jugadorId != null)
                {
                    _gestorConexiones.Remover(jugadorId);

                    await _comandos.Writer.WriteAsync(async () =>
                    {
                        Console.WriteLine($"Jugador desconectado: {jugadorId}");

                        await _gestorConexiones.BroadcastAsync(new Mensaje
                        {
                            Accion = Acciones.JugadorDesconectado,
                            JugadorId = jugadorId,
                            Datos = new { jugadorId }
                        });
                    });
                }

                client.Close();
            }
        }

        /// <summary>Interpreta un mensaje recibido y ejecuta la acción correspondiente.</summary>
        private async Task ProcesarMensajeAsync(Mensaje mensaje, string jugadorId, NetworkStream stream, SemaphoreSlim escrituraLock)
        {
            switch (mensaje.Accion)
            {
                case Acciones.TirarDados:

                    await _comandos.Writer.WriteAsync(async () =>
                    {
                        int dado1 = _random.Next(1, 7);
                        int dado2 = _random.Next(1, 7);

                        Console.WriteLine($"{jugadorId} tiró los dados: {dado1} y {dado2}");

                        await _gestorConexiones.BroadcastAsync(new Mensaje
                        {
                            Accion = Acciones.DadoTirado,
                            JugadorId = jugadorId,
                            Datos = new { jugadorId, dado1, dado2, total = dado1 + dado2 }
                        });
                    });
                    break;

                default:
                    await MensajeIO.EnviarAsync(stream, new Mensaje
                    {
                        Accion = Acciones.Error,
                        Datos = new { motivo = $"Acción desconocida: {mensaje.Accion}" }
                    }, escrituraLock);
                    break;
            }
        }

        /// <summary>Valida un jugadorId propuesto o asigna uno nuevo si no es válido o ya está en uso.</summary>
        private string ValidarOAsignarJugadorId(string? jugadorIdPropuesto)
        {
            if (!string.IsNullOrWhiteSpace(jugadorIdPropuesto) && !_gestorConexiones.EstaConectado(jugadorIdPropuesto))
            {
                return jugadorIdPropuesto;
            }

            return Guid.NewGuid().ToString("N");
        }

        /// <summary>Único consumidor de la cola de comandos; procesa uno a la vez en orden de llegada.</summary>
        private async Task ProcesarComandosAsync()
        {
            await foreach (Func<Task> comando in _comandos.Reader.ReadAllAsync())
            {
                try
                {
                    await comando();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error procesando comando: {ex.Message}");
                }
            }
        }
    }
}
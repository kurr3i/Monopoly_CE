using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Proyecto_MonopoTEC.Client
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			const int HTTP_PORT = 8080;
			const int SERVER_PORT = 5000;
			const string SERVER_HOST = "localhost";

			// Única conexión TCP hacia el Server, abierta una vez y viva toda la partida.
			TcpClient? conexionServidor = null;
			NetworkStream? streamServidor = null;
			StreamReader? lectorServidor = null;
			SemaphoreSlim escrituraServidorLock = new(1, 1);

			// Id de jugador asignado por el Server en el handshake de CONECTAR.
			string? miJugadorId = null;

			// Navegadores (pestañas) conectados a este Cliente por WebSocket.
			ConcurrentDictionary<Guid, WebSocket> navegadoresConectados = new();

			await ConectarAlServidorAsync();

			_ = Task.Run(EscucharServidorAsync);

			HttpListener httpListener = new HttpListener();

			httpListener.Prefixes.Add($"http://localhost:{HTTP_PORT}/");

			httpListener.Start();

			Console.WriteLine($"Cliente iniciado en http://localhost:{HTTP_PORT}");
			Console.WriteLine("Esperando navegador...");

            while (true)
			{
				HttpListenerContext context =
					await httpListener.GetContextAsync();

				if (context.Request.IsWebSocketRequest)
				{
					_ = Task.Run(async () =>
					{
						await HandleWebSocket(context);
					});
				}
				else
				{
					await ServeFile(context);
				}
			}


			/// <summary>Abre la conexión TCP hacia el Server y hace el handshake CONECTAR.</summary>
			async Task ConectarAlServidorAsync()
			{
				conexionServidor = new TcpClient();

				await conexionServidor.ConnectAsync(SERVER_HOST, SERVER_PORT);

				streamServidor = conexionServidor.GetStream();
				lectorServidor = new StreamReader(streamServidor, Encoding.UTF8);

				Console.WriteLine("Conectado al Server.");

				await MensajeIO.EnviarAsync(streamServidor, new Mensaje
				{
					Accion = Acciones.Conectar,
					JugadorId = miJugadorId
				}, escrituraServidorLock);

				Mensaje? respuesta = await MensajeIO.RecibirAsync(lectorServidor);

				if (respuesta != null && respuesta.Accion == Acciones.Conectado)
				{
					miJugadorId = respuesta.JugadorId;
					Console.WriteLine($"Server asignó jugadorId: {miJugadorId}");
				}
				else
				{
					Console.WriteLine("El Server no confirmó la conexión (CONECTADO). Revisar protocolo.");
				}
			}


			

        }
	}
}





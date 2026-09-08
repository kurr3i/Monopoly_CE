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

            /// <summary>Escucha la conexión persistente al Server y reenvía cada mensaje a los navegadores conectados.</summary>
			async Task EscucharServidorAsync()
			{
				if (lectorServidor == null)
					return;

				while (true)
				{
					Mensaje? mensaje;

					try
					{
						mensaje = await MensajeIO.RecibirAsync(lectorServidor);
					}
					catch (IOException)
					{
						break;
					}

					if (mensaje == null)
					{
						Console.WriteLine("Se perdió la conexión con el Server.");
						break;
					}

					Console.WriteLine($"Server -> {mensaje.Accion}");

					string json = JsonSerializer.Serialize(mensaje);

					foreach (var kvp in navegadoresConectados)
					{
						WebSocket socket = kvp.Value;

						if (socket.State != WebSocketState.Open)
						{
							navegadoresConectados.TryRemove(kvp.Key, out _);
							continue;
						}

						byte[] bytes = Encoding.UTF8.GetBytes(json);

						await socket.SendAsync(
							new ArraySegment<byte>(bytes),
							WebSocketMessageType.Text,
							true,
							CancellationToken.None
						);
					}
				}
			}


			/// <summary>Envía una acción al Server sobre la conexión persistente.</summary>
			async Task EnviarAlServidorAsync(string accion, object? datos = null)
			{
				if (streamServidor == null)
				{
					Console.WriteLine("No hay conexión activa con el Server.");
					return;
				}

				await MensajeIO.EnviarAsync(streamServidor, new Mensaje
				{
					Accion = accion,
					JugadorId = miJugadorId,
					Datos = datos
				}, escrituraServidorLock);
			}




            
			/// <summary>Acepta la conexión WebSocket de un navegador y reenvía sus acciones al Server.</summary>
			async Task HandleWebSocket(HttpListenerContext context)
			{
				HttpListenerWebSocketContext wsContext =
					await context.AcceptWebSocketAsync(null);

				WebSocket socket = wsContext.WebSocket;

				Guid idNavegador = Guid.NewGuid();
				navegadoresConectados[idNavegador] = socket;

				Console.WriteLine("Navegador conectado.");

				byte[] buffer = new byte[1024];

				try
				{
					while (socket.State == WebSocketState.Open)
					{
						WebSocketReceiveResult result =
							await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

						if (result.MessageType == WebSocketMessageType.Close)
							break;

						string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

						Console.WriteLine($"Navegador: {message}");

						if (message == "TIRAR_DADOS" || message == "TIRAR_DADO")
						{
							await EnviarAlServidorAsync(Acciones.TirarDados);
						}
					}
				}
				finally
				{
					navegadoresConectados.TryRemove(idNavegador, out _);
				}
			}


			/// <summary>Sirve los archivos estáticos del navegador (index.html, css, js).</summary>
			async Task ServeFile(HttpListenerContext context)
			{
				string path = context.Request.Url!.AbsolutePath;

				if (path == "/")
					path = "/index.html";

				string filePath =
					Path.Combine(
						AppContext.BaseDirectory,
						"www",
						path.TrimStart('/')
					);

				Console.WriteLine($"Buscando archivo: {filePath}");

				if (!File.Exists(filePath))
				{
					context.Response.StatusCode = 404;
					context.Response.Close();
					return;
				}

				byte[] file =
					await File.ReadAllBytesAsync(filePath);

				string extension =
					Path.GetExtension(filePath).ToLower();

				context.Response.ContentType =
					extension switch
					{
						".html" => "text/html",
						".css" => "text/css",
						".js" => "application/javascript",
						_ => "application/octet-stream"
					};

				context.Response.ContentLength64 = file.Length;

				await context.Response.OutputStream.WriteAsync(file);

				context.Response.Close();
			}
	


    
        }
	}
}





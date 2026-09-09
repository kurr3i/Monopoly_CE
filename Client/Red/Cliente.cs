using System.Net.WebSockets;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Net.Sockets;

using Proyecto_MonopoTEC.Compartido;

namespace Proyecto_MonopoTEC.Client.Red
{

        /// <summary>
        /// Actúa como puente de mensajes entre el cliente y el servidor
        /// </summary>
        /// <param name="ServerPort">Puerto de escucha del servidor</param>
        /// <param name="ClientPort">Puerto de escucha del cliente</param>
        public class Cliente
        {
                private Socket? _serverSocket;
                private WebSocket? _clientSocket;
                private StreamWriter? _serverWriter;

                private int _ServerPort = 5000;
                private int _ClientPort = 8080;

                public Cliente(int ServerPort = 5000, int ClientPort = 8080)
                {
                        _ServerPort = ServerPort;
                        _ClientPort = ClientPort;
                }


                /// <summary>
                /// Inicia y deja escuchando al servidor en el puerto dado.
                /// </summary>
                public async Task IniciarServer()
                {
                        // Se crea el Socket
                        _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                        try
                        {
                                // Se conecta al servidor
                                await _serverSocket.ConnectAsync(new IPEndPoint(IPAddress.Loopback, _ServerPort));

                                Console.WriteLine("[Client] Cliente conectado al Servidor.");

                                // Se instancia el Reader y Writer, encargados de manejar la comunicación con el Socket
                                using NetworkStream stream = new NetworkStream(_serverSocket, ownsSocket: false);
                                using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                                using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                                _serverWriter = writer;

                                // Bucle de escucha de mensajes del servidor
                                string? json;
                                while ((json = await reader.ReadLineAsync()) is not null)
                                {
                                        // Comprobación
                                        if (!string.IsNullOrWhiteSpace(json))

                                                // Se reenvía el mensaje al App
                                                await MensajeAlApp(json);
                                }
                        }
                        catch (SocketException ex)
                        {
                                // Caso de error
                                Console.WriteLine("[Client] No se pudo conectar al Servidor: " + ex.Message);
                                Console.WriteLine("[Client] Revisar si el Servidor se está ejecutando.");
                        }
                        catch (IOException)
                        {
                                // Caso de error
                                Console.WriteLine("[Client] La conexión con el Servidor se cerró.");
                        }
                        finally
                        {
                                _serverWriter = null;
                                this._serverSocket = null;
                        }
                }


                /// <summary>
                /// Inicia y deja escuchando al cliente en el puerto dado.
                /// </summary>
                public async Task IniciarCliente()
                {
                        // Abre la comunicación HTTP
                        HttpListener listener = new HttpListener();
                        listener.Prefixes.Add("http://localhost:" + _ClientPort + "/");
                        listener.Start();

                        Console.WriteLine($"[Client] Cliente esperando App en el puerto {_ClientPort}");

                        // Bucle de escucha de mensajes del websocket
                        while (true)
                        {
                                HttpListenerContext context = await listener.GetContextAsync();


                                if (context.Request.IsWebSocketRequest)
                                {
                                        // Se acepta la conexión del WebSocket
                                        HttpListenerWebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
                                        _clientSocket = wsContext.WebSocket;

                                        Console.WriteLine("[Client] App conectado al Cliente.");

                                        // Se envía el protocolo al App manualmente
                                        await MensajeAlApp(JsonSerializer.Serialize(new Mensaje(
                                                Protocolo.EnviarProtocolo,
                                                Protocolo.devolverProtocolo()
                                                )), "Cliente");


                                        _ = Task.Run(() => WebSocketHandler(wsContext.WebSocket));
                                }
                                else
                                {
                                        // Se envía la página web si se está ingresando por HTTP
                                        string path = context.Request.Url?.AbsolutePath ?? "/";
                                        if (path == "/") path = "/index.html";

                                        // Se envían los archivos
                                        string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "www", path.TrimStart('/'));

                                        // Procesador de archivos
                                        if (File.Exists(file))
                                        {
                                                // Se definen los tipos y se envían
                                                byte[] bytes = await File.ReadAllBytesAsync(file);
                                                context.Response.ContentType = ReturnType(file); // Es necesario por MIME
                                                context.Response.ContentLength64 = bytes.Length;
                                                await context.Response.OutputStream.WriteAsync(bytes);
                                        }
                                        else
                                        {
                                                // Caso de error
                                                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                                        }

                                        context.Response.Close();
                                }
                        }
                }


                /// <summary>
                /// Maneja la conexión con el WebSocket.
                /// </summary>
                private async Task WebSocketHandler(WebSocket webSocket)
                {
                        // Buffer de memoria
                        using MemoryStream messageBuffer = new MemoryStream();
                        byte[] buffer = new byte[4096];

                        try
                        {
                                // Bucle de escucha de mensajes
                                while (webSocket.State == WebSocketState.Open)
                                {
                                        // Se recibe el mensaje
                                        WebSocketReceiveResult resultado = await webSocket.ReceiveAsync(
                                                new ArraySegment<byte>(buffer), CancellationToken.None);

                                        // Comprobación de cierre
                                        if (resultado.MessageType == WebSocketMessageType.Close)
                                                break;

                                        // Se guarda en el buffer
                                        messageBuffer.Write(buffer, 0, resultado.Count);

                                        // Comprobación de fin
                                        if (resultado.EndOfMessage)
                                        {
                                                // Procesamiento
                                                string json = Encoding.UTF8.GetString(messageBuffer.ToArray());
                                                messageBuffer.SetLength(0);

                                                // Se reenvía el mensaje al Server
                                                MensajeAlServer(json);
                                        }
                                }
                        }
                        catch (WebSocketException)
                        {
                                // Caso de error
                                Console.WriteLine("[Client] App desconectado del Cliente.");
                        }
                        finally
                        {
                                try
                                {
                                        // En caso de cierre de conexión
                                        if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived)
                                                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                                }
                                catch (WebSocketException)
                                {
                                        // Caso de error
                                }

                                // Se libera la memoria
                                webSocket.Dispose();
                                if (ReferenceEquals(_clientSocket, webSocket))
                                        _clientSocket = null;
                        }
                }


                /// <summary>
                /// Función auxiliar para devolver el tipo de archivo al HTTP
                /// </summary>
                /// <param name="file"></param>
                /// <returns></returns>
                private static string ReturnType(string file)
                {
                        return Path.GetExtension(file).ToLowerInvariant() switch
                        {
                                // Tipos de archivo según MIME
                                ".html" => "text/html; charset=utf-8",
                                ".css" => "text/css; charset=utf-8",
                                ".js" => "application/javascript; charset=utf-8",
                                _ => "application/octet-stream"
                        };
                }



                /// <summary>
                /// Envía un mensaje del App al Server por Socket
                /// </summary>
                public void MensajeAlServer(string json)
                {
                        // Comprobaciones
                        if (_serverWriter == null)
                        {
                                Console.WriteLine("[Client] No se pudo enviar: No conectado al Servidor.");
                                return;
                        }

                        try
                        {
                                // Se envía el mensaje
                                _serverWriter.WriteLine(json);
                                _serverWriter.Flush();
                                Console.WriteLine("[Client] App >>> Cliente >>> Servidor: " + json);
                        }
                        catch (IOException)
                        {
                                // Caso de error
                                Console.WriteLine("[Client] No se pudo enviar el mensaje al Servidor: Conexión cerrada.");
                        }
                }



                /// <summary>
                ///  Envía un mensaje del Server al App por WebSocket
                /// </summary>
                /// <param name="json"></param>
                /// <param name="origen"></param>
                /// <returns></returns>
                public async Task MensajeAlApp(string json, string origen = "Servidor")
                {
                        // Comprobaciones
                        if (_clientSocket != null && _clientSocket.State == WebSocketState.Open)
                        {
                                // Procesamiento
                                byte[] buffer = Encoding.UTF8.GetBytes(json);
                                try
                                {
                                        // Se envía el mensaje
                                        await _clientSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);

                                        // Determinar el origen y si es por protocolo
                                        if (origen == "Servidor")
                                        {
                                                Console.WriteLine("[Client] App <<< Cliente <<< Servidor: " + json);
                                        }
                                        else
                                        {
                                                Console.WriteLine("[Client] App <<< Cliente: Protocolo");
                                        }
                                }
                                catch (WebSocketException)
                                {
                                        // Caso de error
                                        Console.WriteLine("[Client] No se pudo enviar al WebSocket: conexión cerrada.");
                                }
                        }
                }
        }
}
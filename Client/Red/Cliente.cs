using System.Net.WebSockets;
using System.Net;
using System.Text;
using System.Net.Sockets;

using Proyecto_MonopoTEC.Compartido;

namespace Proyecto_MonopoTEC.Client.Red
{
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

                public async Task IniciarServer()
                {
                        _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                        try
                        {
                                await _serverSocket.ConnectAsync(new IPEndPoint(IPAddress.Loopback, _ServerPort));
                                Console.WriteLine("[Client] Client.cs conectado al servidor.");

                                using NetworkStream stream = new NetworkStream(_serverSocket, ownsSocket: false);
                                using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                                using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                                _serverWriter = writer;

                                string? json;
                                while ((json = await reader.ReadLineAsync()) is not null)
                                {
                                        if (!string.IsNullOrWhiteSpace(json))
                                                await MensajeAlClient(json);
                                }
                        }
                        catch (SocketException ex)
                        {
                                Console.WriteLine("[Client] No se pudo conectar al servidor: " + ex.Message);
                        }
                        catch (IOException)
                        {
                                Console.WriteLine("[Client] La conexión TCP con el servidor se cerró.");
                        }
                        finally
                        {
                                _serverWriter = null;
                                this._serverSocket = null;
                        }
                }


                public async Task IniciarCliente()
                {
                        HttpListener listener = new HttpListener();
                        listener.Prefixes.Add("http://localhost:" + _ClientPort + "/");
                        listener.Start();

                        Console.WriteLine($"[Client] Client.cs escuchando WebSockets en puerto {_ClientPort}");

                        while (true)
                        {
                                HttpListenerContext context = await listener.GetContextAsync();

                                if (context.Request.IsWebSocketRequest)
                                {
                                        HttpListenerWebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
                                        _clientSocket = wsContext.WebSocket;

                                        Console.WriteLine("[Client] WebSocket conectado al Client.cs.");
                                        _ = Task.Run(() => WebSocketHandler(wsContext.WebSocket));
                                }
                                else
                                {
                                        string path = context.Request.Url?.AbsolutePath ?? "/";
                                        if (path == "/") path = "/index.html";

                                        string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "www", path.TrimStart('/'));

                                        if (File.Exists(file))
                                        {
                                                byte[] bytes = await File.ReadAllBytesAsync(file);
                                                context.Response.ContentType = ReturnType(file);
                                                context.Response.ContentLength64 = bytes.Length;
                                                await context.Response.OutputStream.WriteAsync(bytes);
                                        }
                                        else
                                        {
                                                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                                        }

                                        context.Response.Close();
                                }
                        }
                }

                private async Task WebSocketHandler(WebSocket webSocket)
                {
                        using MemoryStream messageBuffer = new MemoryStream();
                        byte[] buffer = new byte[4096];

                        try
                        {
                                while (webSocket.State == WebSocketState.Open)
                                {
                                        WebSocketReceiveResult resultado = await webSocket.ReceiveAsync(
                                                new ArraySegment<byte>(buffer), CancellationToken.None);

                                        if (resultado.MessageType == WebSocketMessageType.Close)
                                                break;

                                        messageBuffer.Write(buffer, 0, resultado.Count);

                                        if (resultado.EndOfMessage)
                                        {
                                                string json = Encoding.UTF8.GetString(messageBuffer.ToArray());
                                                messageBuffer.SetLength(0);
                                                MensajeAlServer(json);
                                        }
                                }
                        }
                        catch (WebSocketException)
                        {
                                Console.WriteLine("[Client] WebSocket desconectado.");
                        }
                        finally
                        {
                                try
                                {
                                        if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived)
                                                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                                }
                                catch (WebSocketException)
                                {
                                }

                                webSocket.Dispose();
                                if (ReferenceEquals(_clientSocket, webSocket))
                                        _clientSocket = null;
                        }
                }


                private static string ReturnType(string file)
                {
                        return Path.GetExtension(file).ToLowerInvariant() switch
                        {
                                ".html" => "text/html; charset=utf-8",
                                ".css" => "text/css; charset=utf-8",
                                ".js" => "application/javascript; charset=utf-8",
                                _ => "application/octet-stream"
                        };
                }


                public void MensajeAlServer(string json)
                {
                        if (_serverWriter == null)
                        {
                                Console.WriteLine("[Client] No se pudo enviar: TCP no conectado al servidor.");
                                return;
                        }

                        try
                        {
                                _serverWriter.WriteLine(json);
                                _serverWriter.Flush();
                                Console.WriteLine("[Client] Web -> Servidor: " + json);
                        }
                        catch (IOException)
                        {
                                Console.WriteLine("[Client] No se pudo enviar el mensaje al servidor: conexión cerrada.");
                        }
                }


                public async Task MensajeAlClient(string json)
                {
                        if (_clientSocket != null && _clientSocket.State == WebSocketState.Open)
                        {
                                byte[] buffer = Encoding.UTF8.GetBytes(json);
                                try
                                {
                                        await _clientSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                                        Console.WriteLine("[Client] Servidor -> Web: " + json);
                                }
                                catch (WebSocketException)
                                {
                                        Console.WriteLine("[Client] No se pudo enviar al WebSocket: conexión cerrada.");
                                }
                        }
                }


        }
}

using Proyecto_MonopoTEC.Compartido;
using Proyecto_MonopoTEC.Server.Motor;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Net.Sockets;

namespace Proyecto_MonopoTEC.Server.Red
{
    public class Servidor
    {
        private readonly int _ServerPort;
        private StreamWriter? _clientWriter;

        private Juego? _juego;

        public Servidor(int ServerPort = 5000)
        {
            _ServerPort = ServerPort;
        }


        public void InstanciarJuego(Juego juego)
        {
            _juego = juego;
        }


        public async Task IniciarServer()
        {
            using Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, _ServerPort));
            serverSocket.Listen(1);

            Console.WriteLine($"[Server] Servidor escuchando en puerto {_ServerPort}");


            while (true)
            {
                using Socket clientSocket = await serverSocket.AcceptAsync();
                Console.WriteLine("[Server] Client.cs conectado al servidor");

                using NetworkStream stream = new NetworkStream(clientSocket, ownsSocket: false);
                using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                _clientWriter = writer;

                try
                {
                    string? json;
                    while ((json = await reader.ReadLineAsync()) is not null)
                    {
                        if (string.IsNullOrWhiteSpace(json))
                            continue;

                        Console.WriteLine("[Server] TCP recibido: " + json);

                        try
                        {
                            Mensaje? message = JsonSerializer.Deserialize<Mensaje>(json);
                            RecibirMensaje(message);
                        }
                        catch (JsonException)
                        {
                            Console.WriteLine("[Server] Mensaje JSON inválido recibido.");
                        }
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine("[Server] Client.cs desconectado del servidor.");
                }
                finally
                {
                    _clientWriter = null;
                }
            }
        }



        public void RecibirMensaje(Mensaje? mensaje)
        {
            if (mensaje == null || mensaje.Comando == null || mensaje.Contenido == null)
                return;

            Console.WriteLine("[Server] Comando entrante: " + mensaje.Comando);


            switch (mensaje.Comando)
            {
                case Protocolo.Prueba:
                    _juego?.PruebaConexion(mensaje);
                    break;
            }
        }



        public void EnviarMensaje(Mensaje? mensaje)
        {
            if (_clientWriter == null)
                return;

            string json = JsonSerializer.Serialize(mensaje);
            try
            {
                _clientWriter.WriteLine(json);
                Console.WriteLine("[Server] Comando enviado: " + json);
            }
            catch (IOException)
            {
                _clientWriter = null;
            }
        }

    }
}
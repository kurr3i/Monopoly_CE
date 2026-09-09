
using Proyecto_MonopoTEC.Compartido;
using Proyecto_MonopoTEC.Server.Motor;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Net.Sockets;

namespace Proyecto_MonopoTEC.Server.Red
{

    /// <summary>
    /// Maneja tanto mensajes entrantes como salientes con el cliente
    /// </summary>
    /// <param name="ServerPort">Puerto de escucha del servidor</param>
    public class Servidor
    {
        private readonly int _ServerPort;
        private StreamWriter? _clientWriter;

        private Juego _juego;

        public Servidor(int ServerPort = 5000)
        {
            _ServerPort = ServerPort;
        }

        /// <summary>
        /// Instancia el juego con el servidor, necesario para enviar mensajes al juego directamente.
        /// Es similar al modelo Modelo-Vista-Controlador
        /// </summary>
        /// <param name="juego">Instancia del Juego</param>
        public void InstanciarJuego(Juego juego)
        {
            _juego = juego;
        }

        /// <summary>
        /// Inicia y deja escuchando al servidor en el puerto dado.
        /// </summary>
        public async Task IniciarServer()
        {
            // Creación del Socket
            using Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Configuración del Socket
            serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, _ServerPort));

            // Se abre a escuchar mensajes
            serverSocket.Listen(1);
            Console.WriteLine($"[Server] Servidor esperando Cliente en el puerto {_ServerPort}");

            // Bucle de conexión
            while (true)
            {
                // Se acepta la conexión del Client
                using Socket clientSocket = await serverSocket.AcceptAsync();
                Console.WriteLine("[Server] Cliente conectado al Servidor");

                // Se instancia el Reader y Writer, encargados de manejar la comunicación con el Socket
                using NetworkStream stream = new NetworkStream(clientSocket, ownsSocket: false);
                using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                _clientWriter = writer;

                // Bucle de escucha de mensajes
                try
                {
                    // Se lee el mensaje desde el Client
                    string? json;
                    while ((json = await reader.ReadLineAsync()) is not null)
                    {
                        // Comprobaciones
                        if (string.IsNullOrWhiteSpace(json))
                            continue;

                        // Se procesa el mensaje
                        try
                        {
                            Mensaje? message = JsonSerializer.Deserialize<Mensaje>(json);

                            // Llamada al manejo de mensajes
                            RecibirMensaje(message);
                        }
                        catch (JsonException)
                        {
                            // Caso de excepción
                            Console.WriteLine("[Server] Mensaje JSON inválido.");
                        }
                    }
                }
                catch (IOException)
                {
                    // Caso de error
                    Console.WriteLine("[Server] Cliente desconectado del Servidor.");
                }
                finally
                {
                    _clientWriter = null;
                }
            }
        }


        /// <summary>
        /// Maneja mensajes entrantes y los procesa según el protocolo
        /// </summary>
        /// <param name="mensaje"></param>
        public void RecibirMensaje(Mensaje? mensaje)
        {
            // Comprobaciones
            if (mensaje == null)
                return;

            Console.WriteLine("[Server] Cliente >>> Server: " + JsonSerializer.Serialize(mensaje));

            // Casos del protocolo
            switch (mensaje.Comando)
            {


                case Protocolo.Prueba:
                    _juego?.PruebaConexion(mensaje);
                    break;



                case Protocolo.ConexionLista:
                    Console.WriteLine("[Server] App conectado al Servidor.");
                    break;



                case Protocolo.AutenticarJugador:
                    int id = mensaje.GetDato<int>("id");
                    string nombre = mensaje.GetDato<string>("nombre");

                    _juego?.AutenticarJugador(id, nombre);
                    break;



                case Protocolo.VerificarJugador:
                    int idVerificacion = mensaje.GetDato<int>("id");

                    if (_juego?.VerificarJugador(idVerificacion) == true)
                    {
                        EnviarMensaje(new Mensaje(Protocolo.VerificarJugador, new { id = 0 }));
                    }
                    break;



                case Protocolo.IniciarJuego:
                    _juego.IniciarJuego();
                    break;
            }
        }


        /// <summary>
        /// Envia un mensaje al cliente
        /// </summary>
        public void EnviarMensaje(Mensaje? mensaje)
        {
            // Comprobaciones
            if (_clientWriter == null)
                return;

            // Procesamiento del mensaje
            string json = JsonSerializer.Serialize(mensaje);
            try
            {
                // Se envía el mensaje al Client
                _clientWriter.WriteLine(json);
                Console.WriteLine("[Server] Cliente <<< Server: " + json);
            }
            catch (IOException)
            {
                // Caso de error
                _clientWriter = null;
            }
        }

    }
}
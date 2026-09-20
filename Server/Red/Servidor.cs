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
    public class Servidor
    {
        private readonly int _ServerPort;
        private StreamWriter? _clientWriter;
        private Juego? _juego;

        public Servidor(int ServerPort)
        {
            _ServerPort = ServerPort;
        }

        /// <summary>
        /// Instancia el juego con el servidor, necesario para enviar mensajes al juego directamente.
        /// </summary>
        public void InstanciarJuego(Juego juego)
        {
            _juego = juego;
        }

        /// <summary>
        /// Inicia y deja escuchando al servidor en el puerto dado.
        /// </summary>
        public async Task IniciarServer()
        {
            using Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            serverSocket.Bind(new IPEndPoint(IPAddress.Parse(Config.ServerIp), _ServerPort));

            serverSocket.Listen(1);
            Console.WriteLine($"[Server] Servidor esperando Cliente en el puerto {_ServerPort}");

            while (true)
            {
                using Socket clientSocket = await serverSocket.AcceptAsync();
                Console.WriteLine("[Server] Cliente conectado al Servidor");

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

                        try
                        {
                            Mensaje? message = JsonSerializer.Deserialize<Mensaje>(json);
                            RecibirMensaje(message);
                        }
                        catch (JsonException)
                        {
                            Console.WriteLine("[Server] Mensaje JSON inválido.");
                        }
                    }
                }
                catch (IOException)
                {
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
        public void RecibirMensaje(Mensaje? mensaje)
        {
            if (mensaje == null)
                return;

            string jsonMsg = JsonSerializer.Serialize(mensaje);
            Console.WriteLine("[Server] Cliente >>> Server: " + jsonMsg);

            switch (mensaje.Comando)
            {

                // Caso de conexión establecida con el App
                case Protocolo.ConexionLista:
                    Console.WriteLine("[Server] App conectado al Servidor.");
                    
                    _juego?.EnviarJugadoresRegistrados();
                    break;


                // Caso para autenticar un nuevo jugador
                case Protocolo.AutenticarJugador:
                    int id = mensaje.GetDato<int>("id");
                    string nombre = mensaje.GetDato<string>("nombre");
                    _juego?.AutenticarJugador(id, nombre);
                    break;


                // Caso para verificar un jugador en específico
                case Protocolo.VerificarJugador:
                    int idVerificacion = mensaje.GetDato<int>("id");
                    if (_juego?.VerificarJugador(idVerificacion) == true)
                    {
                        EnviarMensaje(Protocolo.VerificarJugador, new { id = idVerificacion });
                    }
                    break;



                // Caso para iniciar el juego
                case Protocolo.IniciarJuego:
                    _ = Task.Run(() => _juego?.IniciarJuego());
                    break;


                // Caso para recibir la accion de un jugador
                case Protocolo.AccionJugador:
                    int jugadorId = mensaje.GetDato<int>("id");
                    string accion = mensaje.GetDato<string>("accion");
                    string valor = mensaje.GetContenido("valor", out string? datoValor) ? datoValor ?? "" : accion;
                    _juego?.RecibirAccion(jugadorId, accion, valor);
                    break;

                case Protocolo.SolicitarUltimaTransaccion:
                    _juego?.EnviarUltimaTransaccion();
                    break;

                case Protocolo.SolicitarTransacciones:
                    _juego?.EnviarHistorialTransacciones();
                    break;

                case Protocolo.CerrarJuego:
                    Console.WriteLine("[Server] Cierre solicitado por el cliente.");
                    Environment.Exit(0);
                    break;

            }
        }

        /// <summary>
        /// Envia un mensaje al cliente
        /// </summary>
        public void EnviarMensaje(string comando, object contenido)
        {
            if (_clientWriter == null)
                return;

            // Procesamiento del mensaje
            Mensaje mensaje = new Mensaje(comando, contenido);
            string json = JsonSerializer.Serialize(mensaje);
            try
            {
                _clientWriter.WriteLine(json);
                Console.WriteLine("[Server] Cliente <<< Server: " + json);
            }
            catch (IOException)
            {
                _clientWriter = null;
            }
        }
    }
}
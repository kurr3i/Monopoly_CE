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
        private bool _conexionListaRecibida;

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
            // Se crea el Socket
            using Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Se configura el Socket
            serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            serverSocket.Bind(new IPEndPoint(IPAddress.Parse("0.0.0.0"), _ServerPort));

            serverSocket.Listen(1);
            Console.WriteLine($"[Server] Servidor esperando Cliente en {Config.ServerIp}:{_ServerPort}");

            // Bucle activo de escucha
            while (true)
            {
                using Socket clientSocket = await serverSocket.AcceptAsync();
                Console.WriteLine("[Server] Cliente conectado al Servidor");

                // Se instancia el Reader y Writer, encargados de manejar la comunicación con el Socket
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
                            // Procesamiento del mensaje
                            Mensaje? message = JsonSerializer.Deserialize<Mensaje>(json);
                            RecibirMensaje(message); // Llamada al handler
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

                    Console.WriteLine("[Server] Conexión cerrada manualmente.");

                    Environment.Exit(0);
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

            // Procesamiento y registro del mensaje
            string jsonMsg = JsonSerializer.Serialize(mensaje);
            Console.WriteLine("[Server] Cliente >>> Server: " + jsonMsg + "\n");

            // Handler principal de mensajes
            switch (mensaje.Comando)
            {

                // Caso de conexión establecida con el App
                case Protocolo.ConexionLista:
                    if (_conexionListaRecibida)
                    {
                        Console.WriteLine("[Server] Conexión cerrada por duplicado.");
                        EnviarMensaje(Protocolo.CerrarJuego, new { });
                        Environment.Exit(0);
                    }

                    _conexionListaRecibida = true;
                    Console.WriteLine("[Server] App conectado al Servidor.");

#pragma warning disable CS0162
                    // DEBUG TEMPORAL PARA SALTAR AUTENTICACIÓN
                    if (Config.Skip)
                    {
                        _juego?.AutenticarJugador(0, "Jugador1");
                        _juego?.AutenticarJugador(1, "Jugador2");
                        _juego?.AutenticarJugador(2, "Jugador3");
                        _juego?.AutenticarJugador(3, "Jugador4");
                    }

                    break;


                // Caso para autenticar un nuevo jugador
                case Protocolo.RegistrarJugador:
                    int id = mensaje.GetDato<int>("id");
                    string nombre = mensaje.GetDato<string>("nombre");
                    _juego?.AutenticarJugador(id, nombre); // Llamada al RFID y estructuras
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

                    _juego?.RecibirAccion(jugadorId, accion, valor); // Llamada al ManejadorAcciones dentro del juego
                    break;


                // Caso para solicitar la última transacción dada
                case Protocolo.UltimaTransaccion:
                    _juego?.EnviarUltimaTransaccion();
                    break;


                // Caso para solicitar la primera transacción dada
                case Protocolo.PrimeraTransaccion:
                    _juego?.EnviarPrimeraTransaccion();
                    break;


                // Caso para solicitar el historial de transacciones
                case Protocolo.Transacciones:
                    _juego?.EnviarHistorialTransacciones();
                    break;
                    

                // Caso para solicitar transacciones por nombre del jugador
                case Protocolo.TransaccionesPorJugador:
                    string jugadorNombre = mensaje.GetDato<string>("jugador");
                    _juego?.EnviarTransaccionesPorJugador(jugadorNombre);
                    break;


                // Caso para solicitar transacciones por tipo
                case Protocolo.TransaccionesPorTipo:
                    string tipoTransaccion = mensaje.GetDato<string>("tipo");
                    _juego?.EnviarTransaccionesPorTipo(tipoTransaccion);
                    break;


                // Caso para cerrar el juego
                case Protocolo.CerrarJuego:
                    Console.WriteLine("[Server] Conexión cerrada manualmente.");
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
                Console.WriteLine("[Server] Cliente <<< Server: " + json + "\n");
            }
            catch (IOException)
            {
                _clientWriter = null;
            }
        }
    }
}
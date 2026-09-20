using System.IO.Ports;
using Server.Persistencia;

namespace Proyecto_MonopoTEC.Server.Hardware
{
    /// <summary>
    /// Clase para la comunicación con el Arduino
    /// </summary>
    public class RFIDDriver : IDisposable
    {
        /// <summary>
        /// Puerto serial hacia el Arduino.
        /// </summary>
        private readonly SerialPort _serialPort;

        /// <summary>
        /// Logger para el registro de la partida
        /// </summary>
        private readonly RegistroPartida _logger;


        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="RFIDDriver"/>.
        /// </summary>
        /// <param name="portName">Nombre del puerto serie a usar (ej. "COM7").</param>
        /// <param name="logger">Logger para el registro de la partida</param>
        public RFIDDriver(string portName, RegistroPartida logger)
        {
            _logger = logger;
            _serialPort = new SerialPort(portName, 9600);
            _serialPort.ReadTimeout = 10000;
        }


        /// <summary>
        /// Abre la comunicación con el Arduino.
        /// </summary>
        public void Open()
        {
            try
            {
                _serialPort.Open();
                Thread.Sleep(2000);
                _logger.GuardarRegistro($"Puerto serial {_serialPort.PortName} abierto correctamente.", "Hardware");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[RFIDDriver] Error al abrir el puerto serial: " + ex.Message);
                Console.WriteLine("[RFIDDriver] Revisar si el Arduino se encuentra conectado.");
                _logger.GuardarRegistro($"Error al abrir el puerto serial: {ex.Message}", "Hardware");
                return;
            }
        }



        /// <summary>
        /// Lee el UID de un tag RFID.
        /// </summary>
        /// <param name="message">Mensaje a mostrar.</param>
        /// <param name="CompareUID">UID a verificar.</param>
        /// <returns>string con el UID leido.</returns>
        /// <exception cref="ArgumentException">Si el mensaje es demasiado largo.</exception>
        public string ReadUID(string message, string CompareUID = "0")
        {
            // Verifica la longitud adecuada del mensaje
            if (message.Length > 16)
            {
                throw new ArgumentException("El mensaje es demasiado largo.");
            }

            // Bucle hasta devolver un valor válido
            while (true)
            {
                try
                {
                    // Abrir el puerto
                    if (!_serialPort.IsOpen)
                        Open();

                    // Solicitar UID
                    Console.WriteLine("[RFIDDriver] Solicitando UID...");
                    _logger.GuardarRegistro($"Solicitando UID... Mensaje: {message}", "Hardware");

                    // Cuerpo del mensaje
                    _serialPort.WriteLine($"COM_START_READ|{message}|{CompareUID}");

                    // Esperar respuesta
                    string response = _serialPort.ReadLine();

                    // Tiempo de espera agotado
                    if (response.Contains("TIMEOUT"))
                    {
                        Console.WriteLine("[RFIDDriver] Tiempo de espera agotado. Reintentando...");
                        _logger.GuardarRegistro("Tiempo de espera agotado en RFID. Reintentando...", "Hardware");
                        Thread.Sleep(2000);
                    }
                    else if (response.Contains("INVALID"))
                    {
                        Console.WriteLine("[RFIDDriver] UID incorrecta. Reintentando...");
                        _logger.GuardarRegistro("UID incorrecta leída. Reintentando...", "Hardware");
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        Console.WriteLine($"[RFIDDriver] Respuesta: {response}");
                        _logger.GuardarRegistro($"UID leída exitosamente: {response}", "Hardware");
                        Thread.Sleep(2000);
                        return response;
                    }
                }

                // Perdida de conexion
                catch (TimeoutException)
                {
                    Console.WriteLine("[RFIDDriver] El puerto COM no responde. Reintentando...");
                    _logger.GuardarRegistro("El puerto COM no responde (Timeout Exception).", "Hardware");
                    Thread.Sleep(3000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RFIDDriver] Ocurrio un error: {ex.Message}. Reintentando...");
                    _logger.GuardarRegistro($"Error en lectura RFID: {ex.Message}", "Hardware");
                    Thread.Sleep(3000);
                }
            }
        }

        /// <summary>
        /// Cierra la comunicación con el Arduino.
        /// </summary>
        public void Dispose()
        {
            if (_serialPort != null)
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                }
                _serialPort.Dispose();
            }
        }
    }
}
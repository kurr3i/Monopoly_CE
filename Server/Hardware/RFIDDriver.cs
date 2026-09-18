using System.IO.Ports;
using Server.Persistencia;

namespace Proyecto_MonopoTEC.Server.Hardware
{
    /// <summary>
    /// Clase para la comunicación con el Arduino
    /// </summary>
    public class RFIDDriver : IDisposable
    {
        private readonly SerialPort _serialPort;
        private readonly RegistroPartida _logger;

        public RFIDDriver(string portName, RegistroPartida logger)
        {
            _logger = logger;
            _serialPort = new SerialPort(portName, 9600);
            _serialPort.ReadTimeout = 10000;
        }

        public void Open()
        {
            try
            {
                _serialPort.Open();
                Thread.Sleep(2000);
                _logger.GuardarRegistro($"Puerto serial { _serialPort.PortName } abierto correctamente.", "Hardware");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[RFIDDriver] Error al abrir el puerto serial: " + ex.Message);
                Console.WriteLine("[RFIDDriver] Revisar si el Arduino se encuentra conectado.");
                _logger.GuardarRegistro($"Error al abrir el puerto serial: {ex.Message}", "Hardware");
                return;
            }
        }

        public string ReadUID(string message, string CompareUID = "0")
        {
            if (message.Length > 16)
            {
                throw new ArgumentException("El mensaje es demasiado largo.");
            }

            while (true)
            {
                try
                {
                    if (!_serialPort.IsOpen)
                        Open();

                    Console.WriteLine("[RFIDDriver] Solicitando UID...");
                    _logger.GuardarRegistro($"Solicitando UID... Mensaje: {message}", "Hardware");

                    _serialPort.WriteLine($"COM_START_READ|{message}|{CompareUID}");
                    string response = _serialPort.ReadLine();

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
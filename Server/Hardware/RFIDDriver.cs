using System.IO.Ports;

namespace Proyecto_MonopoTEC.Server.Hardware
{
        /// <summary>
        ///  Clase para la comunicación con el Arduino
        /// </summary>
        public class RFIDDriver : IDisposable
        {
                /// <summary>
                /// Puerto serial hacia el Arduino.
                /// </summary>
                private readonly SerialPort _serialPort;

                /// <summary>
                /// Inicializa una nueva instancia de la clase <see cref="RFIDDriver"/>.
                /// </summary>
                /// <param name="portName">Nombre del puerto serie a usar (ej. "COM7").</param>
                public RFIDDriver(string portName)
                {
                        _serialPort = new SerialPort(portName, 9600);
                        _serialPort.ReadTimeout = 10000;
                }

                /// <summary>
                /// Abre la comunicación con el Arduino.
                /// </summary>
                public void Open()
                {
                        _serialPort.Open();
                        Thread.Sleep(2000); // Tiempo de arranque
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

                                        Console.WriteLine("[RFIDDriver] Solicitando UID...");

                                        // Cuerpo del mensaje
                                        _serialPort.WriteLine($"COM_START_READ|{message}|{CompareUID}");

                                        // Respuesta esperada
                                        string response = _serialPort.ReadLine();

                                        // Tiempo de espera agotado
                                        if (response.Contains("TIMEOUT"))
                                        {
                                                Console.WriteLine("[RFIDDriver] Tiempo de espera agotado. Reintentando...");
                                                Thread.Sleep(2000);
                                        }

                                        // UID incorrecta
                                        else if (response.Contains("INVALID"))
                                        {
                                                Console.WriteLine("[RFIDDriver] UID incorrecta. Reintentando...");
                                                Thread.Sleep(2000);
                                        }
                                        else
                                        {
                                                // Respuesta definitiva
                                                Console.WriteLine($"[RFIDDriver] Respuesta: {response}");
                                                Thread.Sleep(2000);
                                                return response;
                                        }

                                }

                                // Perdida de conexion
                                catch (TimeoutException)
                                {
                                        Console.WriteLine("[RFIDDriver] El puerto COM no responde. Reintentando...");
                                        Thread.Sleep(3000);
                                }
                                catch (Exception ex)
                                {
                                        Console.WriteLine($"[RFIDDriver] Ocurrio un error: {ex.Message}. Reintentando...");
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
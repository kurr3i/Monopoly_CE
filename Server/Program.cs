using System.Diagnostics;
using Proyecto_MonopoTEC.Server.Modelo;
using Proyecto_MonopoTEC.Server.Hardware;

namespace Proyecto_MonopoTEC.Server
{
        internal class Program
        {
                static void Main(string[] args)
                {
                        // Revisar si debug está habilitado
                        if (args.Length > 0 && args[0] == "--debug")
                                Debug();

                        // Todo lo que resta de main

                }




                /// <summary>
                /// Funciones de prueba y debug
                /// Revisar Config.cs para agregar otras FLAGS
                /// </summary>
                static void Debug()
                {
                        if (Config.DebugModes.Contains("RFID_TEST"))
                        {
                                Console.WriteLine("---RFID_TEST---");

                                // Revisar si se usa el Arduino Virtual
                                // Este Arduino sólo se debe utilizar para pruebas locales porque no es parte de los requisitos
                                if (Config.ArduinoVirtual)
                                {
                                        RunVirtual();
                                }



                                // Ejemplos de cómo pedir una lectura de RFID:

                                // Se instancia el driver
                                RFIDDriver driver = new RFIDDriver(Config.ArduinoPort);

                                // Lista de tarjetas
                                string[] tarjetas = new string[3];


                                // Pedir pasar cualquier tarjeta
                                string tarjeta1 = driver.ReadUID("PASAR TARJETA1");
                                tarjetas[0] = tarjeta1;
                                Console.WriteLine("Verificado: " + tarjeta1);


                                // Pedir pasar otra tarjeta
                                string tarjeta2 = driver.ReadUID("PASAR TARJETA2");
                                // Bucle hasta que no sea la misma
                                while (tarjetas.Contains(tarjeta2))
                                {
                                        tarjeta2 = driver.ReadUID("YA EXISTE, OTRA");
                                }
                                tarjetas[1] = tarjeta2;
                                Console.WriteLine("Nuevo: " + tarjeta2);


                                // Pedir pasar la tarjeta 2 otra vez
                                // Se debe usar el tercer argumento de ReadUID
                                string response2 = driver.ReadUID("VERIFICAR TARJ 2", tarjeta2);
                                Console.WriteLine("Verificado nuevamente: " + response2);

                                //Pedir pasar tarjeta 3
                                string tarjeta3 = driver.ReadUID("PASAR TARJETA3");
                                while (tarjetas.Contains(tarjeta3))
                                {
                                        tarjeta3 = driver.ReadUID("YA EXISTE, OTRA");
                                }
                                tarjetas[2] = tarjeta3;
                                Console.WriteLine("Nuevo: " + tarjeta3);


                                Console.WriteLine("Todos los usuarios fueron verificados");

                        }

                        if (Config.DebugModes.Contains("SOCKET_TEST"))
                        {
                                Console.WriteLine("SOCKET_TEST");
                        }
                }


                /// <summary>
                /// Función para iniciar el Arduino Virtual
                /// </summary>
                static void RunVirtual()
                {
                        try
                        {
                                // Ejecutar el Arduino Virtual
                                using Process virtualArduino = Process.Start(new ProcessStartInfo
                                {
                                        FileName = "python",
                                        Arguments = "../Hardware/firmware/RFID_VIRTUAL/RFID_VIRTUAL.py",
                                        UseShellExecute = false
                                })!;
                        }
                        // En caso de error
                        catch (Exception ex)
                        {
                                Console.WriteLine($"No se pudo iniciar el Arduino virtual: {ex.Message}");
                        }
                }



        }
}
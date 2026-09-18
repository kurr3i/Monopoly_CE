using System.Diagnostics;
using Proyecto_MonopoTEC.Compartido;
using Proyecto_MonopoTEC.Server.Hardware;
using Proyecto_MonopoTEC.Server.Motor;
using Proyecto_MonopoTEC.Server.Red;
using Server.Persistencia;

namespace Proyecto_MonopoTEC.Server
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // 1. Instanciar e iniciar el logger
            RegistroPartida logger = new RegistroPartida();
            logger.Iniciar();
            logger.GuardarRegistro("Iniciado el servidor.", "Server");

            if (args.Length > 0 && args[0] == "--debug")
                Debug(logger);

            // 2. Iniciar el servidor pasando el logger
            Servidor servidor = new Servidor(Config.ServerPort, logger);

            // 3. Iniciar el driver del lector RFID pasando el logger
            RFIDDriver driver = new RFIDDriver(Config.ArduinoPort, logger);
            driver.Open();

            // 4. Iniciar el juego pasando el logger
            Juego juego = new Juego(servidor, driver, logger);

            // 5. Instanciar el juego en el servidor y arrancar
            servidor.InstanciarJuego(juego);
            await servidor.IniciarServer();
        }

        /// <summary>
        /// Funciones de prueba y debug
        /// Revisar Config.cs para agregar otras FLAGS
        /// </summary>
        static void Debug(RegistroPartida logger)
        {
            if (Config.DebugModes.Contains("RFID_TEST"))
            {
                Console.WriteLine("---RFID_TEST---");
                logger.GuardarRegistro("Ejecutando prueba RFID_TEST en modo Debug", "Server");

                // Revisar si se usa el Arduino Virtual
                if (Config.ArduinoVirtual)
                {
                    RunVirtual(logger);
                }

                // Ejemplos de cómo pedir una lectura de RFID:
                RFIDDriver driver = new RFIDDriver(Config.ArduinoPort, logger);

                // Lista de tarjetas
                string[] tarjetas = new string[3];

                // Pedir pasar cualquier tarjeta
                string tarjeta1 = driver.ReadUID("PASAR TARJETA1");
                tarjetas[0] = tarjeta1;
                Console.WriteLine("Verificado: " + tarjeta1);
                logger.GuardarRegistro($"Tarjeta 1 verificada: {tarjeta1}", "Driver");

                // Pedir pasar otra tarjeta
                string tarjeta2 = driver.ReadUID("PASAR TARJETA2");
                while (tarjetas.Contains(tarjeta2))
                {
                    tarjeta2 = driver.ReadUID("YA EXISTE, OTRA");
                }
                tarjetas[1] = tarjeta2;
                Console.WriteLine("Nuevo: " + tarjeta2);
                logger.GuardarRegistro($"Tarjeta 2 registrada: {tarjeta2}", "Driver");

                // Pedir pasar la tarjeta 2 otra vez
                string response2 = driver.ReadUID("VERIFICAR TARJ 2", tarjeta2);
                Console.WriteLine("Verificado nuevamente: " + response2);

                // Pedir pasar tarjeta 3
                string tarjeta3 = driver.ReadUID("PASAR TARJETA3");
                while (tarjetas.Contains(tarjeta3))
                {
                    tarjeta3 = driver.ReadUID("YA EXISTE, OTRA");
                }
                tarjetas[2] = tarjeta3;
                Console.WriteLine("Nuevo: " + tarjeta3);
                logger.GuardarRegistro($"Tarjeta 3 registrada: {tarjeta3}", "Driver");

                Console.WriteLine("Todos los usuarios fueron verificados");
                logger.GuardarRegistro("Todos los usuarios fueron verificados en RFID_TEST", "Driver");
            }

            if (Config.DebugModes.Contains("SOCKET_TEST"))
            {
                Console.WriteLine("SOCKET_TEST");
                logger.GuardarRegistro("Ejecutando prueba SOCKET_TEST en modo Debug", "Server");
            }
        }

        /// <summary>
        /// Función para iniciar el Arduino Virtual
        /// </summary>
        static void RunVirtual(RegistroPartida logger)
        {
            try
            {
                using Process virtualArduino = Process.Start(new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = "../Hardware/firmware/RFID_VIRTUAL/RFID_VIRTUAL.py",
                    UseShellExecute = false
                })!;
                logger.GuardarRegistro("Arduino Virtual iniciado correctamente", "Hardware");
            }
            catch (Exception ex)
            {
                string errorMsg = $"No se pudo iniciar el Arduino virtual: {ex.Message}";
                Console.WriteLine(errorMsg);
                logger.GuardarRegistro(errorMsg, "Hardware");
            }
        }
    }
}
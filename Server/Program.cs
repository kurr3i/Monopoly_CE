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
            // Instanciar e iniciar el logger
            RegistroPartida logger = new RegistroPartida();
            logger.Iniciar();
            logger.GuardarRegistro("Iniciado el servidor.", "Server");


            // Debug: Revisar si se usa el Arduino Virtual
                if (Config.ArduinoVirtual)
                {
                    RunVirtual(logger);
                }
        

            // Iniciar el servidor
            Servidor servidor = new Servidor(Config.ServerPort, logger);

            // Iniciar el driver del lector RFID
            RFIDDriver driver = new RFIDDriver(Config.ArduinoPort, logger);
            driver.Open();

            // Iniciar el juego
            Juego juego = new Juego(servidor, driver, logger);

            // Instanciar el juego en el servidor y arrancar
            servidor.InstanciarJuego(juego);
            await servidor.IniciarServer();
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
using System.Diagnostics;
using Proyecto_MonopoTEC.Compartido;
using Proyecto_MonopoTEC.Server.Hardware;
using Proyecto_MonopoTEC.Server.Motor;
using Proyecto_MonopoTEC.Server.Red;
using Proyecto_MonopoTEC.Server.Persistencia;

namespace Proyecto_MonopoTEC.Server
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            // Debug: Revisar si se usa el Arduino Virtual
            if (Config.ArduinoVirtual)
            {
                RunVirtual();
            }


            // Instanciar e iniciar el registro
            RegistroTransacciones registro = new RegistroTransacciones();
            registro.Iniciar();

            // Iniciar el servidor
            Servidor servidor = new Servidor(Config.ServerPort);

            // Iniciar el driver del lector RFID
            RFIDDriver driver = new RFIDDriver(Config.ArduinoPort);
            driver.Open();

            // Iniciar el juego
            Juego juego = new Juego(servidor, driver, registro);

            // Instanciar el juego en el servidor y arrancar
            servidor.InstanciarJuego(juego);
            await servidor.IniciarServer();
        }


        /// <summary>
        /// Función para iniciar el Arduino Virtual
        /// </summary>
        static void RunVirtual()
        {
            try
            {
                using Process virtualArduino = Process.Start(new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = "../Hardware/firmware/RFID_VIRTUAL/RFID_VIRTUAL.py",
                    UseShellExecute = false
                })!;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"No se pudo iniciar el Arduino virtual: {ex.Message}");
            }
        }
    }
}
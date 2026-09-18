using System.Diagnostics;
using Proyecto_MonopoTEC.Compartido;
using Proyecto_MonopoTEC.Server.Hardware;
using Proyecto_MonopoTEC.Server.Motor;
using Proyecto_MonopoTEC.Server.Red;

namespace Proyecto_MonopoTEC.Server
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			
			// Revisar si se usa el Arduino Virtual
			if (Config.ArduinoVirtual)
			{
				RunVirtual();
			}

			// Iniciar el servidor
			Servidor servidor = new Servidor(Config.ServerPort);

			// Iniciar el driver del lector RFID
			RFIDDriver driver = new RFIDDriver(Config.ArduinoPort);
			driver.Open();

			// Iniciar el juego
			Juego juego = new Juego(servidor, driver);

			// Instanciar el juego en el servidor
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
using Proyecto_MonopoTEC.Client.Red;
using Proyecto_MonopoTEC.Compartido;

namespace Proyecto_MonopoTEC.Client
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			// Iniciar el cliente
			Cliente cliente = new Cliente(Config.ServerPort, Config.ClientPort);
			
			// Abrir la comunicación
			_ = Task.Run(() => cliente.IniciarServer());
			await cliente.IniciarCliente();
		}
	}
}
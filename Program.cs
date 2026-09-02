using Proyecto_MonopoTEC.Clases;

namespace Proyecto_MonopoTEC
{
        internal class Program
        {
                static void Main(string[] args)
                {
                        // Revisar si debug está habilitado
                        if (args.Length > 0 && args[0] == "--debug")
                        Debug();
                        

                }

                /// <summary>
                /// Funciones de prueba y debug
                /// Revisar Config.cs para agregar más
                /// </summary>
                static void Debug()
                {
                        if (Config.DebugModes.Contains("RFID_TEST"))
                        {
                                Console.WriteLine("RFID_TEST");
                        }

                        if (Config.DebugModes.Contains("SOCKET_TEST"))
                        {
                                Console.WriteLine("SOCKET_TEST");
                        }
                }
        }
}
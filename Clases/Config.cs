namespace Proyecto_MonopoTEC.Clases
{
        /// <summary>
        /// Configuración del juego
        /// </summary>
        public static class Config
        {
                /// <summary>
                /// Simular Arduino
                /// </summary>
                public const bool ArduinoVirtual = false;

                /// <summary>
                /// Puerto del servidor y Arduino físico/virtual
                /// </summary>
                public const string NormalPort = "COM7";
                public const string VirtualPort = "COM10";
                public const string ArduinoPort = ArduinoVirtual ?
                        VirtualPort : NormalPort;
                public const string ServerPort = "8080";

                /// <summary>
                /// Modos de prueba
                /// Añadir los que sean necesarios
                /// </summary>
                public static readonly string[] DebugModes = [
                        //"RFID_TEST",
                        "SOCKET_TEST"
                        ];
        }
}
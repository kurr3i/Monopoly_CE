namespace Proyecto_MonopoTEC.Compartido
{
        /// <summary>
        /// Configuración del juego
        /// </summary>
        public static class Config
        {
                /// <summary>
                /// Simular el Arduino Sí/No
                /// </summary>
                public const bool ArduinoVirtual = false;

                /// <summary>
                /// Puerto del Arduino fisico
                /// </summary>
                public const string NormalPort = "COM7";

                /// <summary>
                /// Puerto del Arduino virtual
                /// </summary>
                public const string VirtualPort = "COM11";

                /// <summary>
                /// Puerto del Arduino
                /// </summary>
                public const string ArduinoPort = ArduinoVirtual ?
                        VirtualPort : NormalPort;

                /// <summary>
                /// Puerto del servidor
                /// </summary>
                public const int ServerPort = 5000;

                /// <summary>
                /// Puerto del servidor web
                /// </summary>
                public const int ClientPort = 8080;

                /// <summary>
                /// Modos de prueba
                /// Añadir los que sean necesarios
                /// </summary>
                public static readonly string[] DebugModes = [
                        "RFID_TEST",
                        //"SOCKET_TEST"
                        //Otras FLAGS de prueba
                        ];
        }
}
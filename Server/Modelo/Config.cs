namespace Proyecto_MonopoTEC.Server.Modelo
{
        /// <summary>
        /// Configuración del juego
        /// </summary>
        public static class Config
        {
                /// <summary>
                /// Simular el Arduino Sí/No
                /// </summary>
                public const bool ArduinoVirtual = true;

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
                public const string ServerPort = "8080";

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
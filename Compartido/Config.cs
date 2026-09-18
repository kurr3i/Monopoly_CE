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
                public const int ServerPort = 5000;

                /// <summary>
                /// Dirección IPv4 del equipo donde se ejecuta el servidor.
                /// Cambiar cuando el Cliente y el Servidor estén en equipos distintos.
                /// </summary>
                public const string ServerIp = "127.0.0.1";

                /// <summary>
                /// Puerto del servidor web
                /// </summary>
                public const int ClientPort = 8080;
        }
}
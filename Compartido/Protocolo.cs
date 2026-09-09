namespace Proyecto_MonopoTEC.Compartido
{
        public static class Protocolo
        {
                public const string EnviarProtocolo = "SOCKET_PROTOCOLO";

                public const string ConexionLista = "SOCKET_CONEXION_LISTA";
                public const string Prueba = "SOCKET_PRUEBA";
                public const string AutenticarJugador = "SOCKET_AUTENTICAR_JUGADOR";
                public const string VerificarJugador = "SOCKET_VERIFICAR_JUGADOR";
                public const string IniciarJuego = "SOCKET_INICIAR_JUEGO";


                public static Dictionary<string, string> devolverProtocolo()
                {
                        return new Dictionary<string, string>
                        {
                        { nameof(ConexionLista), ConexionLista },
                        { nameof(Prueba), Prueba },
                        { nameof(AutenticarJugador), AutenticarJugador },
                        { nameof(VerificarJugador), VerificarJugador },
                        { nameof(IniciarJuego), IniciarJuego },
                        };
                }
        }
}
namespace Proyecto_MonopoTEC.Compartido
{
        /// <summary>
        /// Protocolo compartido del juego
        /// </summary>
        public static class Protocolo
        {
                // Protocolo básico
                public const string PasoProtocolo = "SOCKET_PROTOCOLO";
                public const string ConexionLista = "SOCKET_CONEXION_LISTA";

                // Autenticación y verificación
                public const string RegistrarJugador = "SOCKET_REGISTRAR_JUGADOR";
                public const string VerificarJugador = "SOCKET_VERIFICAR_JUGADOR";

                // Flujo del juego global
                public const string IniciarJuego = "SOCKET_INICIAR_JUEGO";
                public const string TerminarJuego = "SOCKET_TERMINAR_JUEGO";
                public const string CerrarJuego = "SOCKET_CERRAR_JUEGO";

                // Ganador
                public const string Ganador = "SOCKET_GANADOR";

                // Flujo de turnos
                public const string InicioTurno = "SOCKET_TURNO_INICIADO";
                public const string AccionesTurno = "SOCKET_ACCIONES_TURNO";
                public const string DesbloquearTurno = "SOCKET_DESBLOQUEAR TURNO";
                public const string FinTurno = "SOCKET_FIN_TURNO";

                // Acciones
                public const string AccionJugador = "SOCKET_ACCION_JUGADOR";
                public const string Continuar = "SOCKET_CONTINUAR";

                // Caso de error de acción
                public const string ErrorAccion = "SOCKET_ERROR_ACCION";

                // Lanzamiento de dados
                public const string DadosLanzados = "SOCKET_DADOS_LANZADOS";

                // Compra de propiedades
                public const string CompraPropiedad = "SOCKET_COMPRA_PROPIEDAD";
                public const string PropiedadComprada = "SOCKET_PROPIEDAD_COMPRADA";

                // Venta de propiedades
                public const string VentaPropiedad = "SOCKET_VENTA_PROPIEDAD";
                public const string PropiedadVendida = "SOCKET_PROPIEDAD_VENDIDA";

                // Pago de alquiler
                public const string AlquilerPagado = "SOCKET_ALQUILER_PAGADO";

                // Pagos con tarjeta
                public const string PagoRFID = "SOCKET_PAGO_RFID";

                // Bancarrotas y eliminaciones
                public const string Bancarrota = "SOCKET_BANCARROTA";
                public const string JugadorEliminado = "SOCKET_JUGADOR_ELIMINADO";

                // Movimientos
                public const string JugadorMovido = "SOCKET_JUGADOR_MOVIDO";

                // Avisos de carcel
                public const string JugadorCarcel = "SOCKET_JUGADOR_CARCEL";

                // Turnos perdidos
                public const string TurnoPerdido = "SOCKET_TURNO_PERDIDO";

                // Premios y eventos
                public const string PremioSalida = "SOCKET_PREMIO_SALIDA";
                public const string CartaEvento = "SOCKET_CARTA_EVENTO";

                // Transacciones por categoría
                public const string UltimaTransaccion = "SOCKET_ULTIMA_TRANSACCION";
                public const string PrimeraTransaccion = "SOCKET_PRIMERA_TRANSACCION";
                public const string Transacciones = "SOCKET_TRANSACCIONES_LISTA";
                public const string TransaccionesPorJugador = "SOCKET_TRANSACCIONES_JUGADOR";
                public const string TransaccionesPorTipo = "SOCKET_TRANSACCIONES_TIPO";


                /// <summary>
                /// Devuelve el diccionario con todo el protocolo
                /// </summary>
                /// <returns></returns>
                public static Dictionary<string, string> devolverProtocolo()
                {
                        return new Dictionary<string, string>
                        {
                        { nameof(ConexionLista), ConexionLista },
                        { nameof(RegistrarJugador), RegistrarJugador },
                        { nameof(VerificarJugador), VerificarJugador },
                        { nameof(IniciarJuego), IniciarJuego },
                        { nameof(TerminarJuego), TerminarJuego },
                        { nameof(CerrarJuego), CerrarJuego },
                        { nameof(Ganador), Ganador },
                        { nameof(InicioTurno), InicioTurno },
                        { nameof(AccionesTurno), AccionesTurno },
                        { nameof(DesbloquearTurno), DesbloquearTurno },
                        { nameof(AccionJugador), AccionJugador },
                        { nameof(Continuar), Continuar },
                        { nameof(FinTurno), FinTurno },
                        { nameof(ErrorAccion), ErrorAccion },
                        { nameof(DadosLanzados), DadosLanzados },
                        { nameof(CompraPropiedad), CompraPropiedad },
                        { nameof(PropiedadComprada), PropiedadComprada },
                        { nameof(AlquilerPagado), AlquilerPagado },
                        { nameof(PagoRFID), PagoRFID },
                        { nameof(Bancarrota), Bancarrota },
                        { nameof(JugadorEliminado), JugadorEliminado },
                        { nameof(JugadorMovido), JugadorMovido },
                        { nameof(JugadorCarcel), JugadorCarcel },
                        { nameof(TurnoPerdido), TurnoPerdido },
                        { nameof(PremioSalida), PremioSalida },
                        { nameof(CartaEvento), CartaEvento },
                        { nameof(VentaPropiedad), VentaPropiedad },
                        { nameof(PropiedadVendida), PropiedadVendida },
                        { nameof(UltimaTransaccion), UltimaTransaccion },
                        { nameof(PrimeraTransaccion), PrimeraTransaccion },
                        { nameof(Transacciones), Transacciones },
                        { nameof(TransaccionesPorJugador), TransaccionesPorJugador },
                        { nameof(TransaccionesPorTipo), TransaccionesPorTipo }
                        };
                }
        }
}
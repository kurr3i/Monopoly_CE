namespace Proyecto_MonopoTEC.Compartido
{
        public static class Protocolo
        {
                public const string EnviarProtocolo = "SOCKET_PROTOCOLO";

                public const string ConexionLista = "SOCKET_CONEXION_LISTA";
                public const string AutenticarJugador = "SOCKET_AUTENTICAR_JUGADOR";
                public const string VerificarJugador = "SOCKET_VERIFICAR_JUGADOR";
                public const string IniciarJuego = "SOCKET_INICIAR_JUEGO";
                public const string JuegoComenzado = "SOCKET_JUEGO_COMENZADO";
                public const string TurnoIniciado = "SOCKET_TURNO_INICIADO";
                public const string SolicitarAccionTurno = "SOCKET_SOLICITAR_ACCION_TURNO";
                public const string JugadorEnCarcel = "SOCKET_JUGADOR_EN_CARCEL";
                public const string JugadorSaleCarcel = "SOCKET_JUGADOR_SALE_CARCEL";
                public const string TurnosCarcel = "SOCKET_TURNOS_CARCEL";
                public const string TurnoPerdido = "SOCKET_TURNO_PERDIDO";
                public const string DadosLanzados = "SOCKET_DADOS_LANZADOS";
                public const string JugadorMovido = "SOCKET_JUGADOR_MOVIDO";
                public const string SolicitarContinuar = "SOCKET_SOLICITAR_CONTINUAR";
                public const string OpcionInvalida = "SOCKET_OPCION_INVALIDA";
                public const string FinTurno = "SOCKET_FIN_TURNO";
                public const string JugadorEliminado = "SOCKET_JUGADOR_ELIMINADO";
                public const string Ganador = "SOCKET_GANADOR";
                public const string CompraPropiedad = "SOCKET_COMPRA_PROPIEDAD";
                public const string PropiedadComprada = "SOCKET_PROPIEDAD_COMPRADA";
                public const string CompraRechazada = "SOCKET_COMPRA_RECHAZADA";
                public const string CompraNoPermitida = "SOCKET_COMPRA_NO_PERMITIDA";
                public const string AlquilerPagado = "SOCKET_ALQUILER_PAGADO";
                public const string Bancarrota = "SOCKET_BANCARROTA";
                public const string CartaEvento = "SOCKET_CARTA_EVENTO";
                public const string JugadorEntraCarcel = "SOCKET_JUGADOR_ENTRA_CARCEL";
                public const string OpcionCompraInvalida = "SOCKET_OPCION_COMPRA_INVALIDA";
                public const string PropiedadesVenta = "SOCKET_PROPIEDADES_VENTA";
                public const string SolicitarVenta = "SOCKET_SOLICITAR_VENTA";
                public const string SolicitarPropiedades = "SOCKET_SOLICITAR_PROPIEDADES";
                public const string PropiedadVendida = "SOCKET_PROPIEDAD_VENDIDA";
                public const string PremioSalida = "SOCKET_PREMIO_SALIDA";
                public const string AccionJugador = "SOCKET_ACCION_JUGADOR";
                public const string ErrorAccion = "SOCKET_ERROR_ACCION";
                public const string JuegoTerminado = "SOCKET_JUEGO_TERMINADO";
                public const string JugadorRegistrado = "SOCKET_JUGADOR_REGISTRADO";
                public const string JugadorDesbloqueado = "SOCKET_JUGADOR_DESBLOQUEADO";
                public const string SolicitarPagoRFID = "SOCKET_SOLICITAR_PAGO_RFID";
                public const string TransaccionRegistrada = "SOCKET_TRANSACCION_REGISTRADA";
                public const string SolicitarUltimaTransaccion = "SOCKET_SOLICITAR_ULTIMA_TRANSACCION";
                public const string UltimaTransaccion = "SOCKET_ULTIMA_TRANSACCION";
                public const string SolicitarTransacciones = "SOCKET_SOLICITAR_TRANSACCIONES";
                public const string TransaccionesLista = "SOCKET_TRANSACCIONES_LISTA";
                public const string CerrarJuego = "SOCKET_CERRAR_JUEGO";


                public static Dictionary<string, string> devolverProtocolo()
                {
                        return new Dictionary<string, string>
                        {
                        { nameof(ConexionLista), ConexionLista },
                        { nameof(AutenticarJugador), AutenticarJugador },
                        { nameof(VerificarJugador), VerificarJugador },
                        { nameof(IniciarJuego), IniciarJuego },
                        { nameof(JuegoComenzado), JuegoComenzado },
                        { nameof(TurnoIniciado), TurnoIniciado },
                        { nameof(SolicitarAccionTurno), SolicitarAccionTurno },
                        { nameof(JugadorEnCarcel), JugadorEnCarcel },
                        { nameof(JugadorSaleCarcel), JugadorSaleCarcel },
                        { nameof(TurnosCarcel), TurnosCarcel },
                        { nameof(TurnoPerdido), TurnoPerdido },
                        { nameof(DadosLanzados), DadosLanzados },
                        { nameof(JugadorMovido), JugadorMovido },
                        { nameof(SolicitarContinuar), SolicitarContinuar },
                        { nameof(OpcionInvalida), OpcionInvalida },
                        { nameof(FinTurno), FinTurno },
                        { nameof(JugadorEliminado), JugadorEliminado },
                        { nameof(Ganador), Ganador },
                        { nameof(CompraPropiedad), CompraPropiedad },
                        { nameof(PropiedadComprada), PropiedadComprada },
                        { nameof(CompraRechazada), CompraRechazada },
                        { nameof(CompraNoPermitida), CompraNoPermitida },
                        { nameof(AlquilerPagado), AlquilerPagado },
                        { nameof(Bancarrota), Bancarrota },
                        { nameof(CartaEvento), CartaEvento },
                        { nameof(JugadorEntraCarcel), JugadorEntraCarcel },
                        { nameof(OpcionCompraInvalida), OpcionCompraInvalida },
                        { nameof(PropiedadesVenta), PropiedadesVenta },
                        { nameof(SolicitarVenta), SolicitarVenta },
                        { nameof(SolicitarPropiedades), SolicitarPropiedades },
                        { nameof(PropiedadVendida), PropiedadVendida },
                        { nameof(PremioSalida), PremioSalida },
                        { nameof(AccionJugador), AccionJugador },
                        { nameof(ErrorAccion), ErrorAccion },
                        { nameof(JuegoTerminado), JuegoTerminado },
                        { nameof(JugadorRegistrado), JugadorRegistrado },
                        { nameof(JugadorDesbloqueado), JugadorDesbloqueado },
                        { nameof(SolicitarPagoRFID), SolicitarPagoRFID },
                        { nameof(TransaccionRegistrada), TransaccionRegistrada },
                        { nameof(SolicitarUltimaTransaccion), SolicitarUltimaTransaccion },
                        { nameof(UltimaTransaccion), UltimaTransaccion },
                        { nameof(SolicitarTransacciones), SolicitarTransacciones },
                        { nameof(TransaccionesLista), TransaccionesLista },
                        { nameof(CerrarJuego), CerrarJuego },
                        };
                }
        }
}
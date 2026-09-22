// Estado global del juego
const Juego = {
        protocolo: null,
        tablero: [],
        jugadores: [],
        idJugadorActual: null,
        turno: 0,
        modalJugadorId: null,
        modoModal: "register",
        idJugadorDesbloqueado: null,
        modoPropiedad: null,
        colaEventos: [],
        mostrandoEvento: false,
        reciboPendiente: null,
        cierreEnCurso: false,
        dadosAnimando: false
};

// Tablero base del juego, representado por casillas con id, nombre, tipo y precio.
const Tablero = [
        [1, "Salida", "especial"], [2, "Loteria 1", "evento"], [3, "Pococí", "propiedad", 200], [4, "Guácimo", "propiedad", 250],
        [5, "San Carlos", "propiedad", 300], [6, "Zarcero", "propiedad", 350], [7, "San Lucas - Carcel", "especial"], [8, "Heredia", "propiedad", 400],
        [9, "Sarapiquí", "propiedad", 450], [10, "Quepos", "propiedad", 500], [11, "Loteria 2", "evento"], [12, "Golfito", "propiedad", 550],
        [13, "Parque de Diversiones", "especial"], [14, "Liberia", "propiedad", 600], [15, "Nicoya", "propiedad", 650], [16, "Loteria 3", "evento"],
        [17, "Cartago", "propiedad", 700], [18, "Turrialba", "propiedad", 750], [19, "La Cali - Para la Carcel", "especial"], [20, "Desamparados", "propiedad", 800],
        [21, "Perez Zeledón", "propiedad", 850], [22, "Loteria 4", "evento"], [23, "Chepe", "propiedad", 900], [24, "Escazú", "propiedad", 950]
].map(([id, nombre, tipo, precio]) => ({ id, nombre, tipo, precio, propietario: null }));
Juego.tablero = Tablero;

// Conexión WebSocket y elementos del DOM.
const socket = new WebSocket("ws://localhost:8080/");
const $ = (selector) => document.querySelector(selector);

// Protocolo básico para la comunicación con el servidor.
// Luego se obtendrá el protocolo completo desde el Cliente
const Protocolo = {
        PasoProtocolo: "SOCKET_PROTOCOLO"
};

// Paleta de colores para los jugadores.
const colores = ["coral", "mint", "gold", "sky"];

// Secciones del juego.
const secciones = { lobby: $("#lobbySection"), game: $("#gameSection"), results: $("#resultsSection") };

// Manejo inicial del Socket
socket.addEventListener("open", () => { console.log("[App] WebSocket conectado"); });



// Caso de cierre
socket.addEventListener("close", () => {
        if (Juego.cierreEnCurso) return;
        console.warn("[App] WebSocket cerrado");
        mostrarFalloConex("El cliente perdió la conexión con el servidor.");
        cerrarJuego();
});



// Caso de error
socket.addEventListener("error", (error) => {
        if (Juego.cierreEnCurso) return;
        console.error("[App] WebSocket error", error);
        mostrarFalloConex("No se pudo comunicar con el servidor.");
        cerrarJuego();
});



// Manejo de los mensajes del servidor
socket.addEventListener("message", (event) => {
        try {
                // Procesamiento
                const message = JSON.parse(event.data);
                if (!message || !message.Comando) return;
                let content = message.Contenido;
                if (typeof content === "string") content = JSON.parse(content);

                // Registro
                console.log("[App] App <<< Servidor:", { Comando: message.Comando, Contenido: content }, "\n");

                // Revisión de protocolo
                if (message.Comando === Protocolo.PasoProtocolo) {
                        Juego.protocolo = content || {};
                        Object.assign(Protocolo, Juego.protocolo);

                        // Mensaje de verificación
                        enviarMensaje(Protocolo.ConexionLista);

                        // Llamada a inicializar el juego
                        actualizarLobby();
                        return;
                }

                // Traslado de mensajes al handler
                recibirMensaje(message.Comando, content || {});
        } catch (error) {

                // Mensajes inválidos
                console.error("[App] Mensaje inválido", error);
        }
});



// Función para fallos de conexión
function mostrarFalloConex(message) {
        if (Juego.cierreEnCurso) return;
        $("#crashMessage").textContent = message;
        $("#crashModal").classList.remove("hidden");
        $("#authModal").classList.add("hidden");
        $("#actionArea").innerHTML = "";
}



// Función para enviar mensajes por WebSocket
function enviarMensaje(command, content = {}) {
        if (typeof command !== "string" || !command) return false;
        if (socket.readyState !== WebSocket.OPEN) return false;

        // Construcción del mensaje
        const message = { Comando: command, Contenido: content };

        // Registro
        console.log("[App] App >>> Servidor:", message, "\n");

        // Envío
        socket.send(JSON.stringify(message));
        return true;
}



// Función auxiliar para cambiar de sección
function cambiarSeccion(name) {
        Object.values(secciones).forEach((section) => section.classList.add("hidden"));
        secciones[name].classList.remove("hidden");
}



// Función para abrir el modal de autenticación
function abrirRegistro(id) {
        Juego.modalJugadorId = id;
        Juego.modoModal = "register";
        $("#modalEyebrow").textContent = `Jugador ${id + 1}`;
        $("#modalTitle").textContent = "Nombre del jugador";
        $("#nameStep").classList.remove("hidden");
        $("#rfidStep").classList.add("hidden");
        $("#playerName").value = "";
        $("#nameError").textContent = "";
        $("#authModal").dataset.lockDismiss = "false";
        $("#authModal").classList.remove("hidden");
        $("#closeModal").classList.remove("hidden");
        $("#playerName").focus();
}



// Función para abrir el modal de verificación
function abrirVerificador(id) {
        Juego.modalJugadorId = id;
        Juego.modoModal = "verify";
        $("#modalEyebrow").textContent = `Jugador ${id + 1}`;
        $("#modalTitle").textContent = "Verificar jugador";
        $("#rfidStep").classList.remove("hidden");
        $("#authModal").dataset.lockDismiss = "true";
        $("#authModal").classList.remove("hidden");
        $("#closeModal").classList.add("hidden");
        enviarMensaje(Protocolo.VerificarJugador, { id });
}



// Función para abrir el modal de desbloqueo
function abrirDesbloqueo() {
        $("#modalEyebrow").textContent = "Verificar jugador";
        $("#modalTitle").textContent = "Se necesita verificación";
        $("#nameStep").classList.add("hidden");
        $("#rfidStep").classList.remove("hidden");
        $("#rfidStatus").textContent = "Acerca la tarjeta del jugador activo al lector...";
        $("#authModal").dataset.lockDismiss = "true";
        $("#authModal").classList.remove("hidden");
        $("#closeModal").classList.add("hidden");
}



// Función para cerrar el modal
function cerrarModal() {
        $("#authModal").dataset.lockDismiss = "false";
        $("#authModal").classList.add("hidden");
        Juego.modalJugadorId = null;
}



// Función para verificar y enviar el nombre del jugador
function subirNombre() {
        const name = $("#playerName").value.trim();
        if (!name) {
                $("#nameError").textContent = "Ingresa un nombre.";
                return;
        }
        if (name.length > 12) {
                $("#nameError").textContent = "El nombre no puede superar 12 caracteres.";
                return;
        }

        $("#nameStep").classList.add("hidden");
        $("#rfidStep").classList.remove("hidden");
        $("#modalTitle").textContent = "Verificar tarjeta";
        $("#rfidStatus").textContent = "Esperando la tarjeta del jugador...";
        $("#closeModal").classList.add("hidden");
        $("#authModal").dataset.lockDismiss = "true";
        Juego.modoModal = "registerVerify";
        enviarMensaje(Protocolo.RegistrarJugador, { id: Juego.modalJugadorId, nombre: name });
}



// Función para mostrar cajas de diálogo en pantalla
function mostrarMensaje(category, title, message, dismissible = false, duration = 2500) {

        const card = $("#statusCard");
        card.className = `modal-card status-card ${category}`;

        $("#statusEyebrow").textContent = category === "success" ? "Listo" : category === "error" ? "Atención" : "Mensaje";

        $("#statusTitle").textContent = title;
        $("#statusMessage").textContent = message;

        $("#statusMark").textContent = category === "success" ? "✓" : category === "error" ? "!" : "i";

        $("#statusOk").classList.toggle("hidden", !dismissible);
        $("#statusModal").classList.remove("hidden");

        clearTimeout(mostrarMensaje.timer);
        if (!dismissible) mostrarMensaje.timer = setTimeout(ocultarMensaje, duration);
}



// Función para mostrar eventos, en orden
function mostrarEvento(label, message, duration = 2800, receipt = null) {
        // Cola de eventos
        Juego.colaEventos.push({ label, message, duration, receipt });
        if (Juego.mostrandoEvento) return;

        // Procesar el siguiente evento
        const procesarSiguienteEvento = () => {
                const siguiente = Juego.colaEventos.shift();
                if (!siguiente) {
                        Juego.mostrandoEvento = false;
                        $("#eventPopup").classList.add("hidden");
                        $("#eventPopup").onclick = null;
                        if (Juego.reciboPendiente) {
                                mostrarUltimaTransaccion(Juego.reciboPendiente);
                                Juego.reciboPendiente = null;
                        }
                        return;
                }

                // Mostrar el evento
                Juego.mostrandoEvento = true;
                $("#eventPopupLabel").textContent = siguiente.label;
                $("#eventPopupText").textContent = siguiente.message;
                $("#eventPopup").classList.remove("hidden");
                $("#eventPopup").onclick = () => {
                        clearTimeout(mostrarEvento.timer);
                        $("#eventPopup").classList.add("hidden");
                        const recibo = siguiente.receipt ?? Juego.reciboPendiente;
                        if (recibo) {
                                mostrarTransaccion(recibo)
                                Juego.reciboPendiente = null;
                        }
                        procesarSiguienteEvento();
                };

                clearTimeout(mostrarEvento.timer);
                mostrarEvento.timer = setTimeout(() => {
                        $("#eventPopup").classList.add("hidden");
                        const recibo = siguiente.receipt ?? Juego.reciboPendiente;
                        if (recibo) {
                                mostrarTransaccion(recibo);
                                Juego.reciboPendiente = null;
                        }
                        procesarSiguienteEvento();
                }, siguiente.duration);
        };

        procesarSiguienteEvento();
}



// Función para ocultar el caja de diálogo
function ocultarMensaje() {
        $("#statusModal").classList.add("hidden");
}



// Función o handler principal para recibir los mensajes del servidor
function recibirMensaje(command, content) {
        switch (command) {

                // Caso de registrar un nuevo jugador
                case Protocolo.RegistrarJugador:
                        if (content.id === -1) {
                                cerrarModal();

                                mostrarMensaje("error", "Jugador no registrado", content.error || "No se pudo autenticar.");
                                return;
                        }
                        insertarJugador({
                                id: content.id,
                                nombre: content.nombre,
                                saldo: 1000,
                                posicion: 1,
                                posicionNombre: "Salida",
                                activo: true,
                                propiedades: []
                        });

                        cerrarModal();

                        mostrarMensaje("success", "Jugador conectado", `${content.nombre} fue registrado correctamente.`);

                        actualizarLobby();
                        break;

                // Caso de verificar el jugador
                case Protocolo.VerificarJugador:
                        if (content.error) {
                                cerrarModal();
                                mostrarMensaje("error", "Verificación fallida", content.error);
                        } else {
                                cerrarModal();
                                mostrarMensaje("success", "Jugador verificado", "La tarjeta RFID fue aceptada.");
                        }
                        break;



                // Caso de iniciar la partida
                case Protocolo.IniciarJuego:
                        if (content.error) {
                                mostrarMensaje("error", "No se puede iniciar", content.error, true);
                        }
                        else {
                                mostrarEvento("Partida", content.mensaje || "La partida ha comenzado.", 2600);

                                cambiarSeccion("game");
                        }
                        break;

                // Caso de juego terminado
                case Protocolo.TerminarJuego:
                        const ganadorFinal = obtenerNombreJugadorPorId(content.ganadorId ?? content.jugadorId ?? content.id ?? content.ganador);

                        mostrarResultados({ ...content, ganador: ganadorFinal });
                        break;

                // Caso de cerrar el juego
                case Protocolo.CerrarJuego:
                        cerrarJuego();
                        break;



                // Caso de ganador
                case Protocolo.Ganador:
                        mostrarMensaje("success", "Partida decidida", `Ganó ${obtenerNombreJugadorPorId(content.jugadorId ?? content.id ?? content.jugador)}.`);
                        break;



                // Caso de iniciar el turno
                case Protocolo.InicioTurno:
                        Juego.idJugadorActual = content.jugadorId ?? Juego.idJugadorActual;
                        Juego.turno = content.turno ?? Juego.turno;
                        Juego.idJugadorDesbloqueado = null;

                        actualizarJuego();
                        break;

                // Caso de solicitar las acciones de un jugador
                case Protocolo.AccionesTurno:
                        Juego.idJugadorActual = content.id ?? Juego.idJugadorActual;

                        actualizarJuego();
                        if ((content.tipo || "").toLowerCase() === "continuar") {
                                mostrarAcciones("continuar");
                                return;
                        }

                        mostrarAcciones(content.tipo || "turno");
                        break;

                // Caso de desbloquear el turno de un jugador
                case Protocolo.DesbloquearTurno:
                        Juego.idJugadorDesbloqueado = content.jugadorId;
                        cerrarModal();

                        $("#gameNotice").textContent = "Turno desbloqueado. Ya puedes actuar.";

                        mostrarAcciones("turno");

                        mostrarMensaje("success", "Jugador verificado", "Ya puedes usar tu turno.");
                        break;

                // Caso de finalizar turno
                case Protocolo.FinTurno:
                        cerrarModal();
                        break;



                // Caso de continuar
                case Protocolo.Continuar:
                        mostrarAcciones("continuar");
                        break;



                // Caso de error
                case Protocolo.ErrorAccion:
                        mostrarMensaje("error", "Acción no disponible", content.mensaje || "Acción inválida.");
                        break;



                // Caso de mostrar el resultado de los dados
                case Protocolo.DadosLanzados:
                        mostrarResultadoDados(content);
                        break;



                // Caso de compra de propiedad
                case Protocolo.CompraPropiedad:
                        mostrarCompra(content);
                        break;

                // Caso de propiedad comprada
                case Protocolo.PropiedadComprada:
                        cerrarModal();

                        const jugadorIdCompra = content.jugadorId ?? content.id ?? content.jugador;

                        const jugadorCompra = obtenerJugadorPorId(jugadorIdCompra);
                        if (jugadorCompra) {
                                jugadorCompra.saldo = content.saldo ?? jugadorCompra.saldo;
                                jugadorCompra.propiedades = [...(jugadorCompra.propiedades || []), content.propiedad];
                        }

                        const property = Juego.tablero.find((item) => item.nombre === content.propiedad);
                        if (property) property.propietario = jugadorIdCompra;

                        actualizarJuego();
                        break;



                // Caso de venta de propiedad
                case Protocolo.VentaPropiedad:
                        if (content.mensaje) {
                                mostrarMensaje("message", "Venta de propiedad", content.mensaje, true);
                                return;
                        }

                        if (Array.isArray(content.propiedades) && content.propiedades.length > 0) {
                                abrirModalVentaPropiedades(content.propiedades);
                                return;
                        }

                        mostrarMensaje("message", "Venta de propiedad", "No tienes propiedades para vender en este momento.", true);
                        break;

                // Caso de propiedad vendida
                case Protocolo.PropiedadVendida:
                        cerrarModalVentaPropiedades();

                        const jugadorIdVenta = content.jugadorId ?? content.id ?? content.jugador;
                        const jugadorVendido = obtenerJugadorPorId(jugadorIdVenta);

                        if (jugadorVendido) {
                                jugadorVendido.saldo = content.saldo ?? jugadorVendido.saldo;
                                jugadorVendido.propiedades = (jugadorVendido.propiedades || []).filter((propiedad) => propiedad !== content.propiedad);
                        }

                        mostrarMensaje("success", "Propiedad vendida", `${content.propiedad} fue vendida correctamente.`, true, 2600);
                        mostrarJuego();
                        break;



                // Caso de pagar un alquiler
                case Protocolo.AlquilerPagado:
                        cerrarModal();

                        actualizarJugador(content.jugadorId ?? content.id, { saldo: content.saldo });

                        actualizarJugador(content.propietarioId, { saldo: content.saldoPropietario });

                        actualizarJuego();
                        break;




                // Caso de solicitar el pago de un jugador
                case Protocolo.PagoRFID:
                        const nombreJugadorPago = content.jugador || obtenerNombreJugadorPorId(content.jugadorId ?? content.id ?? content.jugador);

                        const montoPago = content.monto ?? 0;
                        const conceptoPago = content.concepto || "Pago";
                        const mensajePago = `${nombreJugadorPago} debe pagar ${montoPago} CRC por ${conceptoPago.toLowerCase()}.`;

                        if (!(conceptoPago.toLowerCase()).includes("carta de evento")) {
                                mostrarMensaje("message", "Pago requerido", mensajePago, true, 3500);
                        }

                        $("#modalEyebrow").textContent = "Pago requerido";
                        $("#modalTitle").textContent = "Confirma el pago";
                        $("#nameStep").classList.add("hidden");
                        $("#rfidStep").classList.remove("hidden");
                        $("#rfidStatus").textContent = `${conceptoPago}: acerca la tarjeta de ${nombreJugadorPago} al lector.`;
                        $("#authModal").dataset.lockDismiss = "true";
                        $("#authModal").classList.remove("hidden");
                        $("#closeModal").classList.add("hidden");
                        break;



                // Caso de jugador en bancarrota
                case Protocolo.Bancarrota:
                        cerrarModal();

                        mostrarMensaje("error", "Bancarrota", `${obtenerNombreJugadorPorId(content.jugadorId ?? content.id ?? content.jugador)} no pudo pagar y queda fuera de la partida.`);

                        break;

                // Caso de jugador eliminado
                case Protocolo.JugadorEliminado:
                        actualizarJugador(content.jugadorId ?? content.id, { activo: false });

                        actualizarJuego();
                        break;



                // Caso de mover un jugador
                case Protocolo.JugadorMovido:
                        const jugadorIdMovimiento = content.jugadorId ?? content.id ?? content.jugador;

                        const casillaNombreMovimiento = content.casilla ?? obtenerNombreCasillaPorId(content.posicion);

                        Juego.slotAnimadoId = content.posicion ?? content.id ?? 1;
                        actualizarJugador(jugadorIdMovimiento, { posicion: content.posicion ?? 1, posicionNombre: casillaNombreMovimiento });

                        mostrarEvento("Movimiento", `${obtenerNombreJugadorPorId(jugadorIdMovimiento)} llegó a ${casillaNombreMovimiento}.`, 2200);

                        actualizarJuego();
                        break;



                // Caso de avisos de la carcel
                case Protocolo.JugadorCarcel:
                        mostrarEvento("Cárcel", `${content.mensaje}`, 2400);
                        break;



                // Caso de perder el turno
                case Protocolo.TurnoPerdido:
                        mostrarEvento("Turno perdido", `${obtenerNombreJugadorPorId(content.jugadorId ?? content.id ?? content.jugador)} pierde un turno.`);
                        break;



                // Caso de premio de salida
                case Protocolo.PremioSalida:
                        mostrarMensaje("message", "Premio de salida", content.descripcion, true, 2600);

                        cerrarModal();

                        actualizarJugador(content.jugadorId ?? content.id, { saldo: content.saldo });

                        actualizarJuego();
                        break;

                // Caso de carta de evento
                case Protocolo.CartaEvento:
                        if (content.tipo === "VayaCarcel") {
                                mostrarMensaje("message", "Carta de evento", content.descripcion, true);

                                mostrarEvento("Cárcel", `${obtenerNombreJugadorPorId(content.jugadorId ?? content.id ?? content.jugador)} fue enviado a la cárcel.`, 2400);

                        } else {
                                mostrarMensaje("message", "Carta de evento", content.descripcion, true);
                        }
                        cerrarModal();

                        actualizarJugador(content.jugadorId ?? content.id, { saldo: content.saldo });

                        actualizarJuego();
                        break;



                // Caso de mostrar la última transacción
                case Protocolo.UltimaTransaccion:
                        const transaccionActiva = content.transaccion || content;
                        if (Juego.mostrandoEvento) {
                                Juego.reciboPendiente = transaccionActiva;
                                return;
                        }
                        mostrarUltimaTransaccion(transaccionActiva);
                        break;

                // Caso de mostrar la primera transacción
                case Protocolo.PrimeraTransaccion:
                        mostrarTransaccion(content.transaccion || content, "Primera transacción");
                        break;

                // Caso de mostrar todas las transacciones
                case Protocolo.Transacciones:
                        mostrarHistorialTransacciones(content.transacciones || [], "Todas las transacciones");
                        break;

                // Caso de mostrar las transacciones de un jugador
                case Protocolo.TransaccionesPorJugador:
                        if ((content.transacciones || []).length === 0) {
                                mostrarTransaccion(null, `Transacciones de ${content.jugador || "jugador"}`);
                                return;
                        }

                        mostrarHistorialTransacciones(content.transacciones || [], `Transacciones de ${content.jugador || "jugador"}`);
                        break;

                // Caso de mostrar las transacciones de un tipo
                case Protocolo.TransaccionesPorTipo:
                        if ((content.transacciones || []).length === 0) {

                                mostrarTransaccion(null, `Transacciones tipo ${content.tipo || "solicitado"}`);
                                return;
                        }
                        mostrarHistorialTransacciones(content.transacciones || [], `Transacciones tipo ${content.tipo || "solicitado"}`);
                        break;



                // Caso de mensaje general
                default:
                        if (content.mensaje || content.descripcion) mostrarMensaje("message", "Actualización", content.mensaje || content.descripcion, true);
                        break;
        }
}

// Función para insertar o actualizar un jugador
function insertarJugador(player) {

        // Insertar jugador
        const basePlayer = {
                id: player.id,
                nombre: player.nombre,
                saldo: 1000,
                posicion: 1,
                posicionNombre: "Salida",
                activo: true,
                propiedades: []
        };

        // Actualizar jugador
        const existing = Juego.jugadores.findIndex((item) => item.id === player.id);
        if (existing >= 0) {
                Juego.jugadores[existing] = {
                        ...basePlayer,
                        ...Juego.jugadores[existing],
                        ...player
                };
                return;
        }

        Juego.jugadores.push({ ...basePlayer, ...player });
}

// Función para actualizar un jugador
function actualizarJugador(id, changes) {
        const player = Juego.jugadores.find((item) => item.id === id);
        if (player) Object.assign(player, changes);
}

// Getter de jugador por id
function obtenerJugadorPorId(id) {
        const identificador = Number(id);
        if (!Number.isFinite(identificador)) return null;
        return Juego.jugadores.find((item) => Number(item.id) === identificador) ?? null;
}

// Getter de nombre de  jugador por id
function obtenerNombreJugadorPorId(id) {
        const jugador = obtenerJugadorPorId(id);
        if (jugador?.nombre) return jugador.nombre;

        const identificador = Number(id);
        if (id == null || id === undefined || id === "" || !Number.isFinite(identificador)) return "Jugador";
        return `Jugador ${identificador + 1}`;
}

// Getter de casilla por id
function obtenerNombreCasillaPorId(id) {
        return Juego.tablero.find((item) => item.id === id)?.nombre ?? "Casilla";
}

// Función para actualizar el lobby
function actualizarLobby() {
        const players = Juego.jugadores;

        const container = $("#lobbyPlayers");
        container.innerHTML = "";

        for (let id = 0; id < 4; id += 1) {
                const player = players.find((item) => item.id === id);

                const card = document.createElement("article");

                card.className = `lobby-seat ${colores[id]}`;

                card.innerHTML = `<span class="seat-number">0${id + 1}</span><div><p>${player ? escapeHtml(player.nombre) : "Espacio disponible"}</p><small>${player ? "Registrado" : "Listo para conectar"}</small></div>`;

                const button = document.createElement("button");
                button.className = player ? "secondary-button" : "primary-button";

                button.textContent = player ? "Verificar" : "Conectar";

                button.addEventListener("click", () => player ? abrirVerificador(id) : abrirRegistro(id));

                card.append(button);

                container.append(card);
        }

        $("#lobbyCount").textContent = `${players.length} / 4`;
        $("#startGameButton").disabled = players.length !== 4;
        $("#lobbyStatus").textContent = players.length === 4 ? "Mesa completa" : "Esperando jugadores";
        $("#lobbyHint").textContent = players.length === 4 ? "Ya se puede iniciar la partida." : "Conecta los cuatro jugadores para habilitar la partida.";
}

// Función para actualizar el juego
function actualizarJuego() {
        $("#turnNumber").textContent = Juego.turno + 1;
        const active = Juego.jugadores.find((player) => player.id === Juego.idJugadorActual);
        $("#activePlayerPanel").className = `panel active-panel ${colores[active?.id]}`;
        $("#activePlayerToken").textContent = active?.id + 1;
        $("#activePlayerToken").className = `token ${colores[active?.id]}`;
        $("#activePlayerText").className = `eyebrow ${colores[active?.id]}`;
        $("#activePlayerName").textContent = active?.nombre || "Esperando turno";
        $("#activePlayerDetails").textContent = active ? `Saldo: ${active.saldo} CRC · Ubicación: ${active.posicionNombre || "En el tablero"}` : "Usa el lector RFID para desbloquear sus acciones.";

        actualizarTablero();

        actualizarJugadores();
}

// Función para actualizar el tablero
function actualizarTablero() {
        const board = $("#board");

        board.innerHTML = "";

        Juego.tablero.forEach((slot, index) => {
                const playersHere = Juego.jugadores.filter((player) => player.posicion === slot.id && player.activo);

                const card = document.createElement("article");
                const shouldAnimate = Juego.slotAnimadoId === slot.id;
                card.className = `slot ${slot.tipo} ${playersHere.length ? "occupied" : ""} ${shouldAnimate ? "jump" : ""}`;

                const position = posicionTabla(index, 7);
                card.style.gridColumn = position.column;
                card.style.gridRow = position.row;

                card.innerHTML = `<span class="slot-id">${String(slot.id).padStart(2, "0")}</span><h3>${escapeHtml(slot.nombre)}</h3>${slot.precio ? `<p>${slot.precio} CRC</p>` : ""}<div class="tokens">${playersHere.map((player) => `<span class="token ${colores[player.id] || "coral"}" title="${escapeHtml(player.nombre)}">${player.id + 1}</span>`).join("")}</div>`;

                board.append(card);
        });

        // Animación de la casilla
        if (Juego.slotAnimadoId != null) {
                setTimeout(() => {
                        Juego.slotAnimadoId = null;
                        actualizarTablero();
                }, 420);
        }
}

// Función para obtener la posición de una casilla en el tablero
function posicionTabla(index, size) {
        if (index < size) return { row: 1, column: index + 1 };
        if (index < size * 2 - 1) return { row: index - size + 2, column: size };
        if (index < size * 3 - 2) return { row: size, column: size - (index - (size * 2 - 2)) };
        return { row: size - (index - (size * 3 - 3)), column: 1 };
}

// Función para actualizar los jugadores
function actualizarJugadores() {
        const container = $("#playerCards");

        container.innerHTML = Juego.jugadores.map((player) => `<article class="player-card ${player.id === Juego.idJugadorActual ? "active" : ""} ${colores[player.id] || "coral"}"><div class="player-card-top"><span class="token ${colores[player.id] || "coral"}">${player.id + 1}</span><strong>${escapeHtml(player.nombre)}</strong><span class="player-status">${player.activo ? "En juego" : "Fuera de juego"}</span></div><div class="player-balance">${player.saldo} <small>CRC</small></div><p>${escapeHtml(player.posicionNombre || "Sin posición")}</p><div class="property-tags">${(player.propiedades || []).map((property) => `<span>${escapeHtml(property)}</span>`).join("") || "Sin propiedades"}</div></article>`).join("");
}

// Función para bloquear los botones
function bloquearAcciones() {
        document.querySelectorAll("#actionArea button").forEach((button) => {
                if (button.dataset.action !== "desbloquear") {
                        button.disabled = true;
                }
        });
}

// Función para mostrar las acciones
function mostrarAcciones(type) {
        const area = $("#actionArea");
        area.innerHTML = "";

        if (type === "turno") {
                if (Juego.idJugadorDesbloqueado !== Juego.idJugadorActual) {
                        area.append(botonAccion("Verificar acceso", "desbloquear", "primary-button"));

                        $("#gameNotice").textContent = "Verifica tu tarjeta una vez para habilitar el turno.";

                } else {

                        area.append(botonAccion("Lanzar dados", "dado", "primary-button"), botonAccion("Vender propiedad", "venta", "secondary-button"));
                }

        } else if (type === "compra") {

                area.append(botonAccion("Comprar", "comprar", "primary-button"), botonAccion("Pasar", "rechazar", "secondary-button"));

        } else if (type === "venta") {
                area.append(botonAccion("Solicitar venta", "venta", "primary-button"));
        } else {

                area.append(botonAccion("Continuar", "continuar", "primary-button"));
        }

        document.querySelectorAll("#actionArea button").forEach((button) => {
                button.disabled = false;
        });
}

// Función para mostrar la compra
function mostrarCompra(content) {
        if (!content.propiedad) return;
        $("#activePlayerDetails").textContent = `${content.propiedad} · ${content.precio} CRC`;
        mostrarAcciones("compra");
}

function abrirModalVentaPropiedades(propiedades) {
        const container = $("#propertySaleList");
        container.innerHTML = "";

        if (!Array.isArray(propiedades) || propiedades.length === 0) {
                mostrarMensaje("message", "Venta de propiedad", "No tienes propiedades para vender.", true);
                return;
        }

        propiedades.forEach((propiedad) => {
                const button = document.createElement("button");
                button.className = "secondary-button";
                button.textContent = `${propiedad.nombre} · ${propiedad.precio} CRC`;
                button.addEventListener("click", () => {
                        $("#propertySaleModal").classList.add("hidden");
                        enviarMensaje(Protocolo.AccionJugador, {
                                id: Juego.idJugadorActual,
                                accion: "venta",
                                valor: String(propiedad.id ?? propiedad.nombre)
                        });
                });
                container.append(button);
        });

        $("#propertySaleModal").classList.remove("hidden");
}

function cerrarModalVentaPropiedades() {
        $("#propertySaleModal").classList.add("hidden");
}

// Función para crear botones
function botonAccion(label, action, className) {
        const button = document.createElement("button");

        button.className = className;
        button.dataset.action = action;
        button.textContent = label;

        button.addEventListener("click", () => {
                if (action === "venta") {
                        enviarMensaje(Protocolo.AccionJugador, { id: Juego.idJugadorActual, accion: action, valor: action });
                        button.disabled = true;
                        return;
                }

                if (action === "desbloquear") abrirDesbloqueo();

                if (action === "dado") mostrarAnimacionDados();

                if (action === "continuar") {
                        enviarMensaje(Protocolo.AccionJugador, { id: Juego.idJugadorActual, accion: action, valor: action });
                        return;
                }

                enviarMensaje(Protocolo.AccionJugador, { id: Juego.idJugadorActual, accion: action, valor: action });
                if (action !== "continuar") button.disabled = true;
        });
        return button;
}

// Función para solicitar las transacciones
function solicitarTransacciones() {
        abrirMenuTransacciones();
}

// Función para mostrar la animación de los dados
function mostrarAnimacionDados() {
        const popup = $("#eventPopup");
        const label = $("#eventPopupLabel");
        const text = $("#eventPopupText");

        Juego.dadosAnimando = true;
        popup.onclick = null;

        label.textContent = "Lanzando dados";

        popup.classList.remove("hidden");
        bloquearAcciones();

        // Render de dados cambiando
        const render = () => {
                const dado1 = Math.floor(Math.random() * 6) + 1;
                const dado2 = Math.floor(Math.random() * 6) + 1;

                text.innerHTML = `<div class="dice-row"><span class="dice-box">${dado1}</span><span class="dice-box">${dado2}</span></div><div class="dice-pending"># #</div>`;
        };

        clearInterval(mostrarAnimacionDados.timer);
        render();
        mostrarAnimacionDados.timer = setInterval(render, 120);
}

// Función para mostrar el resultado de los dados
function mostrarResultadoDados(content) {
        clearInterval(mostrarAnimacionDados.timer);

        Juego.dadosAnimando = false;

        const popup = $("#eventPopup");
        const label = $("#eventPopupLabel");
        const text = $("#eventPopupText");

        label.textContent = "Resultado de dados";

        text.innerHTML = `<div class="dice-row"><span class="dice-box gold">${content.dado1}</span><span class="dice-box gold">${content.dado2}</span></div><div class="dice-result">${content.dado1} + ${content.dado2} = ${content.resultado}</div>`;

        popup.classList.remove("hidden");

        popup.onclick = () => {
                popup.classList.add("hidden");
        };

        clearTimeout(mostrarResultadoDados.timer);

        mostrarResultadoDados.timer = setTimeout(() => popup.classList.add("hidden"), 3200);
}

// Función para convertir las transacciones
function parsearTransaccion(item) {
        if (!item) return null;

        const base = item.transaccion ?? item;

        if (!base || typeof base !== "object") return null;

        const claves = Object.keys(base).filter((key) => base[key] !== undefined && base[key] !== null && base[key] !== "");

        if (claves.length === 0) return null;
        return {
                id: base.id ?? base.ID ?? 0,
                fechaHora: base.fechaHora ?? base.FechaHora ?? new Date().toISOString(),
                numeroTurno: base.numeroTurno ?? base.NumeroTurno ?? 0,
                tipo: base.tipo ?? base.Tipo ?? "Transaccion",
                jugadorOrigen: base.jugadorOrigen ?? base.JugadorOrigen ?? "Banco",
                jugadorDestino: base.jugadorDestino ?? base.JugadorDestino ?? "Banco",
                monto: base.monto ?? base.Monto ?? 0,
                descripcion: base.descripcion ?? base.Descripcion ?? "Sin descripción"
        };
}

// Función para formatear las transacciones
function formatearTransaccion(item) {
        const transaccion = parsearTransaccion(item);
        if (!transaccion) return "No hay transacciones registradas.";
        return `---------------------------------\nFecha: ${new Date(transaccion.fechaHora).toLocaleString()}\nMovimiento: ${transaccion.jugadorOrigen || "Banco"} → ${transaccion.jugadorDestino || "Banco"}\nTipo: ${transaccion.tipo}\nTotal: ${transaccion.monto} CRC\nConcepto: ${transaccion.descripcion}\n---------------------------------`;
}

// Función para mostrar una transacción
function mostrarTransaccion(transaccion, titulo = "Última transacción") {
        const item = parsearTransaccion(transaccion);

        $("#lastTransactionModal h2").textContent = titulo;

        if (!item) {
                $("#lastTransactionText").textContent = "No hay transacciones registradas.";

                $("#lastTransactionModal").classList.remove("hidden");
                return;
        }

        $("#lastTransactionText").textContent = formatearTransaccion(item);

        $("#lastTransactionModal").classList.remove("hidden");
}

// Función para mostrar el historial de transacciones
function mostrarHistorialTransacciones(transacciones, titulo = "Transacciones") {
        const items = (transacciones || []).map((item) => parsearTransaccion(item)).filter(Boolean);

        const contenido = items.length
                ? items.map((item) => formatearTransaccion(item)).join("\n\n")
                : "No hay transacciones registradas.";

        $("#transactionHistoryModal h2").textContent = titulo;
        $("#transactionHistoryText").textContent = contenido;
        $("#transactionHistoryModal").classList.remove("hidden");
}

// Función para mostrar la última transacción
function mostrarUltimaTransaccion(content) {
        const transaccion = parsearTransaccion(content);
        mostrarTransaccion(transaccion, "Última transacción");
}

// Función para ocultar el historial de transacciones
function ocultarHistorialTransacciones() {
        $("#transactionHistoryModal").classList.add("hidden");
}

// Función para ocultar la última transacción
function ocultarUltimaTransaccion() {
        $("#lastTransactionModal").classList.add("hidden");
}

// Función para abrir el menu de transacciones
function abrirMenuTransacciones() {
        $("#transactionMenuModal").classList.remove("hidden");
}

// Función para cerrar el menu de transacciones
function cerrarMenuTransacciones() {
        $("#transactionMenuModal").classList.add("hidden");
}

// Función para consultar la última transacción
function consultarUltimaTransaccion() {
        cerrarMenuTransacciones();
        enviarMensaje(Protocolo.UltimaTransaccion);
}

// Función para consultar la primer transacción
function consultarPrimeraTransaccion() {
        cerrarMenuTransacciones();
        enviarMensaje(Protocolo.PrimeraTransaccion);
}

// Función para consultar las transacciones por jugador
function consultarTransaccionesPorJugador() {
        cerrarMenuTransacciones();
        const jugadores = Array.from({ length: 4 }, (_, index) => {
                const jugador = Juego.jugadores.find((item) => item.id === index);

                return jugador ? jugador.nombre : `Jugador ${index + 1}`;
        });

        const container = $("#playerSelectorList");

        container.innerHTML = jugadores.map((nombre, index) => `<button class="secondary-button" data-jugador="${escapeHtml(nombre)}">${escapeHtml(nombre)}</button>`).join("");

        $("#playerSelectorModal").classList.remove("hidden");
        container.querySelectorAll("button[data-jugador]").forEach((button) => {
                button.addEventListener("click", () => {
                        $("#playerSelectorModal").classList.add("hidden");
                        enviarMensaje(Protocolo.TransaccionesPorJugador, { jugador: button.dataset.jugador });
                });
        });
}

// Función para consultar las transacciones por tipo
function consultarTransaccionesPorTipo() {
        cerrarMenuTransacciones();
        $("#typeSelectorModal").classList.remove("hidden");
}

// Función para consultar todas las transacciones
function consultarTodasTransacciones() {
        cerrarMenuTransacciones();
        enviarMensaje(Protocolo.Transacciones);
}

// Función para mostrar los resultados
function mostrarResultados(content) {
        cambiarSeccion("results");

        const ganadorNombre = content.ganador || (Number.isFinite(Number(content.ganadorId)) ? obtenerNombreJugadorPorId(content.ganadorId) : "Nadie");
        const textoGanador = ganadorNombre && ganadorNombre !== "Nadie" ? `Ganador: ${ganadorNombre}` : "Tenemos un ganador";

        $("#winnerName").textContent = textoGanador;

        $("#winnerMessage").textContent = "La partida terminó. Estas son las posiciones.";

        $("#resultsList").innerHTML = (content.jugadores || []).map((player, index) => `<div class="result-row"><span class="rank">0${index + 1}</span><strong>${escapeHtml(player.jugador)}</strong><span>${player.saldo} CRC</span></div>`).join("");
}

// Función para cerrar el juego
function cerrarJuego() {
        if (Juego.cierreEnCurso) return;

        Juego.cierreEnCurso = true;

        window.onerror = null;
        window.onunhandledrejection = null;

        $("#shutdownOverlay").classList.remove("hidden");
        Object.values(secciones).forEach((section) => section.classList.add("hidden"));
        $("#statusModal").classList.add("hidden");
        $("#crashModal").classList.add("hidden");
        $("#authModal").classList.add("hidden");
        $("#transactionHistoryModal").classList.add("hidden");
        $("#lastTransactionModal").classList.add("hidden");
        $("#eventPopup").classList.add("hidden");
        $("#shutdownOverlay .shutdown-card h2").textContent = "Juego terminado.";

        // Enviar cierre
        try {
                enviarMensaje(Protocolo.CerrarJuego, {});
                if (socket.readyState === WebSocket.OPEN || socket.readyState === WebSocket.CONNECTING) {
                        socket.close();
                }
        } catch (_error) {
                console.warn("No se pudo enviar cierre al servidor.");
        }
}

// Función para parsear HTML
function escapeHtml(value) {
        return String(value ?? "").replace(/[&<>'"]/g, (character) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", "'": "&#39;", '"': "&quot;" }[character]));
}

// Asignación de eventos
$("#statusModal").addEventListener("click", (event) => {
        if (event.target === $("#statusModal")) ocultarMensaje();
});
$("#lastTransactionModal").addEventListener("click", (event) => {
        if (event.target === $("#lastTransactionModal")) ocultarUltimaTransaccion();
});
$("#transactionHistoryModal").addEventListener("click", (event) => {
        if (event.target === $("#transactionHistoryModal")) ocultarHistorialTransacciones();
});
$("#closeModal").addEventListener("click", cerrarModal);
$("#statusOk").addEventListener("click", ocultarMensaje);
$("#submitName").addEventListener("click", subirNombre);
$("#playerName").addEventListener("keydown", (event) => { if (event.key === "Enter") subirNombre(); });
$("#startGameButton").addEventListener("click", () => {
        enviarMensaje(Protocolo.IniciarJuego);
});
$("#transactionHistoryButton").addEventListener("click", solicitarTransacciones);
$("#latestTransactionButton").addEventListener("click", consultarUltimaTransaccion);
$("#firstTransactionButton").addEventListener("click", consultarPrimeraTransaccion);
$("#playerTransactionButton").addEventListener("click", consultarTransaccionesPorJugador);
$("#typeTransactionButton").addEventListener("click", consultarTransaccionesPorTipo);
$("#allTransactionsButton").addEventListener("click", consultarTodasTransacciones);
$("#transactionMenuClose").addEventListener("click", cerrarMenuTransacciones);
$("#playerSelectorClose").addEventListener("click", () => $("#playerSelectorModal").classList.add("hidden"));
$("#typeSelectorClose").addEventListener("click", () => $("#typeSelectorModal").classList.add("hidden"));
$("#propertySaleClose").addEventListener("click", cerrarModalVentaPropiedades);
$("#transactionHistoryClose").addEventListener("click", ocultarHistorialTransacciones);
$("#lastTransactionClose").addEventListener("click", ocultarUltimaTransaccion);
$("#endGameButton").addEventListener("click", () => cerrarJuego());
Array.from(document.querySelectorAll("[data-tipo]")).forEach((button) => {
        button.addEventListener("click", () => {
                $("#typeSelectorModal").classList.add("hidden");
                enviarMensaje(Protocolo.TransaccionesPorTipo, { tipo: button.dataset.tipo });
        });
});
$("#eventPopup").addEventListener("click", () => {
        if (Juego.dadosAnimando) return;
        $("#eventPopup").classList.add("hidden");
        clearTimeout(mostrarEvento.timer);
        clearTimeout(mostrarResultadoDados.timer);
        clearInterval(mostrarAnimacionDados.timer);
});

// Actualizar el lobby al inicio
actualizarLobby();
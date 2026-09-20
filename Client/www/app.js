// Nota: falta comentar y acomodar

const Juego = {
        protocolo: null,
        tablero: [],
        jugadores: [],
        idJugadorActual: null,
        turno: 0,
        modalJugadorId: null,
        modoModal: "register",
        propiedadesPendientes: [],
        idJugadorDesbloqueado: null,
        modoPropiedad: null,
        colaEventos: [],
        mostrandoEvento: false,
        cierreEnCurso: false
};

const Tablero = [
        [1, "Salida", "especial"], [2, "Loteria 1", "evento"], [3, "Pococí", "propiedad", 200], [4, "Guácimo", "propiedad", 250],
        [5, "San Carlos", "propiedad", 300], [6, "Zarcero", "propiedad", 350], [7, "Carcel - San Lucas", "especial"], [8, "Heredia", "propiedad", 400],
        [9, "Sarapiquí", "propiedad", 450], [10, "Quepos", "propiedad", 500], [11, "Loteria 2", "evento"], [12, "Golfito", "propiedad", 550],
        [13, "Parque de Diversiones", "especial"], [14, "Liberia", "propiedad", 600], [15, "Nicoya", "propiedad", 650], [16, "Loteria 3", "evento"],
        [17, "Cartago", "propiedad", 700], [18, "Turrialba", "propiedad", 750], [19, "Vaya a la Carcel - La Cali", "especial"], [20, "Desamparados", "propiedad", 800],
        [21, "Perez Zeledón", "propiedad", 850], [22, "Loteria 4", "evento"], [23, "Chepe", "propiedad", 800], [24, "Escazú", "propiedad", 850]
].map(([id, nombre, tipo, precio]) => ({ id, nombre, tipo, precio, propietario: null }));
Juego.tablero = Tablero;

const socket = new WebSocket("ws://localhost:8080/");
const $ = (selector) => document.querySelector(selector);

const Protocol = Object.freeze({
        EnviarProtocolo: "SOCKET_PROTOCOLO",
        ConexionLista: "SOCKET_CONEXION_LISTA",
        AutenticarJugador: "SOCKET_AUTENTICAR_JUGADOR",
        VerificarJugador: "SOCKET_VERIFICAR_JUGADOR",
        IniciarJuego: "SOCKET_INICIAR_JUEGO",
        JuegoComenzado: "SOCKET_JUEGO_COMENZADO",
        TurnoIniciado: "SOCKET_TURNO_INICIADO",
        SolicitarAccionTurno: "SOCKET_SOLICITAR_ACCION_TURNO",
        JugadorEnCarcel: "SOCKET_JUGADOR_EN_CARCEL",
        JugadorSaleCarcel: "SOCKET_JUGADOR_SALE_CARCEL",
        TurnosCarcel: "SOCKET_TURNOS_CARCEL",
        TurnoPerdido: "SOCKET_TURNO_PERDIDO",
        DadosLanzados: "SOCKET_DADOS_LANZADOS",
        JugadorMovido: "SOCKET_JUGADOR_MOVIDO",
        SolicitarContinuar: "SOCKET_SOLICITAR_CONTINUAR",
        OpcionInvalida: "SOCKET_OPCION_INVALIDA",
        FinTurno: "SOCKET_FIN_TURNO",
        JugadorEliminado: "SOCKET_JUGADOR_ELIMINADO",
        Ganador: "SOCKET_GANADOR",
        CompraPropiedad: "SOCKET_COMPRA_PROPIEDAD",
        PropiedadComprada: "SOCKET_PROPIEDAD_COMPRADA",
        CompraRechazada: "SOCKET_COMPRA_RECHAZADA",
        CompraNoPermitida: "SOCKET_COMPRA_NO_PERMITIDA",
        AlquilerPagado: "SOCKET_ALQUILER_PAGADO",
        Bancarrota: "SOCKET_BANCARROTA",
        CartaEvento: "SOCKET_CARTA_EVENTO",
        JugadorEntraCarcel: "SOCKET_JUGADOR_ENTRA_CARCEL",
        OpcionCompraInvalida: "SOCKET_OPCION_COMPRA_INVALIDA",
        PropiedadesVenta: "SOCKET_PROPIEDADES_VENTA",
        SolicitarVenta: "SOCKET_SOLICITAR_VENTA",
        SolicitarPropiedades: "SOCKET_SOLICITAR_PROPIEDADES",
        PropiedadVendida: "SOCKET_PROPIEDAD_VENDIDA",
        PremioSalida: "SOCKET_PREMIO_SALIDA",
        AccionJugador: "SOCKET_ACCION_JUGADOR",
        ErrorAccion: "SOCKET_ERROR_ACCION",
        JuegoTerminado: "SOCKET_JUEGO_TERMINADO",
        JugadorRegistrado: "SOCKET_JUGADOR_REGISTRADO",
        JugadorDesbloqueado: "SOCKET_JUGADOR_DESBLOQUEADO",
        SolicitarPagoRFID: "SOCKET_SOLICITAR_PAGO_RFID",
        TransaccionRegistrada: "SOCKET_TRANSACCION_REGISTRADA",
        SolicitarUltimaTransaccion: "SOCKET_SOLICITAR_ULTIMA_TRANSACCION",
        UltimaTransaccion: "SOCKET_ULTIMA_TRANSACCION",
        SolicitarTransacciones: "SOCKET_SOLICITAR_TRANSACCIONES",
        TransaccionesLista: "SOCKET_TRANSACCIONES_LISTA",
        CerrarJuego: "SOCKET_CERRAR_JUEGO"
});

const colores = ["coral", "mint", "gold", "sky"];
const secciones = { lobby: $("#lobbySection"), game: $("#gameSection"), results: $("#resultsSection") };

socket.addEventListener("open", () => { console.log("[App] WebSocket conectado"); setConnection(true); });
socket.addEventListener("close", () => {
        if (Juego.cierreEnCurso) return;
        console.warn("[App] WebSocket cerrado");
        mostrarFalloFatal("El cliente perdió la conexión con el servidor.");
        cerrarJuego();
});
socket.addEventListener("error", (error) => {
        if (Juego.cierreEnCurso) return;
        console.error("[App] WebSocket error", error);
        mostrarFalloFatal("No se pudo comunicar con el servidor.");
        cerrarJuego();
});
socket.addEventListener("message", (event) => {
        try {
                const message = JSON.parse(event.data);
                if (!message || !message.Comando) return;
                let content = message.Contenido;
                if (typeof content === "string") content = JSON.parse(content);
                console.log("[App] App <<< Servidor:", { Comando: message.Comando, Contenido: content });
                if (message.Comando === Protocol.EnviarProtocolo && Juego.protocolo) {
                        cerrarJuego();
                        return;
                }
                recibirMensaje(message.Comando, content || {});
        } catch (error) {
                console.error("[App] Mensaje inválido", error);
        }
});

function setConnection(connected) {
        $("#connectionDot").classList.toggle("offline", !connected);
        $("#connectionText").textContent = connected ? "Cliente conectado" : "Cliente desconectado";
        $("#serverStatus").textContent = connected ? "Conectado" : "Desconectado";
}

function mostrarFalloFatal(message) {
        if (Juego.cierreEnCurso) return;
        setConnection(false);
        $("#crashMessage").textContent = message;
        $("#crashModal").classList.remove("hidden");
        $("#authModal").classList.add("hidden");
        $("#actionArea").innerHTML = "";
}

function obtenerValorProtocolo(key) {
        return Juego.protocolo?.[key] ?? key;
}

function resolverComando(command) {
        return Object.keys(Juego.protocolo || {}).find((key) => Juego.protocolo[key] === command) || command;
}

function enviarMensaje(commandName, content = {}) {
        const command = obtenerValorProtocolo(commandName);
        if (socket.readyState !== WebSocket.OPEN || !command) return false;
        const message = { Comando: command, Contenido: content };
        console.log("[App] App >>> Servidor:", message);
        socket.send(JSON.stringify(message));
        return true;
}

function mostrarSeccion(name) {
        Object.values(secciones).forEach((section) => section.classList.add("hidden"));
        secciones[name].classList.remove("hidden");
}

function abrirRegistro(id) {
        Juego.modalJugadorId = id;
        Juego.modoModal = "register";
        $("#modalEyebrow").textContent = `Asiento ${id + 1}`;
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

function abrirVerificador(id) {
        Juego.modalJugadorId = id;
        Juego.modoModal = "verify";
        $("#modalEyebrow").textContent = `Asiento ${id + 1}`;
        $("#modalTitle").textContent = "Verificar jugador";
        $("#rfidStep").classList.remove("hidden");
        $("#authModal").dataset.lockDismiss = "true";
        $("#authModal").classList.remove("hidden");
        $("#closeModal").classList.add("hidden");
        enviarMensaje("VerificarJugador", { id });
}

function abrirDesbloqueo() {
        $("#modalEyebrow").textContent = "Turno protegido";
        $("#modalTitle").textContent = "Desbloquear acciones";
        $("#nameStep").classList.add("hidden");
        $("#rfidStep").classList.remove("hidden");
        $("#rfidStatus").textContent = "Acerca la tarjeta del jugador activo al lector...";
        $("#authModal").dataset.lockDismiss = "true";
        $("#authModal").classList.remove("hidden");
        $("#closeModal").classList.add("hidden");
}

function cerrarModal() {
        $("#authModal").dataset.lockDismiss = "false";
        $("#authModal").classList.add("hidden");
        Juego.modalJugadorId = null;
}

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
        enviarMensaje("AutenticarJugador", { id: Juego.modalJugadorId, nombre: name });
}

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

function ocultarMensaje() {
        $("#statusModal").classList.add("hidden");
}

function recibirMensaje(command, content) {
        const comandoActual = resolverComando(command);
        if (comandoActual === "SOCKET_PROTOCOLO") {
                Juego.protocolo = content;
                enviarMensaje("ConexionLista");
                mostrarLobby();
                return;
        }

        switch (comandoActual) {
                case "AutenticarJugador":
                        if (content.id === -1) {
                                cerrarModal();
                                mostrarMensaje("error", "Jugador no registrado", content.error || "No se pudo autenticar.");
                                return;
                        }
                        break;

                case "JugadorRegistrado":
                        insertarJugador({ id: content.id, nombre: content.nombre, saldo: content.saldo, posicion: content.posicion, posicionNombre: content.posicionNombre, activo: true, propiedades: [] });
                        cerrarModal();
                        mostrarMensaje("success", "Jugador conectado", `${content.nombre} fue registrado correctamente.`);
                        mostrarLobby();
                        break;

                case "JugadorDesbloqueado":
                        Juego.idJugadorDesbloqueado = content.jugadorId;
                        cerrarModal();
                        $("#gameNotice").textContent = "Turno desbloqueado. Ya puedes actuar.";
                        mostrarAcciones("turno");
                        mostrarMensaje("success", "Turno desbloqueado", "Ya puedes lanzar los dados y actuar.");
                        break;

                case "SolicitarPagoRFID":
                        $("#modalEyebrow").textContent = "Pago protegido";
                        $("#modalTitle").textContent = "Confirma el pago";
                        $("#nameStep").classList.add("hidden");
                        $("#rfidStep").classList.remove("hidden");
                        $("#rfidStatus").textContent = `${content.concepto}: acerca la tarjeta de ${content.jugador} al lector.`;
                        $("#authModal").dataset.lockDismiss = "true";
                        $("#authModal").classList.remove("hidden");
                        $("#closeModal").classList.add("hidden");
                        break;

                case "VerificarJugador":
                        if (content.error) {
                                cerrarModal();
                                mostrarMensaje("error", "Verificación fallida", content.error);
                        } else {
                                cerrarModal();
                                mostrarMensaje("success", "Jugador verificado", "La tarjeta RFID fue aceptada.");
                        }
                        break;

                case "JuegoComenzado":
                        mostrarEvento("Partida", content.mensaje || "La partida ha comenzado.", 2600);
                        mostrarSeccion("game");
                        break;

                case "IniciarJuego":
                        if (content.error) mostrarMensaje("error", "No se puede iniciar", content.error, true);
                        else mostrarSeccion("game");
                        break;

                case "SolicitarAccionTurno":
                        Juego.idJugadorActual = content.id ?? Juego.idJugadorActual;
                        mostrarJuego();
                        mostrarAcciones(content.tipo || "turno");
                        break;

                case "DadosLanzados":
                        mostrarAcciones("turno");
                        mostrarResultadoDados(content);
                        break;

                case "SolicitarPropiedades":
                        Juego.modoPropiedad = "view";
                        Juego.propiedadesPendientes = [];
                        mostrarPropiedades();
                        break;

                case "TurnoIniciado":
                        Juego.idJugadorActual = content.jugadorId ?? Juego.idJugadorActual;
                        Juego.turno = content.turno ?? Juego.turno;
                        Juego.idJugadorDesbloqueado = null;
                        mostrarJuego();
                        break;

                case "JugadorMovido":
                        Juego.slotAnimadoId = content.posicion;
                        actualizarJugador(content.jugadorId, { posicion: content.posicion, posicionNombre: content.casilla });
                        mostrarJuego();
                        break;

                case "PropiedadComprada": {
                        cerrarModal();
                        const player = Juego.jugadores.find((item) => item.id === content.jugadorId);
                        if (player) {
                                player.saldo = content.saldo ?? player.saldo;
                                player.propiedades = [...(player.propiedades || []), content.propiedad];
                        }
                        const property = Juego.tablero.find((item) => item.nombre === content.propiedad);
                        if (property) property.propietario = content.jugadorId;
                        mostrarMensaje("success", "Propiedad comprada", `${content.jugador} compró ${content.propiedad}.`);
                        mostrarJuego();
                        break;
                }

                case "CompraRechazada":
                        mostrarMensaje("message", "Compra rechazada", `${content.jugador} decidió no comprar ${content.propiedad}.`);
                        break;

                case "CompraNoPermitida":
                        break;

                case "JugadorEntraCarcel":
                        if (content.origen === "carta") return;
                        mostrarEvento("Cárcel", `${content.jugador} fue enviado a la cárcel.`);
                        break;

                case "JugadorSaleCarcel":
                        mostrarEvento("Cárcel", `${content.jugador} sale de la cárcel.`);
                        break;

                case "TurnosCarcel":
                        mostrarEvento("Cárcel", `${content.jugador} tiene ${content.turnosRestantes} turno(s) restantes en prisión.`);
                        break;

                case "TurnoPerdido":
                        mostrarEvento("Turno perdido", `${content.jugador} pierde un turno.`);
                        break;

                case "TransaccionRegistrada":
                        break;

                case "UltimaTransaccion":
                        mostrarUltimaTransaccion(content.transaccion || content);
                        break;

                case "TransaccionesLista":
                        mostrarHistorialTransacciones(content.transacciones || []);
                        break;

                case "PropiedadVendida":
                        actualizarJugador(content.jugadorId, { saldo: content.saldo });
                        const jugadorVendido = Juego.jugadores.find((item) => item.id === content.jugadorId);
                        if (jugadorVendido) {
                                jugadorVendido.propiedades = (jugadorVendido.propiedades || []).filter((propiedad) => propiedad !== content.propiedad);
                        }
                        mostrarMensaje("success", "Propiedad vendida", `${content.jugador} vendió ${content.propiedad}.`);
                        mostrarJuego();
                        break;

                case "AlquilerPagado":
                        cerrarModal();
                        actualizarJugador(content.jugadorId, { saldo: content.saldo });
                        actualizarJugador(content.propietarioId, { saldo: content.saldoPropietario });
                        mostrarJuego();
                        break;

                case "CartaEvento":
                case "PremioSalida":
                        cerrarModal();
                        actualizarJugador(content.jugadorId, { saldo: content.saldo });
                        if (content.descripcion) mostrarMensaje("message", "Carta de evento", content.descripcion, true);
                        mostrarJuego();
                        break;

                case "Bancarrota":
                        cerrarModal();
                        mostrarMensaje("error", "Bancarrota", `${content.jugador} no pudo pagar y queda fuera de la partida.`);
                        break;

                case "FinTurno":
                        cerrarModal();
                        break;

                case "JugadorEliminado":
                        actualizarJugador(content.jugadorId, { activo: false });
                        mostrarJuego();
                        break;

                case "CompraPropiedad":
                        mostrarCompra(content);
                        break;

                case "SolicitarVenta":
                        Juego.modoPropiedad = "sell";
                        Juego.propiedadesPendientes = [];
                        mostrarAcciones("venta");
                        break;

                case "PropiedadesVenta":
                        if (content.propiedad) Juego.propiedadesPendientes.push({ index: content.indice, name: content.propiedad });
                        if (Juego.modoPropiedad === "view") mostrarPropiedades();
                        if (content.mensaje) {
                                mostrarMensaje("message", "Propiedades", content.mensaje);
                                mostrarAcciones("continuar");
                        }
                        break;

                case "SolicitarContinuar":
                        mostrarAcciones("continuar");
                        break;

                case "JuegoTerminado":
                        mostrarResultados(content);
                        break;

                case "Ganador":
                        mostrarMensaje("success", "Partida decidida", `Ganó ${content.jugador}.`);
                        break;

                case "ErrorAccion":
                case "OpcionInvalida":
                case "OpcionCompraInvalida":
                        mostrarMensaje("error", "Acción no disponible", content.mensaje || "Acción inválida.");
                        break;

                default:
                        if (content.mensaje || content.descripcion) mostrarMensaje("message", "Actualización", content.mensaje || content.descripcion, true);
                        break;
        }
}

function insertarJugador(player) {
        const existing = Juego.jugadores.findIndex((item) => item.id === player.id);
        if (existing >= 0) Juego.jugadores[existing] = { ...Juego.jugadores[existing], ...player };
        else Juego.jugadores.push(player);
}

function actualizarJugador(id, changes) {
        const player = Juego.jugadores.find((item) => item.id === id);
        if (player) Object.assign(player, changes);
}

function mostrarLobby() {
        const players = Juego.jugadores;
        const container = $("#lobbyPlayers");
        container.innerHTML = "";
        for (let id = 0; id < 4; id += 1) {
                const player = players.find((item) => item.id === id);
                const card = document.createElement("article");
                card.className = `lobby-seat ${colores[id]}`;
                card.innerHTML = `<span class="seat-number">0${id + 1}</span><div><p>${player ? escapeHtml(player.nombre) : "Asiento disponible"}</p><small>${player ? "Registrado" : "Listo para conectar"}</small></div>`;
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
        $("#lobbyHint").textContent = players.length === 4 ? "Todos los jugadores pueden iniciar la partida." : "Conecta los cuatro asientos para habilitar la partida.";
}

function mostrarJuego() {
        $("#turnNumber").textContent = Juego.turno;
        const active = Juego.jugadores.find((player) => player.id === Juego.idJugadorActual);
        $("#activePlayerName").textContent = active?.nombre || "Esperando turno";
        $("#activePlayerDetails").textContent = active ? `${active.saldo} CRC · ${active.posicionNombre || "En el tablero"}` : "Usa el lector RFID para desbloquear sus acciones.";
        $("#stateLabel").textContent = `${Juego.jugadores.length} jugadores sincronizados`;
        mostrarTablero();
        mostrarJugadores();
}

function mostrarTablero() {
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
                card.innerHTML = `<span class="slot-id">${String(slot.id).padStart(2, "0")}</span><h3>${escapeHtml(slot.nombre)}</h3>${slot.precio ? `<p>${slot.precio} CRC</p>` : "<p>Casilla especial</p>"}<div class="tokens">${playersHere.map((player) => `<span class="token ${colores[player.id] || "coral"}" title="${escapeHtml(player.nombre)}">${player.id + 1}</span>`).join("")}</div>`;
                board.append(card);
        });
        if (Juego.slotAnimadoId != null) {
                setTimeout(() => {
                        Juego.slotAnimadoId = null;
                        mostrarTablero();
                }, 420);
        }
}

function posicionTabla(index, size) {
        if (index < size) return { row: 1, column: index + 1 };
        if (index < size * 2 - 1) return { row: index - size + 2, column: size };
        if (index < size * 3 - 2) return { row: size, column: size - (index - (size * 2 - 2)) };
        return { row: size - (index - (size * 3 - 3)), column: 1 };
}

function mostrarJugadores() {
        const container = $("#playerCards");
        container.innerHTML = Juego.jugadores.map((player) => `<article class="player-card ${player.id === Juego.idJugadorActual ? "active" : ""} ${colores[player.id] || "coral"}"><div class="player-card-top"><span class="token">${player.id + 1}</span><strong>${escapeHtml(player.nombre)}</strong><span class="player-status">${player.activo ? "Activo" : "Fuera"}</span></div><div class="player-balance">${player.saldo} <small>CRC</small></div><p>${escapeHtml(player.posicionNombre || "Sin posición")}</p><div class="property-tags">${(player.propiedades || []).map((property) => `<span>${escapeHtml(property)}</span>`).join("") || "Sin propiedades"}</div></article>`).join("");
}

function mostrarAcciones(type) {
        const area = $("#actionArea");
        area.innerHTML = "";
        if (type === "turno") {
                if (Juego.idJugadorDesbloqueado !== Juego.idJugadorActual) {
                        area.append(botonAccion("Desbloquear turno", "desbloquear", "primary-button"));
                        $("#gameNotice").textContent = "Verifica tu tarjeta una vez para habilitar el turno.";
                } else {
                        area.append(botonAccion("Lanzar dados", "dice", "primary-button"), botonAccion("Vender propiedad", "sell", "secondary-button"));
                }
        } else if (type === "compra") {
                area.append(botonAccion("Comprar", "comprar", "primary-button"), botonAccion("Pasar", "rechazar", "secondary-button"));
        } else if (type === "venta") {
                const input = document.createElement("input");
                input.id = "saleIndex";
                input.type = "number";
                input.min = "1";
                input.placeholder = "Número de propiedad";
                area.append(input, botonAccion("Confirmar venta", "venta", "primary-button"));
        } else {
                area.append(botonAccion("Continuar", "continuar", "primary-button"));
        }
}

function mostrarPropiedades() {
        const area = $("#actionArea");
        area.innerHTML = "";
        const list = document.createElement("div");
        list.className = "property-list";
        list.innerHTML = Juego.propiedadesPendientes.length
                ? Juego.propiedadesPendientes.map((property) => `<span>${property.index}. ${escapeHtml(property.name)}</span>`).join("")
                : "<span>Sin propiedades adquiridas.</span>";
        area.append(list, botonAccion("Continuar", "continuar", "primary-button"));
}

function mostrarCompra(content) {
        if (!content.propiedad) return;
        $("#activePlayerDetails").textContent = `${content.propiedad} · ${content.precio} CRC`;
        mostrarAcciones("compra");
}

function botonAccion(label, action, className) {
        const button = document.createElement("button");
        button.className = className;
        button.textContent = label;
        button.addEventListener("click", () => {
                const value = action === "venta" ? $("#saleIndex")?.value : action;
                if (action === "venta" && !value) return mostrarMensaje("error", "Venta no disponible", "Indica el número de propiedad.");
                if (action === "desbloquear") abrirDesbloqueo();
                if (action === "dice") mostrarAnimacionDados();
                enviarMensaje("AccionJugador", { id: Juego.idJugadorActual, accion: action, valor: value || action });
                if (action !== "continuar") button.disabled = true;
        });
        return button;
}

function solicitarTransacciones() {
        enviarMensaje("SolicitarTransacciones");
}

function mostrarEvento(label, message, duration = 2800) {
        Juego.colaEventos.push({ label, message, duration });
        if (Juego.mostrandoEvento) return;

        const procesarSiguienteEvento = () => {
                const siguiente = Juego.colaEventos.shift();
                if (!siguiente) {
                        Juego.mostrandoEvento = false;
                        $("#eventPopup").classList.add("hidden");
                        $("#eventPopup").onclick = null;
                        return;
                }

                Juego.mostrandoEvento = true;
                $("#eventPopupLabel").textContent = siguiente.label;
                $("#eventPopupText").textContent = siguiente.message;
                $("#eventPopup").classList.remove("hidden");
                $("#eventPopup").onclick = () => {
                        clearTimeout(mostrarEvento.timer);
                        $("#eventPopup").classList.add("hidden");
                        procesarSiguienteEvento();
                };

                clearTimeout(mostrarEvento.timer);
                mostrarEvento.timer = setTimeout(() => {
                        $("#eventPopup").classList.add("hidden");
                        procesarSiguienteEvento();
                }, siguiente.duration);
        };

        procesarSiguienteEvento();
}

function mostrarAnimacionDados() {
        const popup = $("#eventPopup");
        const label = $("#eventPopupLabel");
        const text = $("#eventPopupText");

        label.textContent = "Lanzando dados";
        popup.classList.remove("hidden");
        popup.onclick = () => {
                clearInterval(mostrarAnimacionDados.timer);
                popup.classList.add("hidden");
        };

        const render = () => {
                const dado1 = Math.floor(Math.random() * 6) + 1;
                const dado2 = Math.floor(Math.random() * 6) + 1;
                text.innerHTML = `<div class="dice-row"><span class="dice-box">${dado1}</span><span class="dice-box">${dado2}</span></div><div class="dice-pending"># #</div>`;
        };

        clearInterval(mostrarAnimacionDados.timer);
        render();
        mostrarAnimacionDados.timer = setInterval(render, 120);
}

function mostrarResultadoDados(content) {
        clearInterval(mostrarAnimacionDados.timer);
        const popup = $("#eventPopup");
        const label = $("#eventPopupLabel");
        const text = $("#eventPopupText");

        label.textContent = "Resultado de dados";
        text.innerHTML = `<div class="dice-row"><span class="dice-box">${content.dado1}</span><span class="dice-box">${content.dado2}</span></div><div class="dice-result">${content.dado1} + ${content.dado2} = ${content.resultado}</div>`;
        popup.classList.remove("hidden");
        popup.onclick = () => popup.classList.add("hidden");

        clearTimeout(mostrarResultadoDados.timer);
        mostrarResultadoDados.timer = setTimeout(() => popup.classList.add("hidden"), 3200);
}

function formatearTransaccion(item) {
        return `${new Date(item.fechaHora).toLocaleString()}\n${item.jugadorOrigen || "Banco"} → ${item.jugadorDestino || "Banco"}\n${item.tipo}: ${item.monto} CRC\n${item.descripcion}`;
}

function mostrarHistorialTransacciones(transacciones) {
        const contenido = (transacciones || []).length
                ? transacciones.map((item) => formatearTransaccion(item)).join("\n\n")
                : "No hay transacciones registradas todavía.";

        $("#transactionHistoryText").textContent = contenido;
        $("#transactionHistoryModal").classList.remove("hidden");
}

function mostrarUltimaTransaccion(content) {
        const transaccion = {
                fechaHora: content.fechaHora || new Date().toISOString(),
                jugadorOrigen: content.jugadorOrigen || "Banco",
                jugadorDestino: content.jugadorDestino || "Banco",
                tipo: content.tipo || "Transaccion",
                monto: content.monto ?? 0,
                descripcion: content.descripcion || "Sin descripción"
        };

        $("#lastTransactionText").textContent = formatearTransaccion(transaccion);
        $("#lastTransactionModal").classList.remove("hidden");
}

function ocultarHistorialTransacciones() {
        $("#transactionHistoryModal").classList.add("hidden");
}

function ocultarUltimaTransaccion() {
        $("#lastTransactionModal").classList.add("hidden");
}

function mostrarResultados(content) {
        mostrarSeccion("results");
        $("#winnerName").textContent = content.ganador || "Tenemos un ganador";
        $("#winnerMessage").textContent = "La partida terminó. Este es el resultado final de la mesa.";
        $("#resultsList").innerHTML = (content.jugadores || []).map((player, index) => `<div class="result-row"><span class="rank">0${index + 1}</span><strong>${escapeHtml(player.jugador)}</strong><span>${player.saldo} CRC</span></div>`).join("");
}

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

        try {
                if (socket.readyState === WebSocket.OPEN || socket.readyState === WebSocket.CONNECTING) {
                        socket.close();
                }
                enviarMensaje("CerrarJuego", { motivo: "fin_partida" });
        } catch (_error) {
                console.warn("No se pudo enviar cierre al servidor.");
        }

        setTimeout(() => {
                try {
                        window.close();
                } catch (_error) {
                        console.warn("No se pudo cerrar la ventana del cliente.");
                }
        }, 250);
}

function popup(message, error) {
        $("#toast").classList.toggle("error", Boolean(error));
        $("#toastText").textContent = message;
        $("#toast").classList.remove("hidden");
        clearTimeout(popup.timer);
        popup.timer = setTimeout(() => $("#toast").classList.add("hidden"), 3200);
}

function escapeHtml(value) {
        return String(value ?? "").replace(/[&<>'"]/g, (character) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", "'": "&#39;", '"': "&quot;" }[character]));
}

$("#statusModal").addEventListener("click", (event) => {
        if (event.target === $("#statusModal")) ocultarMensaje();
});
$("#closeModal").addEventListener("click", cerrarModal);
$("#statusOk").addEventListener("click", ocultarMensaje);
$("#submitName").addEventListener("click", subirNombre);
$("#playerName").addEventListener("keydown", (event) => { if (event.key === "Enter") subirNombre(); });
$("#startGameButton").addEventListener("click", () => {
        enviarMensaje("IniciarJuego");
});
$("#transactionHistoryButton").addEventListener("click", solicitarTransacciones);
$("#transactionHistoryClose").addEventListener("click", ocultarHistorialTransacciones);
$("#lastTransactionClose").addEventListener("click", ocultarUltimaTransaccion);
$("#endGameButton").addEventListener("click", () => cerrarJuego());
$("#eventPopup").addEventListener("click", () => {
        $("#eventPopup").classList.add("hidden");
        clearTimeout(mostrarEvento.timer);
        clearTimeout(mostrarResultadoDados.timer);
        clearInterval(mostrarAnimacionDados.timer);
});

mostrarLobby();
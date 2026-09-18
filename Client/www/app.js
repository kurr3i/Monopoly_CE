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
        modoPropiedad: null
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

const colores = ["coral", "mint", "gold", "sky"];
const secciones = { lobby: $("#lobbySection"), game: $("#gameSection"), results: $("#resultsSection") };

socket.addEventListener("open", () => { console.log("[App] WebSocket conectado"); setConnection(true); });
socket.addEventListener("close", () => { console.warn("[App] WebSocket cerrado"); mostrarFalloFatal("El cliente perdió la conexión con el servidor."); });
socket.addEventListener("error", (error) => { console.error("[App] WebSocket error", error); mostrarFalloFatal("No se pudo comunicar con el servidor."); });
socket.addEventListener("message", (event) => {
        try {
                const message = JSON.parse(event.data);
                if (!message || !message.Comando) return;
                let content = message.Contenido;
                if (typeof content === "string") content = JSON.parse(content);
                console.log("[App] App <<< Servidor:", { Comando: message.Comando, Contenido: content });
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
        setConnection(false);
        $("#crashMessage").textContent = message;
        $("#crashModal").classList.remove("hidden");
        $("#authModal").classList.add("hidden");
        $("#actionArea").innerHTML = "";
}

function enviarMensaje(commandName, content = {}) {
        const command = Juego.protocolo?.[commandName] || commandName;
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
        $("#authModal").classList.remove("hidden");
        $("#closeModal").classList.add("hidden");
}

function cerrarModal() {
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
        if (command === "SOCKET_PROTOCOLO") {
                Juego.protocolo = content;
                enviarMensaje("ConexionLista");
                mostrarLobby();
                return;
        }

        const commandName = Object.keys(Juego.protocolo || {}).find((key) => Juego.protocolo[key] === command) || command;
        switch (commandName) {

                // Nota: cambiar casos a protocolos
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

                case "IniciarJuego":
                        if (content.error) mostrarMensaje("error", "No se puede iniciar", content.error);
                        else mostrarSeccion("game");
                        break;

                case "SolicitarAccionTurno":
                        Juego.idJugadorActual = content.id ?? Juego.idJugadorActual;
                        mostrarJuego();
                        mostrarAcciones(content.tipo || "turno");
                        break;

                case "DadosLanzados":
                        mostrarAcciones("turno");
                        mostrarEvento("Resultado de dados", `${content.jugador}: ${content.dado1} + ${content.dado2} = ${content.resultado}`);
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
                        mostrarMensaje("error", "Compra no disponible", `${content.jugador} no puede comprar ${content.propiedad}.`);
                        break;

                case "PropiedadVendida":
                        actualizarJugador(content.jugadorId, { saldo: content.saldo });
                        mostrarMensaje("success", "Propiedad vendida", `${content.jugador} vendió ${content.propiedad}.`);
                        mostrarJuego();
                        break;

                case "AlquilerPagado":
                        cerrarModal();
                        actualizarJugador(content.jugadorId, { saldo: content.saldo });
                        actualizarJugador(content.propietarioId, { saldo: content.saldoPropietario });
                        mostrarMensaje("success", "Alquiler pagado", `${content.jugador} pagó ${content.monto} CRC a ${content.propietario}.`);
                        mostrarJuego();
                        break;

                case "CartaEvento":
                case "PremioSalida":
                        cerrarModal();
                        actualizarJugador(content.jugadorId, { saldo: content.saldo });
                        if (content.descripcion) mostrarMensaje("message", "Carta de evento", content.descripcion, true, 4000);
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
                        if (content.mensaje || content.descripcion) mostrarMensaje("message", "Actualización", content.mensaje || content.descripcion);
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
                card.className = `slot ${slot.tipo} ${playersHere.length ? "occupied" : ""}`;
                const position = posicionTabla(index, 7);
                card.style.gridColumn = position.column;
                card.style.gridRow = position.row;
                card.innerHTML = `<span class="slot-id">${String(slot.id).padStart(2, "0")}</span><h3>${escapeHtml(slot.nombre)}</h3>${slot.precio ? `<p>${slot.precio} CRC</p>` : "<p>Casilla especial</p>"}<div class="tokens">${playersHere.map((player) => `<span class="token ${colores[player.id] || "coral"}" title="${escapeHtml(player.nombre)}">${player.id + 1}</span>`).join("")}</div>`;
                board.append(card);
        });
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
                if (action === "dice") mostrarEvento("Lanzando dados", "El resultado está por llegar...");
                enviarMensaje("AccionJugador", { id: Juego.idJugadorActual, accion: action, valor: value || action });
                if (action !== "continuar") button.disabled = true;
        });
        return button;
}

function mostrarEvento(label, message, duration = 2500) {
        $("#eventPopupLabel").textContent = label;
        $("#eventPopupText").textContent = message;
        $("#eventPopup").classList.remove("hidden");
        clearTimeout(mostrarEvento.timer);
        mostrarEvento.timer = setTimeout(() => $("#eventPopup").classList.add("hidden"), duration);
}

function mostrarResultados(content) {
        mostrarSeccion("results");
        $("#winnerName").textContent = content.ganador || "Tenemos un ganador";
        $("#winnerMessage").textContent = "La partida terminó. Este es el resultado final de la mesa.";
        $("#resultsList").innerHTML = (content.jugadores || []).map((player, index) => `<div class="result-row"><span class="rank">0${index + 1}</span><strong>${escapeHtml(player.jugador)}</strong><span>${player.saldo} CRC</span></div>`).join("");
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

$("#closeModal").addEventListener("click", cerrarModal);
$("#statusOk").addEventListener("click", ocultarMensaje);
$("#submitName").addEventListener("click", subirNombre);
$("#playerName").addEventListener("keydown", (event) => { if (event.key === "Enter") subirNombre(); });
$("#startGameButton").addEventListener("click", () => enviarMensaje("IniciarJuego"));

mostrarLobby();
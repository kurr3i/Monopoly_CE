// Definiciones de elementos
const dialogCont = document.querySelector(".dialogVerif-container");
const registroVerif = document.querySelector(".registroVerif");
const dialogVerif = document.querySelector(".dialogVerif");
const inputVerif = document.getElementById("inputVerif");
const inputVerifBtn = document.getElementById("inputBtnVerif");
const errorRegist = document.getElementById("errorRegist");
const statusVerif = document.getElementById("errorVerif");


// Variables globales
let protocolo = null;

let JUGADOR_A_VERIFICAR = null;
let PLAYERS = [];


// Se crea el socket
const socket = new WebSocket("ws://localhost:8080/");

// Caso de conexión correcta
socket.onopen = function () {
    console.log("[App] Conectado al Cliente");
}


// Escucha de mensajes
socket.onmessage = function (event) {

    // Procesamiento
    let mensaje = JSON.parse(event.data);

    // Comprobaciones
    if (mensaje == null || mensaje.Comando == null || mensaje.Contenido == null)
        return;

    // Conversión del contenido
    const contenido = typeof mensaje.Contenido === "string"
        ? JSON.parse(mensaje.Contenido)
        : mensaje.Contenido;

    // Se pasa al manejo
    recibirMensaje(mensaje.Comando, contenido);
}




// Función para registrar un nuevo jugador
async function registrarJugador(player) {
    JUGADOR_A_VERIFICAR = player;

    console.log(player);

    let nombre = await mostrarRegistro();
    console.log(nombre);

    let id = parseInt(player.id.slice(-1));
    console.log(player, id);

    enviarMensaje(protocolo.AutenticarJugador, {
        id: id,
        nombre: nombre
    });

    mostrarVerificacion();
}



function verificarJugador(player) {
    JUGADOR_A_VERIFICAR = player;

    let id = parseInt(player.id.slice(-1));

    enviarMensaje(protocolo.VerificarJugador, {
        id: id
    });

    mostrarVerificacion();
}



// Función para enviar mensajes al server
function enviarMensaje(comando, contenido) {
    if (socket.readyState !== WebSocket.OPEN)
        return;

    if (comando == null || comando == undefined)
        return;

    // Se envía el mensaje
    socket.send(JSON.stringify({
        Comando: comando,
        Contenido: contenido
    }));

    // Se registra
    console.log("[App] App >>> Servidor: " + JSON.stringify({
        Comando: comando,
        Contenido: contenido
    }));
}


function mostrarRegistro() {
    dialogCont.style.display = "block";
    registroVerif.style.display = "block";
    inputVerif.focus();

    return new Promise((resolve) => {
        const completarRegistro = () => {
            const nombre = inputVerif.value.trim();

            if (nombre === "") {
                errorRegist.innerText = "Ingresa un nombre válido.";
                inputVerif.focus();
                return;
            }

            inputVerifBtn.removeEventListener("click", completarRegistro);
            inputVerif.removeEventListener("keydown", clickEnter);
            inputVerif.value = "";
            errorRegist.innerText = "";
            dialogCont.style.display = "none";
            registroVerif.style.display = "none";
            resolve(nombre);
        };

        const clickEnter = (event) => {
            if (event.key === "Enter") {
                completarRegistro();
            }
        };

        inputVerifBtn.addEventListener("click", completarRegistro);
        inputVerif.addEventListener("keydown", clickEnter);
    });
}



function mostrarVerificacion() {
    dialogCont.style.display = "block";
    dialogVerif.style.display = "block";
}

function finalizarVerificacion(status = "") {
    if (status != "") {
        statusVerif.innerText = status;
    }

    setTimeout(() => {
        dialogCont.style.display = "none";
        dialogVerif.style.display = "none";
        statusVerif.innerText = "";
    }, 2000);
}





// Función para recibir y manejar mensajes
function recibirMensaje(comando, contenido) {

    console.log("[App] App <<< Servidor: " + JSON.stringify({
        Comando: comando,
        Contenido: contenido
    }));

    switch (comando) {
        case "SOCKET_PRUEBA":
            console.log("Prueba recibida." + contenido.prueba1);
            break;


        case "SOCKET_PROTOCOLO":
            protocolo = contenido;
            console.log("[App] Protocolo recibido");

            enviarMensaje(protocolo.ConexionLista, {});
            break;


        case protocolo.AutenticarJugador:
            console.log("[App] Jugador autenticado");

            if (contenido.id == -1) {
                finalizarVerificacion(contenido.error);
                JUGADOR_A_VERIFICAR = null;
                return;
            }

            JUGADOR_A_VERIFICAR.querySelector("p").innerText = contenido.nombre;

            JUGADOR_A_VERIFICAR.querySelector("button").setAttribute("onclick", "verificarJugador(this.parentNode)");
            JUGADOR_A_VERIFICAR.querySelector("button").innerText = "Verificar";
            JUGADOR_A_VERIFICAR.querySelector("button").classList.add("green");

            finalizarVerificacion("Jugador autenticado correctamente.");

            JUGADOR_A_VERIFICAR = null;
            break;


        case protocolo.VerificarJugador:
            finalizarVerificacion("Jugador verificado correctamente.");

            JUGADOR_A_VERIFICAR = null;
            break;
    }
}

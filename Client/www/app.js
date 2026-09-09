// Definiciones de elementos

const secciones = document.querySelectorAll("section");


const dialogCont = document.querySelector(".dialogVerif-container");
const registroVerif = document.querySelector(".registroVerif");
const dialogVerif = document.querySelector(".dialogVerif");
const inputVerif = document.getElementById("inputVerif");
const inputVerifBtn = document.getElementById("inputBtnVerif");
const errorRegist = document.getElementById("errorRegist");
const statusVerif = document.getElementById("errorVerif");
const dialogoCont = document.getElementById("dialogo-container");
const dialogo = document.getElementById("dialogo");
const dialogoMensaje = document.getElementById("dialogo-mensaje");
let dialogoTimeout = null;


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


function comenzarJuego() {
    enviarMensaje(protocolo.IniciarJuego, {});
}



// Función para enviar mensajes al server
function enviarMensaje(comando, contenido) {
    if (socket.readyState !== WebSocket.OPEN)
        return;

    if (comando == null || comando == undefined) {
        console.log("[App] Comando inválido.");
        return;
    }

    // Si el mensaje esta fuera del protocolo
    if (protocolo == null || !Object.values(protocolo).includes(comando)) {
        console.log("[App] Protocolo inválido.");
        return;
    }

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


function cerrarDialogo() {
    if (dialogoTimeout !== null) {
        clearTimeout(dialogoTimeout);
        dialogoTimeout = null;
    }

    dialogoCont.style.display = "none";
    dialogoMensaje.textContent = "";
}


function mostrarDialogo(message, color) {
    if (dialogoTimeout !== null) {
        clearTimeout(dialogoTimeout);
    }

    dialogoMensaje.textContent = message;
    dialogo.style.setProperty("--dialogo-color", color || "#52d7fc");
    dialogoCont.style.display = "flex";

    dialogoTimeout = setTimeout(cerrarDialogo, 2000);
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

function finalizarVerificacion() {
    dialogCont.style.display = "none";
    dialogVerif.style.display = "none";
    statusVerif.innerText = "";
}



function cambiarSeccion(seccion) {
    for (const sec of secciones) {
        sec.style.display = "none";
    }

    document.querySelector("#" + seccion+"Section").style.display = "block";
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
                finalizarVerificacion();
                mostrarDialogo(contenido.error, "#ff0000");
                JUGADOR_A_VERIFICAR = null;
                return;
            }

            JUGADOR_A_VERIFICAR.querySelector("p").innerText = contenido.nombre;

            JUGADOR_A_VERIFICAR.querySelector("button").setAttribute("onclick", "verificarJugador(this.parentNode)");
            JUGADOR_A_VERIFICAR.querySelector("button").innerText = "Verificar";
            JUGADOR_A_VERIFICAR.querySelector("button").classList.add("green");

            finalizarVerificacion();

            mostrarDialogo("Jugador verificado correctamente.", "#48e33d");

            JUGADOR_A_VERIFICAR = null;
            break;




        case protocolo.VerificarJugador:
            finalizarVerificacion();

            mostrarDialogo("Jugador verificado correctamente.", "#48e33d", true);

            JUGADOR_A_VERIFICAR = null;
            break;



        case protocolo.IniciarJuego:
            if(contenido.error) {
                mostrarDialogo(contenido.error, "#ff0000");
                return;
            }
            console.log("[App] Juego iniciado.");
            cambiarSeccion("game");
            break;
    }
}

// Definiciones de elementos


// Variables globales
let protocolo = null;

let JUGADOR_A_REGISTRAR = null;
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
function registrarJugador(player) {
    JUGADOR_A_REGISTRAR = player;

    enviarMensaje(protocolo.AutenticarJugador, {
        id: 0,
        nombre: "Jugador 1"
    });
}

function verificarJugador(player) {
    JUGADOR_A_VERIFICAR = player;

    enviarMensaje(protocolo.VerificarJugador, {
        id: 0
    });
}



// Función para enviar mensajes al server
function enviarMensaje(comando, contenido) {
    if (socket.readyState !== WebSocket.OPEN)
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
            console.log(contenido);
            break;
    }
}

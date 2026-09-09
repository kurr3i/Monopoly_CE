// Definiciones de elementos
const button = document.getElementById("diceButton");
const result = document.getElementById("result");

// Variables globales
let JUGADOR_A_REGISTRAR = null

let PLAYERS = [];


// Funciones del socket
const socket = new WebSocket("ws://localhost:8080/");

socket.onopen = function () {
    console.log("Conectado al Client.cs");
}


socket.onmessage = function (event) {
    let mensaje = JSON.parse(event.data);
    console.log(mensaje);

    if (mensaje == null || mensaje.Comando == null || mensaje.Contenido == null)
        return;

    const contenido = typeof mensaje.Contenido === "string"
        ? JSON.parse(mensaje.Contenido)
        : mensaje.Contenido;
    recibirMensaje(mensaje.Comando, contenido);
}


function registrarJugador(player) {
    JUGADOR_A_REGISTRAR = player;


    enviarMensaje("SOCKET_PRUEBA", {
        prueba1: "prueba1",
        prueba2: "prueba2"
    });
}


function enviarMensaje(comando, contenido) {
    if (socket.readyState !== WebSocket.OPEN)
        return;

    socket.send(JSON.stringify({
        Comando: comando,
        Contenido: contenido
    }));
}



function recibirMensaje(comando, contenido) {

    switch (comando) {
        case "SOCKET_PRUEBA":
            console.log("Prueba recibida." + contenido.prueba1);
            break;
    }
}

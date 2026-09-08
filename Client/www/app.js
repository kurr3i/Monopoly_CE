const socket = new WebSocket("ws://localhost:8080");

const button = document.getElementById("diceButton");
const result = document.getElementById("result");


socket.onopen = function () {

    console.log("Conectado al Client C#");

};


button.onclick = function () {

    console.log("Solicitando dado...");
    socket.send("TIRAR_DADOS");

};

socket.onmessage = function (event) {

    console.log("Mensaje del servidor:", event.data);

    let mensaje;

    try {
        mensaje = JSON.parse(event.data);
    } catch (e) {
        // Por compatibilidad, si llegara a venir un string plano.
        result.textContent = event.data;
        return;
    }

    switch (mensaje.accion) {

        case "DADO_TIRADO":
            result.textContent =
                `${mensaje.datos.dado1} + ${mensaje.datos.dado2} = ${mensaje.datos.total}`;
            break;

        case "CONECTADO":
            console.log("Jugador asignado por el Server:", mensaje.jugadorId);
            break;

        case "JUGADOR_CONECTADO":
        case "JUGADOR_DESCONECTADO":
            console.log(mensaje.accion, mensaje.jugadorId);
            break;

        default:
            console.log("Mensaje sin manejar:", mensaje);
    }

};


socket.onerror = function (error) {

    console.error("Error WebSocket:", error);

};

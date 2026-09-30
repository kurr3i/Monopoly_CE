# 1. Protocolo cliente–servidor

## 1.1 Capas y formato

| Tramo | Transporte | Delimitación |

| --- | --- | --- |

| Navegador ⇄ Client | WebSocket ws://localhost:8080/ | Un mensaje de texto es un JSON. |

| Client ⇄ Server | TCP en el puerto 5000 (UTF-8) | Un JSON por línea (terminado en salto de línea). |

| Server ⇄ Arduino | Serial a 9600 baudios | Una línea por mensaje. |

El Client reenvía sin modificar los mensajes entre ambos lados; solo interpreta CerrarJuego, tras lo cual termina. Todo mensaje (Compartido/Mensaje.cs) tiene la forma:

{ "Comando": "SOCKET_ACCION_JUGADOR", "Contenido": { "id": 0, "accion": "dado", "valor": "dado" } }

Comando y Contenido van en PascalCase; los campos del contenido, en camelCase. El servidor los lee sin distinguir mayúsculas de minúsculas.

## 1.2 Handshake

El navegador abre el WebSocket. El Client responde con SOCKET_PROTOCOLO, cuyo contenido es el diccionario {NombreConstante: "SOCKET_..."}; así el frontend no tiene los comandos escritos de forma fija.

El navegador envía ConexionLista y el servidor marca la conexión como establecida. Un segundo ConexionLista provoca CerrarJuego y el cierre del servidor.

## 1.3 Mensajes del navegador al servidor

| Comando | Contenido | Efecto |

| --- | --- | --- |

| SOCKET_CONEXION_LISTA | {} | Confirma la conexión. |

| SOCKET_REGISTRAR_JUGADOR | {id (0–3), nombre (máx. 12)} | Lee la tarjeta y crea al jugador. |

| SOCKET_VERIFICAR_JUGADOR | {id} | Pide la tarjeta del jugador registrado. |

| SOCKET_INICIAR_JUEGO | {} | Inicia la partida (requiere 4 jugadores). |

| SOCKET_ACCION_JUGADOR | {id, accion, valor} | Acción del turno (ver 4.5). |

| SOCKET_ULTIMA_TRANSACCION, SOCKET_PRIMERA_TRANSACCION, SOCKET_TRANSACCIONES_LISTA | {} | Consultas de historial. |

| SOCKET_TRANSACCIONES_JUGADOR | {jugador} | Historial de un jugador. |

| SOCKET_TRANSACCIONES_TIPO | {tipo} | Historial por tipo. |

| SOCKET_CERRAR_JUEGO | {} | Cierra cliente y servidor. |

## 1.4 Mensajes del servidor al navegador

| Comando | Campos principales |

| --- | --- |

| SOCKET_REGISTRAR_JUGADOR | {id, nombre}; en caso de error {id: -1, error} |

| SOCKET_VERIFICAR_JUGADOR | {id} (solo si la verificación tuvo éxito) |

| SOCKET_INICIAR_JUEGO | {} o {error} |

| SOCKET_TURNO_INICIADO | {jugadorId, turno} |

| SOCKET_ACCIONES_TURNO | {id, tipo}, con tipo: turno, compra, venta o continuar |

| SOCKET_DESBLOQUEAR TURNO | {jugadorId} (el nombre real de la constante lleva un espacio) |

| SOCKET_DADOS_LANZADOS | {jugadorId, dado1, dado2, resultado} |

| SOCKET_JUGADOR_MOVIDO | {jugadorId, casilla, posicion, pasoPorSalida} |

| SOCKET_PREMIO_SALIDA | {jugadorId, monto, saldo, descripcion} |

| SOCKET_COMPRA_PROPIEDAD | {jugadorId, propiedad, precio} (oferta de compra) |

| SOCKET_PROPIEDAD_COMPRADA, SOCKET_PROPIEDAD_VENDIDA | {jugadorId, propiedad, precio, saldo} |

| SOCKET_VENTA_PROPIEDAD | {jugadorId, propiedades: [{id, nombre, precio}], cantidad} o {mensaje} |

| SOCKET_PAGO_RFID | {jugadorId, jugador, monto, concepto} (indica que se espera la tarjeta) |

| SOCKET_ALQUILER_PAGADO | {jugadorId, propietarioId, propiedad, monto, saldo, saldoPropietario} |

| SOCKET_CARTA_EVENTO | {jugadorId, tipo, descripcion, valor, saldo} |

| SOCKET_JUGADOR_CARCEL, SOCKET_TURNO_PERDIDO | {jugadorId (opcional), mensaje}; TurnoPerdido añade turnosRestantes |

| SOCKET_BANCARROTA, SOCKET_JUGADOR_ELIMINADO | {jugadorId, monto} / {jugadorId, mensaje} |

| SOCKET_ULTIMA_TRANSACCION, SOCKET_PRIMERA_TRANSACCION | {transaccion: {...} o null} |

| SOCKET_TRANSACCIONES_LISTA, _JUGADOR, _TIPO | {transacciones: [...]}, más jugador o tipo según el caso |

| SOCKET_FIN_TURNO | {jugadorId, turno} |

| SOCKET_GANADOR | {jugadorId} |

| SOCKET_TERMINAR_JUEGO | {ganadorId, jugadores: [{jugador, saldo}]} |

| SOCKET_ERROR_ACCION | {mensaje} |

| SOCKET_CERRAR_JUEGO | {} |

Objeto transacción: {id, fechaHora, numeroTurno, tipo, jugadorOrigen, jugadorDestino, monto, descripcion}.

## 1.5 Acciones válidas según el estado

| Estado esperado | Acción aceptada | Notas |

| --- | --- | --- |

| turno | desbloquear (siempre); luego dado/1, venta/2, propiedades, 3 | Antes de desbloquear, cualquier otra acción produce ErrorAccion ("Primero verifica tu tarjeta RFID"). |

| compra | comprar/1, rechazar/2 |  |

| venta | valor = ID de la propiedad o índice de la lista |  |

| continuar | continuar |  |

Cualquier acción de un jugador que no es el del turno, o que no corresponde al estado, recibe ErrorAccion.

## 1.6 Ejemplo: comprar una propiedad

S = servidor, N = navegador.

| Sentido | Comando | Contenido |

| --- | --- | --- |

| S → N | SOCKET_TURNO_INICIADO | {jugadorId: 0, turno: 4} |

| S → N | SOCKET_ACCIONES_TURNO | {id: 0, tipo: "turno"} |

| N → S | SOCKET_ACCION_JUGADOR | {id: 0, accion: "desbloquear"} (el lector pide la tarjeta) |

| S → N | SOCKET_DESBLOQUEAR TURNO | {jugadorId: 0} |

| N → S | SOCKET_ACCION_JUGADOR | {id: 0, accion: "dado", valor: "dado"} |

| S → N | SOCKET_DADOS_LANZADOS | {jugadorId: 0, dado1: 3, dado2: 3, resultado: 6} |

| S → N | SOCKET_JUGADOR_MOVIDO | {jugadorId: 0, casilla: "Zarcero", posicion: 6, pasoPorSalida: false} |

| S → N | SOCKET_ACCIONES_TURNO | {id: 0, tipo: "continuar"} |

| N → S | SOCKET_ACCION_JUGADOR | {id: 0, accion: "continuar", valor: "continuar"} |

| S → N | SOCKET_COMPRA_PROPIEDAD | {jugadorId: 0, propiedad: "Zarcero", precio: 350} |

| S → N | SOCKET_ACCIONES_TURNO | {id: 0, tipo: "compra"} |

| N → S | SOCKET_ACCION_JUGADOR | {id: 0, accion: "comprar", valor: "comprar"} |

| S → N | SOCKET_PAGO_RFID | {jugadorId: 0, monto: 350, concepto: "Compra de propiedad"} (paga con tarjeta) |

| S → N | SOCKET_ULTIMA_TRANSACCION | {transaccion: {...}} |

| S → N | SOCKET_PROPIEDAD_COMPRADA | {jugadorId: 0, propiedad: "Zarcero", precio: 350, saldo: 650} |

| S → N | SOCKET_FIN_TURNO | {jugadorId: 0, turno: 4} |

## 1.7 Protocolo serial con el Arduino

Servidor → Arduino: una línea con el formato COM_START_READ|<mensaje LCD, máx. 16 caracteres>|<UID esperado o 0>. Ejemplo: COM_START_READ|J1 PAGUE 350 CRC|865D2507.

Arduino → servidor, una línea:

| Respuesta | Significado |

| --- | --- |

| UID en hexadecimal mayúsculas | Lectura válida (o cualquiera si el UID esperado es 0). |

| INVALID | La tarjeta no coincide con la esperada. |

| TIMEOUT | Pasaron 7 segundos sin tarjeta. |

Ante TIMEOUT, INVALID o un error del puerto (la lectura tiene un timeout de 10 s), RFIDDriver.ReadUID reintenta indefinidamente tras una pausa. El simulador usa los UID 865D2507, 215A436E, 41054E6E y 415A1F6E para los jugadores 1 a 4.

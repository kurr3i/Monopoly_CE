using Proyecto_MonopoTEC.Server.Motor;
using Proyecto_MonopoTEC.Server.Estructuras;

namespace Proyecto_MonopoTEC.Server.Modelo
{

    /// <summary>
    /// Representa el tablero del juego como una lista circular doblemente enlazada.
    /// </summary>
    public class Tablero
    {
        public NodeCasilla Head { get; private set; }
        public NodeCasilla Tail { get; private set; }
        public int Size { get; private set; }

        /// <summary>
        /// Agrega una nueva casilla al final del tablero.
        /// </summary>
        /// <param name="casilla">Casilla que se agregará al tablero.</param>
        private void Add(Casilla casilla)
        {
            NodeCasilla newNode = new NodeCasilla { Data = casilla };

            if (Size == 0)
            {
                Head = newNode;
                Tail = newNode;

                Head.Next = Head; // Conectamos la cabeza con ella misma
                Head.Previous = Head;
            }
            else
            {
                Tail.Next = newNode; // El siguiente nodo de la cola actual será el nuevo nodo
                newNode.Previous = Tail; // El nodo anterior del nuevo nodo será la cola actual
                Tail = newNode; // Actualizamos la cola para que sea el nuevo nodo

                Head.Previous = Tail; // Conectamos la cabeza con la nueva cola
                Tail.Next = Head; // Conectamos la nueva cola con la cabeza
            }

            Size++;
        }

        /// <summary>
        /// Obtiene la acción correspondiente a una casilla.
        /// </summary>
        /// <param name="casilla">La casilla cuya acción se desea obtener.</param>
        /// <param name="jugador">El jugador a comparar en caso de que sea una propiedad.</param>
        /// <returns>La acción correspondiente a la casilla.</returns>
        public AccionCasilla ObtenerAccion(Casilla casilla, Jugador jugador)
        {
            return casilla.DevolverAccion(jugador);
        }

        /// <summary>
        /// Avanza al jugador en el tablero.
        /// </summary>
        /// <param name="jugador">El jugador que se moverá.</param>
        /// <param name="movimiento">La cantidad de casillas a mover.</param>
        /// <param name="pasoPorSalida">Indica si el jugador pasó por la casilla de salida durante el movimiento.</param>
        /// <returns>La casilla en la que termina el jugador después del movimiento.</returns> 
        public Casilla AvanzarJugador(Jugador jugador, int movimiento, out bool pasoPorSalida)
        {
            Casilla casillaActual = jugador.Posicion; // La casilla actual del jugador antes de moverse
            NodeCasilla nodoCasillaActual = Head; // Nodo que representa la casilla actual del jugador
            pasoPorSalida = false;

            while (nodoCasillaActual.Data != casillaActual) // Buscar el nodo que contiene la casilla actual del jugador
            {
                nodoCasillaActual = nodoCasillaActual.Next;
            }

            for (int i = 0; i < movimiento; i++) // Avanzamos el número de casillas indicado por el movimiento 
            {

                nodoCasillaActual = nodoCasillaActual.Next;

                if (nodoCasillaActual.Data is CasillaEspecial casillaEspecial && casillaEspecial.Subtipo == SubtipoCasillaEspecial.Salida) // Esto es para el out de si pasa por la salida o no
                {
                    pasoPorSalida = true;
                }
            }

            jugador.CambiarPosicion(nodoCasillaActual.Data); // Actualizamos la posición del jugador a la nueva casilla

            return nodoCasillaActual.Data; // Retornamos la casilla en la que termina el jugador después del movimiento
        }

        /// <summary>
        /// Retrocede al jugador en el tablero.
        /// </summary>
        /// <param name="jugador">El jugador que se moverá.</param>
        /// <param name="movimiento">La cantidad de casillas a mover.</param>
        /// <returns>La casilla en la que termina el jugador después del movimiento.</returns>
        public Casilla RetrocederJugador(Jugador jugador, int movimiento)
        {
            Casilla casillaActual = jugador.Posicion; // La casilla actual del jugador antes de moverse
            NodeCasilla nodoCasillaActual = Head; // Nodo que representa la casilla actual del jugador

            while (nodoCasillaActual.Data != casillaActual) // Buscar el nodo que contiene la casilla actual del jugador
            {
                nodoCasillaActual = nodoCasillaActual.Next;
            }

            for (int i = 0; i < movimiento; i++) // Retrocedemos el número de casillas indicado por el movimiento 
            {
                nodoCasillaActual = nodoCasillaActual.Previous;
            }

            jugador.CambiarPosicion(nodoCasillaActual.Data); // Actualizamos la posición del jugador a la nueva casilla

            return nodoCasillaActual.Data; // Retornamos la casilla en la que termina el jugador después del movimiento
        }

        /// <summary>
        /// Mueve al jugador a la casilla de la cárcel.
        /// </summary>
        /// <param name="jugador">El jugador que será enviado a la cárcel.</param>
        public void MoverJugadorACarcel(Jugador jugador)
        {
            NodeCasilla nodoCasillaActual = Head; // Empezamos desde la cabeza

            while (nodoCasillaActual.Data is not CasillaEspecial casillaCarcel || casillaCarcel.Subtipo != SubtipoCasillaEspecial.Carcel) // Avanzamos hasta encontrar el nodo que contiene la carcel
            {
                nodoCasillaActual = nodoCasillaActual.Next;
            }

            jugador.CambiarPosicion(nodoCasillaActual.Data); // Actualizamos la posición del jugador a la carcel
        }

        /// <summary>
        /// Mueve al jugador a la casilla del parque de diversiones.
        /// </summary>
        /// <param name="jugador">El jugador que se moverá al parque de diversiones.</param>
        /// <param name="pasoPorSalida">Indica si el jugador pasó por la casilla de salida durante el movimiento.</param>
        public void MoverJugadorAParque(Jugador jugador, out bool pasoPorSalida)
        {
            Casilla casillaActual = jugador.Posicion; // La casilla actual del jugador antes de moverse
            NodeCasilla nodoCasillaActual = Head; // Nodo que representa la casilla actual del jugador
            pasoPorSalida = false;

            while (nodoCasillaActual.Data != casillaActual) // Buscar el nodo que contiene la casilla actual del jugador
            {
                nodoCasillaActual = nodoCasillaActual.Next;
            }

            while (nodoCasillaActual.Data is not CasillaEspecial casillaEspecial || casillaEspecial.Subtipo != SubtipoCasillaEspecial.ParqueDiversiones) // Avanzamos hasta encontrar el nodo que contiene el parque de diversiones
            {
                nodoCasillaActual = nodoCasillaActual.Next;

                if (nodoCasillaActual.Data is CasillaEspecial casillaParque && casillaParque.Subtipo == SubtipoCasillaEspecial.Salida) // Esto es para el out de si pasa por la salida o no
                {
                    pasoPorSalida = true;
                }
            }

            jugador.CambiarPosicion(nodoCasillaActual.Data); // Actualizamos la posición del jugador al parque de diversiones
        }

        /// <summary>
        /// Mueve al jugador a la casilla de la salida.
        /// </summary>
        /// <param name="jugador">El jugador que será avanzado hasta la salida.</param>
        public void MoverJugadorASalida(Jugador jugador)
        {
            NodeCasilla nodoCasillaActual = Head; // Empezamos desde la cabeza

            while (nodoCasillaActual.Data is not CasillaEspecial casillaSalida || casillaSalida.Subtipo != SubtipoCasillaEspecial.Salida) // Avanzamos hasta encontrar el nodo que contiene la salida
            {
                nodoCasillaActual = nodoCasillaActual.Next;
            }

            jugador.CambiarPosicion(nodoCasillaActual.Data); // Actualizamos la posición del jugador a la salida
        }

        /// <summary>
        /// Inicializa el tablero agregando las 24 casillas necesarias.
        /// </summary>
        public void Inicializar()
        {
            CasillaEspecial salida = new CasillaEspecial(1, "Salida", SubtipoCasillaEspecial.Salida);
            Add(salida);

            CasillaEvento loteria1 = new CasillaEvento(2, "Loteria 1");
            Add(loteria1);

            Propiedad pococi = new Propiedad(3, "Pococí", 200, 50);
            Add(pococi);

            Propiedad guacimo = new Propiedad(4, "Guácimo", 250, 60);
            Add(guacimo);

            Propiedad sanCarlos = new Propiedad(5, "San Carlos", 300, 70);
            Add(sanCarlos);

            Propiedad zarcero = new Propiedad(6, "Zarcero", 350, 80);
            Add(zarcero);

            CasillaEspecial carcel = new CasillaEspecial(7, "Carcel - San Lucas", SubtipoCasillaEspecial.Carcel);
            Add(carcel);

            Propiedad heredia = new Propiedad(8, "Heredia", 400, 90);
            Add(heredia);

            Propiedad sarapiqui = new Propiedad(9, "Sarapiquí", 450, 100);
            Add(sarapiqui);

            Propiedad quepos = new Propiedad(10, "Quepos", 500, 110);
            Add(quepos);

            CasillaEvento loteria2 = new CasillaEvento(11, "Loteria 2");
            Add(loteria2);

            Propiedad golfito = new Propiedad(12, "Golfito", 550, 120);
            Add(golfito);

            CasillaEspecial parqueDeDiversiones = new CasillaEspecial(13, "Parque de Diversiones", SubtipoCasillaEspecial.ParqueDiversiones);
            Add(parqueDeDiversiones);

            Propiedad liberia = new Propiedad(14, "Liberia", 600, 130);
            Add(liberia);

            Propiedad nicoya = new Propiedad(15, "Nicoya", 650, 140);
            Add(nicoya);

            CasillaEvento loteria3 = new CasillaEvento(16, "Loteria 3");
            Add(loteria3);

            Propiedad cartago = new Propiedad(17, "Cartago", 700, 150);
            Add(cartago);

            Propiedad turrialba = new Propiedad(18, "Turrialba", 750, 160);
            Add(turrialba);

            CasillaEspecial vayaCarcel = new CasillaEspecial(19, "Vaya a la Carcel - La Cali", SubtipoCasillaEspecial.VayaCarcel);
            Add(vayaCarcel);

            Propiedad desamparados = new Propiedad(20, "Desamparados", 800, 170);
            Add(desamparados);

            Propiedad perezZeledon = new Propiedad(21, "Perez Zeledón", 850, 180);
            Add(perezZeledon);

            CasillaEvento loteria4 = new CasillaEvento(22, "Loteria 4");
            Add(loteria4);

            Propiedad chepe = new Propiedad(23, "Chepe", 800, 170);
            Add(chepe);

            Propiedad escazu = new Propiedad(24, "Escazú", 850, 180);
            Add(escazu);
        }
    }
}
using System;

/// <summary>
/// Representa el tablero del juego como una lista circular doblemente enlazada.
/// </summary>
public class Tablero
{
    public NodeCasilla Head { get; private set; }
    public NodeCasilla Tail { get; private set; }
    public int Size { get; private set; }

    /// <summary>
    /// Agrega un nuevo nodo al final de la lista circular doblemente enlazada.
    /// </summary>
    /// <param name="casilla">Dato que almacenará el nodo.</param>
    public void Add(Casilla casilla)
    {
        NodeCasilla newNode = new NodeCasilla { Data = casilla };

        if (Size == 0) // Si la lista está vacía, el nuevo nodo será tanto la cabeza como la cola
        {
            Head = newNode;
            Tail = newNode;

            Head.Next = Head; // Conectamos la cabeza con ella misma
            Head.Previous = Head;
        }
        else // Si la lista no está vacía, agregamos el nuevo nodo al final y lo conectamos con el primero
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
    /// Inicializar el tablero agregando las 24 casillas necesarias.
    /// </summary>
    
    public void InicializarTablero()
    {
        CasillaEspecial salida = new CasillaEspecial(1, "Salida");
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

        CasillaEspecial carcel = new CasillaEspecial(7, "Carcel - San Lucas");
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

        CasillaEspecial parqueDeDiversiones = new CasillaEspecial(13, "Parque de Diversiones");
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

        CasillaEspecial vayaCarcel = new CasillaEspecial(19, "Vaya a la Carcel - La Cali");
        Add(vayaCarcel);

        Propiedad Desamparados = new Propiedad(20, "Desamparados", 800, 170);
        Add(Desamparados);

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

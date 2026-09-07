
/// <summary>
/// Representa a un jugador dentro de la partida.
/// </summary>
public class Jugador
{

	public int ID { get; private set; }
    public string UID { get; private set; }
    public string Nombre { get; private set; }
	public int Saldo { get; private set; }
	public bool Activo { get; private set; }
    public bool EnCarcel { get; private set; }
    public int TurnosCarcel { get; private set; }
    public int TurnosPerdidos { get; private set; }

    /// <summary>
    /// Representa la posición actual del jugador en el tablero, que es una instancia de la clase Casilla.
    /// </summary>
    public Casilla Posicion { get; private set; }

    /// <summary>
    /// Representa una lista doblemente enlazada que contiene las propiedades adquiridas por el jugador. Cada nodo contiene una instancia de la clase Propiedad.
    /// </summary>
    public ListaDobleEnlazada PropiedadesAdquiridas { get; private set; }


    /// <summary>
    /// Inicializa un jugador.
    /// </summary>
    /// <param name="id">Identificador del jugador.</param>
    /// <param name="nombre">Nombre del jugador.</param>
    /// <param name="posicion">Posición inicial del jugador.</param>
    public Jugador(int id, string nombre, Casilla posicion)
	{
		this.ID = id;
		this.Nombre = nombre;
		this.Saldo = 2000; 
        this.Posicion = posicion; 
        // Falta un estado o contador que se pueda emplear para evalúar si el jugador está en la cárcel o no.
		this.Activo = true; // Si el jugador está activo o no en la partida (perdió)
        this.PropiedadesAdquiridas = new ListaDobleEnlazada();
    }

    /// <summary>
    /// Cambia la posición del jugador.
    /// </summary>
    /// <param name="posicion">La casilla que será la nueva posición.</param>
    public void CambiarPosicion(Casilla posicion)
    {
        Posicion = posicion;
    }

    /// <summary>
    /// Aumenta el saldo al jugador según un monto.
    /// </summary>
    /// <param name="monto">Monto en el que aumentará el saldo.</param>
    public void AumentarSaldo(int monto)
    {
        Saldo += monto;
    }

    /// <summary>
    /// Disminuye el saldo al jugador según un monto.
    /// </summary>
    /// <param name="monto">Monto en el que disminuirá el saldo.</param>
    public void DisminuirSaldo(int monto)
    {
        Saldo -= monto;
    }

    /// <summary>
    /// Inicializa el UID del jugador.
    /// </summary>
    public void InicializarUID(string uid)
    {
        UID = uid;
    }

    /// <summary>
    /// Cambia el estado para que el jugador esté en carcel y cambia los turnos en carcel a 3.
    /// </summary>
    public void EntrarCarcel()
    {
        EnCarcel = true;
        TurnosCarcel = 3;
    }

    /// <summary>
    /// Reduce los turnos de la condena en 1.
    /// </summary>
    public void ReducirCondena()
    {
        if(TurnosCarcel > 0)
        {
            TurnosCarcel --;
        }      
    }

    /// <summary>
    /// Cambia el estado para que el jugador esté en carcel y cambia los turnos en carcel a 0.
    /// </summary>
    public void SalirCarcel()
    {
        EnCarcel = false;
        TurnosCarcel = 0;
    }

    /// <summary>
    /// Suma los turnos que perderá el jugador.
    /// </summary>
    /// <param name="turnosPerdidos">Cantidad de turnos que perderá el jugador.</param>
    public void PerderTurnos(int turnosPerdidos)
    {
        TurnosPerdidos+= turnosPerdidos;
    }

    /// <summary>
    /// Reduce en 1 los turnos que perderá el jugador.
    /// </summary>
    public void ReducirTurnoPerdido()
    {
        if (TurnosPerdidos > 0)
        {
            TurnosPerdidos--;
        }
    }
}

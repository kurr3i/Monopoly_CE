using System;

/// <summary>
/// Representa a un jugador dentro de la partida.
/// </summary>
public class Jugador
{

	public int ID { get; set; } 
	public string Nombre { get; set; }
	public int Saldo { get; set; }
	public bool Activo { get; set; }

    /// <summary>
    /// Representa la posición actual del jugador en el tablero, que es una instancia de la clase Casilla.
    /// </summary>
    public Casilla PosicionActual { get; set; }

    /// <summary>
    /// Representa una lista doblemente enlazada que contiene las propiedades adquiridas por el jugador. Cada nodo contiene una instancia de la clase Propiedad.
    /// </summary>
    public ListaDobleEnlazada PropiedadesAdquiridas { get; set; } 



    public Jugador(int ID, string Nombre, Casilla PosicionActual)
	{
		this.ID = ID;
		this.Nombre = Nombre;
		this.Saldo = 2000; 
        this.PosicionActual = PosicionActual; 
		this.Activo = false; // Pendiente: Acordar si el jugador inicia activo o inactivo, creo que es false, pues solo un jugador puede tomar acciones a la vez
        this.PropiedadesAdquiridas = new ListaDobleEnlazada();
    }
}

using System;

/// <summary>
/// Representa a un jugador dentro de la partida.
/// </summary>
public class Jugador
{

	public int ID { get; set; } 
	public string Nombre { get; set; }
	public int Saldo { get; set; }
	public int PosicionActual { get; set; } // Pendiente: Cual será el tipo de dato de la posición actual
	public bool Activo { get; set; }
    /* public DATO propiedadesAdquiridasJugador { get; set; } */ // Pendientes: La clase propiedad y la estructura lineal para guardar las propiedades adquiridas



    public Jugador(int ID, string Nombre)
	{
		this.ID = ID;
		this.Nombre = Nombre;
		this.Saldo = 0; // Pendiente: Acordar el saldo inicial
        this.PosicionActual = 0; // Pendiente:  el dato de posición y cual será la posición inicial
		this.Activo = false; // Pendiente: Acordar si el jugador inicia activo o inactivo, creo que es false, pues solo un jugador puede tomar acciones a la vez
    }
}

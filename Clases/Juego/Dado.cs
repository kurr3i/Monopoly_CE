using System;

/// <summary>
/// Representa un dado.
/// </summary>
public class Dado
{
	Random dado = new Random();

    /// <summary>
    /// Simula el lanzamiento de un dado retornando un valor entre 1 y 6.
    /// </summary>
    /// <returns>Un valor entre 1 y 6.</returns>
    public int Lanzar()
	{
		return dado.Next(1,7); // Genera un número aleatorio entre 1 y 6
    }
}

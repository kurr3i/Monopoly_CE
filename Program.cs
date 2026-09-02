using Proyecto_MonopoTEC.Clases;

namespace Proyecto_MonopoTEC
{
        internal class Program
        {
                static void Main(string[] args)
                {
                        ClaseTest test = new ClaseTest();
                        test.Test();

                        Juego juego = new Juego();
                        juego.IniciarJuego();
                }
        }
}
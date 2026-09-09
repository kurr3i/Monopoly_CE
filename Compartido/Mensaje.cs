namespace Proyecto_MonopoTEC.Compartido
{
        public class Mensaje
        {
                public string? Comando { get; set; }
                public object? Contenido { get; set; }

                public Mensaje(string comando, object contenido)
                {
                        Comando = comando;
                        Contenido = contenido;
                }
        }
}
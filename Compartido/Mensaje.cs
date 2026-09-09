using System.Text.Json;

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


                /// <summary>
                /// Obtiene un dato del contenido del mensaje.
                /// </summary>
                /// <param name="nombre">El nombre del dato.</param>
                public bool GetContenido<T>(string nombre, out T? valor)
                {
                        valor = default;

                        // Verificar si el contenido es un objeto
                        if (Contenido is not JsonElement contenido || contenido.ValueKind != JsonValueKind.Object)
                                return false;

                        // Obtener la propiedad
                        JsonProperty? propiedad = null;
                        foreach (JsonProperty item in contenido.EnumerateObject())
                        {
                                // Comparar el nombre
                                if (string.Equals(item.Name, nombre, StringComparison.OrdinalIgnoreCase))
                                {
                                        // Si se encuentra la propiedad, se guarda
                                        propiedad = item;
                                        break;
                                }
                        }

                        // Verificar si se encontro la propiedad
                        if (propiedad is null)
                                return false;

                        try
                        {
                                // Deserializar
                                valor = propiedad.Value.Value.Deserialize<T>();
                                return valor is not null || typeof(T).IsValueType;
                        }
                        catch (JsonException)
                        {
                                // Caso de error
                                return false;
                        }
                }


                /// <summary>
                /// Obtiene un dato del contenido del mensaje.
                /// </summary>
                public T GetDato<T>(string nombre)
                {
                        // Validación
                        if (!GetContenido(nombre, out T? valor))
                                throw new InvalidOperationException($"El contenido no tiene un valor válido para '{nombre}'.");

                        // Devuelve el dato
                        return valor!;
                }
        }
}
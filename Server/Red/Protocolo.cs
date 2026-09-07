using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Proyecto_MonopoTEC.Server.Red
{
    /// <summary>
    /// Mensaje del protocolo Cliente-Server.
    /// </summary>
    public class Mensaje
    {
        public string Accion { get; set; } = string.Empty;
        public string? JugadorId { get; set; }
        public object? Datos { get; set; }
    }

    /// <summary>
    /// Nombres de las acciones reconocidas por el protocolo.
    /// </summary>
    public static class Acciones
    {
        public const string Conectar = "CONECTAR";
        public const string Conectado = "CONECTADO";
        public const string TirarDados = "TIRAR_DADOS";
        public const string DadoTirado = "DADO_TIRADO";
        public const string JugadorConectado = "JUGADOR_CONECTADO";
        public const string JugadorDesconectado = "JUGADOR_DESCONECTADO";
        public const string Error = "ERROR";
    }

    /// <summary>
    /// Lectura y escritura de mensajes sobre un NetworkStream.
    /// </summary>
    public static class MensajeIO
    {
        /// <summary>Envía un mensaje serializado como JSON, protegido por un lock de escritura.</summary>
        public static async Task EnviarAsync(NetworkStream stream, Mensaje mensaje, SemaphoreSlim writeLock)
        {
            string json = JsonSerializer.Serialize(mensaje);
            byte[] bytes = Encoding.UTF8.GetBytes(json + "\n");

            await writeLock.WaitAsync();
            try
            {
                await stream.WriteAsync(bytes);
                await stream.FlushAsync();
            }
            finally
            {
                writeLock.Release();
            }
        }

        /// <summary>Lee y deserializa un mensaje; retorna null si la conexión se cerró o el mensaje es inválido.</summary>
        public static async Task<Mensaje?> RecibirAsync(StreamReader reader)
        {
            string? linea = await reader.ReadLineAsync();

            if (linea == null || linea.Length == 0)
                return null;

            try
            {
                return JsonSerializer.Deserialize<Mensaje>(linea);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>Obtiene un campo de "Datos" cuando llega como JsonElement.</summary>
        public static bool TryGetDato(object? datos, string propiedad, out JsonElement valor)
        {
            if (datos is JsonElement el && el.ValueKind == JsonValueKind.Object &&
                el.TryGetProperty(propiedad, out valor))
            {
                return true;
            }

            valor = default;
            return false;
        }
    }
}
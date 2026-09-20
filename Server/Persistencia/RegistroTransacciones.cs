using System;
using System.Collections.Generic;
using System.IO;
using Proyecto_MonopoTEC.Server.Modelo;

namespace Proyecto_MonopoTEC.Server.Persistencia
{
    public class RegistroTransacciones
    {
        private string _rutaArchivo = string.Empty;

        /// <summary>
        /// Inicia el registro de la partida.
        /// </summary>
        public void Iniciar()
        {
            string directorio = "Registros";
            if (!Directory.Exists(directorio))
            {
                Directory.CreateDirectory(directorio);
            }

            string fechaHora = DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss");
            _rutaArchivo = Path.Combine(directorio, $"Partida_{fechaHora}.txt");

            File.WriteAllText(_rutaArchivo, $"--- Registro de Partida Iniciado: {DateTime.Now} ---\n");
        }

        /// <summary>
        /// Guarda una transacción en el archivo de registro.
        /// </summary>
        public void RegistrarTransaccion(Transaccion transaccion)
        {
            if (string.IsNullOrEmpty(_rutaArchivo))
            {
                Console.WriteLine("[Registro] Error: El registro no ha sido iniciado.");
                return;
            }

            string origen = transaccion.JugadorOrigen is null ? "Banco" : transaccion.JugadorOrigen.Nombre;
            string destino = transaccion.JugadorDestino is null ? "Banco" : transaccion.JugadorDestino.Nombre;
            string timestamp = transaccion.FechaHora.ToString("yyyy-MM-dd HH:mm:ss");

            string contenido = $"\n{timestamp}\n" +
                              $"Timestamp={timestamp}\n" +
                              $"ID={transaccion.ID}\n" +
                              $"NumeroTurno={transaccion.NumeroTurno}\n" +
                              $"Tipo={transaccion.Tipo}\n" +
                              $"JugadorOrigen={origen}\n" +
                              $"JugadorDestino={destino}\n" +
                              $"Monto={transaccion.Monto}\n" +
                              $"Descripcion={transaccion.Descripcion.Replace("\n", " ").Replace("\r", " ")}\n\n";

            File.AppendAllText(_rutaArchivo, contenido);
            Console.WriteLine($"[Registro] Transacción {transaccion.ID} registrada correctamente.");
        }

        public Transaccion VerUltimaTransaccion()
        {
            Transaccion[] transacciones = VerListaTransacciones();
            return transacciones.Length == 0 ? null! : transacciones[^1];
        }

        public Transaccion[] VerListaTransacciones()
        {
            if (string.IsNullOrEmpty(_rutaArchivo) || !File.Exists(_rutaArchivo))
            {
                return Array.Empty<Transaccion>();
            }

            string contenido = File.ReadAllText(_rutaArchivo);
            if (string.IsNullOrWhiteSpace(contenido))
            {
                return Array.Empty<Transaccion>();
            }

            string[] bloques = contenido.Split(new[] { "\r\n\r\n", "\n\n", "\r\r" }, StringSplitOptions.RemoveEmptyEntries);
            List<Transaccion> transacciones = new List<Transaccion>();

            foreach (string bloque in bloques)
            {
                string[] lineas = bloque.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (lineas.Length == 0)
                    continue;

                Dictionary<string, string> datos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (string linea in lineas)
                {
                    if (string.IsNullOrWhiteSpace(linea) || linea.Contains("--- Registro de Partida"))
                        continue;

                    if (linea.Contains('='))
                    {
                        int separador = linea.IndexOf('=');
                        string clave = linea.Substring(0, separador).Trim();
                        string valor = linea.Substring(separador + 1).Trim();
                        datos[clave] = valor;
                        continue;
                    }

                    if (DateTime.TryParse(linea, out DateTime fecha))
                    {
                        datos["Timestamp"] = fecha.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                }

                if (datos.Count == 0)
                    continue;

                int id = int.TryParse(datos.GetValueOrDefault("ID"), out int valorId) ? valorId : 0;
                int numeroTurno = int.TryParse(datos.GetValueOrDefault("NumeroTurno"), out int valorTurno) ? valorTurno : 0;
                TipoTransaccion tipo = Enum.TryParse<TipoTransaccion>(datos.GetValueOrDefault("Tipo"), out TipoTransaccion valorTipo) ? valorTipo : TipoTransaccion.PagoBanco;
                int monto = int.TryParse(datos.GetValueOrDefault("Monto"), out int valorMonto) ? valorMonto : 0;
                string descripcion = datos.GetValueOrDefault("Descripcion", string.Empty);
                string timestamp = datos.ContainsKey("Timestamp") ? datos["Timestamp"] : DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                string origenNombre = datos.GetValueOrDefault("JugadorOrigen", "Banco");
                string destinoNombre = datos.GetValueOrDefault("JugadorDestino", "Banco");

                Jugador? origen = null;
                Jugador? destino = null;

                if (!string.Equals(origenNombre, "Banco", StringComparison.OrdinalIgnoreCase))
                {
                    origen = new Jugador(0, "", origenNombre, new Casilla(0, "Inicio"));
                }

                if (!string.Equals(destinoNombre, "Banco", StringComparison.OrdinalIgnoreCase))
                {
                    destino = new Jugador(1, "", destinoNombre, new Casilla(0, "Inicio"));
                }

                DateTime fechaHora = DateTime.TryParse(timestamp, out DateTime parsed) ? parsed : DateTime.Now;

                transacciones.Add(new Transaccion(id, numeroTurno, tipo, origen ?? new Jugador(0, "", "Banco", new Casilla(0, "Inicio")), destino ?? new Jugador(1, "", "Banco", new Casilla(0, "Inicio")), monto, descripcion)
                {
                    FechaHora = fechaHora
                });
            }

            return transacciones.ToArray();
        }
    }
}
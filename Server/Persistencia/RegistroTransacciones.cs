using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            // Crea la carpeta de registros
            string carpeta = "Registros";
            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            // Crea el archivo de registro de esta partida
            string fechaHora = DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss");
            _rutaArchivo = Path.Combine(carpeta, $"Partida_{fechaHora}.txt");

            // Cabeza del archivo
            File.WriteAllText(_rutaArchivo, $"--- Registro de Partida Iniciado: {DateTime.Now} ---\n");
        }



        /// <summary>
        /// Guarda una transacción en el archivo de registro.
        /// </summary>
        public void RegistrarTransaccion(Transaccion transaccion)
        {
            // Caso de error
            if (string.IsNullOrEmpty(_rutaArchivo))
            {
                Console.WriteLine("[Registro] Error: El registro no ha sido iniciado.");
                return;
            }

            // Propiedades de la transacción
            string origen = transaccion.JugadorOrigen is null ? "Banco" : transaccion.JugadorOrigen.Nombre;
            string destino = transaccion.JugadorDestino is null ? "Banco" : transaccion.JugadorDestino.Nombre;
            string timestamp = transaccion.FechaHora.ToString("yyyy-MM-dd HH:mm:ss");

            // Cuerpo de la transacción
            string contenido = $"\n{timestamp}\n" +
                              $"Timestamp={timestamp}\n" +
                              $"ID={transaccion.ID}\n" +
                              $"NumeroTurno={transaccion.NumeroTurno}\n" +
                              $"Tipo={transaccion.Tipo}\n" +
                              $"JugadorOrigen={origen}\n" +
                              $"JugadorDestino={destino}\n" +
                              $"Monto={transaccion.Monto}\n" +
                              $"Descripcion={transaccion.Descripcion.Replace("\n", " ").Replace("\r", " ")}\n\n";

            // Guarda la transacción
            File.AppendAllText(_rutaArchivo, contenido);
            Console.WriteLine($"[Registro] Transacción {transaccion.ID} registrada correctamente.");
        }


        /// <summary>
        /// Obtiene la lista de transacciones del archivo de registro.
        /// </summary>
        /// <returns></returns>
        public Transaccion[] VerListaTransacciones()
        {
            // Caso de error
            if (string.IsNullOrEmpty(_rutaArchivo) || !File.Exists(_rutaArchivo))
            {
                return Array.Empty<Transaccion>();
            }

            // Contenido en bruto
            string contenido = File.ReadAllText(_rutaArchivo);
            if (string.IsNullOrWhiteSpace(contenido))
            {
                return Array.Empty<Transaccion>();
            }

            // Divide el contenido en bloques
            string[] bloques = contenido.Split(new[] { "\r\n\r\n", "\n\n", "\r\r" }, StringSplitOptions.RemoveEmptyEntries);
            List<Transaccion> transacciones = new List<Transaccion>();

            // Recorre los bloques (transacciones)
            foreach (string bloque in bloques)
            {
                string[] lineas = bloque.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (lineas.Length == 0)
                    continue;

                // Diccionario de datos
                Dictionary<string, string> datos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                
                // Recorre las lineas (propiedades)
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

                // Valores parseados de las propiedades
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

                // Procesamiento de datos
                if (!string.Equals(origenNombre, "Banco", StringComparison.OrdinalIgnoreCase))
                {
                    origen = new Jugador(0, "", origenNombre, new Casilla(0, "Inicio"));
                }

                if (!string.Equals(destinoNombre, "Banco", StringComparison.OrdinalIgnoreCase))
                {
                    destino = new Jugador(1, "", destinoNombre, new Casilla(0, "Inicio"));
                }

                DateTime fechaHora = DateTime.TryParse(timestamp, out DateTime parsed) ? parsed : DateTime.Now;

                // Agrega la transacción a la lista de búsqueda
                transacciones.Add(new Transaccion(id, numeroTurno, tipo, origen ?? new Jugador(0, "", "Banco", new Casilla(0, "Inicio")), destino ?? new Jugador(1, "", "Banco", new Casilla(0, "Inicio")), monto, descripcion)
                {
                    FechaHora = fechaHora
                });
            }

            // Devuelve la transacciones
            return transacciones.ToArray();
        }



        /// <summary>
        /// Obtiene la última transacción del archivo de registro.
        /// </summary>
        /// <returns></returns>
        public Transaccion VerUltimaTransaccion()
        {
            Transaccion[] transacciones = VerListaTransacciones();
            return transacciones.Length == 0 ? null! : transacciones[^1];
        }



        /// <summary>
        /// Obtiene la primera (más antigua) transacción del archivo de registro.
        /// </summary>
        /// <returns></returns>
        public Transaccion VerPrimeraTransaccion()
        {
            Transaccion[] transacciones = VerListaTransacciones();
            return transacciones.Length == 0 ? null! : transacciones[0];
        }



        /// <summary>
        /// Obtiene las transacciones realizadas por un jugador.
        /// </summary>
        /// <param name="nombreJugador"></param>
        /// <returns></returns>
        public Transaccion[] VerTransaccionesPorJugador(string nombreJugador)
        {
            if (string.IsNullOrWhiteSpace(nombreJugador))
            {
                return Array.Empty<Transaccion>();
            }

            // Procesamiento del nombre
            string nombreNormalizado = nombreJugador.Trim();
            return VerListaTransacciones()
                .Where(transaccion =>
                    string.Equals(transaccion.JugadorOrigen?.Nombre, nombreNormalizado, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(transaccion.JugadorDestino?.Nombre, nombreNormalizado, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }



        /// <summary>
        /// Obtiene las transacciones realizadas de un tipo en específico.
        /// </summary>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public Transaccion[] VerTransaccionesPorTipo(TipoTransaccion tipo)
        {
            return VerListaTransacciones()
                .Where(transaccion => transaccion.Tipo == tipo)
                .ToArray();
        }


        /// <summary>
        /// Función auxiliar para obtener las transacciones de un tipo, buscadas por nombre
        /// </summary>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public Transaccion[] VerTransaccionesPorTipo(string tipo)
        {
            if (string.IsNullOrWhiteSpace(tipo))
            {
                return Array.Empty<Transaccion>();
            }

            // Procesamiento del tipo
            string tipoNormalizado = tipo.Trim();
            if (Enum.TryParse<TipoTransaccion>(tipoNormalizado, true, out TipoTransaccion tipoExacto))
            {
                return VerTransaccionesPorTipo(tipoExacto);
            }

            // Busqueda de transacciones por coincidencia
            return tipoNormalizado.ToLowerInvariant() switch
            {
                "compra" => VerListaTransacciones().Where(t => t.Tipo == TipoTransaccion.CompraPropiedad || t.Tipo == TipoTransaccion.VentaPropiedad).ToArray(),
                "pago" => VerListaTransacciones().Where(t => t.Tipo == TipoTransaccion.PagoBanco || t.Tipo == TipoTransaccion.PagoAlquiler || t.Tipo == TipoTransaccion.PagoEntreJugadores).ToArray(),
                "evento" => VerListaTransacciones().Where(t => t.Tipo == TipoTransaccion.GananciaEvento || t.Tipo == TipoTransaccion.PerdidaEvento || t.Tipo == TipoTransaccion.PremioPorInicio).ToArray(),
                _ => Array.Empty<Transaccion>()
            };
        }
    }
}
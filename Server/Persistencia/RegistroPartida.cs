using System;
using System.IO;

namespace Server.Persistencia 
{
    public class RegistroPartida
    {
        private string _rutaArchivo = string.Empty;

        public void Iniciar()
        {
            string directorio = "LogsPartidas";
            if (!Directory.Exists(directorio))
            {
                Directory.CreateDirectory(directorio);
            }

            string fechaHora = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            _rutaArchivo = Path.Combine(directorio, $"Partida_{fechaHora}.txt");

            File.WriteAllText(_rutaArchivo, $"--- Registro de Partida Iniciado: {DateTime.Now} ---\n");
        }

        public void GuardarRegistro(string mensaje, string origen)
        {
            if (string.IsNullOrEmpty(_rutaArchivo))
            {
                Console.WriteLine("Error: El registro no ha sido iniciado.");
                return;
            }

            string horaActual = DateTime.Now.ToString("HH:mm:ss");
            string lineaRegistro = $"{horaActual}: [{origen}] {mensaje}";

            File.AppendAllText(_rutaArchivo, lineaRegistro + Environment.NewLine);
        }
    }
}
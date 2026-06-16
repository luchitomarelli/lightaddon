using System;
using System.IO;
using System.Text;

namespace CargaMasivaPOI
{
    /// <summary>
    /// Log simple a archivo de texto (igual que hacia el addon original con
    /// PuntosEmision_Log.txt). Cada linea queda con fecha y hora.
    /// </summary>
    public class Logger
    {
        private readonly string _ruta;

        public Logger(string carpeta)
        {
            _ruta = Path.Combine(carpeta, "PuntosEmision_Log.txt");
        }

        public void Escribir(string mensaje)
        {
            string linea = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + mensaje;
            try
            {
                File.AppendAllText(_ruta, linea + Environment.NewLine, Encoding.UTF8);
            }
            catch
            {
                // Si no se puede escribir el log, no rompemos la carga.
            }
        }
    }
}

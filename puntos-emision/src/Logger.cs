using System;
using System.IO;
using System.Text;

namespace CargaPuntosEmision
{
    /// <summary>Log simple a archivo de texto.</summary>
    public class Logger
    {
        private readonly string _ruta;

        public Logger(string carpeta)
        {
            _ruta = Path.Combine(carpeta, "PuntosEmisionDef_Log.txt");
        }

        public void Escribir(string mensaje)
        {
            string linea = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + mensaje;
            try { File.AppendAllText(_ruta, linea + Environment.NewLine, Encoding.UTF8); }
            catch { }
        }
    }
}

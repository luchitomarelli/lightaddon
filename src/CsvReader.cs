using System;
using System.Collections.Generic;
using System.IO;

namespace CargaMasivaPOI
{
    /// <summary>
    /// Lee el archivo CSV y lo convierte en una lista de SerieNumeracion.
    ///
    /// Formato (separado por ";", con encabezado). FirstNum y LastNum son OPCIONALES:
    /// si no los necesitas, podes dejarlos vacios o directamente no ponerlos.
    ///
    ///     Name;PTICode;Letter;FirstNum;LastNum   (5 columnas, completo)
    ///     Name;PTICode;Letter                    (3 columnas, sin numeracion)
    ///
    /// Es C# puro: nada de SDK de SAP aca. Por eso esta parte es la mas facil.
    /// </summary>
    public static class CsvReader
    {
        private const char Separador = ';';

        public static List<SerieNumeracion> Leer(string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo))
                throw new FileNotFoundException("No se encontro el archivo CSV: " + rutaArchivo);

            var resultado = new List<SerieNumeracion>();
            string[] lineas = File.ReadAllLines(rutaArchivo);

            // Arrancamos en i = 1 para saltar la fila de encabezados.
            for (int i = 1; i < lineas.Length; i++)
            {
                string linea = lineas[i].Trim();
                if (linea.Length == 0)
                    continue; // saltar lineas vacias

                string[] campos = linea.Split(Separador);
                if (campos.Length < 3)
                    throw new FormatException(
                        "La linea " + (i + 1) + " necesita al menos 3 columnas " +
                        "(Name;PTICode;Letter): " + linea);

                resultado.Add(new SerieNumeracion
                {
                    Name     = campos[0].Trim(),
                    PTICode  = campos[1].Trim(),
                    Letter   = campos[2].Trim(),
                    FirstNum = campos.Length > 3 ? campos[3].Trim() : "",   // opcional
                    LastNum  = campos.Length > 4 ? campos[4].Trim() : ""    // opcional
                });
            }

            return resultado;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace CargaMasivaPOI
{
    /// <summary>
    /// Lee el archivo CSV y lo convierte en una lista de SerieNumeracion.
    ///
    /// Formato esperado (separado por ";", con encabezado en la primer linea):
    ///
    ///     Name;PTICode;Letter;FirstNum;NextNum;LastNum
    ///     Ventas A 0001;0001;A;1;1;99999999
    ///     Ventas B 0001;0001;B;1;1;99999999
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
                if (campos.Length < 6)
                    throw new FormatException(
                        "La linea " + (i + 1) + " no tiene las 6 columnas esperadas " +
                        "(Name;PTICode;Letter;FirstNum;NextNum;LastNum): " + linea);

                var serie = new SerieNumeracion
                {
                    Name    = campos[0].Trim(),
                    PTICode = campos[1].Trim(),
                    Letter  = campos[2].Trim(),
                    FirstNum = ParseNum(campos[3], i + 1, "FirstNum"),
                    NextNum  = ParseNum(campos[4], i + 1, "NextNum"),
                    LastNum  = ParseNum(campos[5], i + 1, "LastNum")
                };

                resultado.Add(serie);
            }

            return resultado;
        }

        private static int ParseNum(string valor, int nroLinea, string columna)
        {
            int numero;
            if (!int.TryParse(valor.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out numero))
                throw new FormatException(
                    "El valor de '" + columna + "' en la linea " + nroLinea +
                    " no es un numero valido: '" + valor + "'");
            return numero;
        }
    }
}

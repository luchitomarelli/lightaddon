using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace CargaMasivaPOI
{
    /// <summary>
    /// Lee el archivo CSV y lo convierte en una lista de PuntoEmision.
    ///
    /// Formato esperado (separado por ";", con encabezado en la primer linea).
    /// La 3er columna (NumeroEmision) es OPCIONAL:
    ///
    ///     Codigo;Descripcion
    ///     0001;Casa Central - Facturacion A
    ///     0002;Sucursal Norte - Facturacion B
    ///
    /// Es C# puro: nada de SDK de SAP aca. Por eso esta parte es la mas facil.
    /// </summary>
    public static class CsvReader
    {
        private const char Separador = ';';

        public static List<PuntoEmision> Leer(string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo))
                throw new FileNotFoundException("No se encontro el archivo CSV: " + rutaArchivo);

            var resultado = new List<PuntoEmision>();
            string[] lineas = File.ReadAllLines(rutaArchivo);

            // Arrancamos en i = 1 para saltar la fila de encabezados.
            for (int i = 1; i < lineas.Length; i++)
            {
                string linea = lineas[i].Trim();
                if (linea.Length == 0)
                    continue; // saltar lineas vacias

                string[] campos = linea.Split(Separador);
                if (campos.Length < 2)
                    throw new FormatException(
                        "La linea " + (i + 1) + " no tiene al menos 2 columnas (Codigo;Descripcion): " + linea);

                var punto = new PuntoEmision
                {
                    Codigo = campos[0].Trim(),
                    Descripcion = campos[1].Trim()
                };

                // 3er columna (numero de emision) es opcional.
                if (campos.Length >= 3 && campos[2].Trim().Length > 0)
                {
                    int numero;
                    if (!int.TryParse(campos[2].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out numero))
                        throw new FormatException(
                            "El numero de emision de la linea " + (i + 1) + " no es valido: '" + campos[2] + "'");
                    punto.NumeroEmision = numero;
                }

                resultado.Add(punto);
            }

            return resultado;
        }
    }
}

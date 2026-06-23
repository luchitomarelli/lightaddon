using System;
using System.Collections.Generic;
using System.IO;

namespace CargaPuntosEmision
{
    /// <summary>
    /// Lee el CSV de Puntos de Emision.
    ///
    /// Formato (separado por ";", con encabezado):
    ///     Codigo;Descripcion;Tipo;FechaOpInicial
    ///     0997;0997;Fiscal;01/01/2026
    ///     10000;10000;Electronico domestico;01/01/2026
    /// </summary>
    public static class CsvReader
    {
        private const char Separador = ';';

        public static List<PuntoEmisionDef> Leer(string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo))
                throw new FileNotFoundException("No se encontro el archivo CSV: " + rutaArchivo);

            var resultado = new List<PuntoEmisionDef>();
            string[] lineas = File.ReadAllLines(rutaArchivo);

            for (int i = 1; i < lineas.Length; i++) // i=1 salta el encabezado
            {
                string linea = lineas[i].Trim();
                if (linea.Length == 0) continue;

                string[] campos = linea.Split(Separador);
                if (campos.Length < 4)
                    throw new FormatException(
                        "La linea " + (i + 1) + " no tiene las 4 columnas esperadas " +
                        "(Codigo;Descripcion;Tipo;FechaOpInicial): " + linea);

                resultado.Add(new PuntoEmisionDef
                {
                    Codigo         = campos[0].Trim(),
                    Descripcion    = campos[1].Trim(),
                    Tipo           = campos[2].Trim(),
                    FechaOpInicial = campos[3].Trim()
                });
            }

            return resultado;
        }
    }
}

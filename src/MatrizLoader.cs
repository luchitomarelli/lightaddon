using System;
using System.Collections.Generic;
using SAPbouiCOM;

namespace CargaMasivaPOI
{
    /// <summary>
    /// Carga las series ESCRIBIENDO EN LAS CELDAS de la matriz (como tipear a mano).
    /// La pantalla de Series de Numeracion es del sistema y NO permite escribir
    /// directo en su origen de datos (da "The item is not a user-defined item"),
    /// asi que vamos celda por celda.
    ///
    /// Pantalla: Series de Numeracion (matriz Item "3").
    /// Columnas: Name, PTICode, Letter (desplegable), FirstNum, LastNum.
    /// NextNum NO se escribe: lo autocompleta SAP a partir de FirstNum.
    ///
    /// Es resistente: si una celda no se puede escribir, lo anota en el log y sigue.
    /// NO guarda solo: deja las filas para que las revises y guardes vos (Ctrl+A).
    /// </summary>
    public class MatrizLoader
    {
        private const string MatrizItem  = "3";
        private const string ColName     = "Name";
        private const string ColPtiCode  = "PTICode";
        private const string ColLetter   = "Letter";
        private const string ColFirstNum = "FirstNum";
        private const string ColLastNum  = "LastNum";

        private readonly Application _app;
        private readonly Logger _logger;

        public MatrizLoader(Application app, Logger logger)
        {
            _app = app;
            _logger = logger;
        }

        public int Cargar(List<SerieNumeracion> series)
        {
            Form form = _app.Forms.ActiveForm;
            if (form == null)
                throw new Exception("No hay ningun formulario activo. Abri la pantalla de Series de Numeracion.");

            Matrix matriz;
            try
            {
                matriz = (Matrix)form.Items.Item(MatrizItem).Specific;
            }
            catch
            {
                throw new Exception(
                    "No se encontro la matriz (Item " + MatrizItem + ") en el formulario activo.\n" +
                    "Abri la pantalla de Series de Numeracion y dejala activa antes de cargar.");
            }

            int escritas = 0;
            foreach (SerieNumeracion s in series)
            {
                matriz.AddRow();                 // agrega una fila vacia al final
                int fila = matriz.RowCount;      // numero de la ultima fila

                SetCelda(matriz, ColName,     fila, s.Name);
                SetCelda(matriz, ColPtiCode,  fila, s.PTICode);
                SetCelda(matriz, ColLetter,   fila, s.Letter);
                SetCelda(matriz, ColFirstNum, fila, s.FirstNum.ToString());
                SetCelda(matriz, ColLastNum,  fila, s.LastNum.ToString());

                escritas++;
                _logger.Escribir("FILA " + fila + ": " + s);
            }

            // NO guarda solo: revisa las filas y guarda vos (Ctrl+A).
            return escritas;
        }

        private void SetCelda(Matrix matriz, string columna, int fila, string valor)
        {
            // Una celda puede ser texto (EditText) o desplegable (ComboBox).
            // Si no se puede escribir, lo anotamos y seguimos (no cortamos todo).
            try
            {
                object specific = matriz.Columns.Item(columna).Cells.Item(fila).Specific;

                var combo = specific as ComboBox;
                if (combo != null)
                {
                    combo.Select(valor, BoSearchKey.psk_ByValue);
                    return;
                }

                var edit = specific as EditText;
                if (edit != null)
                {
                    edit.Value = valor;
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.Escribir("  AVISO: columna '" + columna + "' fila " + fila +
                    " no se pudo cargar: " + ex.Message);
            }
        }
    }
}

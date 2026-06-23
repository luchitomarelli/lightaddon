using System;
using System.Collections.Generic;
using SAPbouiCOM;

namespace CargaPuntosEmision
{
    /// <summary>
    /// Carga los Puntos de Emision escribiendo en las celdas de la matriz de la
    /// pantalla "Puntos de emision - Definiciones" (Form 234000028, matriz Item "3",
    /// tabla OPTI).
    ///
    /// Columnas: Code (texto), Desc (texto), Type (desplegable), SOpDate (fecha).
    ///
    /// IMPORTANTE: la pantalla tiene que estar ABIERTA y ACTIVA al ejecutar.
    /// Es resistente: si una celda falla, la anota en el log y sigue.
    /// NO guarda solo: revisa y guarda vos (Ctrl+A).
    /// </summary>
    public class MatrizLoader
    {
        private const string MatrizItem = "3";
        private const string ColCodigo  = "Code";
        private const string ColDescrip = "Desc";
        private const string ColTipo    = "Type";     // desplegable
        private const string ColFecha   = "SOpDate";  // fecha

        private readonly Application _app;
        private readonly Logger _logger;

        public MatrizLoader(Application app, Logger logger)
        {
            _app = app;
            _logger = logger;
        }

        public int Cargar(List<PuntoEmisionDef> puntos)
        {
            Form form = _app.Forms.ActiveForm;
            if (form == null)
                throw new Exception("No hay ningun formulario activo. Abri la pantalla de Puntos de Emision.");

            Matrix matriz;
            try
            {
                matriz = (Matrix)form.Items.Item(MatrizItem).Specific;
            }
            catch
            {
                throw new Exception(
                    "No se encontro la matriz (Item " + MatrizItem + ") en el formulario activo.\n" +
                    "Abri la pantalla de Puntos de Emision y dejala activa antes de cargar.");
            }

            int escritas = 0;
            foreach (PuntoEmisionDef p in puntos)
            {
                matriz.AddRow();
                int fila = matriz.RowCount;

                // Orden recomendado: codigo, descripcion, tipo (desbloquea), fecha.
                SetCelda(matriz, ColCodigo,  fila, p.Codigo);
                SetCelda(matriz, ColDescrip, fila, p.Descripcion);
                SetCelda(matriz, ColTipo,    fila, p.Tipo);
                SetCelda(matriz, ColFecha,   fila, p.FechaOpInicial);

                escritas++;
                _logger.Escribir("FILA " + fila + ": " + p);
            }

            return escritas; // NO guarda solo
        }

        private void SetCelda(Matrix matriz, string columna, int fila, string valor)
        {
            try
            {
                object specific = matriz.Columns.Item(columna).Cells.Item(fila).Specific;

                var combo = specific as ComboBox;
                if (combo != null)
                {
                    // Desplegable (Tipo): probamos por valor; si no, por descripcion (texto visible).
                    try { combo.Select(valor, BoSearchKey.psk_ByValue); }
                    catch { combo.Select(valor, BoSearchKey.psk_ByDescription); }
                    return;
                }

                var edit = specific as EditText;
                if (edit != null)
                {
                    edit.Value = valor;   // texto o fecha (ej "01/01/2026")
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

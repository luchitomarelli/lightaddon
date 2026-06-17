using System;
using System.Collections.Generic;
using SAPbouiCOM;

namespace CargaMasivaPOI
{
    /// <summary>
    /// Carga las series de numeracion escribiendo DIRECTO en el origen de datos
    /// (DBDataSource "OFNS") que esta detras de la matriz, y despues refresca la
    /// grilla. Es mas robusto que tipear celda por celda: evita el error
    /// "Form item is not editable" y no deja filas placeholder.
    ///
    /// Pantalla: Series de Numeracion (matriz Item "3", tabla OFNS).
    /// Campos: Name, PTICode, Letter, FirstNum, NextNum, LastNum.
    ///
    /// IMPORTANTE: la pantalla tiene que estar ABIERTA y ACTIVA al ejecutar.
    /// NO guarda solo: deja las filas cargadas para que las revises y guardes vos
    /// (Ctrl+A / Actualizar). Cuando confirmes que esta todo bien, se puede activar
    /// el guardado automatico (ver el final de Cargar()).
    /// </summary>
    public class MatrizLoader
    {
        private const string MatrizItem    = "3";      // Item de la matriz
        private const string DataSourceOFNS = "OFNS";  // origen de datos enlazado

        // Nombres de campo en OFNS (coinciden con "Informacion del sistema").
        private const string FName     = "Name";
        private const string FPtiCode  = "PTICode";
        private const string FLetter   = "Letter";
        private const string FFirstNum = "FirstNum";
        private const string FNextNum  = "NextNum";
        private const string FLastNum  = "LastNum";

        // Boton "Agregar/Actualizar" (por si se activa el guardado automatico).
        private const string BotonOk = "1";

        private readonly Application _app;
        private readonly Logger _logger;

        public MatrizLoader(Application app, Logger logger)
        {
            _app = app;
            _logger = logger;
        }

        /// <summary>
        /// Carga todas las series en la matriz del formulario activo.
        /// Devuelve cuantas filas cargo.
        /// </summary>
        public int Cargar(List<SerieNumeracion> series)
        {
            // 1) Formulario activo.
            Form form = _app.Forms.ActiveForm;
            if (form == null)
                throw new Exception("No hay ningun formulario activo. Abri la pantalla de Series de Numeracion.");

            // 2) Matriz (para refrescar al final).
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

            // 3) Origen de datos enlazado (OFNS).
            DBDataSource dbs;
            try
            {
                dbs = form.DataSources.DBDataSources.Item(DataSourceOFNS);
            }
            catch
            {
                throw new Exception(
                    "No se encontro el origen de datos '" + DataSourceOFNS + "' en el formulario activo.");
            }

            // Primero pasamos lo que haya tipeado a mano al origen de datos.
            matriz.FlushToDataSource();

            // 4) Insertar cada serie como un registro nuevo al final del origen.
            int escritas = 0;
            foreach (SerieNumeracion s in series)
            {
                int idx = dbs.Size;        // posicion del nuevo registro (al final)
                dbs.InsertRecord(idx);     // inserta un registro vacio
                dbs.Offset = idx;

                dbs.SetValue(FName,     idx, s.Name);
                dbs.SetValue(FPtiCode,  idx, s.PTICode);
                dbs.SetValue(FLetter,   idx, s.Letter);
                dbs.SetValue(FFirstNum, idx, s.FirstNum.ToString());
                dbs.SetValue(FNextNum,  idx, s.NextNum.ToString());
                dbs.SetValue(FLastNum,  idx, s.LastNum.ToString());

                escritas++;
                _logger.Escribir("PREPARADO " + s);
            }

            // 5) Refrescar la grilla para que muestre los registros nuevos.
            matriz.LoadFromDataSource();

            // 6) NO guardamos automaticamente: revisa las filas y guarda vos (Ctrl+A).
            //    Cuando confirmes que carga bien, descomenta la linea de abajo para
            //    que guarde solo:
            // form.Items.Item(BotonOk).Click(BoCellClickType.ct_Regular);

            return escritas;
        }
    }
}

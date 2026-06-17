using System;
using System.Collections.Generic;
using SAPbouiCOM;

namespace CargaMasivaPOI
{
    /// <summary>
    /// Carga las series de numeracion ESCRIBIENDO EN LA MATRIZ de la pantalla,
    /// igual que hacia el addon original. NO inserta en la base directo: llena la
    /// grilla fila por fila (como si lo tipearas a mano) y deja que SAP guarde.
    ///
    /// Destino: tabla OFNS (Series de Numeracion).
    /// Columnas de la matriz que escribimos:
    ///   Name, PTICode, Letter, FirstNum, NextNum, LastNum
    ///
    /// IMPORTANTE:
    ///  - La pantalla de Series de Numeracion tiene que estar ABIERTA y ACTIVA.
    ///  - Verifica el UID de la matriz (MatrizItem) y de las columnas con
    ///    "Ver > Informacion del sistema" (abajo a la izquierda te muestra
    ///    Form / Item / Column). Si en tu pantalla difieren, ajustalos aca.
    /// </summary>
    public class MatrizLoader
    {
        // ---- IDs de la pantalla (AJUSTAR si tu Informacion del sistema dice otra cosa) ----
        private const string MatrizItem = "3";   // Item de la matriz

        // UIDs de las columnas (suelen coincidir con el nombre del campo de OFNS)
        private const string ColName     = "Name";
        private const string ColPtiCode  = "PTICode";
        private const string ColLetter   = "Letter";
        private const string ColFirstNum = "FirstNum";
        private const string ColNextNum  = "NextNum";
        private const string ColLastNum  = "LastNum";

        // Boton "Agregar/Actualizar" del formulario (equivale a Ctrl+A).
        private const string BotonOk = "1";

        private readonly Application _app;

        public MatrizLoader(Application app)
        {
            _app = app;
        }

        /// <summary>
        /// Escribe todas las series en la matriz del formulario activo.
        /// Devuelve cuantas filas escribio.
        /// </summary>
        public int Cargar(List<SerieNumeracion> series)
        {
            // 1) Agarrar el formulario que esta abierto y activo.
            Form form = _app.Forms.ActiveForm;
            if (form == null)
                throw new Exception("No hay ningun formulario activo. Abri la pantalla de Series de Numeracion.");

            // 2) Obtener la matriz. Si falla, es que no es la pantalla correcta.
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

            // 3) Escribir cada serie en una fila nueva.
            int escritas = 0;
            foreach (SerieNumeracion s in series)
            {
                matriz.AddRow();                 // agrega una fila vacia al final
                int fila = matriz.RowCount;      // numero de la ultima fila

                SetCelda(matriz, ColName,     fila, s.Name);
                SetCelda(matriz, ColPtiCode,  fila, s.PTICode);
                SetCelda(matriz, ColLetter,   fila, s.Letter);
                SetCelda(matriz, ColFirstNum, fila, s.FirstNum.ToString());
                SetCelda(matriz, ColNextNum,  fila, s.NextNum.ToString());
                SetCelda(matriz, ColLastNum,  fila, s.LastNum.ToString());

                escritas++;
            }

            // 4) Empujar lo escrito al origen de datos enlazado (OFNS).
            //    Necesario en matrices enlazadas: sin esto SAP a veces no "ve"
            //    los valores que cargamos por codigo.
            matriz.FlushToDataSource();

            // 5) Apretar "Agregar/Actualizar" para que SAP guarde todo.
            form.Items.Item(BotonOk).Click(BoCellClickType.ct_Regular);

            return escritas;
        }

        private void SetCelda(Matrix matriz, string columna, int fila, string valor)
        {
            // Una celda de matriz puede ser texto (EditText) o un desplegable (ComboBox).
            // Detectamos el tipo y la cargamos de la forma correcta.
            object specific = matriz.Columns.Item(columna).Cells.Item(fila).Specific;

            var combo = specific as ComboBox;
            if (combo != null)
            {
                // Columna desplegable (ej: Carta, Tipo de POI): seleccionar por valor.
                combo.Select(valor, BoSearchKey.psk_ByValue);
                return;
            }

            var edit = specific as EditText;
            if (edit != null)
            {
                // Columna de texto (ej: Nombre, Codigo POI, numeros).
                edit.Value = valor;
                return;
            }
        }
    }
}

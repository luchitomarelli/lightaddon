using System;
using System.Collections.Generic;
using SAPbouiCOM;

namespace CargaMasivaPOI
{
    /// <summary>
    /// Carga los puntos ESCRIBIENDO EN LA MATRIZ de la pantalla de Puntos de Emision,
    /// igual que hacia el addon original. NO inserta en la base directo: llena la
    /// grilla fila por fila (como si lo tipearas a mano) y deja que SAP guarde.
    ///
    /// IDs reales de la pantalla (sacados con Ver > Informacion del sistema):
    ///   - Formulario: 234000028   (tabla detras: OPTI)
    ///   - Matriz:     Item "3"
    ///   - Columnas:   "Code" (codigo) y "Desc" (descripcion)
    ///
    /// IMPORTANTE: la pantalla de Puntos de Emision tiene que estar ABIERTA y ACTIVA
    /// cuando se ejecuta la carga.
    /// </summary>
    public class MatrizLoader
    {
        private const string MatrizItem = "3";       // Item de la matriz
        private const string ColCodigo  = "Code";    // columna Codigo
        private const string ColDescrip = "Desc";    // columna Descripcion
        // private const string ColNroEmis = "????"; // <-- columna del numero de emision (falta el UID)

        // Boton "Agregar/Actualizar" del formulario (equivale a Ctrl+A).
        private const string BotonOk = "1";

        private readonly Application _app;

        public MatrizLoader(Application app)
        {
            _app = app;
        }

        /// <summary>
        /// Escribe todos los puntos en la matriz del formulario activo.
        /// Devuelve cuantas filas escribio.
        /// </summary>
        public int Cargar(List<PuntoEmision> puntos)
        {
            // 1) Agarrar el formulario que esta abierto y activo.
            Form form = _app.Forms.ActiveForm;
            if (form == null)
                throw new Exception("No hay ningun formulario activo. Abri la pantalla de Puntos de Emision.");

            // 2) Obtener la matriz (Item 3). Si falla, es que no es la pantalla correcta.
            Matrix matriz;
            try
            {
                matriz = (Matrix)form.Items.Item(MatrizItem).Specific;
            }
            catch
            {
                throw new Exception(
                    "No se encontro la matriz en el formulario activo.\n" +
                    "Abri la pantalla de Puntos de Emision y dejala activa antes de cargar.");
            }

            // 3) Escribir cada punto en una fila nueva.
            int escritas = 0;
            foreach (PuntoEmision p in puntos)
            {
                matriz.AddRow();                 // agrega una fila vacia al final
                int fila = matriz.RowCount;      // numero de la ultima fila

                SetCelda(matriz, ColCodigo,  fila, p.Codigo);
                SetCelda(matriz, ColDescrip, fila, p.Descripcion);

                // Cuando tengas el UID de la columna del numero de emision, descomenta:
                // SetCelda(matriz, ColNroEmis, fila, p.NumeroEmision.ToString());

                escritas++;
            }

            // 4) Apretar "Agregar/Actualizar" para que SAP guarde todo.
            form.Items.Item(BotonOk).Click(BoCellClickType.ct_Regular);

            return escritas;
        }

        private void SetCelda(Matrix matriz, string columna, int fila, string valor)
        {
            var celda = (EditText)matriz.Columns.Item(columna).Cells.Item(fila).Specific;
            celda.Value = valor;
        }
    }
}

namespace CargaPuntosEmision
{
    /// <summary>
    /// Una fila del CSV: un Punto de Emision (tabla OPTL/OPTI).
    /// </summary>
    public class PuntoEmisionDef
    {
        /// <summary>Codigo del punto (columna Code).</summary>
        public string Codigo { get; set; }

        /// <summary>Descripcion (columna Desc).</summary>
        public string Descripcion { get; set; }

        /// <summary>Tipo: Fiscal / Electronico domestico / Impresion domestica (columna Type, desplegable).</summary>
        public string Tipo { get; set; }

        /// <summary>Fecha de operacion inicial (columna SOpDate). Se guarda como texto, ej: "01/01/2026".</summary>
        public string FechaOpInicial { get; set; }

        public override string ToString()
        {
            return Codigo + " - " + Descripcion + " (" + Tipo + ", desde " + FechaOpInicial + ")";
        }
    }
}

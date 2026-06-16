namespace CargaMasivaPOI
{
    /// <summary>
    /// Representa una fila del CSV: un Punto de Emision con su numero.
    ///
    /// Esta es la diferencia con el addon original (que solo cargaba el POI):
    /// aca ademas guardamos el NUMERO DE EMISION (el ultimo numero usado / proximo).
    /// </summary>
    public class PuntoEmision
    {
        /// <summary>Codigo del punto de emision. Ej: "0001".</summary>
        public string Codigo { get; set; }

        /// <summary>Descripcion. Ej: "Casa Central - Facturacion A".</summary>
        public string Descripcion { get; set; }

        /// <summary>
        /// Numero de emision asociado al punto.
        /// (lo nuevo respecto del addon de ejemplo)
        /// </summary>
        public int NumeroEmision { get; set; }

        public override string ToString()
        {
            string texto = Codigo + " - " + Descripcion;
            if (NumeroEmision > 0)
                texto += " (Nro emision: " + NumeroEmision + ")";
            return texto;
        }
    }
}

namespace CargaMasivaPOI
{
    /// <summary>
    /// Representa una Serie de Numeracion (tabla OFNS de SAP B1).
    /// Es una fila del CSV que vamos a cargar en la pantalla.
    /// </summary>
    public class SerieNumeracion
    {
        /// <summary>Nombre de la serie (campo Name).</summary>
        public string Name { get; set; }

        /// <summary>Codigo del punto de emision (campo PTICode).</summary>
        public string PTICode { get; set; }

        /// <summary>Letra de la serie: A, B, C... (campo Letter).</summary>
        public string Letter { get; set; }

        /// <summary>Primer numero de la serie (campo FirstNum).</summary>
        public int FirstNum { get; set; }

        /// <summary>Proximo numero a usar (campo NextNum).</summary>
        public int NextNum { get; set; }

        /// <summary>Ultimo numero de la serie (campo LastNum).</summary>
        public int LastNum { get; set; }

        public override string ToString()
        {
            return Name + " (PTI " + PTICode + ", Letra " + Letter +
                   ", " + FirstNum + "/" + NextNum + "/" + LastNum + ")";
        }
    }
}

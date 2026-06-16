using System;
using SAPbobsCOM;

namespace CargaMasivaPOI
{
    /// <summary>
    /// Crea (si no existe) la tabla de usuario donde guardamos los puntos de emision.
    ///
    /// En SAP B1, una "UDT" (User Defined Table) es una tabla propia tuya. Se crea
    /// con la DI API usando los objetos de METADATA (UserTablesMD / UserFieldsMD).
    /// Una vez creada, en la base aparece como "@POI_PEMISION" y los campos como
    /// "U_NroEmision".
    ///
    /// Hacemos que el addon cree su propia tabla para que el ejemplo FUNCIONE en
    /// cualquier SAP B1 10, sin depender de pantallas de localizacion. Cuando ya
    /// entiendas como anda, podes cambiar el destino por el objeto/pantalla real.
    /// </summary>
    public class UdtManager
    {
        // Nombre de la tabla SIN el "@" (SAP lo agrega solo).
        public const string Tabla = "POI_PEMISION";
        public const string CampoNroEmision = "NroEmision"; // queda como U_NroEmision

        private readonly Company _company;

        public UdtManager(Company company)
        {
            _company = company;
        }

        public void AsegurarEstructura()
        {
            CrearTablaSiNoExiste();
            CrearCampoNroEmisionSiNoExiste();
        }

        private void CrearTablaSiNoExiste()
        {
            var tablaMd = (UserTablesMD)_company.GetBusinessObject(BoObjectTypes.oUserTables);
            try
            {
                if (tablaMd.GetByKey(Tabla))
                    return; // ya existe, no hacemos nada

                tablaMd.TableName = Tabla;
                tablaMd.TableDescription = "Puntos de Emision (addon Carga Masiva POI)";
                // bott_NoObject = tabla "suelta", no ligada a un objeto de negocio.
                tablaMd.TableType = BoUTBTableType.bott_NoObject;

                int res = tablaMd.Add();
                if (res != 0)
                    throw new Exception("No se pudo crear la tabla @" + Tabla + ": " + _company.GetLastErrorDescription());
            }
            finally
            {
                // Importante en COM: liberar el objeto para no dejar memoria colgada.
                System.Runtime.InteropServices.Marshal.ReleaseComObject(tablaMd);
            }
        }

        private void CrearCampoNroEmisionSiNoExiste()
        {
            // La tabla ya trae de fabrica "Code" (codigo) y "Name" (descripcion).
            // Solo agregamos el campo extra que pide el requisito: el numero de emision.
            //
            // Para saber si ya existe el campo deberiamos consultar la tabla CUFD,
            // pero esa consulta cambia entre HANA y SQL Server. Es mas simple y
            // robusto INTENTAR crearlo y, si SAP responde que ya existe, ignorarlo.
            var campoMd = (UserFieldsMD)_company.GetBusinessObject(BoObjectTypes.oUserFields);
            try
            {
                campoMd.TableName = Tabla;                 // aca SIN "@"
                campoMd.Name = CampoNroEmision;            // queda como U_NroEmision
                campoMd.Description = "Numero de emision";
                campoMd.Type = BoFieldTypes.db_Numeric;
                campoMd.EditSize = 11;

                int res = campoMd.Add();
                if (res != 0)
                {
                    // -2035 = "el campo ya existe". Cualquier otro error si es real.
                    int codigoError = _company.GetLastErrorCode();
                    if (codigoError != -2035)
                        throw new Exception("No se pudo crear el campo U_" + CampoNroEmision +
                            ": " + _company.GetLastErrorDescription());
                }
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(campoMd);
            }
        }
    }
}

using System;
using SAPbobsCOM;

namespace CargaMasivaPOI
{
    /// <summary>
    /// Guarda los puntos de emision en la tabla de usuario usando la DI API.
    ///
    /// Esta es la parte mas "valiosa" de aprender: como escribir datos en SAP B1
    /// desde codigo, de forma transaccional y con manejo de errores.
    /// </summary>
    public class PuntoEmisionRepository
    {
        private readonly Company _company;

        public PuntoEmisionRepository(Company company)
        {
            _company = company;
        }

        /// <summary>
        /// Inserta el punto, o lo actualiza si el codigo ya existe.
        /// Devuelve true si grabo bien; si no, deja el error en 'error'.
        /// </summary>
        public bool Guardar(PuntoEmision punto, out string error)
        {
            error = null;

            // UserTables es la coleccion de tablas de usuario "sueltas".
            UserTable udt = _company.UserTables.Item(UdtManager.Tabla);

            bool existe = udt.GetByKey(punto.Codigo);

            udt.Code = punto.Codigo;             // clave
            udt.Name = punto.Descripcion;        // descripcion
            udt.UserFields.Fields.Item("U_" + UdtManager.CampoNroEmision).Value = punto.NumeroEmision;

            int res = existe ? udt.Update() : udt.Add();

            if (res != 0)
            {
                error = _company.GetLastErrorCode() + ": " + _company.GetLastErrorDescription();
                return false;
            }

            return true;
        }
    }
}

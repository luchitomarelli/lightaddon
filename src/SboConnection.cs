using System;
using SAPbouiCOM;

namespace CargaMasivaPOI
{
    /// <summary>
    /// Maneja la conexion con SAP Business One.
    ///
    /// Hay DOS APIs y este addon usa las dos:
    ///   - UI API  (SAPbouiCOM): la interfaz de usuario (menus, formularios, eventos).
    ///   - DI  API (SAPbobsCOM): los datos / objetos de negocio (lo que escribe en la base).
    ///
    /// El truco importante: NO hacemos un login con usuario y clave. Nos colgamos de
    /// la sesion que ya tiene abierta el cliente de SAP, y desde la UI API pedimos
    /// "prestada" la Company de la DI API. Asi heredamos la conexion, la empresa y el
    /// usuario que ya estan logueados.
    /// </summary>
    public class SboConnection
    {
        /// <summary>La aplicacion de UI API (la GUI de SAP B1).</summary>
        public Application Application { get; private set; }

        /// <summary>La Company de la DI API (para leer/grabar datos).</summary>
        public SAPbobsCOM.Company Company { get; private set; }

        public void Connect()
        {
            // SAP nos pasa la connection string como primer argumento del .exe.
            // En modo desarrollo (F5 en Visual Studio) no hay argumento, asi que
            // usamos una cadena fija que le dice "conectate al cliente abierto".
            string connectionString;
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                connectionString = args[1];
            }
            else
            {
                // Cadena estandar para depurar con un cliente de SAP ya abierto.
                connectionString =
                    "0030002C0030002C00530041005000420044005F00440061007400650076002C0050004C006F006D0044006200";
            }

            // 1) Conexion a la UI API.
            var sboGuiApi = new SboGuiApi();
            sboGuiApi.Connect(connectionString);
            Application = sboGuiApi.GetApplication();

            // 2) Pedir la Company de la DI API a traves de la UI API.
            //    Esto evita tener que hacer Company.Connect() con credenciales.
            Company = (SAPbobsCOM.Company)Application.Company.GetDICompany();

            Application.StatusBar.SetText(
                "Add-on Carga Masiva POI conectado.",
                BoMessageTime.bmt_Short,
                BoStatusBarMessageType.smt_Success);
        }
    }
}

using System;
using SAPbouiCOM;

namespace CargaPuntosEmision
{
    /// <summary>
    /// Conexion con SAP Business One: se engancha a la sesion abierta del cliente.
    /// </summary>
    public class SboConnection
    {
        public Application Application { get; private set; }
        public SAPbobsCOM.Company Company { get; private set; }

        public void Connect()
        {
            string[] args = Environment.GetCommandLineArgs();
            string connectionString = args.Length > 1
                ? args[1]
                : "0030002C0030002C00530041005000420044005F00440061007400650076002C0050004C006F006D0044006200";

            var sboGuiApi = new SboGuiApi();
            sboGuiApi.Connect(connectionString);
            Application = sboGuiApi.GetApplication();

            Company = (SAPbobsCOM.Company)Application.Company.GetDICompany();

            Application.StatusBar.SetText(
                "Add-on Carga Puntos de Emision conectado.",
                BoMessageTime.bmt_Short,
                BoStatusBarMessageType.smt_Success);
        }
    }
}

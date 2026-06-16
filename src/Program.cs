using System;
using System.Windows.Forms;

namespace CargaMasivaPOI
{
    /// <summary>
    /// Punto de entrada del add-on.
    ///
    /// Como es un add-on "light" de UI API, SAP Business One arranca este .exe y le
    /// pasa por linea de comandos una "connection string" para engancharse a la
    /// sesion del cliente que ya esta abierta. Nosotros no hacemos login: nos
    /// conectamos a la GUI viva.
    /// </summary>
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            // Necesario para poder usar dialogos de System.Windows.Forms
            // (por ejemplo el OpenFileDialog para elegir el CSV).
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // 1) Conectarse a SAP B1 (UI API) y obtener la Company de la DI API.
                var conexion = new SboConnection();
                conexion.Connect();

                // 2) Crear el add-on, que registra el menu y los eventos.
                var addon = new Addon(conexion);
                addon.Inicializar();

                // 3) Quedarse vivo escuchando eventos de SAP.
                //    SAPbouiCOM.Framework normalmente hace esto por vos; aca lo
                //    hacemos a mano con el bucle de mensajes de WinForms para que
                //    se vea como funciona por dentro.
                Application.Run();
            }
            catch (Exception ex)
            {
                // Si fallo antes de conectar, no tenemos GUI de SAP para avisar:
                // mostramos un cuadro de Windows y salimos.
                MessageBox.Show(
                    "No se pudo iniciar el add-on Carga Masiva POI.\n\n" + ex.Message,
                    "Carga Masiva POI",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}

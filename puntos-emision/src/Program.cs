using System;
using System.Windows.Forms;

namespace CargaPuntosEmision
{
    /// <summary>
    /// Punto de entrada del add-on (carga de Puntos de Emision / tabla OPTI).
    /// </summary>
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                var conexion = new SboConnection();
                conexion.Connect();

                var addon = new Addon(conexion);
                addon.Inicializar();

                Application.Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo iniciar el add-on Carga Puntos de Emision.\n\n" + ex.Message,
                    "Carga Puntos de Emision",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}

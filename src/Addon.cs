using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using SAPbouiCOM;

namespace CargaMasivaPOI
{
    /// <summary>
    /// El cerebro del add-on. Hace tres cosas:
    ///   1) Agrega un item de menu en SAP ("Carga Masiva POI...").
    ///   2) Escucha el evento de click de ese menu.
    ///   3) Cuando se hace click, lee el CSV y graba los puntos de emision.
    /// </summary>
    public class Addon
    {
        // ID unico de nuestro item de menu. Lo usamos para crearlo y para
        // reconocer su click en el evento.
        private const string MenuUidCargar = "POI_Cargar";

        // "43520" es el UID del menu "Modulos" de SAP B1. Colgamos ahi nuestro item.
        private const string MenuModulos = "43520";

        private readonly SboConnection _conexion;
        private readonly Application _app;
        private readonly SAPbobsCOM.Company _company;

        public Addon(SboConnection conexion)
        {
            _conexion = conexion;
            _app = conexion.Application;
            _company = conexion.Company;
        }

        public void Inicializar()
        {
            CrearMenu();

            // Suscribirse a los eventos de menu de SAP.
            _app.MenuEvent += App_MenuEvent;

            // Tambien escuchamos el evento de "App" para poder cerrar el addon
            // limpiamente si el usuario cierra SAP B1.
            _app.AppEvent += App_AppEvent;
        }

        // ---------------------------------------------------------------------
        // MENU
        // ---------------------------------------------------------------------
        private void CrearMenu()
        {
            try
            {
                Menus menusModulos = _app.Menus.Item(MenuModulos).SubMenus;

                // Si ya existe (por una corrida anterior), no lo agregamos de nuevo.
                if (ExisteMenu(MenuUidCargar))
                    return;

                var creationParams = (MenuCreationParams)_app.CreateObject(BoCreatableObjectType.cot_MenuCreationParams);
                creationParams.Type = BoMenuType.mt_STRING;
                creationParams.UniqueID = MenuUidCargar;
                creationParams.String = "Carga Masiva POI...";
                creationParams.Enabled = true;

                menusModulos.AddEx(creationParams);
            }
            catch (Exception ex)
            {
                _app.StatusBar.SetText(
                    "No se pudo crear el menu del addon: " + ex.Message,
                    BoMessageTime.bmt_Short,
                    BoStatusBarMessageType.smt_Error);
            }
        }

        private bool ExisteMenu(string uid)
        {
            try
            {
                // Si el item existe, Item(uid) no tira excepcion.
                var item = _app.Menus.Item(uid);
                return item != null;
            }
            catch
            {
                return false;
            }
        }

        // ---------------------------------------------------------------------
        // EVENTOS
        // ---------------------------------------------------------------------
        private void App_MenuEvent(ref MenuEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            // Nos interesa SOLO el click ya consumado (BeforeAction == false)
            // de NUESTRO menu.
            if (pVal.BeforeAction)
                return;
            if (pVal.MenuUID != MenuUidCargar)
                return;

            EjecutarCarga();
        }

        private void App_AppEvent(BoAppEventTypes eventType)
        {
            // Si SAP se cierra o cambia de compania, terminamos el proceso del addon.
            if (eventType == BoAppEventTypes.aet_ShutDown ||
                eventType == BoAppEventTypes.aet_CompanyChanged ||
                eventType == BoAppEventTypes.aet_ServerTerminition)
            {
                Application.Exit(); // cierra el bucle de mensajes de WinForms
            }
        }

        // ---------------------------------------------------------------------
        // LOGICA PRINCIPAL: leer CSV -> cargar series de numeracion
        // ---------------------------------------------------------------------
        private void EjecutarCarga()
        {
            string carpeta = ResolverCarpetaDatos();
            var logger = new Logger(carpeta);

            try
            {
                // 1) Elegir el archivo CSV.
                string rutaCsv = ElegirArchivoCsv(carpeta);
                if (rutaCsv == null)
                    return; // el usuario cancelo

                // 2) Leer el CSV.
                List<SerieNumeracion> series = CsvReader.Leer(rutaCsv);
                logger.Escribir("=== Inicio carga. Archivo: " + rutaCsv + " (" + series.Count + " filas) ===");

                if (series.Count == 0)
                {
                    _app.StatusBar.SetText("El CSV no tiene filas para cargar.",
                        BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                    return;
                }

                // 3) Confirmar con el usuario (mensaje nativo de SAP con botones).
                int boton = _app.MessageBox(
                    "Se van a cargar " + series.Count + " series de numeracion. Continuar?",
                    2, "Si", "No", "");
                if (boton != 1) // 1 = primer boton ("Si")
                    return;

                // 4) Escribir las series en la MATRIZ de la pantalla de Series de Numeracion
                //    (misma logica que el addon original: no inserta, escribe en la grilla).
                var loader = new MatrizLoader(_app);
                int escritas = loader.Cargar(series);

                // 5) Resumen.
                string resumen = "Se escribieron " + escritas + " series en la matriz.";
                foreach (SerieNumeracion s in series)
                    logger.Escribir("ESCRITO " + s);
                logger.Escribir("=== " + resumen + " ===");
                _app.StatusBar.SetText(resumen, BoMessageTime.bmt_Long, BoStatusBarMessageType.smt_Success);
                _app.MessageBox(resumen + "\n\nVer detalle en PuntosEmision_Log.txt");
            }
            catch (Exception ex)
            {
                logger.Escribir("ERROR: " + ex.Message);
                _app.StatusBar.SetText("Error en la carga: " + ex.Message,
                    BoMessageTime.bmt_Long, BoStatusBarMessageType.smt_Error);
            }
        }

        // ---------------------------------------------------------------------
        // Ayudas
        // ---------------------------------------------------------------------
        private static string ResolverCarpetaDatos()
        {
            string carpeta = ConfigurationManager.AppSettings["CarpetaDatos"];
            if (string.IsNullOrWhiteSpace(carpeta))
            {
                // Por defecto: la carpeta donde esta el .exe del addon.
                carpeta = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
            return carpeta;
        }

        private static string ElegirArchivoCsv(string carpeta)
        {
            string nombreDefault = ConfigurationManager.AppSettings["ArchivoCsv"];
            if (string.IsNullOrWhiteSpace(nombreDefault))
                nombreDefault = "PuntosEmision.csv";

            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = "Elegi el CSV de puntos de emision";
                dlg.Filter = "Archivos CSV (*.csv)|*.csv|Todos (*.*)|*.*";
                dlg.InitialDirectory = carpeta;
                dlg.FileName = nombreDefault;

                return dlg.ShowDialog() == DialogResult.OK ? dlg.FileName : null;
            }
        }
    }
}

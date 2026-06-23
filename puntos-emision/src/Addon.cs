using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using SAPbouiCOM;

namespace CargaPuntosEmision
{
    /// <summary>
    /// Crea el menu "Carga Puntos Emision..." en Modulos y, al hacer clic, lee el CSV
    /// y carga los puntos en la pantalla de Puntos de Emision (tabla OPTI).
    /// </summary>
    public class Addon
    {
        private const string MenuUidCargar = "PE_Cargar";
        private const string MenuModulos = "43520";

        private readonly SboConnection _conexion;
        private readonly SAPbouiCOM.Application _app;
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
            _app.MenuEvent += App_MenuEvent;
            _app.AppEvent += App_AppEvent;
        }

        private void CrearMenu()
        {
            try
            {
                Menus menusModulos = _app.Menus.Item(MenuModulos).SubMenus;
                if (ExisteMenu(MenuUidCargar))
                    return;

                var p = (MenuCreationParams)_app.CreateObject(BoCreatableObjectType.cot_MenuCreationParams);
                p.Type = BoMenuType.mt_STRING;
                p.UniqueID = MenuUidCargar;
                p.String = "Carga Puntos Emision...";
                p.Enabled = true;

                menusModulos.AddEx(p);
            }
            catch (Exception ex)
            {
                _app.StatusBar.SetText("No se pudo crear el menu del addon: " + ex.Message,
                    BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
        }

        private bool ExisteMenu(string uid)
        {
            try { return _app.Menus.Item(uid) != null; }
            catch { return false; }
        }

        private void App_MenuEvent(ref MenuEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;
            if (pVal.BeforeAction) return;
            if (pVal.MenuUID != MenuUidCargar) return;
            EjecutarCarga();
        }

        private void App_AppEvent(BoAppEventTypes eventType)
        {
            if (eventType == BoAppEventTypes.aet_ShutDown ||
                eventType == BoAppEventTypes.aet_CompanyChanged ||
                eventType == BoAppEventTypes.aet_ServerTerminition)
            {
                System.Windows.Forms.Application.Exit();
            }
        }

        private void EjecutarCarga()
        {
            string carpeta = ResolverCarpetaDatos();
            var logger = new Logger(carpeta);

            try
            {
                string nombreCsv = ConfigurationManager.AppSettings["ArchivoCsv"];
                if (string.IsNullOrWhiteSpace(nombreCsv))
                    nombreCsv = "PuntosEmisionDef.csv";
                string rutaCsv = Path.Combine(carpeta, nombreCsv);

                if (!File.Exists(rutaCsv))
                {
                    _app.MessageBox("No se encontro el archivo:\n" + rutaCsv +
                        "\n\nColoque el CSV en esa carpeta y vuelva a ejecutar.");
                    return;
                }

                List<PuntoEmisionDef> puntos = CsvReader.Leer(rutaCsv);
                logger.Escribir("=== Inicio carga. Archivo: " + rutaCsv + " (" + puntos.Count + " filas) ===");

                if (puntos.Count == 0)
                {
                    _app.StatusBar.SetText("El CSV no tiene filas para cargar.",
                        BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                    return;
                }

                int boton = _app.MessageBox(
                    "Se van a cargar " + puntos.Count + " puntos de emision. Continuar?",
                    2, "Si", "No", "");
                if (boton != 1) return;

                var loader = new MatrizLoader(_app, logger);
                int escritas = loader.Cargar(puntos);

                string resumen = "Se cargaron " + escritas + " puntos. Revisa la grilla y guarda (Ctrl+A).";
                logger.Escribir("=== " + resumen + " ===");
                _app.StatusBar.SetText(resumen, BoMessageTime.bmt_Long, BoStatusBarMessageType.smt_Success);
                _app.MessageBox(resumen + "\n\nVer detalle en PuntosEmisionDef_Log.txt");
            }
            catch (Exception ex)
            {
                logger.Escribir("ERROR: " + ex.Message);
                _app.StatusBar.SetText("Error en la carga: " + ex.Message,
                    BoMessageTime.bmt_Long, BoStatusBarMessageType.smt_Error);
            }
        }

        private static string ResolverCarpetaDatos()
        {
            string carpeta = ConfigurationManager.AppSettings["CarpetaDatos"];
            if (string.IsNullOrWhiteSpace(carpeta))
                carpeta = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return carpeta;
        }
    }
}

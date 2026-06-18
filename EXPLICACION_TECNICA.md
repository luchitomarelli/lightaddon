# Explicación técnica — Carga Masiva POI

Documento para entender el proyecto: **qué hace cada archivo**, **cómo lo hace
técnicamente** y **por qué está armado así**.

---

## 0. Visión general

**Qué es:** un *light add-on* de SAP Business One, hecho en **C# / .NET Framework
4.6.1**, que usa el **SDK (UI API)** para automatizar la carga masiva de Series de
Numeración (Puntos de Emisión) desde un CSV.

**Idea central:** en vez de cargar miles de series a mano en la pantalla de SAP, el
addon lee un CSV y completa la grilla por código, como si las tipeara un humano.

### Por qué está dividido en varios archivos
Cada archivo tiene **una sola responsabilidad** (principio de *separación de
responsabilidades*). Ventajas:
- Sabés exactamente dónde tocar para cada cambio.
- Es fácil de leer (cada pieza hace una cosa).
- Si algo falla, sabés qué archivo mirar.

Lo contrario sería un solo archivo gigante: funciona, pero es difícil de mantener.

### El flujo (cómo se conectan los archivos)
```
SAP lanza el .exe
  └─ Program.cs ............ arranca todo
       ├─ SboConnection.cs .. se conecta a SAP
       └─ Addon.cs .......... crea el menú y escucha el clic
            └─ (clic) EjecutarCarga():
                 ├─ CsvReader.cs ....... lee el CSV
                 ├─ SerieNumeracion.cs . molde de cada fila
                 ├─ MatrizLoader.cs .... escribe en la pantalla de SAP
                 └─ Logger.cs .......... guarda el log
```

---

## 1. Program.cs — el arranque

**Funcional:** es el punto de entrada. Conecta a SAP, arma el addon y lo deja vivo.

**Técnico:**
```csharp
[STAThread]                                  // (1)
private static void Main()
{
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);

    var conexion = new SboConnection();
    conexion.Connect();                      // (2)

    var addon = new Addon(conexion);
    addon.Inicializar();                     // (3)

    Application.Run();                        // (4)
}
```
- **(1) `[STAThread]`**: COM exige el modelo de hilos "Single-Threaded Apartment".
  Sin esto, las llamadas a SAP fallan.
- **(2)** crea y conecta la sesión con SAP.
- **(3)** registra el menú y los eventos.
- **(4) `Application.Run()`**: bucle de mensajes. Mantiene el `.exe` corriendo
  esperando eventos. Sin esta línea, el proceso se cerraría al instante.

**Por qué así:** un addon es un programa que tiene que quedarse "escuchando".
`Main` solo orquesta (conectar → armar → esperar); la lógica vive en otras clases.

---

## 2. SboConnection.cs — la conexión

**Funcional:** conecta el addon con SAP sin pedir usuario/clave.

**Técnico:**
```csharp
string[] args = Environment.GetCommandLineArgs();
string connectionString = args.Length > 1 ? args[1] : "0030002C...";

var sboGuiApi = new SboGuiApi();
sboGuiApi.Connect(connectionString);              // (1)
Application = sboGuiApi.GetApplication();          // (2)

Company = (SAPbobsCOM.Company)Application.Company.GetDICompany();  // (3)
```
- **(1)** SAP, al lanzar el addon, le pasa una *connection string* como argumento
  (`args[1]`). Es la "llave" para engancharse a la sesión abierta.
- **(2) `GetApplication()`**: devuelve el objeto `Application` = toda la GUI de SAP.
  Es la puerta de entrada a la UI API.
- **(3) `GetDICompany()`**: obtiene la Company de la DI API (datos). Acá queda
  disponible aunque este addon no la use.

**Por qué así:** colgarse de la sesión abierta evita manejar credenciales y hereda
la empresa/usuario logueado. Es el patrón estándar de los light add-ons.

---

## 3. Addon.cs — el cerebro (menú + eventos)

**Funcional:** crea el menú "Carga Masiva POI" y reacciona cuando lo clickeás.

**Técnico — crear el menú:**
```csharp
Menus menusModulos = _app.Menus.Item("43520").SubMenus;  // "43520" = menú Módulos
var p = (MenuCreationParams)_app.CreateObject(BoCreatableObjectType.cot_MenuCreationParams);
p.UniqueID = "POI_Cargar";             // DNI de nuestro menú
p.String   = "Carga Masiva POI...";    // texto visible
menusModulos.AddEx(p);
```

**Técnico — escuchar el clic:**
```csharp
_app.MenuEvent += App_MenuEvent;       // suscribirse a TODOS los clics de menú

private void App_MenuEvent(ref MenuEvent pVal, out bool bubbleEvent)
{
    bubbleEvent = true;
    if (pVal.BeforeAction) return;            // (A) ignorar el "antes de"
    if (pVal.MenuUID != "POI_Cargar") return; // (B) ¿es NUESTRO menú?
    EjecutarCarga();
}
```
- **(A) `BeforeAction`**: SAP dispara el evento dos veces (antes y después de la
  acción). Actuamos solo en el "después".
- **(B)** filtramos por `MenuUID`: SAP nos avisa de *todos* los menús, así que
  reconocemos el nuestro por su `UniqueID`.
- **`bubbleEvent`**: si lo ponés en `false`, cancelás la acción (útil para
  validaciones). Acá lo dejamos en `true` (dejar que siga normal).

**Técnico — la lógica principal:**
```csharp
string rutaCsv = Path.Combine(carpeta, "PuntosEmision.csv");
if (!File.Exists(rutaCsv)) { _app.MessageBox("No se encontro..."); return; }

List<SerieNumeracion> series = CsvReader.Leer(rutaCsv);

int boton = _app.MessageBox("Se van a cargar " + series.Count + "... Continuar?", 2, "Si", "No", "");
if (boton != 1) return;

var loader = new MatrizLoader(_app, logger);
loader.Cargar(series);
```
Lee el CSV de una **ruta fija** (sin diálogos de Windows, que cuelgan el cliente de
SAP), pregunta confirmación con un `MessageBox` nativo, y manda a cargar.

**Por qué así:** separa "armar el menú" de "qué hacer al clickear". El método
`EjecutarCarga` solo coordina; el trabajo pesado (leer/escribir) está en otras clases.

---

## 4. SerieNumeracion.cs — el molde de una fila

**Funcional:** define cómo es una fila del CSV.

**Técnico:**
```csharp
public class SerieNumeracion
{
    public string Name { get; set; }       // Nombre
    public string PTICode { get; set; }    // Código POI
    public string Letter { get; set; }     // Carta (A/B/R)
    public int FirstNum { get; set; }      // Primer número
    public int LastNum { get; set; }       // Último número
}
```

**Por qué así:** en vez de andar con textos sueltos (`campos[0]`, `campos[1]`...),
cada fila se vuelve un objeto con nombres claros. Más legible y menos propenso a
errores. Es una "clase modelo" (solo datos, sin lógica).

---

## 5. CsvReader.cs — leer el archivo

**Funcional:** convierte el CSV en una lista de `SerieNumeracion`.

**Técnico:**
```csharp
string[] lineas = File.ReadAllLines(ruta);
for (int i = 1; i < lineas.Length; i++)        // i=1 salta el encabezado
{
    string[] campos = lineas[i].Split(';');    // partir por ";"
    resultado.Add(new SerieNumeracion
    {
        Name     = campos[0].Trim(),
        PTICode  = campos[1].Trim(),           // string => conserva ceros (0570)
        Letter   = campos[2].Trim(),
        FirstNum = ParseNum(campos[3], ...),
        LastNum  = ParseNum(campos[4], ...)
    });
}
```

**Por qué así:**
- Es **C# puro**, sin nada de SAP: la parte más fácil y aislada.
- `PTICode` se lee como **string**, no como número, para no perder los ceros a la
  izquierda (`0570` no debe volverse `570`).
- Es `static` porque no guarda estado: entra una ruta, sale una lista.

---

## 6. MatrizLoader.cs — escribir en la pantalla (la parte "SAP")

**Funcional:** toma la pantalla de Series de Numeración y escribe las filas en su grilla.

**Técnico:**
```csharp
Form form = _app.Forms.ActiveForm;                     // la pantalla activa
Matrix matriz = (Matrix)form.Items.Item("3").Specific; // la grilla (Item "3")

foreach (SerieNumeracion s in series)
{
    matriz.AddRow();                  // agregar una fila vacía
    int fila = matriz.RowCount;       // la última fila

    SetCelda(matriz, "Name",     fila, s.Name);
    SetCelda(matriz, "PTICode",  fila, s.PTICode);
    SetCelda(matriz, "Letter",   fila, s.Letter);
    SetCelda(matriz, "FirstNum", fila, s.FirstNum.ToString());
    SetCelda(matriz, "LastNum",  fila, s.LastNum.ToString());
}
```
**Concepto clave — direccionar por UID:** el código no "ve" la pantalla. Llega a
cada celda bajando por el árbol de objetos y usando los **IDs internos** que sacamos
con *Ver → Información del sistema*: el Item de la matriz (`"3"`) y los UID de
columna (`"Name"`, `"PTICode"`...).

```csharp
private void SetCelda(Matrix matriz, string columna, int fila, string valor)
{
    try
    {
        object specific = matriz.Columns.Item(columna).Cells.Item(fila).Specific;

        var combo = specific as ComboBox;
        if (combo != null) { combo.Select(valor, BoSearchKey.psk_ByValue); return; } // desplegable

        var edit = specific as EditText;
        if (edit != null) { edit.Value = valor; return; }                            // texto
    }
    catch (Exception ex)
    {
        _logger.Escribir("AVISO: columna '" + columna + "' ... " + ex.Message);
    }
}
```

**Por qué así:**
- **`.Specific`**: un `Item`/celda es genérico; `.Specific` da el control concreto
  ya tipado (`Matrix`, `EditText`, `ComboBox`). Por eso casteamos.
- **Detección de tipo de celda**: una columna puede ser texto o desplegable; el
  método decide cómo cargar cada una.
- **`try/catch` por celda (resiliencia)**: si una celda no se puede escribir, lo
  anota en el log y sigue, en vez de abortar todo el lote.
- **No usa DI API ni SQL**: la tabla OFNS es del sistema y no permite escritura
  directa ("not a user-defined item"). La GUI es la única vía soportada.

---

## 7. Logger.cs — el log

**Funcional:** guarda un registro con fecha/hora de lo que pasó.

**Técnico:**
```csharp
string linea = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + mensaje;
File.AppendAllText(_ruta, linea + Environment.NewLine);
```

**Por qué así:** un addon corre "sin que lo veas"; el log es la forma de saber qué
cargó y qué falló. `AppendAllText` agrega al final sin pisar lo anterior.

---

## 8. app.config — la configuración

**Funcional:** datos que se pueden cambiar sin tocar código.

**Técnico:**
```xml
<add key="CarpetaDatos" value="C:\Seidor\POI" />   <!-- dónde está el CSV -->
<add key="ArchivoCsv" value="PuntosEmision.csv" />  <!-- nombre del CSV -->
```

**Por qué así:** la ruta del CSV cambia entre tu máquina y el cliente. Tenerla en el
`.config` permite cambiarla **sin recompilar** (y sin regenerar el `.ard`, porque el
exe no cambia).

---

## 9. Cosas técnicas transversales (el "por qué" de fondo)

- **¿Por qué .NET Framework y no .NET Core?** El SDK de SAP es **COM**; necesita
  Framework.
- **¿Por qué x64?** El *bitness* del addon debe coincidir con el cliente de SAP.
- **¿Por qué los Interop DLLs viajan con el exe?** Son el "traductor" COM↔.NET; sin
  ellos el exe no puede hablar con SAP.
- **¿Por qué un `.ard`?** Es el archivo de registro que firma el exe (hash) para que
  SAP lo instale y confíe en él. Si recompilás, hay que regenerarlo.
- **¿Por qué la pantalla y no un INSERT?** OFNS es tabla del sistema, sin objeto DI
  API; el SQL directo está prohibido. La UI API es lo soportado.

---

## 10. Resumen para el handoff

| Archivo | Responsabilidad |
|---|---|
| `Program.cs` | Arranque y bucle de vida |
| `SboConnection.cs` | Conexión a SAP (UI + DI) |
| `Addon.cs` | Menú + eventos + orquestación |
| `SerieNumeracion.cs` | Modelo de una fila |
| `CsvReader.cs` | Lectura del CSV |
| `MatrizLoader.cs` | Escritura en la matriz (UI API) |
| `Logger.cs` | Log a archivo |
| `app.config` | Configuración (ruta del CSV) |
| `Carga Masiva POI.ard` | Registro/instalación en SAP |

**Para compilar y desplegar:** ver `GUIA_PASO_A_PASO.md` y `README.md`.

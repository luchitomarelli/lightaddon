# Carga Masiva POI — Light Addon de ejemplo para SAP Business One 10

Addon **didáctico** en C# / .NET Framework para aprender a programar un *light addon*
de SAP Business One usando el **SDK (UI API + DI API)**.

Está inspirado en el addon original "Carga Masiva POI", pero acá:

- Tenés **todo el código fuente** (el original venía solo compilado).
- Además del POI, carga el **número de emisión** de cada punto (lo que pediste).
- Se **crea su propia tabla** para que funcione en cualquier SAP B1 10 sin depender
  de pantallas de la localización. Cuando lo entiendas, cambiás el destino por el real.

---

## ¿Qué hace?

1. Agrega un menú en **Módulos → Carga Masiva POI...**.
2. Al hacer clic, te deja elegir un archivo **CSV**.
3. Lee el CSV con las 6 columnas de una **Serie de Numeración** (tabla **OFNS**):
   `Name ; PTICode ; Letter ; FirstNum ; NextNum ; LastNum`.
4. Toma la pantalla de **Series de Numeración** que tenés abierta y **escribe los datos
   en su matriz**, igual que si los cargaras a mano.
5. Aprieta "Agregar/Actualizar" para que **SAP guarde** (no hace un INSERT directo).
6. Deja un **log** (`PuntosEmision_Log.txt`) y muestra el resumen.

> **Importante:** abrí la pantalla de Series de Numeración y dejala **activa** antes de
> ejecutar la carga. El addon escribe en la matriz del formulario activo. Verificá el
> UID de la matriz y de las columnas con *Ver → Información del sistema* y ajustalos en
> `MatrizLoader.cs` si difieren.

---

## Requisitos (tu máquina de desarrollo)

Son exactamente los que te pasaron:

- **Visual Studio 2019** (Community sirve si la licencia aplica).
- **SAP Business One SDK / DI API** instalado (trae `SAPbouiCOM` y `SAPbobsCOM`).
- Un **cliente de SAP Business One 10** abierto y logueado para probar.
- Saber **C# / .NET** básico (el resto se aprende con este ejemplo).

> ⚠️ Este proyecto **no se puede compilar en Linux ni sin el SDK**: el SDK es COM y
> solo existe en Windows con SAP B1 instalado. Por eso se compila en tu VS 2019.

---

## Preparar el entorno con Visual Studio 2019 (paso a paso)

### 1) Instalar Visual Studio 2019 Community
- Descargalo de `visualstudio.microsoft.com` → sección **"Older downloads"** (necesitás
  una cuenta Microsoft gratuita para bajar versiones anteriores).
- En el **instalador (Workloads)**, tildá:
  - **Desarrollo de escritorio de .NET** (incluye .NET Framework y WinForms).
- En la pestaña **"Componentes individuales"**, asegurate de tener:
  - **.NET Framework 4.8 SDK** y **.NET Framework 4.8 targeting pack**.

### 2) Instalar el SAP Business One SDK
- Está en el **DVD/instalador de SAP Business One 10**, carpeta `Packages\SDK` (o pedíselo
  a tu consultor/partner de SAP). Instalá **"SAP Business One SDK"**.
- Esto deja en tu máquina las DLLs `Interop.SAPbouiCOM.dll` e `Interop.SAPbobsCOM.dll`
  (típicamente en `C:\Program Files (x86)\SAP\SAP Business One DI API\`).

### 3) (Opcional) Complemento "SAP Business One Studio" para VS 2019
- Sirve para el **diseñador visual de formularios `.b1f`** y para generar el `.ard`.
- **Este proyecto NO lo necesita** (es solo código, sin formularios propios), pero si
  vas a hacer addons con pantallas propias, instalalo: viene junto al SDK y se integra
  con VS 2019.

### 4) Abrir y configurar el proyecto
1. Abrí `CargaMasivaPOI.sln` con VS 2019.
2. Si las referencias `Interop.SAPbouiCOM` / `Interop.SAPbobsCOM` aparecen en rojo:
   borralas → clic derecho en el proyecto → **Agregar referencia** → pestaña **COM** →
   tildá **SAP Business One UI API** y **SAP Business One DI API**.
3. Verificá que la **plataforma** sea **x86** (combo de arriba). Debe coincidir con el
   *bitness* del cliente de SAP que uses.

---

## Estructura del proyecto

```
CargaMasivaPOI.sln              -> la solución (abrila con VS 2019)
Carga Masiva POI.ard            -> archivo de registro del addon (ejemplo)
ejemplos/PuntosEmision.csv      -> CSV de prueba
src/
  CargaMasivaPOI.csproj         -> el proyecto (.NET Framework 4.8, x86)
  app.config                    -> config (carpeta y nombre del CSV)
  Program.cs                    -> arranque (Main): conecta y deja vivo el addon
  SboConnection.cs              -> conexión a UI API + DI API (sin login)
  Addon.cs                      -> menú + eventos + lógica de la carga
  CsvReader.cs                  -> lee el CSV (C# puro)
  PuntoEmision.cs               -> modelo de una fila
  UdtManager.cs                 -> crea la tabla/campo con la DI API (metadata)
  PuntoEmisionRepository.cs     -> inserta/actualiza datos con la DI API
  Logger.cs                     -> log a archivo de texto
```

---

## Los 3 conceptos del SDK (lo único "nuevo")

Todo lo demás es C# normal. Lo específico de SAP son estas tres ideas:

### 1) Conectarse (`SboConnection.cs`)
No hacés login. Te "colgás" de la sesión abierta del cliente con la **UI API**, y desde
ahí pedís prestada la **Company** de la **DI API**:

```csharp
var gui = new SAPbouiCOM.SboGuiApi();
gui.Connect(connectionString);                // me engancho a la GUI viva
var app = gui.GetApplication();
var company = (SAPbobsCOM.Company)app.Company.GetDICompany();  // DI prestada
```

### 2) Menú + eventos (`Addon.cs`)
Creás un item de menú con un **UniqueID**, y escuchás el evento de click:

```csharp
app.MenuEvent += App_MenuEvent;
// ...
if (!pVal.BeforeAction && pVal.MenuUID == "POI_Cargar")
    EjecutarCarga();
```

### 3) Escribir datos con la DI API (`UdtManager.cs` + `PuntoEmisionRepository.cs`)
- **Metadata** (crear tabla/campo): objetos `UserTablesMD` / `UserFieldsMD`.
- **Datos** (insertar filas): `company.UserTables.Item(...)` → `Add()` / `Update()`.

```csharp
var udt = company.UserTables.Item("POI_PEMISION");
udt.Code = "0001";
udt.Name = "Casa Central";
udt.UserFields.Fields.Item("U_NroEmision").Value = 15;
udt.Add();
```

> Regla de oro con COM: lo que pedís con `GetBusinessObject(...)` conviene liberarlo
> con `Marshal.ReleaseComObject(...)` (mirá `UdtManager.cs`).

---

## Cómo compilar y probar

1. Abrí `CargaMasivaPOI.sln` en **Visual Studio 2019**.
2. **Referencias del SDK**: si VS marca en rojo `Interop.SAPbouiCOM` / `Interop.SAPbobsCOM`,
   borralas y agregalas de nuevo: clic derecho en el proyecto → *Agregar referencia* →
   pestaña **COM** → tildá **SAP Business One UI API** y **SAP Business One DI API**.
3. Verificá que la plataforma activa sea **x86** (combo arriba). Debe coincidir con el
   *bitness* de tu cliente de SAP. Si usás el cliente de 64 bits, creá una config **x64**.
4. **Probar en modo desarrollo (F5):**
   - Abrí y logueate en el cliente de SAP B1.
   - En `SboConnection.cs`, la `connectionString` por defecto (`SAPBDdatev...`) sirve
     para engancharse al cliente abierto. Si tu cliente usa otra, ajustala (SAP B1 Studio
     te da la cadena correcta en *Connectivity Settings*).
   - Apretá **F5**. Debería aparecer **Módulos → Carga Masiva POI...** en SAP.
5. Hacé clic en el menú, elegí `ejemplos/PuntosEmision.csv` y confirmá. Mirá el resultado
   en *Herramientas → Consultas* sobre `@POI_PEMISION`, o en el log.

---

## Cómo registrar el addon (para que arranque solo)

1. Compilá en **Release**.
2. SAP B1 **Studio** genera el `.ard` con la firma del `.exe` (Build → Generate ARD).
3. En SAP: **Administración → Gestión de Add-Ons** → registrar usando el `.ard` y asignar
   a las empresas/usuarios.

El `Carga Masiva POI.ard` incluido es solo un **modelo de referencia**: la firma (`AddonSig`)
real la pone Studio según tu `.exe`.

---

## Cómo adaptarlo a tu caso real

Hoy escribe en una tabla propia para que veas el mecanismo. Para llevarlo al objeto real:

- Si los puntos de emisión viven en una **UDT estándar** de tu localización, cambiá el
  nombre de tabla/campos en `UdtManager.cs` y `PuntoEmisionRepository.cs`.
- Si hay que cargarlos en una **pantalla** (como hacía el original, llenando una matriz
  por la GUI), eso se hace con la **UI API**: abrir el `Form`, posicionarse en la `Matrix`,
  setear celdas y disparar el `Click` de "Agregar fila". Es más frágil que la DI API, por
  eso para aprender arrancamos por la DI API.

---

## Glosario rápido

| Término | Qué es |
|---|---|
| **UI API** (`SAPbouiCOM`) | Controla la interfaz: menús, formularios, eventos. |
| **DI API** (`SAPbobsCOM`) | Lee/graba datos y objetos de negocio. |
| **UDT** | *User Defined Table*: una tabla propia tuya (`@NOMBRE`). |
| **UDF** | *User Defined Field*: un campo propio (`U_Nombre`). |
| **.ard** | Archivo de registro del addon en SAP. |
| **light addon** | Addon que corre como `.exe` enganchado al cliente de SAP. |

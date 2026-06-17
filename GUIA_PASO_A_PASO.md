# Guía paso a paso — De cero a tu addon andando

Esta guía te lleva desde una máquina vacía hasta ver el addon cargando puntos
en SAP. Marcá cada paso a medida que lo completes.

---

## FASE 1 — Preparar la máquina (una sola vez)

### Paso 1 · Instalar Visual Studio 2019
- [ ] Bajalo de `visualstudio.microsoft.com` → sección **"Older downloads"**
      (necesitás una cuenta Microsoft gratuita).
- [ ] En el instalador, marcá el workload **"Desarrollo de escritorio de .NET"**.
- [ ] En *Componentes individuales*, confirmá **.NET Framework 4.8 SDK** y
      **targeting pack**.

> Licencia: si tu empresa es chica (≤250 personas y ≤USD 1M de facturación) podés
> usar **Community gratis**. Si es grande, necesitás **Professional** (la provee la
> empresa). En ambos casos: usá una copia legal, no pirateada.

### Paso 2 · Instalar el SAP Business One SDK  (EL MÁS IMPORTANTE)
- [ ] Está en el instalador de **SAP Business One 10**, carpeta `Packages\SDK`.
      Si no lo tenés, pedíselo a tu **partner/consultor de SAP**.
- [ ] Instalá **"SAP Business One SDK"**. Deja las DLLs `SAPbouiCOM` y `SAPbobsCOM`.

### Paso 3 · Cliente de SAP B1 10
- [ ] Tené un cliente de SAP Business One 10 instalado, con un usuario para probar.

---

## FASE 2 — Abrir y compilar el addon

### Paso 4 · Abrir el proyecto
- [ ] Descomprimí el proyecto y abrí **`CargaMasivaPOI.sln`** en VS 2019.

### Paso 5 · Arreglar las referencias del SDK (si están en rojo)
- [ ] Borrá `Interop.SAPbouiCOM` / `Interop.SAPbobsCOM` → clic derecho en el proyecto
      → **Agregar referencia** → pestaña **COM** → tildá **SAP Business One UI API**
      y **SAP Business One DI API**.

### Paso 6 · Configurar plataforma
- [ ] Arriba, poné **x86** (plataforma) y **Debug** (configuración).

### Paso 7 · Compilar
- [ ] Menú **Compilar → Compilar solución**. Sin errores rojos = `.exe` listo.

---

## FASE 3 — Probar (primera corrida)

### Paso 8 · Abrir SAP
- [ ] Abrí el cliente de SAP B1 y logueate.

### Paso 9 · Abrir la pantalla destino
- [ ] Abrí **Puntos de Emisión** y dejala **activa** (el addon escribe en la
      pantalla activa).

### Paso 10 · Ejecutar
- [ ] En VS apretá **F5**. Debería aparecer **Módulos → Carga Masiva POI...** en SAP.

### Paso 11 · Cargar el CSV de ejemplo
- [ ] Clic en el menú → elegí **`ejemplos/PuntosEmision.csv`** → confirmá.

### Paso 12 · Verificar
- [ ] Mirá cómo se llenan solas las filas de la matriz con los 5 puntos.
- [ ] Revisá el archivo `PuntosEmision_Log.txt` con el detalle.

---

## FASE 4 — Adaptarlo a lo real (después)

### Paso 13 · Columna del número de emisión
- [ ] Con *Ver → Información del sistema*, pasá el mouse por esa columna y anotá su
      `Column=...`. Pasámelo y lo activamos en `MatrizLoader.cs`.

### Paso 14 · Tus datos reales
- [ ] Reemplazá el CSV de ejemplo por tus puntos reales.

### Paso 15 · Publicar en el cliente
- [ ] Compilá en **Release**, generá el `.ard` y registralo en SAP:
      **Administración → Gestión de Add-Ons**.

---

## Si algo falla

| Síntoma | Causa probable |
|---|---|
| Referencias en rojo al abrir | Falta instalar el SAP B1 SDK (Paso 2) |
| "No se encontró la matriz" | No tenías la pantalla de Puntos de Emisión activa (Paso 9) |
| No aparece el menú | El addon no llegó a conectar (revisá la connection string en `SboConnection.cs`) |
| Error de plataforma / bitness | x86 vs x64: la plataforma debe coincidir con tu cliente de SAP |

Cualquier error concreto, copialo y lo resolvemos.

# Centro de estabilidad — diseño

## Objetivo

Nueva sección **Estabilidad** que analiza el equipo en modo lectura y explica, con evidencia,
por qué Windows se cuelga, reinicia o muestra pantallazos azules. Los hallazgos con arreglo
seguro llevan un botón **Arreglar** que usa el `TweakEngine` existente (backup + deshacer desde
la sección Backup). Los problemas de hardware/BIOS solo muestran instrucciones.

Origen: el diagnóstico manual del 2026-09-17 en un ASUS TUF A15 (FA506IV) con WinterOS encontró
6 BugChecks con códigos distintos, error fatal del TPM, `TdrDelay=360`, políticas de Windows
Update que desactivan protecciones, RAM mezclada y drivers de 2022. NexaTweaks no detectaba nada
de eso.

## Arquitectura

`src/NexaTweaks.Core/Stability/`

- `StabilityFinding` — `CheckId`, `Title`, `Severity` (`Info`/`Low`/`Medium`/`High`),
  `Evidence` (líneas de texto), `Recommendation`, `Fix` (`ITweak?`).
- `IStabilityProbe` — única puerta al sistema: `ReadEvents(log, providers, ids, since)`,
  `GetRegistryValue(hive, subKey, name)`, `QueryWmi(className, properties)`.
- `WindowsStabilityProbe` — implementación real (`EventLogReader`, `Registry`, `System.Management`).
- `IStabilityCheck` — `Id`, `Name`, `Run(probe, now)` → hallazgos. Una clase por verificación.
- `StabilityAnalyzer` — ejecuta todas las verificaciones; si una lanza excepción devuelve un
  hallazgo `Info` "No se pudo verificar …" y sigue; ordena por severidad.
- `StabilityFixes` — los `RegistryTweak` de arreglo (ids `stability.*`), expuestos para que
  `BackupViewModel` pueda restaurar sus snapshots.

`src/NexaTweaks.App/`

- `StabilityViewModel` + `StabilityFindingViewModel` — botón Analizar (en `Task.Run`), botón
  Arreglar (confirmación si el riesgo no es `Safe`, `ApplyOneWithEntry`, marca reinicio).
- `StabilityView.xaml` — mismo estilo de tarjetas que `RepairView`.
- `MainWindow` — entrada de navegación "Estabilidad" tras "Reparación".

## Verificaciones v1

| Id | Fuente | Severidad | Arreglo |
|---|---|---|---|
| `bugcheck` | System 1001 `Microsoft-Windows-WER-SystemErrorReporting`, 30 días, agrupado por código | High | — (guía; ≥3 códigos distintos ⇒ RAM/overclock/sistema modificado) |
| `unexpected-shutdown` | System 41 `Microsoft-Windows-Kernel-Power`, 30 días; `[0]` = BugcheckCode | Medium | — |
| `tpm` | System 14 proveedor `TPM`, 30 días | High | — (actualizar BIOS) |
| `whea` | System 17/18/19/47 `Microsoft-Windows-WHEA-Logger`, 30 días | High | — |
| `app-crash` | Application 1000 `Application Error`, 7 días, agrupado por app + módulo (`[0]`, `[3]`), ≥3 cierres | Medium | — |
| `memory-test` | System 1101/1102 `Microsoft-Windows-MemoryDiagnostics-Results` | 1102 ⇒ High; sin eventos y hay BugChecks ⇒ Medium | — (`mdsched.exe`) |
| `ram-mixed` | WMI `Win32_PhysicalMemory` (fabricante+modelo o velocidad distintos) | Medium | — |
| `tdr` | `HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers` `TdrDelay`/`TdrDdiDelay` > 10 | High | `TdrDelay=2`, `TdrDdiDelay=5` |
| `wu-policies` | `HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate` `DisableWUfBSafeguards=1`, `ExcludeWUDriversInQualityUpdate=1` | Medium | poner `0` |
| `laptop-throttling` | `PowerThrottlingOff=1` y existe `Win32_Battery` | Low | `PowerThrottlingOff=0` |
| `old-drivers` | WMI `Win32_PnPSignedDriver` clase DISPLAY/NET, no Microsoft, > 3 años | Low | — |
| `old-bios` | WMI `Win32_BIOS.ReleaseDate` > 2 años | Low | — |
| `modded-windows` | `RegisteredOwner`/`RegisteredOrganization` contiene WinterOS, ReviOS, AtlasOS, Tiny11, Ghost Spectre, … | Medium | — |
| `kernel-drivers` | WMI `Win32_SystemDriver` en ejecución: `vgk`, `AMDRyzenMasterDriver*`, `WinRing0*`, `RTCore64` | Low | — |

## Pruebas

- Unitarias (xUnit) con un `FakeStabilityProbe`: cada verificación con datos que deben y no deben
  generar hallazgo; el analizador sobrevive a una verificación que lanza excepción.
- Integración opcional (`NEXA_INTEGRATION=1`) que ejecuta `WindowsStabilityProbe` real e imprime
  los hallazgos. En el equipo de referencia debe mostrar: BugChecks 0x3B/0x139, TPM, cierres de
  AsusSplendid por `amdadlx64.dll`, RAM mezclada, WinterOS, `vgk`, BIOS y driver AMD de 2022; y
  **no** debe mostrar `tdr`, `wu-policies` ni `laptop-throttling` (ya corregidos).

## Fuera de alcance (v1)

Análisis de minidumps, centro de actualizaciones (winget), protección de tweaks en portátiles.

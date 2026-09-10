# Nexa Tweaks

Suite de optimización y mantenimiento para Windows, escrita en C# / WPF (.NET 8).
Centraliza en una sola app tweaks de sistema, limpieza, monitoreo, gestión de arranque, backups y reparación — todo con vista previa antes de aplicar cambios y opción de deshacer.

![CI](https://github.com/Zukuth/nexatweaks/actions/workflows/ci.yml/badge.svg)
![version](https://img.shields.io/badge/versi%C3%B3n-0.1.0-blue)
![platform](https://img.shields.io/badge/plataforma-Windows%20x64-0078D6)

## Secciones de la app

- **Dashboard** — estado general del equipo de un vistazo.
- **Windows (Tweaks)** — catálogo de ajustes del sistema (registro, servicios, tareas programadas, planes de energía, Nagle, comandos), aplicables y reversibles.
- **Network** — benchmark de servidores DNS y utilidades de ping/latencia.
- **Input / Gpu** — monitoreo de uso de GPU y estadísticas del sistema en tiempo real, incluida temperatura de CPU.
- **Cleanup** — limpieza de archivos temporales y caché de navegadores, analizador de espacio en disco, escáner de registro y listado de apps instaladas.
- **Booster** — perfiles de juego y boost de procesos (prioridad, working-set trim) para sesiones de gaming.
- **Apps / Autorun** — gestión de aplicaciones instaladas y de programas que arrancan con Windows.
- **Repair** — catálogo de acciones de reparación rápida del sistema.
- **Backup** — snapshots antes/después de cada tanda de cambios y gestión de puntos de restauración de Windows.
- **Advanced / Settings** — exportación de reportes de soporte (logs, info del sistema) y preferencias de la app.

## Arquitectura

- `src/NexaTweaks.Core` — lógica de negocio: motor de tweaks (`TweakEngine`), catálogos, cleanup, backup, monitoreo, diagnóstico y booster. Sin dependencias de UI. El catálogo de tweaks (`Catalog/TweakCatalog.*.cs`) está partido en un archivo parcial por categoría (Windows, Network, Input, Gpu, Cleanup, Advanced) para que cada uno sea chico y fácil de revisar en un PR.
- `src/NexaTweaks.App` — cliente WPF (MVVM con `CommunityToolkit.Mvvm`): vistas, view models y servicios de la app.
- `src/NexaTweaks.Tests` — pruebas unitarias sobre el motor de tweaks, catálogos y componentes de cleanup/backup.
- `installer/` — script de Inno Setup para generar el instalador de Windows.
- `Directory.Build.props` — versión del producto en un solo lugar (`NexaTweaksVersion`); `NexaTweaks.App.csproj` la lee de ahí y `installer/NexaTweaks.iss` la recibe por línea de comandos (`/DMyAppVersion=X.Y.Z`) para que el `.exe` y el instalador nunca queden desincronizados.
- `.github/workflows/ci.yml` — build + tests en cada push/PR a `master`.

## Requisitos

- Windows 10/11 x64.
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) para compilar.
- Privilegios de administrador para ejecutar la app (varios tweaks tocan registro/servicios del sistema).

## Compilar y ejecutar

```bash
dotnet build NexaTweaks.sln
dotnet run --project src/NexaTweaks.App
```

## Pruebas

```bash
dotnet test src/NexaTweaks.Tests
```

## Instalador

La release incluye `NexaTweaks-Setup-<versión>.exe`, generado con Inno Setup a partir de la publicación self-contained de `NexaTweaks.App` (`installer/NexaTweaks.iss`). Para que el nombre del instalador coincida con la versión del `.csproj`, compilarlo con:

```bash
ISCC installer\NexaTweaks.iss /DMyAppVersion=0.1.0
```

(si se omite `/DMyAppVersion`, usa el valor por defecto embebido en el propio `.iss`).

## Firma de código

Los binarios (`NexaTweaks.exe`, DLLs propias) y el instalador se firman con `signtool.exe` (Windows SDK) usando un certificado de firma de código:

```bash
signtool sign /f "<ruta al .pfx>" /p "<password>" /fd SHA256 /tr http://timestamp.digicert.com /td SHA256 "archivo.exe"
```

Actualmente se usa un **certificado autofirmado**: garantiza que el binario no fue alterado, pero Windows SmartScreen igual muestra "editor no reconocido" porque no proviene de una entidad certificadora de confianza.

Este proyecto está aplicando al programa de firma de código gratuito para proyectos open source de **[SignPath Foundation](https://signpath.org/)** — una vez aprobado, los binarios e instaladores de las releases se firmarán con un certificado de una CA de confianza provisto por SignPath, sin costo, gracias a la licencia MIT de este repositorio.

Para que `installer/NexaTweaks.iss` firme automáticamente el instalador generado, configura una vez en el IDE de Inno Setup: **Tools > Configure Sign Tools**, agregando una herramienta llamada `signtool` (ver comentario en el propio `.iss`).

## Estado

Versión **0.1.0** — en desarrollo activo, sujeta a cambios de UI/UX y nuevas categorías de tweaks.

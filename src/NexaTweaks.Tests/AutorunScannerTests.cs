using NexaTweaks.Core.Diagnostics;
using Xunit;

namespace NexaTweaks.Tests;

public class AutorunScannerTests
{
    // Captured verbatim from `schtasks /query /fo csv /v` on a real Spanish-locale Windows 11
    // install, to make sure the fixed column-position parsing (TaskName=1, Task To Run=8,
    // Scheduled Task State=11) and the CSV quote-escaping actually hold up against real output,
    // not just an assumption about schtasks' documented column order.
    private const string SampleCsv =
        "\"Nombre de host\",\"Nombre de tarea\",\"Hora próxima ejecución\",\"Estado\",\"Modo de inicio de sesión\",\"Último tiempo de ejecución\",\"Último resultado\",\"Autor\",\"Tarea que se ejecutará\",\"Iniciar en\",\"Comentario\",\"Estado de tarea programada\",\"Tiempo de inactividad\"\r\n" +
        "\"DESKTOP-4PD2EV6\",\"\\AMDRyzenMasterSDKTask\",\"N/A\",\"En ejecución\",\"Solo interactivo\",\"19-08-2026 21:49:24\",\"267009\",\"Advanced Micro Devices\",\"\"\"C:\\Program Files\\AMD\\CNext\\CNext\\cpumetricsserver.exe\"\" \",\"N/A\",\"AMDRyzenMasterSDKTask\",\"Habilitado\",\"Deshabilitado\"\r\n" +
        "\"DESKTOP-4PD2EV6\",\"\\CreateExplorerShellUnelevatedTask\",\"N/A\",\"Listo\",\"Solo interactivo\",\"04-08-2026 0:56:25\",\"1073807364\",\"ExplorerShellUnelevated\",\"C:\\WINDOWS\\explorer.exe /NoUACCheck\",\"N/A\",\"N/A\",\"Habilitado\",\"Deshabilitado\"\r\n";

    [Fact]
    public void ParseCsv_HandlesRealSchtasksOutput_SpanishLocale()
    {
        var rows = AutorunScanner.ParseCsv(SampleCsv);

        Assert.Equal(3, rows.Count); // header + 2 data rows

        var row1 = rows[1];
        Assert.Equal(@"\AMDRyzenMasterSDKTask", row1[1]);
        Assert.Equal("\"C:\\Program Files\\AMD\\CNext\\CNext\\cpumetricsserver.exe\" ", row1[8]);
        Assert.Equal("Habilitado", row1[11]);

        var row2 = rows[2];
        Assert.Equal(@"\CreateExplorerShellUnelevatedTask", row2[1]);
        Assert.Equal(@"C:\WINDOWS\explorer.exe /NoUACCheck", row2[8]);
        Assert.Equal("Habilitado", row2[11]);
    }

    [Fact]
    public void ExtractExecutablePath_UnwrapsQuotedPathWithEmbeddedQuotesAndTrailingSpace()
    {
        var field = "\"C:\\Program Files\\AMD\\CNext\\CNext\\cpumetricsserver.exe\" ";

        var path = FileSignatureChecker.ExtractExecutablePath(field);

        Assert.Equal(@"C:\Program Files\AMD\CNext\CNext\cpumetricsserver.exe", path);
    }

    [Fact]
    public void ExtractExecutablePath_HandlesUnquotedPathWithArguments()
    {
        var path = FileSignatureChecker.ExtractExecutablePath(@"C:\WINDOWS\explorer.exe /NoUACCheck");

        Assert.Equal(@"C:\WINDOWS\explorer.exe", path);
    }
}

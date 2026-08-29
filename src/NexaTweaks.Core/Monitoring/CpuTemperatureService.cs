using System.Management;

namespace NexaTweaks.Core.Monitoring;

/// <summary>
/// Best-effort CPU temperature via the ACPI thermal zone WMI class. Many OEM boards disable or
/// misreport this, so a null result is expected and normal on plenty of real machines - callers
/// should treat it as "unavailable on this PC", not as an error.
/// </summary>
public sealed class CpuTemperatureService
{
    private bool _knownUnavailable;

    public double? SampleCelsius()
    {
        if (_knownUnavailable) return null;

        try
        {
            using var searcher = new ManagementObjectSearcher(
                "root\\WMI", "SELECT CurrentTemperature FROM MSAcpi_ThermalZoneTemperature");

            foreach (ManagementObject mo in searcher.Get())
            {
                var tenthsKelvin = Convert.ToDouble(mo["CurrentTemperature"]);
                var celsius = tenthsKelvin / 10.0 - 273.15;
                if (celsius is > -50 and < 150) return Math.Round(celsius, 1);
            }
        }
        catch
        {
            // no WMI provider for this - stop trying every sample
        }

        _knownUnavailable = true;
        return null;
    }
}

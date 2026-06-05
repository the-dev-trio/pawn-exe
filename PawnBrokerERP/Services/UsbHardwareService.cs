using System.IO;
using System.Management;
using System.Reflection;

namespace PawnBrokerERP.Services;

public class UsbHardwareService : IUsbHardwareService
{
    public string? GetCurrentUsbSerial()
    {
        try
        {
            var exePath = Assembly.GetExecutingAssembly().Location;
            if (string.IsNullOrEmpty(exePath))
                exePath = AppDomain.CurrentDomain.BaseDirectory;

            var driveLetter = Path.GetPathRoot(exePath)?.TrimEnd('\\');
            if (string.IsNullOrEmpty(driveLetter)) return null;

            // Walk: LogicalDisk -> Partition -> DiskDrive
            using var logicalToPart = new ManagementObjectSearcher(
                $"ASSOCIATORS OF {{Win32_LogicalDisk.DeviceID='{driveLetter}'}} " +
                $"WHERE AssocClass=Win32_LogicalDiskToPartition");

            foreach (ManagementObject partition in logicalToPart.Get())
            {
                using var partToDisk = new ManagementObjectSearcher(
                    $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}} " +
                    $"WHERE AssocClass=Win32_DiskDriveToDiskPartition");

                foreach (ManagementObject disk in partToDisk.Get())
                {
                    var interfaceType = disk["InterfaceType"]?.ToString();
                    var serial = disk["SerialNumber"]?.ToString()?.Trim();

                    // Accept USB or removable drives
                    if (!string.IsNullOrWhiteSpace(serial))
                        return serial;
                }
            }
        }
        catch
        {
            // WMI not available (dev environment / non-Windows)
        }

        return GetFallbackSerial();
    }

    public bool IsRunningFromUsb()
    {
        try
        {
            var exePath = Assembly.GetExecutingAssembly().Location;
            if (string.IsNullOrEmpty(exePath))
                exePath = AppDomain.CurrentDomain.BaseDirectory;

            var driveLetter = Path.GetPathRoot(exePath)?.TrimEnd('\\');
            if (string.IsNullOrEmpty(driveLetter)) return false;

            using var searcher = new ManagementObjectSearcher(
                $"SELECT MediaType FROM Win32_LogicalDisk WHERE DeviceID='{driveLetter}'");

            foreach (ManagementObject disk in searcher.Get())
            {
                var mediaType = disk["MediaType"]?.ToString();
                // MediaType 11 = Removable media, also check DriveType
                return mediaType == "Removable Media" || mediaType == "11";
            }
        }
        catch { }

        return false;
    }

    // Returns a machine-stable fallback for dev/non-USB scenarios
    private static string GetFallbackSerial()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
            foreach (ManagementObject obj in searcher.Get())
            {
                var serial = obj["SerialNumber"]?.ToString()?.Trim();
                if (!string.IsNullOrWhiteSpace(serial)) return $"DEV-{serial}";
            }
        }
        catch { }

        return "DEV-FALLBACK-00000";
    }
}

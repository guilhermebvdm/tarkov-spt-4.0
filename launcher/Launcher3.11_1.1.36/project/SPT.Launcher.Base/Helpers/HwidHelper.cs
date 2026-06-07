using System;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using SPT.Launcher.Controllers;

namespace SPT.Launcher.Helpers
{
    public static class HwidHelper
    {
        private static string _cachedHwid;

        /// <summary>
        /// Gera um HWID único baseado em: CPU ID + Motherboard Serial + Volume Serial do disco C
        /// O resultado é um hash SHA256 truncado para 32 caracteres
        /// </summary>
        public static string GetHwid()
        {
            if (!string.IsNullOrEmpty(_cachedHwid))
                return _cachedHwid;

            try
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    _cachedHwid = "UNSUPPORTED_OS";
                    return _cachedHwid;
                }

                string cpuId = GetWmiValue("Win32_Processor", "ProcessorId");
                string mbSerial = GetWmiValue("Win32_BaseBoard", "SerialNumber");
                string volumeSerial = GetWmiValue("Win32_LogicalDisk", "VolumeSerialNumber", "DeviceID = 'C:'");

                string combined = $"{cpuId}|{mbSerial}|{volumeSerial}";

                using (var sha256 = SHA256.Create())
                {
                    byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                    _cachedHwid = BitConverter.ToString(hash).Replace("-", "").Substring(0, 32);
                }

                LogManager.Instance.Info($"[HWID] Generated: {_cachedHwid.Substring(0, 8)}...");
            }
            catch (Exception ex)
            {
                LogManager.Instance.Exception(ex);
                _cachedHwid = "ERROR_GENERATING_HWID";
            }

            return _cachedHwid;
        }

        private static string GetWmiValue(string wmiClass, string property, string condition = null)
        {
            try
            {
                string query = $"SELECT {property} FROM {wmiClass}";
                if (!string.IsNullOrEmpty(condition))
                    query += $" WHERE {condition}";

                using (var searcher = new ManagementObjectSearcher(query))
                {
                    foreach (var obj in searcher.Get())
                    {
                        var value = obj[property]?.ToString();
                        if (!string.IsNullOrEmpty(value))
                            return value;
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Warning($"[HWID] Failed to get {wmiClass}.{property}: {ex.Message}");
            }

            return "UNKNOWN";
        }
    }
}

using Microsoft.Win32;
using SPT_AKI_Installer.Aki.Core.Model;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace SPT_AKI_Installer.Aki.Helper
{
    public static class PreCheckHelper
    {
        private const string registryInstall = @"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\EscapeFromTarkov";

        public static string DetectOriginalGamePath()
        {
            // We can't detect the installed path on non-Windows
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return null;

            var uninstallStringValue = Registry.LocalMachine.OpenSubKey(registryInstall, false)
                ?.GetValue("UninstallString");
            var info = (uninstallStringValue is string key) ? new FileInfo(key) : null;

            return info?.DirectoryName;
        }

        public static GenericResult DetectOriginalGameVersion(string gamePath)
        {
            try
            {
                string version = FileVersionInfo.GetVersionInfo(Path.Join(gamePath + "/EscapeFromTarkov.exe")).ProductVersion.Replace('-', '.').Split('.')[^2];
                return GenericResult.FromSuccess(version);
            }
            catch (Exception ex)
            {
                return GenericResult.FromError($"File not found: {ex.Message}");
            }
        }
    }
}

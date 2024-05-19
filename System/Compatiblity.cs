using System.Runtime.InteropServices;

public static class Compatiblity {
    public static string buildPath(string windowsPath) {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            return windowsPath;
        } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            return windowsPath.Replace('\\', '/');
        } else {
            return windowsPath; // not a supported OS for buildPath, we do not need total support
                                // due to the current deployment plan (deployed on Linux Intra w/o MacOS or Other OS support)
        }
    }
}
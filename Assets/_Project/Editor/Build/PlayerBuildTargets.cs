using System;
using UnityEditor;

namespace LastHaven.Editor
{
    /// <summary>Build menülerinin platformları.</summary>
    public enum PlayerBuildPlatform
    {
        Windows,
        MacOS,
    }

    /// <summary>Platform → Unity build hedefi, çıktı klasörü ve dosya uzantısı (saf; testler doğrudan çağırır).</summary>
    public static class PlayerBuildTargets
    {
        public static BuildTarget Target(PlayerBuildPlatform platform)
        {
            return platform == PlayerBuildPlatform.MacOS ? BuildTarget.StandaloneOSX : BuildTarget.StandaloneWindows64;
        }

        /// <summary>Çıktının alt klasörü ve loglardaki ad: Windows, macOS.</summary>
        public static string FolderName(PlayerBuildPlatform platform)
        {
            return platform == PlayerBuildPlatform.MacOS ? "macOS" : "Windows";
        }

        /// <summary>Unity Hub'daki modül adı (modül kurulu değilse hata mesajında).</summary>
        public static string ModuleName(PlayerBuildPlatform platform)
        {
            return platform == PlayerBuildPlatform.MacOS ? "Mac Build Support (Mono)" : "Windows Build Support (Mono)";
        }

        /// <summary>
        /// Proje köküne göre çıktı yolu, '/' ayraçlı: <c>Builds/Windows/LastHaven.exe</c>, <c>Builds/macOS/LastHaven.app</c>.
        /// </summary>
        public static string OutputPath(string outputFolder, PlayerBuildPlatform platform, string executableName)
        {
            string folder = (outputFolder ?? string.Empty).Replace('\\', '/').Trim('/');
            string extension = platform == PlayerBuildPlatform.MacOS ? ".app" : ".exe";
            string name = executableName ?? string.Empty;
            string file = name.EndsWith(extension, StringComparison.OrdinalIgnoreCase) ? name : name + extension;
            return $"{folder}/{FolderName(platform)}/{file}";
        }
    }
}

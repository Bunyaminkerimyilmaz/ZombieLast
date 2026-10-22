using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LastHaven.Editor
{
    /// <summary>
    /// <see cref="PlayerBuildConfig"/> değerlerinin kuralları (saf; testler doğrudan çağırır). Hatalı config Player Settings'e
    /// yazılmaz ve build alınmaz: ürün/şirket adı kayıt klasörünün adı olduğu için dosya adında geçersiz karakter içeremez.
    /// </summary>
    public static class PlayerBuildRules
    {
        // Windows ve macOS'ta dosya/klasör adında sorun çıkaran karakterler (ordinal; kültürden bağımsız).
        private const string InvalidNameCharacters = "<>:\"/\\|?*";

        private static readonly Regex VersionPattern = new Regex(@"^\d+\.\d+\.\d+$", RegexOptions.CultureInvariant);

        private static readonly Regex IdentifierPattern =
            new Regex(@"^[A-Za-z][A-Za-z0-9-]*(\.[A-Za-z][A-Za-z0-9-]*)+$", RegexOptions.CultureInvariant);

        /// <summary>Config'teki sorunlar (Türkçe; boşsa geçerli).</summary>
        public static List<string> Collect(PlayerBuildConfig config)
        {
            var problems = new List<string>();
            if (config == null)
            {
                problems.Add("PlayerBuildConfig yok.");
                return problems;
            }

            CheckName("Ürün adı", config.ProductName, problems);
            CheckName("Şirket adı", config.CompanyName, problems);
            CheckName("Çalıştırılabilir dosya adı", config.ExecutableName, problems);
            if (!IsValidVersion(config.Version))
            {
                problems.Add($"Sürüm '{config.Version}' büyük.küçük.yama biçiminde değil (ör. 0.1.0).");
            }

            if (!IsValidApplicationIdentifier(config.ApplicationIdentifier))
            {
                problems.Add($"Uygulama kimliği '{config.ApplicationIdentifier}' ters alan adı biçiminde değil (ör. com.lasthaven.game).");
            }

            if (!IsValidOutputFolder(config.OutputFolder))
            {
                problems.Add($"Çıktı klasörü '{config.OutputFolder}' proje köküne göre göreli ve '..' içermeyen bir yol olmalı (ör. Builds).");
            }

            return problems;
        }

        /// <summary>Boş değil, baş/son boşluk ve sonda nokta yok, dosya adında geçersiz veya kontrol karakteri yok.</summary>
        public static bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || !string.Equals(name, name.Trim(), StringComparison.Ordinal) || name.EndsWith(".", StringComparison.Ordinal))
            {
                return false;
            }

            foreach (char character in name)
            {
                if (char.IsControl(character) || InvalidNameCharacters.IndexOf(character) >= 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>"0.1.0" gibi üç sayılı sürüm.</summary>
        public static bool IsValidVersion(string version)
        {
            return version != null && VersionPattern.IsMatch(version);
        }

        /// <summary>"com.lasthaven.game" gibi en az iki parçalı, harfle başlayan parçalardan oluşan kimlik.</summary>
        public static bool IsValidApplicationIdentifier(string identifier)
        {
            return identifier != null && IdentifierPattern.IsMatch(identifier);
        }

        /// <summary>Boş değil, kök veya sürücü içermeyen, '..' parçası olmayan göreli klasör.</summary>
        public static bool IsValidOutputFolder(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder) || folder.StartsWith("/", StringComparison.Ordinal)
                || folder.StartsWith("\\", StringComparison.Ordinal) || folder.IndexOf(':') >= 0)
            {
                return false;
            }

            foreach (string part in folder.Split('/', '\\'))
            {
                if (string.Equals(part, "..", StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        private static void CheckName(string label, string value, List<string> problems)
        {
            if (!IsValidName(value))
            {
                problems.Add($"{label} '{value}' boş olamaz, boşlukla başlayıp bitemez ve {InvalidNameCharacters} karakterlerini içeremez.");
            }
        }
    }
}

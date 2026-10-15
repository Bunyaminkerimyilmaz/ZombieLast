namespace LastHaven.Editor
{
    /// <summary>
    /// Bir modülün proje doğrulaması (Tools/Last Haven/Validate Project). <c>TypeCache</c> ile otomatik bulunur ve
    /// <see cref="Order"/>'a göre çalışır; merkezi listeye kayıt gerekmez. Parametresiz constructor zorunludur.
    /// </summary>
    /// <remarks>
    /// Sadece okur: asset veya sahne değiştirmez, log basmaz (sonuçları <see cref="ValidationReport"/>'a yazar).
    /// Test assembly'sinde bu arayüzü uygulayan sınıf yazma; gerçek menü onu da çalıştırır.
    /// </remarks>
    public interface IValidationStep
    {
        /// <summary>Küçük olan önce çalışır; raporda bölümler bu sırayla görünür.</summary>
        int Order { get; }

        /// <summary>Rapordaki bölüm başlığı.</summary>
        string Name { get; }

        void Validate(ValidationReport report);
    }
}

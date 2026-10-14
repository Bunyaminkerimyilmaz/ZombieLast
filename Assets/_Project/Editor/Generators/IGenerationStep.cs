namespace LastHaven.Editor
{
    /// <summary>
    /// Bir modülün data ve prefab üreticisi. <c>TypeCache</c> ile otomatik bulunur ve <see cref="Order"/>'a göre çalışır.
    /// Merkezi bir listeye kayıt gerekmez. Parametresiz constructor zorunludur.
    /// </summary>
    /// <remarks>
    /// İdempotent olmalı: asset varsa dokunma, yoksa oluştur. Sadece eksik referansları doldurabilirsin.
    /// Test assembly'sinde bu arayüzü uygulayan sınıf yazma; gerçek menüler onu da çalıştırır.
    /// </remarks>
    public interface IGenerationStep
    {
        /// <summary>Küçük olan önce çalışır. Diğer adımların asset'ine ihtiyaç duyan adım daha büyük değer alır.</summary>
        int Order { get; }

        /// <summary>Log'larda görünen kısa ad.</summary>
        string Name { get; }

        /// <summary>ScriptableObject asset'lerini (ve materyalleri) eksikse oluşturur.</summary>
        void GenerateData();

        /// <summary>Prefab'ları eksikse oluşturur. Data adımlarının hepsinden sonra çalışır.</summary>
        void GeneratePrefabs();
    }
}

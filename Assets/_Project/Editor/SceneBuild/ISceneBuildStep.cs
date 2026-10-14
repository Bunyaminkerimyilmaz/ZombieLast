namespace LastHaven.Editor
{
    /// <summary>
    /// Game.unity'nin bir modüle ait kısmını kurar. <c>TypeCache</c> ile otomatik bulunur ve <see cref="Order"/>'a göre çalışır.
    /// Parametresiz constructor zorunludur.
    /// </summary>
    /// <remarks>
    /// Objeleri sadece <see cref="SceneBuildContext.Systems"/> ve <see cref="SceneBuildContext.Gameplay"/> altına kur.
    /// Bu kökler her build'de silinip yeniden kurulur; [Environment]'a dokunulmaz.
    /// </remarks>
    public interface ISceneBuildStep
    {
        /// <summary>Küçük olan önce çalışır. Başka adımın objesine ihtiyaç duyan adım daha büyük değer alır.</summary>
        int Order { get; }

        void Build(SceneBuildContext context);
    }
}

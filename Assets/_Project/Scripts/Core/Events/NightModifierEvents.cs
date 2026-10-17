namespace LastHaven.Core
{
    /// <summary>
    /// Gece olayı başladı (GDD §14): gecenin dalgası başlarken, <see cref="NightStartedEvent"/>'ten önce. Işık (sis, ay rengi),
    /// ekonomi (jeton çarpanı) ve jeneratör (güç kesintileri) dinler. Düşman adedi ve hızı dalga tarafında uygulanır.
    /// </summary>
    public readonly struct NightModifierActivatedEvent : IEvent
    {
        public NightModifierActivatedEvent(int nightIndex, NightModifierSettings modifier)
        {
            NightIndex = nightIndex;
            Modifier = modifier;
        }

        /// <summary>0 tabanlı gece indeksi.</summary>
        public int NightIndex { get; }

        /// <summary>Olayın tüm etkileri.</summary>
        public NightModifierSettings Modifier { get; }
    }

    /// <summary>Gece olayı bitti (gece tamamlanınca, <see cref="NightCompletedEvent"/>'ten önce). Dinleyiciler etkilerini geri alır.</summary>
    public readonly struct NightModifierEndedEvent : IEvent
    {
        public NightModifierEndedEvent(int nightIndex, NightModifierSettings modifier)
        {
            NightIndex = nightIndex;
            Modifier = modifier;
        }

        public int NightIndex { get; }

        /// <summary>Biten olayın etkileri (geri almak için; ör. <see cref="NightModifierSettings.SourceId"/>).</summary>
        public NightModifierSettings Modifier { get; }
    }
}

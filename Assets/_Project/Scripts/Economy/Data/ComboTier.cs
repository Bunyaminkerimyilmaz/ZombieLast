using System;
using UnityEngine;

namespace LastHaven.Economy
{
    /// <summary>Kombo eşiği (GDD §10): bu kadar ardışık öldürmede jeton değerine uygulanan çarpan.</summary>
    [Serializable]
    public struct ComboTier
    {
        [SerializeField, Min(1), Tooltip("Çarpanın başladığı ardışık öldürme sayısı.")]
        private int _kills;

        [SerializeField, Min(1f), Tooltip("Öldürme anında jeton değerine uygulanan çarpan.")]
        private float _multiplier;

        public ComboTier(int kills, float multiplier)
        {
            _kills = kills;
            _multiplier = multiplier;
        }

        public int Kills => _kills;

        public float Multiplier => _multiplier;
    }
}

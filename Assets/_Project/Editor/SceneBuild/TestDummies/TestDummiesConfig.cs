using System.Collections.Generic;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Silah testi kuklaları. Sadece sahne üreticisi okur; değiştirip Build Game Scene'i çalıştır.
    /// Asset: <c>Data/Debug/TestDummiesConfig.asset</c>.
    /// </summary>
    public sealed class TestDummiesConfig : ScriptableObject
    {
        [SerializeField, Tooltip("Build Game Scene kuklaları kursun mu. Tools/Last Haven/Options/Build Test Dummies menüsüyle de değişir.")]
        private bool _enabled = true;

        [SerializeField, Tooltip("Kukla konumları (m). Varsayılan: üs alanının (14 m) 2 m dışında, doğu-güneydoğuda, 2.5 m arayla; " +
                                 "engel halkasının (18 m) içinde kalır. Oyuncunun başlangıcına ~15 m.")]
        private Vector3[] _positions =
        {
            new Vector3(14.9f, 0f, -5.7f),
            new Vector3(13.9f, 0f, -8f),
            new Vector3(12.4f, 0f, -10.1f),
        };

        [SerializeField, Min(1f), Tooltip("Kukla canı (tabanca 20 hasarla 5 atış).")]
        private float _maxHealth = 100f;

        [SerializeField, Min(0f), Tooltip("Ölümden sonra dolu canla geri gelme süresi (sn).")]
        private float _respawnDelay = 3f;

        public bool Enabled => _enabled;

        public IReadOnlyList<Vector3> Positions => _positions;

        public float MaxHealth => _maxHealth;

        public float RespawnDelay => _respawnDelay;
    }
}

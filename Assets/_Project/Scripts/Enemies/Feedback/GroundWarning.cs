using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Yerdeki uyarı dairesi (GDD §7 Brute: vuruştan 1 sn önce): soluk kırmızı disk darbenin tam alanını gösterir, içindeki parlak disk
    /// süre boyunca dışa doğru büyür (dolunca vurur). Pasif görünüm: süreyi sahibi yürütür (<see cref="Show"/>, <see cref="SetProgress"/>)
    /// ve işi bitince havuza iade eder. Renkler MaterialPropertyBlock ile yazılır (materyale dokunulmaz).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GroundWarning : MonoBehaviour, IPoolable
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        // Unity silindiri 2 m yüksek: Y ölçeği = kalınlık / 2.
        private const float CylinderHeight = 2f;

        [SerializeField, Tooltip("Darbe alanının tamamı (soluk disk). Üretici bağlar.")]
        private Renderer _area;

        [SerializeField, Tooltip("Süreyle büyüyen iç disk. Üretici bağlar.")]
        private Renderer _fill;

        [SerializeField, Tooltip("Alan diskinin rengi (alfa = saydamlık).")]
        private Color _areaColor = new Color(0.95f, 0.1f, 0.06f, 0.22f);

        [SerializeField, Tooltip("İç diskin rengi; dolarken alfa bu değere çıkar.")]
        private Color _fillColor = new Color(1f, 0.15f, 0.08f, 0.55f);

        [SerializeField, Min(0.001f), Tooltip("Disklerin kalınlığı (m).")]
        private float _thickness = 0.02f;

        private MaterialPropertyBlock _block;
        private float _radius;

        public float Radius => _radius;

        /// <summary>İlerleme 0 → 1 (iç diskin yarıçap oranı).</summary>
        public float Progress { get; private set; }

        /// <summary>Daireyi <paramref name="radius"/> m yarıçapla, iç disk boş olarak gösterir.</summary>
        public void Show(float radius)
        {
            _radius = Mathf.Max(0f, radius);
            if (_area != null)
            {
                _area.transform.localScale = DiscScale(_radius, 1f);
                SetColor(_area, _areaColor);
            }

            SetProgress(0f);
        }

        /// <summary>İç diski <paramref name="progress"/> oranında büyütür (0 = görünmez, 1 = alan kadar).</summary>
        public void SetProgress(float progress)
        {
            Progress = Mathf.Clamp01(progress);
            if (_fill == null)
            {
                return;
            }

            _fill.transform.localScale = DiscScale(_radius * Progress, 1.5f);
            Color color = _fillColor;
            color.a *= Mathf.Lerp(0.4f, 1f, Progress);
            SetColor(_fill, color);
        }

        public void OnSpawned()
        {
            Progress = 0f;
        }

        public void OnDespawned()
        {
            Progress = 0f;
        }

        private Vector3 DiscScale(float radius, float thicknessScale)
        {
            float diameter = 2f * radius;
            return new Vector3(diameter, _thickness * thicknessScale / CylinderHeight, diameter);
        }

        private void SetColor(Renderer target, Color color)
        {
            _block ??= new MaterialPropertyBlock();
            _block.SetColor(BaseColorId, color);
            target.SetPropertyBlock(_block);
        }
    }
}

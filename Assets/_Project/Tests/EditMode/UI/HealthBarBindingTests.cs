using LastHaven.Core;
using LastHaven.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.Tests.EditMode
{
    /// <summary>BarView ve HealthBarBinding (6.3): bar yüzdesi ve etiket; Health'in yerel olaylarıyla güncellenir, çözülünce durur.</summary>
    public class HealthBarBindingTests
    {
        private const float Tolerance = 0.001f;

        private GameObject _owner;
        private Health _health;
        private VisualElement _fill;
        private Label _value;
        private BarView _bar;

        [SetUp]
        public void SetUp()
        {
            _owner = new GameObject("HealthBarBindingTests");
            _health = _owner.AddComponent<Health>();
            _health.SetMax(200f, refill: true);
            _fill = new VisualElement();
            _value = new Label();
            _bar = new BarView(_fill, _value);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_owner);
        }

        [Test]
        public void BarView_Set_WritesPercentWidthAndText()
        {
            _bar.Set(50f, 200f);

            Assert.AreEqual(LengthUnit.Percent, _fill.style.width.value.unit);
            Assert.AreEqual(25f, _fill.style.width.value.value, Tolerance);
            Assert.AreEqual("50/200", _value.text);
        }

        [Test]
        public void Attach_RefreshesImmediately()
        {
            var binding = new HealthBarBinding();

            binding.Attach(_health, _bar);

            Assert.AreSame(_health, binding.Health);
            Assert.AreEqual(100f, _fill.style.width.value.value, Tolerance);
            Assert.AreEqual("200/200", _value.text);
        }

        [Test]
        public void DamageAndHeal_UpdateBar()
        {
            var binding = new HealthBarBinding();
            binding.Attach(_health, _bar);

            _health.TakeDamage(new DamageInfo(50f, DamageType.Melee, null, Vector3.zero, Vector3.forward));
            Assert.AreEqual("150/200", _value.text);
            Assert.AreEqual(75f, _fill.style.width.value.value, Tolerance);

            _health.Heal(20f);
            Assert.AreEqual("170/200", _value.text);
        }

        [Test]
        public void Detach_StopsUpdates()
        {
            var binding = new HealthBarBinding();
            binding.Attach(_health, _bar);

            binding.Detach();
            _health.TakeDamage(new DamageInfo(50f, DamageType.Melee, null, Vector3.zero, Vector3.forward));

            Assert.IsNull(binding.Health);
            Assert.AreEqual("200/200", _value.text);
        }

        [Test]
        public void Attach_Null_DoesNothing()
        {
            var binding = new HealthBarBinding();

            Assert.DoesNotThrow(() => binding.Attach(null, _bar));
            Assert.DoesNotThrow(binding.Refresh);
            Assert.IsNull(binding.Health);
        }
    }
}

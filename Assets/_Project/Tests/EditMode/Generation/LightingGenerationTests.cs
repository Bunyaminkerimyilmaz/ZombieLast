using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Editor;
using LastHaven.GameFlow;
using LastHaven.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Gün/gece ışığı üreticisi (6.2): profil varsayılanları gündüzdür, gece daha karanlık ve mavi; post-process profilleri
    /// Vignette, Color Adjustments ve Bloom içerir; oyuncu feneri Visual dışında, kapalı spot ışık. Asset oluşturmaz.
    /// </summary>
    public class LightingGenerationTests
    {
        private const float Tolerance = 0.0001f;

        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object created in _created)
            {
                if (created != null)
                {
                    Object.DestroyImmediate(created);
                }
            }

            _created.Clear();
        }

        [Test]
        public void LightingProfile_Defaults_MatchDayDefinition()
        {
            var profile = Track(ScriptableObject.CreateInstance<LightingProfile>());

            AssertMatches(LightingDefinitions.Day, profile.Values);
            Assert.IsNull(profile.VolumeProfile, "Post-process profilini üretici bağlar.");
        }

        [Test]
        public void WriteTo_WritesNightValues()
        {
            var profile = Track(ScriptableObject.CreateInstance<LightingProfile>());

            LightingDefinitions.Night.WriteTo(profile);

            AssertMatches(LightingDefinitions.Night, profile.Values);
        }

        [Test]
        public void Night_IsDarkerAndBluerThanDay()
        {
            LightingDefinition day = LightingDefinitions.Day;
            LightingDefinition night = LightingDefinitions.Night;

            Assert.Less(night.SunIntensity, day.SunIntensity);
            Assert.Less(night.AmbientColor.grayscale, day.AmbientColor.grayscale);
            Assert.Greater(night.AmbientColor.b, night.AmbientColor.r, "Gece ambiyansı mavi (GDD §18).");
            Assert.Greater(night.SunColor.b, night.SunColor.r, "Ay ışığı mavi.");
            Assert.Greater(day.SunColor.r, day.SunColor.b, "Gündüz sıcak beyaz (GDD §18).");
            Assert.IsTrue(day.FogEnabled, "Sahne sis açık kaydedilsin (build'de sis varyantları silinmesin).");
            Assert.Greater(night.FogDensity, day.FogDensity);
        }

        [Test]
        public void PostProcess_Populate_AddsVignetteColorAdjustmentsBloom()
        {
            VolumeProfile day = Populate(LightingDefinitions.DayPostProcess);
            VolumeProfile night = Populate(LightingDefinitions.NightPostProcess);

            foreach (VolumeProfile profile in new[] { day, night })
            {
                Assert.AreEqual(3, profile.components.Count);
                Assert.IsTrue(profile.TryGet(out Vignette vignette));
                Assert.IsTrue(profile.TryGet(out ColorAdjustments colorAdjustments));
                Assert.IsTrue(profile.TryGet(out Bloom bloom));
                Assert.IsTrue(vignette.intensity.overrideState && vignette.smoothness.overrideState && vignette.color.overrideState);
                Assert.IsTrue(colorAdjustments.contrast.overrideState && colorAdjustments.saturation.overrideState
                              && colorAdjustments.colorFilter.overrideState && colorAdjustments.postExposure.overrideState);
                Assert.IsTrue(bloom.threshold.overrideState && bloom.intensity.overrideState && bloom.scatter.overrideState);
                Assert.IsFalse(vignette.center.overrideState, "Tanımda olmayan parametre URP varsayılanında kalır.");
            }

            day.TryGet(out Vignette dayVignette);
            night.TryGet(out Vignette nightVignette);
            night.TryGet(out ColorAdjustments nightColor);
            Assert.AreEqual(LightingDefinitions.DayPostProcess.VignetteIntensity, dayVignette.intensity.value, Tolerance);
            Assert.Greater(nightVignette.intensity.value, dayVignette.intensity.value, "Gece kenarlar daha koyu.");
            Assert.Greater(nightColor.colorFilter.value.b, nightColor.colorFilter.value.r, "Gece renk filtresi mavi.");
        }

        [Test]
        public void Flashlight_Ensure_BuildsDisabledSpotOutsideVisual()
        {
            GameObject root = CreatePlayerRoot();

            int changes = PlayerFlashlight.Ensure(root);

            Assert.Greater(changes, 0);
            Transform child = root.transform.Find(PlayerFlashlight.ObjectName);
            Assert.IsNotNull(child);
            Assert.AreSame(root.transform, child.parent, "Fener kökün çocuğu (Visual dışında).");
            Assert.IsEmpty(VisualRoot.Find(root.transform).GetComponentsInChildren<Light>());

            var light = child.GetComponent<Light>();
            Assert.IsNotNull(light);
            Assert.AreEqual(LightType.Spot, light.type);
            Assert.IsFalse(light.enabled, "Oyun gündüz başlar; fener kapalı.");
            Assert.Greater(child.localPosition.z, 0.5f, "Gövdenin önünde.");
            Assert.Greater(child.forward.z, 0f, "Nişan yönüne (ileri) bakar.");
            Assert.Less(child.forward.y, 0f, "Hafif yere bakar.");

            var flashlight = child.GetComponent<Flashlight>();
            Assert.IsNotNull(flashlight);
            Assert.AreSame(light, new SerializedObject(flashlight).FindProperty("_light").objectReferenceValue);
            Assert.IsFalse(flashlight.IsOn);
        }

        [Test]
        public void Flashlight_Ensure_IsIdempotentAndRepairsMissingParts()
        {
            GameObject root = CreatePlayerRoot();
            PlayerFlashlight.Ensure(root);

            Assert.AreEqual(0, PlayerFlashlight.Ensure(root), "İkinci çağrı değişiklik yapmaz.");

            Transform child = root.transform.Find(PlayerFlashlight.ObjectName);
            var light = child.GetComponent<Light>();
            Object.DestroyImmediate(child.GetComponent<Flashlight>());
            light.intensity = 3f;

            Assert.AreEqual(2, PlayerFlashlight.Ensure(root), "Eksik bileşen + boş referans.");
            Assert.AreEqual(3f, light.intensity, Tolerance, "Var olan ışığın ayarlarına dokunulmaz.");
        }

        private GameObject CreatePlayerRoot()
        {
            var root = Track(new GameObject("Player"));
            new GameObject(VisualRoot.Name).transform.SetParent(root.transform, false);
            return root;
        }

        private VolumeProfile Populate(in PostProcessDefinition definition)
        {
            var profile = Track(ScriptableObject.CreateInstance<VolumeProfile>());
            PostProcessProfiles.Populate(profile, definition);
            _created.AddRange(profile.components);
            return profile;
        }

        private T Track<T>(T created) where T : Object
        {
            _created.Add(created);
            return created;
        }

        private static void AssertMatches(in LightingDefinition expected, in LightingValues actual)
        {
            Assert.AreEqual(0f, Quaternion.Angle(Quaternion.Euler(expected.SunRotation), actual.SunRotation), 0.01f);
            Assert.AreEqual(expected.SunColor, actual.SunColor);
            Assert.AreEqual(expected.SunIntensity, actual.SunIntensity, Tolerance);
            Assert.AreEqual(expected.AmbientColor, actual.AmbientColor);
            Assert.AreEqual(expected.FogEnabled, actual.FogEnabled);
            Assert.AreEqual(expected.FogColor, actual.FogColor);
            Assert.AreEqual(expected.FogDensity, actual.FogDensity, Tolerance);
        }
    }
}

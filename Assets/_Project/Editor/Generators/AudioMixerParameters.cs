using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace LastHaven.Editor
{
    /// <summary>
    /// Mixer'ın açık parametreleri (14.3: ayarlar ekranı grupların ses düzeyini <c>AudioMixer.SetFloat</c> ile yazar). Public API yok;
    /// editörün "Volume → Expose" yolu (<c>AudioMixerController.AddExposedParameter</c> + <c>AudioGroupParameterPath</c>) reflection
    /// ile çağrılır (<see cref="AudioMixerFactory"/> ile aynı iç tip; Unity 6000.3'te doğrulandı). API değişirse hata + elle açma
    /// talimatı loglanır; sesler varsayılan düzeyde çalar.
    /// </summary>
    public static class AudioMixerParameters
    {
        private const BindingFlags InstanceFlags = AudioMixerFactory.InstanceFlags;
        private const string GroupParameterPathTypeName = "UnityEditor.Audio.AudioGroupParameterPath";

        /// <summary>
        /// Grubun ses düzeyini (Attenuation → Volume) <paramref name="parameterName"/> adıyla açık parametre yapar. Zaten açıksa sadece adı
        /// farklıysa düzeltir; bu adda başka bir parametre varsa dokunmaz (uyarı). Değiştiyse true. Başarısızsa hata loglar, false döner.
        /// </summary>
        public static bool EnsureExposedVolume(AudioMixer mixer, string path, string groupName, string parameterName)
        {
            AudioMixerGroup group = AudioMixerFactory.FindGroup(path, groupName);
            if (mixer == null || group == null || string.IsNullOrEmpty(parameterName))
            {
                return false;
            }

            try
            {
                Type type = AudioMixerFactory.GetControllerType();
                object guid = group.GetType().GetMethod("GetGUIDForVolume", InstanceFlags).Invoke(group, null);
                PropertyInfo exposedProperty = type.GetProperty("exposedParameters", InstanceFlags);
                if (!(bool)type.GetMethod("ContainsExposedParameter", InstanceFlags).Invoke(mixer, new[] { guid }))
                {
                    if (FindIndex((Array)exposedProperty.GetValue(mixer), "name", parameterName) >= 0)
                    {
                        Debug.LogWarning($"[Last Haven] Audio: {path} içinde '{parameterName}' adlı başka bir açık parametre var; " +
                                         $"{groupName} ses düzeyi açılmadı.");
                        return false;
                    }

                    Type pathType = typeof(UnityEditor.Editor).Assembly.GetType(GroupParameterPathTypeName);
                    object parameterPath = Activator.CreateInstance(pathType, group, guid);
                    type.GetMethod("AddExposedParameter", InstanceFlags).Invoke(mixer, new[] { parameterPath });
                }

                return Rename(mixer, exposedProperty, guid, parameterName);
            }
            catch (Exception exception)
            {
                Exception cause = AudioMixerFactory.Unwrap(exception);
                Debug.LogError($"[Last Haven] Audio: {path} içinde {groupName} ses düzeyi açılamadı ({cause.GetType().Name}: {cause.Message}). " +
                               $"Elle aç: Audio Mixer → {groupName} grubu → Inspector'da Volume'e sağ tık → Expose, adını '{parameterName}' yap.");
                return false;
            }
        }

        /// <summary>Mixer'ın açık parametre adları (reflection; okunamazsa boş). Testler kullanır.</summary>
        public static List<string> GetExposedNames(AudioMixer mixer)
        {
            var names = new List<string>();
            try
            {
                var exposed = (Array)AudioMixerFactory.GetControllerType().GetProperty("exposedParameters", InstanceFlags).GetValue(mixer);
                foreach (object parameter in exposed)
                {
                    names.Add((string)parameter.GetType().GetField("name", InstanceFlags).GetValue(parameter));
                }
            }
            catch (Exception)
            {
                names.Clear();
            }

            return names;
        }

        // Yeni açılan parametrenin adı "MyExposedParam"; dizinin elemanları struct olduğu için kopya düzenlenip dizi geri yazılır.
        private static bool Rename(AudioMixer mixer, PropertyInfo exposedProperty, object guid, string parameterName)
        {
            var exposed = (Array)exposedProperty.GetValue(mixer);
            int index = FindIndex(exposed, "guid", guid);
            if (index < 0)
            {
                return false;
            }

            object parameter = exposed.GetValue(index);
            FieldInfo nameField = parameter.GetType().GetField("name", InstanceFlags);
            if (string.Equals((string)nameField.GetValue(parameter), parameterName, StringComparison.Ordinal))
            {
                return false;
            }

            nameField.SetValue(parameter, parameterName);
            exposed.SetValue(parameter, index);
            exposedProperty.SetValue(mixer, exposed);
            EditorUtility.SetDirty(mixer);
            return true;
        }

        private static int FindIndex(Array exposed, string fieldName, object value)
        {
            for (int i = 0; i < exposed.Length; i++)
            {
                object parameter = exposed.GetValue(i);
                if (Equals(parameter.GetType().GetField(fieldName, InstanceFlags).GetValue(parameter), value))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}

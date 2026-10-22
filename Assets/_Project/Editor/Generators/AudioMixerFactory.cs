using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

namespace LastHaven.Editor
{
    /// <summary>
    /// AudioMixer asset'ini kodla üretir: Master grubu altında Music ve SFX. Unity'nin mixer oluşturma API'si public değil; editörün
    /// kendi "Create → Audio Mixer" ve "Add child group" yolu (<c>UnityEditor.Audio.AudioMixerController</c>) reflection ile çağrılır.
    /// Kodla oluşan mixer'da Audio Mixer penceresinin "View" listesi boş gelir; tüm grupları gösteren varsayılan görünüm eklenir.
    /// API değişirse hata loglanır ve null döner: sesler mixer'sız (doğrudan dinleyiciye) çalmaya devam eder, Validate Project boş
    /// grup referanslarını gösterir. Var olan mixer'a sadece eksik grup / görünüm eklenir.
    /// </summary>
    public static class AudioMixerFactory
    {
        public const string MasterGroupName = "Master";
        public const string MusicGroupName = "Music";
        public const string SfxGroupName = "SFX";

        /// <summary>Kodla eklenen varsayılan görünümün adı (Unity'nin menüyle oluşturduğu mixer'daki gibi).</summary>
        public const string DefaultViewName = "View";

        private const string ControllerTypeName = "UnityEditor.Audio.AudioMixerController";
        internal const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Mixer'ı yükler; yoksa oluşturur. Master altında eksik Music/SFX gruplarını ve boşsa varsayılan görünümü ekler. Başarısızsa null.
        /// </summary>
        public static AudioMixer LoadOrCreate(string path)
        {
            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(path);
            bool created = false;
            if (mixer == null)
            {
                mixer = Create(path);
                if (mixer == null)
                {
                    return null;
                }

                created = true;
                Debug.Log($"[Last Haven] Audio: {path} oluşturuldu.");
            }

            var addedGroups = new List<object>();
            EnsureChildGroup(mixer, path, MusicGroupName, addedGroups);
            EnsureChildGroup(mixer, path, SfxGroupName, addedGroups);
            bool viewChanged = EnsureView(mixer, addedGroups);
            if (addedGroups.Count > 0 || viewChanged)
            {
                EditorUtility.SetDirty(mixer);
                AssetDatabase.SaveAssetIfDirty(mixer);
                if (!created)
                {
                    Debug.Log($"[Last Haven] Audio: {path} eksikleri tamamlandı ({addedGroups.Count} grup{(viewChanged ? ", görünüm" : string.Empty)}).");
                }
            }

            return mixer;
        }

        /// <summary>Mixer asset'indeki adı verilen grup (ordinal); yoksa null.</summary>
        public static AudioMixerGroup FindGroup(string mixerPath, string groupName)
        {
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(mixerPath))
            {
                if (asset is AudioMixerGroup group && string.Equals(group.name, groupName, StringComparison.Ordinal))
                {
                    return group;
                }
            }

            return null;
        }

        /// <summary>Audio Mixer penceresinin görünüm sayısı (reflection; okunamazsa -1). Testler kullanır.</summary>
        public static int GetViewCount(AudioMixer mixer)
        {
            try
            {
                return ((Array)GetControllerType().GetProperty("views", InstanceFlags).GetValue(mixer)).Length;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        private static AudioMixer Create(string path)
        {
            try
            {
                EditorAssetUtility.EnsureFolder(path.Substring(0, path.LastIndexOf('/')));
                MethodInfo create = GetControllerType().GetMethod("CreateMixerControllerAtPath", StaticFlags);
                var mixer = (AudioMixer)create.Invoke(null, new object[] { path });
                AssetDatabase.SaveAssetIfDirty(mixer);
                return mixer;
            }
            catch (Exception exception)
            {
                LogFailure(path, exception);
                return null;
            }
        }

        // Unity'nin "Add child group" akışı: yeni grup (Attenuation efektiyle) → Master'ın çocuğu.
        private static void EnsureChildGroup(AudioMixer mixer, string path, string groupName, List<object> added)
        {
            if (FindGroup(path, groupName) != null)
            {
                return;
            }

            try
            {
                Type type = GetControllerType();
                object master = type.GetProperty("masterGroup", InstanceFlags).GetValue(mixer);
                object group = type.GetMethod("CreateNewGroup", InstanceFlags).Invoke(mixer, new object[] { groupName, false });
                type.GetMethod("AddChildToParent", InstanceFlags).Invoke(mixer, new[] { group, master });
                added.Add(group);
            }
            catch (Exception exception)
            {
                LogFailure(path, exception);
            }
        }

        // Görünüm yoksa tüm grupları (yeni eklenenler dahil) gösteren bir görünüm kurar; varsa sadece yeni grupları ekler.
        // Görünüm yalnızca pencerenin şeridini etkiler; başarısız olursa gruplar yine çalışır.
        private static bool EnsureView(AudioMixer mixer, List<object> addedGroups)
        {
            try
            {
                Type type = GetControllerType();
                PropertyInfo viewsProperty = type.GetProperty("views", InstanceFlags);
                var views = (Array)viewsProperty.GetValue(mixer);
                IEnumerable<object> toShow = addedGroups;
                bool changed = false;
                if (views.Length == 0)
                {
                    Type viewType = views.GetType().GetElementType();
                    object view = Activator.CreateInstance(viewType);
                    viewType.GetField("name", InstanceFlags).SetValue(view, DefaultViewName);
                    FieldInfo guids = viewType.GetField("guids", InstanceFlags);
                    guids.SetValue(view, Array.CreateInstance(guids.FieldType.GetElementType(), 0));
                    Array created = Array.CreateInstance(viewType, 1);
                    created.SetValue(view, 0);
                    viewsProperty.SetValue(mixer, created);
                    type.GetProperty("currentViewIndex", InstanceFlags).SetValue(mixer, 0);
                    toShow = mixer.FindMatchingGroups(string.Empty);
                    changed = true;
                }

                MethodInfo addToView = type.GetMethod("AddGroupToCurrentView", InstanceFlags);
                foreach (object group in toShow)
                {
                    addToView.Invoke(mixer, new[] { group });
                    changed = true;
                }

                return changed;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Last Haven] Audio: mixer görünümü kurulamadı ({Unwrap(exception).GetType().Name}); gruplar çalışır, " +
                                 "Audio Mixer penceresinde görünmezse görünüme elle ekle.");
                return false;
            }
        }

        internal static Type GetControllerType()
        {
            Type type = typeof(UnityEditor.Editor).Assembly.GetType(ControllerTypeName);
            if (type == null)
            {
                throw new MissingMemberException(ControllerTypeName);
            }

            return type;
        }

        internal static Exception Unwrap(Exception exception)
        {
            return exception is TargetInvocationException { InnerException: not null } invocation ? invocation.InnerException : exception;
        }

        private static void LogFailure(string path, Exception exception)
        {
            Exception cause = Unwrap(exception);
            Debug.LogError($"[Last Haven] Audio: {path} üretilemedi ({cause.GetType().Name}: {cause.Message}). Unity'nin iç mixer API'si " +
                           "değişmiş olabilir. Elle oluştur: Create → Audio Mixer, Master altına 'Music' ve 'SFX' grupları; sonra Generate " +
                           "Default Data AudioConfig'e bağlar.");
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LastHaven.Editor
{
    /// <summary>
    /// ScriptableObject'lerin nesne referansı kuralları. Serileştirilen her <see cref="Object"/> alanı zorunludur; şunlar hariç:
    /// Tooltip'inde "Opsiyonel" geçen alanlar ve ses/görsel tipleri (<see cref="AudioClip"/>, <see cref="Sprite"/>,
    /// <see cref="Texture"/>; CLAUDE.md "Ses/VFX/ikon referansları opsiyonel"). Projenin <c>[Serializable]</c> iç tipleri ve
    /// onların listeleri de gezilir. Object listelerinin boş elemanları sorun sayılmaz (ör. boş inşa yuvası).
    /// Kayıp referans (silinmiş/taşınmış asset'e işaret eden) opsiyonel alanda da hatadır.
    /// </summary>
    public static class RequiredReferenceValidator
    {
        /// <summary>Tooltip'te bu kelime geçerse alan boş bırakılabilir (büyük/küçük harf duyarsız, ordinal).</summary>
        public const string OptionalMarker = "opsiyonel";

        private const string ProjectNamespace = "LastHaven";
        private const string ArrayPathPart = ".Array.data[";
        private const int MaxDepth = 8;

        /// <summary>Hatalar: zorunlu referans boş; herhangi bir referans kayıp.</summary>
        public static void ValidateAsset(ScriptableObject asset, ValidationReport report)
        {
            var serialized = new SerializedObject(asset);
            Walk(asset, asset.GetType(), null, new Context(asset, serialized, report), 0);
        }

        /// <summary>Alan boş bırakılabilir mi (Tooltip işareti veya ses/görsel tipi).</summary>
        public static bool IsOptional(FieldInfo field)
        {
            Type type = field.FieldType;
            if (typeof(AudioClip).IsAssignableFrom(type) || typeof(Sprite).IsAssignableFrom(type) || typeof(Texture).IsAssignableFrom(type))
            {
                return true;
            }

            var tooltip = field.GetCustomAttribute<TooltipAttribute>();
            return tooltip != null && tooltip.tooltip.IndexOf(OptionalMarker, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>Tip bu projenin mi (<c>LastHaven.*</c> namespace'i).</summary>
        public static bool IsProjectType(Type type)
        {
            string ns = type.Namespace;
            return ns != null && (string.Equals(ns, ProjectNamespace, StringComparison.Ordinal)
                                  || ns.StartsWith(ProjectNamespace + ".", StringComparison.Ordinal));
        }

        /// <summary>Unity özellik yolunu okunur hale getirir: <c>_enemies.Array.data[2]._data</c> → <c>_enemies[2]._data</c>.</summary>
        public static string DescribePath(string propertyPath)
        {
            return propertyPath.Replace(ArrayPathPart, "[");
        }

        private static void Walk(object target, Type type, string path, Context context, int depth)
        {
            if (target == null || depth > MaxDepth)
            {
                return;
            }

            foreach (FieldInfo field in GetSerializedFields(type))
            {
                string fieldPath = path == null ? field.Name : path + "." + field.Name;
                object value = field.GetValue(target);
                if (typeof(Object).IsAssignableFrom(field.FieldType))
                {
                    CheckReference(value as Object, field, fieldPath, context);
                }
                else if (TryGetElementType(field.FieldType, out Type elementType))
                {
                    WalkList(value as IList, elementType, fieldPath, context, depth);
                }
                else if (value != null && IsWalkable(value.GetType()))
                {
                    Walk(value, value.GetType(), fieldPath, context, depth + 1);
                }
            }
        }

        private static void WalkList(IList list, Type elementType, string fieldPath, Context context, int depth)
        {
            if (list == null)
            {
                return;
            }

            bool references = typeof(Object).IsAssignableFrom(elementType);
            for (int i = 0; i < list.Count; i++)
            {
                string elementPath = $"{fieldPath}{ArrayPathPart}{i}]";
                object element = list[i];
                if (references)
                {
                    // Boş eleman bilinçli olabilir (boş yuva); sadece kayıp referans hata.
                    if ((element as Object) == null && IsMissing(elementPath, context))
                    {
                        ReportMissing(elementPath, context);
                    }
                }
                else if (element != null && IsWalkable(element.GetType()))
                {
                    Walk(element, element.GetType(), elementPath, context, depth + 1);
                }
            }
        }

        private static void CheckReference(Object reference, FieldInfo field, string fieldPath, Context context)
        {
            if (reference != null)
            {
                return;
            }

            if (IsMissing(fieldPath, context))
            {
                ReportMissing(fieldPath, context);
            }
            else if (!IsOptional(field))
            {
                context.Report.Error($"{context.Asset.name}: zorunlu '{DescribePath(fieldPath)}' boş.", context.Asset);
            }
        }

        // Kayıp referans C#'ta null görünür; serileştirilmiş özellik hâlâ bir nesne kimliği taşır.
        private static bool IsMissing(string propertyPath, Context context)
        {
            SerializedProperty property = context.Serialized.FindProperty(propertyPath);
            return property != null
                   && property.propertyType == SerializedPropertyType.ObjectReference
                   && property.objectReferenceValue == null
                   && property.objectReferenceInstanceIDValue != 0;
        }

        private static void ReportMissing(string propertyPath, Context context)
        {
            context.Report.Error(
                $"{context.Asset.name}: '{DescribePath(propertyPath)}' kayıp bir asset'e işaret ediyor (silinmiş veya taşınmış).", context.Asset);
        }

        private static IEnumerable<FieldInfo> GetSerializedFields(Type type)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            for (Type current = type; current != null && IsProjectType(current); current = current.BaseType)
            {
                foreach (FieldInfo field in current.GetFields(flags))
                {
                    if (IsSerialized(field))
                    {
                        yield return field;
                    }
                }
            }
        }

        private static bool IsSerialized(FieldInfo field)
        {
            if (field.IsInitOnly || field.IsLiteral || field.IsDefined(typeof(NonSerializedAttribute)))
            {
                return false;
            }

            return field.IsPublic || field.IsDefined(typeof(SerializeField)) || field.IsDefined(typeof(SerializeReference));
        }

        private static bool TryGetElementType(Type type, out Type elementType)
        {
            if (type.IsArray)
            {
                elementType = type.GetElementType();
                return true;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                elementType = type.GetGenericArguments()[0];
                return true;
            }

            elementType = null;
            return false;
        }

        // Sadece projenin [Serializable] tipleri gezilir (Vector3, Color gibi Unity tiplerinde referans yok).
        private static bool IsWalkable(Type type)
        {
            return !type.IsPrimitive && !type.IsEnum && type != typeof(string) && type.IsSerializable && IsProjectType(type);
        }

        private readonly struct Context
        {
            public Context(ScriptableObject asset, SerializedObject serialized, ValidationReport report)
            {
                Asset = asset;
                Serialized = serialized;
                Report = report;
            }

            public ScriptableObject Asset { get; }

            public SerializedObject Serialized { get; }

            public ValidationReport Report { get; }
        }
    }
}

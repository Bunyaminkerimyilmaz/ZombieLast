using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Bir asset'in inspector'da görünen tüm serileştirilmiş alanlarını "Alan | Değer" tablosuna döker (denge tablosunun ham bölümü).
    /// Listeler "ad (adet)" satırı ve elemanlarıyla, <c>[SerializeReference]</c> elemanları tip adıyla, referanslar asset adıyla yazılır.
    /// Alan yolu sadeleşir: <c>_levels.Array.data[0]._cost</c> → <c>levels[0].cost</c>.
    /// </summary>
    public static class SerializedValueTable
    {
        private const string ScriptField = "m_Script";

        public static MarkdownTable Build(Object asset)
        {
            var table = new MarkdownTable("Alan", "Değer");
            using (var serialized = new SerializedObject(asset))
            {
                SerializedProperty property = serialized.GetIterator();
                bool enterChildren = true;
                while (property.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    if (property.propertyPath == ScriptField || property.propertyType == SerializedPropertyType.ArraySize)
                    {
                        continue;
                    }

                    if (TryFormatLeaf(property, out string value))
                    {
                        table.AddRow(CleanPath(property.propertyPath), value);
                        continue;
                    }

                    // Bileşik değer: liste, struct/sınıf veya [SerializeReference]; satırı yazılır, çocuklarına inilir.
                    string summary = property.isArray ? "(" + property.arraySize.ToString(CultureInfo.InvariantCulture) + ")"
                        : property.propertyType == SerializedPropertyType.ManagedReference ? ShortTypeName(property.managedReferenceFullTypename)
                        : string.Empty;
                    if (summary.Length > 0)
                    {
                        table.AddRow(CleanPath(property.propertyPath), summary);
                    }

                    enterChildren = true;
                }
            }

            return table;
        }

        /// <summary><c>_a.Array.data[2]._b</c> → <c>a[2].b</c>.</summary>
        public static string CleanPath(string propertyPath)
        {
            string[] segments = propertyPath.Replace(".Array.data[", "[").Split('.');
            var text = new StringBuilder();
            for (int i = 0; i < segments.Length; i++)
            {
                if (i > 0)
                {
                    text.Append('.');
                }

                text.Append(segments[i].TrimStart('_'));
            }

            return text.ToString();
        }

        private static bool TryFormatLeaf(SerializedProperty property, out string value)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.ArraySize:
                    value = property.longValue.ToString(CultureInfo.InvariantCulture);
                    return true;
                case SerializedPropertyType.Boolean:
                    value = property.boolValue ? "evet" : "hayır";
                    return true;
                case SerializedPropertyType.Float:
                    value = BalanceFormat.Number(property.doubleValue);
                    return true;
                case SerializedPropertyType.String:
                    value = string.IsNullOrEmpty(property.stringValue) ? BalanceFormat.None : property.stringValue;
                    return true;
                case SerializedPropertyType.Enum:
                    value = property.enumValueIndex >= 0 && property.enumValueIndex < property.enumNames.Length
                        ? property.enumNames[property.enumValueIndex]
                        : property.intValue.ToString(CultureInfo.InvariantCulture);
                    return true;
                case SerializedPropertyType.ObjectReference:
                    value = property.objectReferenceValue != null ? property.objectReferenceValue.name : BalanceFormat.None;
                    return true;
                case SerializedPropertyType.Color:
                    value = "#" + ColorUtility.ToHtmlStringRGBA(property.colorValue);
                    return true;
                case SerializedPropertyType.Vector2:
                    value = Vector(property.vector2Value.x, property.vector2Value.y);
                    return true;
                case SerializedPropertyType.Vector3:
                    value = Vector(property.vector3Value.x, property.vector3Value.y, property.vector3Value.z);
                    return true;
                case SerializedPropertyType.Vector2Int:
                    value = Vector(property.vector2IntValue.x, property.vector2IntValue.y);
                    return true;
                case SerializedPropertyType.Vector3Int:
                    value = Vector(property.vector3IntValue.x, property.vector3IntValue.y, property.vector3IntValue.z);
                    return true;
                case SerializedPropertyType.AnimationCurve:
                    value = "eğri (" + property.animationCurveValue.length.ToString(CultureInfo.InvariantCulture) + " anahtar)";
                    return true;
                case SerializedPropertyType.Vector4:
                case SerializedPropertyType.Rect:
                case SerializedPropertyType.Bounds:
                case SerializedPropertyType.Quaternion:
                case SerializedPropertyType.Gradient:
                case SerializedPropertyType.ExposedReference:
                case SerializedPropertyType.Character:
                case SerializedPropertyType.FixedBufferSize:
                case SerializedPropertyType.RectInt:
                case SerializedPropertyType.BoundsInt:
                case SerializedPropertyType.Hash128:
                    value = property.propertyType.ToString();
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        private static string Vector(params double[] components)
        {
            var text = new StringBuilder("(");
            for (int i = 0; i < components.Length; i++)
            {
                if (i > 0)
                {
                    text.Append(", ");
                }

                text.Append(BalanceFormat.Number(components[i]));
            }

            return text.Append(')').ToString();
        }

        /// <summary>"Assembly Namespace.Type" → "Type".</summary>
        private static string ShortTypeName(string fullTypeName)
        {
            if (string.IsNullOrEmpty(fullTypeName))
            {
                return BalanceFormat.None;
            }

            int start = System.Math.Max(fullTypeName.LastIndexOf(' '), fullTypeName.LastIndexOf('.')) + 1;
            return fullTypeName.Substring(start);
        }
    }
}

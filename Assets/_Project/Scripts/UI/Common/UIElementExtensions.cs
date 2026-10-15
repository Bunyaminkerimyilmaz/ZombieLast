using UnityEngine.UIElements;

namespace LastHaven.UI
{
    public static class UIElementExtensions
    {
        /// <summary>Elementi <see cref="UIClassNames.Hidden"/> sınıfıyla gösterir/gizler (stil USS'te kalır).</summary>
        public static void SetShown(this VisualElement element, bool shown)
        {
            element.EnableInClassList(UIClassNames.Hidden, !shown);
        }
    }
}

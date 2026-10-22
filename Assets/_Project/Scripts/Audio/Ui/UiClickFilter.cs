using UnityEngine.UIElements;

namespace LastHaven.Audio
{
    /// <summary>Bir UI olayının hedefi tıklanabilir bir buton mu (kendisi ya da ataları arasında etkin bir <see cref="Button"/>).</summary>
    public static class UiClickFilter
    {
        /// <summary>Hedefin kendisi veya en yakın atası olan buton; yoksa null (ör. butonun içindeki etikete tıklama → buton).</summary>
        public static Button FindButton(IEventHandler target)
        {
            for (var element = target as VisualElement; element != null; element = element.parent)
            {
                if (element is Button button)
                {
                    return button;
                }
            }

            return null;
        }

        /// <summary>Hedef etkin bir butonun içinde mi (devre dışı buton ses çıkarmaz).</summary>
        public static bool IsClickable(IEventHandler target)
        {
            Button button = FindButton(target);
            return button != null && button.enabledInHierarchy;
        }
    }
}

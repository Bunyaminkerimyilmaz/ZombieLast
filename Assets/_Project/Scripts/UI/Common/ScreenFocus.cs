using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// Menü ekranlarının odak yardımcıları: açılınca ilk butona odak (klavye / gamepad gezinmesi ve Enter / A ile basma; UI Toolkit
    /// odağı panel yeniden çizildikten sonra kabul ettiği için bir sonraki adımda), kapanınca ekrandaki odağı bırakma (gizli butonda
    /// kalırsa oyundaki Enter / A ona basardı).
    /// </summary>
    public static class ScreenFocus
    {
        /// <summary><paramref name="target"/>'a bir sonraki zamanlayıcı adımında odaklanır (ekran hâlâ görünürse).</summary>
        public static void FocusLater(VisualElement screen, Focusable target)
        {
            if (screen == null || target == null)
            {
                return;
            }

            screen.schedule.Execute(() =>
            {
                if (!screen.ClassListContains(UIClassNames.Hidden))
                {
                    target.Focus();
                }
            });
        }

        /// <summary>Odak <paramref name="screen"/>'in içindeyse bırakılır.</summary>
        public static void Release(VisualElement screen)
        {
            if (Contains(screen, screen?.panel?.focusController?.focusedElement as VisualElement, out VisualElement focused))
            {
                focused.Blur();
            }
        }

        /// <summary>Odaktaki element <paramref name="screen"/>'in içinde mi.</summary>
        public static bool HasFocus(VisualElement screen)
        {
            return Contains(screen, screen?.panel?.focusController?.focusedElement as VisualElement, out _);
        }

        private static bool Contains(VisualElement screen, VisualElement element, out VisualElement focused)
        {
            focused = element;
            return screen != null && element != null && (element == screen || screen.Contains(element));
        }
    }
}

using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// Bir <see cref="UIDocument"/>'ın elementlerine bağlanan bileşenlerin tabanı. <c>OnEnable</c>'da UXML elementlerini
    /// <see cref="BindElements"/> ile bağlar; eksik element varsa hata loglar ve <see cref="IsBound"/> false kalır.
    /// UIDocument'ın execution order'ı (-100) daha erken olduğu için kök element <c>OnEnable</c>'da hazırdır.
    /// Aynı UIDocument'a birden çok denetleyici bağlanabilir (ör. HUD'un bölümleri). Görünürlüğü <see cref="UIScreen"/> yönetir.
    /// </summary>
    public abstract class UIDocumentController : MonoBehaviour
    {
        [SerializeField, Tooltip("Bu ekranın UIDocument'ı (aynı objede). Sahne adımı bağlar.")]
        private UIDocument _document;

        private bool _missingElement;

        /// <summary>UIDocument'ın kök elementi; bağlanamadıysa null.</summary>
        protected VisualElement Root { get; private set; }

        /// <summary>UXML kökündeki <see cref="UIClassNames.Screen"/> sınıflı element; bağlanamadıysa null.</summary>
        protected VisualElement ScreenElement { get; private set; }

        /// <summary>Tüm elementler bulunduysa true. Abonelikler sadece bu durumda yapılır.</summary>
        protected bool IsBound { get; private set; }

        protected virtual void OnEnable()
        {
            IsBound = false;
            _missingElement = false;
            ScreenElement = null;
            Root = _document != null ? _document.rootVisualElement : null;
            if (Root == null)
            {
                Debug.LogError($"{GetType().Name}: UIDocument veya UXML atanmamış; Tools/Last Haven/Build Game Scene çalıştır.", this);
                return;
            }

            ScreenElement = Root.Q(className: UIClassNames.Screen);
            if (ScreenElement == null)
            {
                Debug.LogError($"{GetType().Name}: UXML kökünde '{UIClassNames.Screen}' sınıflı element yok.", this);
                return;
            }

            BindElements();
            IsBound = !_missingElement;
        }

        /// <summary>UXML elementlerini <see cref="Require{T}"/> ile alanlara bağlar.</summary>
        protected abstract void BindElements();

        /// <summary>Adı verilen elementi bulur; yoksa hata loglar (bağlanmamış sayılır).</summary>
        protected T Require<T>(string elementName) where T : VisualElement
        {
            T element = Root.Q<T>(elementName);
            if (element == null)
            {
                Debug.LogError($"{GetType().Name}: UXML'de '{elementName}' ({typeof(T).Name}) bulunamadı.", this);
                _missingElement = true;
            }

            return element;
        }
    }
}

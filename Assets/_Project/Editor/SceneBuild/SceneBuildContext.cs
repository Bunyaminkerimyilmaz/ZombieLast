using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastHaven.Editor
{
    /// <summary>
    /// Sahne adımlarına verilen bağlam: üreticinin sahip olduğu kökler ve materyal sağlayıcı.
    /// [Environment] kökü bilerek verilmez; o kök manuel çevre/art içindir ve üretici dokunmaz.
    /// </summary>
    public sealed class SceneBuildContext
    {
        private readonly IMaterialProvider _materials;

        public SceneBuildContext(Scene scene, Transform systems, Transform gameplay, IMaterialProvider materials)
        {
            Scene = scene;
            Systems = systems;
            Gameplay = gameplay;
            _materials = materials;
        }

        public Scene Scene { get; }

        /// <summary>[Systems] kökü: servisler ve yöneticiler.</summary>
        public Transform Systems { get; }

        /// <summary>[Gameplay] kökü: harita, oyuncu, spawn bölgeleri gibi oynanış objeleri.</summary>
        public Transform Gameplay { get; }

        public Material GetMaterial(PlaceholderMaterialId id)
        {
            return _materials.Get(id);
        }

        /// <summary>Parent altında bu adda çocuk varsa onu, yoksa yeni boş obje döndürür. Adımlar ortak grupları böyle paylaşır.</summary>
        public Transform GetOrCreateGroup(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            return existing != null ? existing : CreateObject(name, parent).transform;
        }

        /// <summary>Parent altında, yerel orijinde boş obje oluşturur.</summary>
        public GameObject CreateObject(string name, Transform parent, int layer = 0)
        {
            var gameObject = new GameObject(name) { layer = layer };
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        /// <summary>Renk kodlu primitive oluşturur. <paramref name="withCollider"/> false ise collider silinir (sadece görsel).</summary>
        public GameObject CreatePrimitive(
            PrimitiveType type,
            string name,
            Transform parent,
            PlaceholderMaterialId material,
            int layer = 0,
            bool withCollider = true)
        {
            GameObject gameObject = GameObject.CreatePrimitive(type);
            gameObject.name = name;
            gameObject.layer = layer;
            gameObject.transform.SetParent(parent, false);
            gameObject.GetComponent<Renderer>().sharedMaterial = GetMaterial(material);

            if (!withCollider)
            {
                Object.DestroyImmediate(gameObject.GetComponent<Collider>());
            }

            return gameObject;
        }
    }
}

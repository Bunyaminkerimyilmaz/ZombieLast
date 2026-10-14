using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>Sahne adımlarının ve prefab üreticilerinin ortak renk kodlu primitive oluşturucusu.</summary>
    public static class PrimitiveFactory
    {
        /// <summary>Parent altında primitive oluşturur. <paramref name="withCollider"/> false ise collider silinir (sadece görsel).</summary>
        public static GameObject Create(
            PrimitiveType type,
            string name,
            Transform parent,
            Material material,
            int layer = 0,
            bool withCollider = true)
        {
            GameObject gameObject = GameObject.CreatePrimitive(type);
            gameObject.name = name;
            gameObject.layer = layer;
            gameObject.transform.SetParent(parent, false);
            gameObject.GetComponent<Renderer>().sharedMaterial = material;

            if (!withCollider)
            {
                Object.DestroyImmediate(gameObject.GetComponent<Collider>());
            }

            return gameObject;
        }
    }
}

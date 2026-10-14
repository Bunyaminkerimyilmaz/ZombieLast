using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>Üreticilere materyal veren kaynak. Şimdilik yer tutucular; ileride gerçek materyallere geçilebilir.</summary>
    public interface IMaterialProvider
    {
        Material Get(PlaceholderMaterialId id);
    }
}

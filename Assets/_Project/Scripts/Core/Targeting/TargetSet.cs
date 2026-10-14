using System;
using System.Collections.Generic;
using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// <see cref="ITargetRegistry"/>'nin saf C# implementasyonu. <see cref="TargetRegistry"/> bunu sarar;
    /// testler ve düşman hedef seçimi testleri doğrudan kullanabilir.
    /// </summary>
    public sealed class TargetSet : ITargetRegistry
    {
        private readonly List<ITargetable>[] _byType;

        public TargetSet()
        {
            int typeCount = Enum.GetValues(typeof(TargetType)).Length;
            _byType = new List<ITargetable>[typeCount];
            for (int i = 0; i < typeCount; i++)
            {
                _byType[i] = new List<ITargetable>();
            }
        }

        /// <summary>Tüm türlerdeki toplam kayıt sayısı.</summary>
        public int Count
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _byType.Length; i++)
                {
                    count += _byType[i].Count;
                }

                return count;
            }
        }

        public bool Register(ITargetable target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            List<ITargetable> list = _byType[(int)target.TargetType];
            if (list.Contains(target))
            {
                return false;
            }

            list.Add(target);
            return true;
        }

        public bool Unregister(ITargetable target)
        {
            if (target == null)
            {
                return false;
            }

            // Tür kayıttan sonra değişmez varsayılır; yine de hepsine bakmak ucuz ve güvenli.
            for (int i = 0; i < _byType.Length; i++)
            {
                if (_byType[i].Remove(target))
                {
                    return true;
                }
            }

            return false;
        }

        public ITargetable FindNearest(TargetType type, Vector3 position, float maxDistance)
        {
            List<ITargetable> list = _byType[(int)type];
            ITargetable nearest = null;
            float bestSqr = maxDistance * maxDistance;

            for (int i = 0; i < list.Count; i++)
            {
                ITargetable target = list[i];
                if (IsDestroyed(target) || !target.IsTargetable)
                {
                    continue;
                }

                Vector3 offset = target.Transform.position - position;
                float sqr = offset.x * offset.x + offset.z * offset.z;
                if (sqr <= bestSqr)
                {
                    bestSqr = sqr;
                    nearest = target;
                }
            }

            return nearest;
        }

        public IReadOnlyList<ITargetable> GetAll(TargetType type)
        {
            return _byType[(int)type];
        }

        public void Clear()
        {
            for (int i = 0; i < _byType.Length; i++)
            {
                _byType[i].Clear();
            }
        }

        // Kaydını silmeden yok edilen bir bileşenin Transform'una erişmek exception fırlatır.
        private static bool IsDestroyed(ITargetable target)
        {
            return target is UnityEngine.Object unityObject && unityObject == null;
        }
    }
}

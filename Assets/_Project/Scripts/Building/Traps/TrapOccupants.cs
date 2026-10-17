using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// Tuzağın üstündekiler: kutu alanında (<c>Physics.OverlapBoxNonAlloc</c>, hedef layer'ları, trigger'lar yok sayılır) canlı ve
    /// etkin hasar alabilen nesneler, durum efekti alıcılarıyla (<see cref="DamageableLookup"/>, <see cref="StatusEffectLookup"/>).
    /// Birden fazla collider'ı olan nesne bir kez sayılır. Tampon ve liste bir kez ayrılır; tarama bellek ayırmaz.
    /// </summary>
    /// <remarks>
    /// OnTriggerEnter/Exit yerine sorgu: düşmanlarda Rigidbody yok, ölünce collider kapanır ve havuza dönünce Exit gelmez.
    /// Alan yine kökteki trigger kutusudur (üstünden geçilir, mermiler ve inşa çakışması onu görmez).
    /// </remarks>
    public sealed class TrapOccupants
    {
        private readonly Collider[] _buffer;
        private readonly LayerMask _layers;
        private readonly List<TrapOccupant> _occupants;

        public TrapOccupants(int bufferSize, LayerMask layers)
        {
            _buffer = new Collider[Mathf.Max(1, bufferSize)];
            _layers = layers;
            _occupants = new List<TrapOccupant>(_buffer.Length);
        }

        /// <summary>Son taramanın sonucu. Bir sonraki taramaya kadar geçerli.</summary>
        public IReadOnlyList<TrapOccupant> Current => _occupants;

        /// <summary>Kutunun dünya uzayındaki merkezini, yarı boyutlarını ve dönüşünü verir (kökün ölçeği dahil).</summary>
        public static void GetWorldBox(BoxCollider box, out Vector3 center, out Vector3 halfExtents, out Quaternion rotation)
        {
            Transform transform = box.transform;
            Vector3 scale = transform.lossyScale;
            center = transform.TransformPoint(box.center);
            halfExtents = new Vector3(
                Mathf.Abs(box.size.x * scale.x), Mathf.Abs(box.size.y * scale.y), Mathf.Abs(box.size.z * scale.z)) * 0.5f;
            rotation = transform.rotation;
        }

        /// <summary>Kutu collider'ının alanını tarar; üstündeki sayısını döndürür.</summary>
        public int Scan(BoxCollider area)
        {
            GetWorldBox(area, out Vector3 center, out Vector3 halfExtents, out Quaternion rotation);
            return Scan(center, halfExtents, rotation);
        }

        /// <summary>Kutuyu tarar; üstündeki sayısını döndürür.</summary>
        public int Scan(Vector3 center, Vector3 halfExtents, Quaternion rotation)
        {
            _occupants.Clear();
            int count = Physics.OverlapBoxNonAlloc(center, halfExtents, _buffer, rotation, _layers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                Collider collider = _buffer[i];
                IDamageable damageable = DamageableLookup.Find(collider);
                if (damageable is not Component component || !damageable.IsAlive || !component.gameObject.activeInHierarchy
                    || Contains(damageable))
                {
                    continue;
                }

                _occupants.Add(new TrapOccupant(damageable, StatusEffectLookup.Find(collider), component.transform.position));
            }

            // Bir sonraki taramaya kadar ölü collider referansı tutulmasın.
            System.Array.Clear(_buffer, 0, count);
            return _occupants.Count;
        }

        public void Clear()
        {
            _occupants.Clear();
        }

        private bool Contains(IDamageable damageable)
        {
            for (int i = 0; i < _occupants.Count; i++)
            {
                if (ReferenceEquals(_occupants[i].Damageable, damageable))
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>Tuzağın üstündeki bir nesne: hasar alan, (varsa) durum efekti alıcısı ve tarama anındaki konumu.</summary>
    public readonly struct TrapOccupant
    {
        public TrapOccupant(IDamageable damageable, IStatusEffectReceiver receiver, Vector3 position)
        {
            Damageable = damageable;
            Receiver = receiver;
            Position = position;
        }

        public IDamageable Damageable { get; }

        /// <summary>Null olabilir (yavaşlatılamayan nesne).</summary>
        public IStatusEffectReceiver Receiver { get; }

        public Vector3 Position { get; }
    }
}

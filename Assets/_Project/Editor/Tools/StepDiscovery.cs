using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>Bir arayüzü uygulayan tüm adımları <see cref="TypeCache"/> ile bulur, örnekler ve sıralar.</summary>
    public static class StepDiscovery
    {
        /// <summary>
        /// <typeparamref name="T"/>'yi uygulayan somut tipleri örnekler; <paramref name="orderOf"/>'a, eşitlikte tip adına göre sıralar.
        /// </summary>
        public static List<T> CreateAll<T>(Func<T, int> orderOf) where T : class
        {
            var steps = new List<T>();
            foreach (Type type in TypeCache.GetTypesDerivedFrom<T>())
            {
                if (type.IsAbstract || type.IsInterface || type.ContainsGenericParameters)
                {
                    continue;
                }

                if (!type.IsValueType && type.GetConstructor(Type.EmptyTypes) == null)
                {
                    Debug.LogWarning($"[Last Haven] {type.FullName} parametresiz constructor'a sahip değil, atlandı.");
                    continue;
                }

                steps.Add((T)Activator.CreateInstance(type));
            }

            Sort(steps, orderOf);
            return steps;
        }

        /// <summary>Önce sıraya, eşitlikte tam tip adına (ordinal) göre sıralar; sonuç her makinede aynıdır.</summary>
        public static void Sort<T>(List<T> steps, Func<T, int> orderOf) where T : class
        {
            steps.Sort((a, b) =>
            {
                int byOrder = orderOf(a).CompareTo(orderOf(b));
                return byOrder != 0
                    ? byOrder
                    : string.CompareOrdinal(a.GetType().FullName, b.GetType().FullName);
            });
        }
    }
}

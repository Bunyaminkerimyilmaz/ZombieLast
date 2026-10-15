using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Testlerde ayarlanabilir <see cref="IInteractable"/>: noktadan uzaklık, çağrı sayıları ve ilerleme geçmişi.</summary>
    public sealed class FakeInteractable : IInteractable
    {
        public FakeInteractable(Vector3 position, float range = 2f, float holdDuration = 1f)
        {
            Position = position;
            InteractionRange = range;
            HoldDuration = holdDuration;
        }

        public Vector3 Position { get; set; }

        public Vector3 PromptPosition => Position;

        public string Prompt { get; set; } = "Test";

        public bool CanInteract { get; set; } = true;

        public float HoldDuration { get; set; }

        public float InteractionRange { get; set; }

        /// <summary>OnInteract'in döneceği değer.</summary>
        public bool InteractResult { get; set; } = true;

        /// <summary>Bu kadar etkileşimden sonra CanInteract false olur (ör. tam onarıldı); 0 = sınırsız.</summary>
        public int InteractionsUntilDone { get; set; }

        public int InteractCount { get; private set; }

        public List<float> Progress { get; } = new List<float>();

        public float LastProgress => Progress.Count > 0 ? Progress[Progress.Count - 1] : -1f;

        public float GetDistance(Vector3 point)
        {
            return PlanarDistance.Between(point, Position);
        }

        public void OnHoldProgress(float progress)
        {
            Progress.Add(progress);
        }

        public bool OnInteract()
        {
            InteractCount++;
            if (InteractionsUntilDone > 0 && InteractCount >= InteractionsUntilDone)
            {
                CanInteract = false;
            }

            return InteractResult;
        }
    }
}

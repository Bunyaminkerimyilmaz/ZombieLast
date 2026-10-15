using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>Bir yapının belirli bir noktaya yerleşimi ve sonucu (<see cref="BuildSite.Evaluate"/>). Önizleme ve yerleştirme bunu kullanır.</summary>
    public readonly struct BuildPlacement
    {
        public BuildPlacement(BuildableData data, RectInt cells, int rotationSteps, Vector3 position, PlacementStatus status)
        {
            Data = data;
            Cells = cells;
            RotationSteps = rotationSteps;
            Position = position;
            Status = status;
        }

        public BuildableData Data { get; }

        /// <summary>Kaplanan hücreler (dünya eksenlerine hizalı, döndürülmüş ayak izi).</summary>
        public RectInt Cells { get; }

        /// <summary>Y ekseninde 90°'lik adım sayısı (0–3).</summary>
        public int RotationSteps { get; }

        /// <summary>Ayak izinin merkezi, yer seviyesinde (yapının pivotu).</summary>
        public Vector3 Position { get; }

        public Quaternion Rotation => Quaternion.Euler(0f, RotationSteps * 90f, 0f);

        public PlacementStatus Status { get; }

        public bool IsValid => Status == PlacementStatus.Valid;
    }

    /// <summary>Izgaraya yerleştirilmiş yapının kaydı: yapı bileşeni, tipi, hücreleri ve dönüşü.</summary>
    public readonly struct PlacedStructure
    {
        public PlacedStructure(Structure structure, BuildableData data, RectInt cells, int rotationSteps)
        {
            Structure = structure;
            Data = data;
            Cells = cells;
            RotationSteps = rotationSteps;
        }

        /// <summary>Yapının kök bileşeni (testlerde null olabilir).</summary>
        public Structure Structure { get; }

        public BuildableData Data { get; }

        public RectInt Cells { get; }

        public int RotationSteps { get; }
    }
}

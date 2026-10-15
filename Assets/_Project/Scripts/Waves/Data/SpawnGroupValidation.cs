namespace LastHaven.Waves
{
    /// <summary>Bir <see cref="SpawnGroup"/>'un neden doğurulamayacağı. <see cref="None"/> = geçerli.</summary>
    public enum SpawnGroupProblem
    {
        None,
        MissingGroup,
        MissingEnemy,
        ZeroCount,
        NoDirection,
    }

    /// <summary>
    /// Spawn grubu geçerlilik kuralı; tek kaynak. Runtime (<see cref="WavePlan"/> geçersiz grubu atlar) ve editördeki
    /// Validate Project aynı kuralı kullanır.
    /// </summary>
    public static class SpawnGroupValidation
    {
        /// <summary>Grubun ilk sorununu döndürür; sırayla: grup yok, düşman verisi yok, adet ≤ 0, yön yok.</summary>
        public static SpawnGroupProblem GetProblem(SpawnGroup group)
        {
            if (group == null)
            {
                return SpawnGroupProblem.MissingGroup;
            }

            if (group.Enemy == null)
            {
                return SpawnGroupProblem.MissingEnemy;
            }

            if (group.Count <= 0)
            {
                return SpawnGroupProblem.ZeroCount;
            }

            return group.Directions == null || group.Directions.Count == 0
                ? SpawnGroupProblem.NoDirection
                : SpawnGroupProblem.None;
        }

        /// <summary>Log ve rapor metni (Türkçe).</summary>
        public static string Describe(SpawnGroupProblem problem)
        {
            switch (problem)
            {
                case SpawnGroupProblem.MissingGroup:
                    return "grup boş";
                case SpawnGroupProblem.MissingEnemy:
                    return "düşman verisi yok";
                case SpawnGroupProblem.ZeroCount:
                    return "adet 0";
                case SpawnGroupProblem.NoDirection:
                    return "yön yok";
                default:
                    return "sorun yok";
            }
        }
    }
}

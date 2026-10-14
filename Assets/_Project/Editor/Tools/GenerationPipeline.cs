using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>Tüm <see cref="IGenerationStep"/>'leri sırayla çalıştırır. Bir adım hata verirse loglanır, diğerleri devam eder.</summary>
    public static class GenerationPipeline
    {
        /// <summary>Data aşamasını çalıştırır; hata sayısını döndürür.</summary>
        public static int GenerateData()
        {
            return Run("Generate Default Data", step => step.GenerateData());
        }

        /// <summary>Prefab aşamasını çalıştırır; hata sayısını döndürür.</summary>
        public static int GeneratePrefabs()
        {
            return Run("Generate Prefabs", step => step.GeneratePrefabs());
        }

        private static int Run(string phase, Action<IGenerationStep> action)
        {
            List<IGenerationStep> steps = StepDiscovery.CreateAll<IGenerationStep>(step => step.Order);
            int failed = 0;
            foreach (IGenerationStep step in steps)
            {
                try
                {
                    action(step);
                }
                catch (Exception exception)
                {
                    failed++;
                    Debug.LogError($"[Last Haven] {phase}: '{step.Name}' başarısız.");
                    Debug.LogException(exception);
                }
            }

            AssetDatabase.SaveAssets();

            string summary = $"[Last Haven] {phase}: {steps.Count} adım, {failed} hata.";
            if (failed > 0)
            {
                Debug.LogError(summary);
            }
            else
            {
                Debug.Log(summary);
            }

            return failed;
        }
    }
}

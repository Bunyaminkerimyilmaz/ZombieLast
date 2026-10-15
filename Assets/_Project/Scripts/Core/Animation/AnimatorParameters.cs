using System.Collections.Generic;
using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// <see cref="AnimationDriver"/>'ın Animator köprüsü: controller'da olan parametreleri yazar, olmayanları sessizce atlar
    /// (konsolda "parameter does not exist" uyarısı çıkmaz). Parametre listesi controller başına bir kez okunur.
    /// </summary>
    internal sealed class AnimatorParameters
    {
        private readonly Animator _animator;
        private readonly HashSet<int> _ids = new HashSet<int>();
        private RuntimeAnimatorController _source;

        /// <param name="animator">Null olabilir (Animator yok).</param>
        public AnimatorParameters(Animator animator)
        {
            _animator = animator;
        }

        /// <summary>Controller'ı atanmış bir Animator var mı. Yoksa prosedürel yedek oynatılır.</summary>
        public bool IsUsable => _animator != null && _animator.runtimeAnimatorController != null;

        public void SetFloat(int id, float value)
        {
            if (CanSet(id))
            {
                _animator.SetFloat(id, value);
            }
        }

        public void SetBool(int id, bool value)
        {
            if (CanSet(id))
            {
                _animator.SetBool(id, value);
            }
        }

        public void SetTrigger(int id)
        {
            if (CanSet(id))
            {
                _animator.SetTrigger(id);
            }
        }

        /// <summary>Animator'ı varsayılan durumuna döndürür (havuzdan çıkış, yeniden doğma).</summary>
        public void Rebind()
        {
            if (IsUsable && _animator.isActiveAndEnabled)
            {
                _animator.Rebind();
                _animator.Update(0f);
            }
        }

        private bool CanSet(int id)
        {
            if (!IsUsable || !_animator.isInitialized)
            {
                return false;
            }

            // Update'te bellek ayırmasın diye liste controller değişince bir kez okunur.
            RuntimeAnimatorController controller = _animator.runtimeAnimatorController;
            if (_source != controller)
            {
                _source = controller;
                _ids.Clear();
                AnimatorControllerParameter[] parameters = _animator.parameters;
                for (int i = 0; i < parameters.Length; i++)
                {
                    _ids.Add(parameters[i].nameHash);
                }
            }

            return _ids.Contains(id);
        }
    }
}

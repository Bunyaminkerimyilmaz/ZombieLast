namespace LastHaven.Enemies
{
    /// <summary>Durum makinesinin bir durumu. Her düşmanda bir kez oluşturulur ve havuz döngüleri boyunca yeniden kullanılır.</summary>
    internal abstract class EnemyState
    {
        protected EnemyState(EnemyStateMachine machine)
        {
            Machine = machine;
        }

        public abstract EnemyStateId Id { get; }

        protected EnemyStateMachine Machine { get; }

        protected IEnemyBody Body => Machine.Body;

        protected EnemyBehaviourSettings Settings => Machine.Settings;

        public virtual void Enter()
        {
        }

        public abstract void Tick(float deltaTime);

        public virtual void Exit()
        {
        }
    }
}

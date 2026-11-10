namespace Project.Core
{
    public sealed class StateMachine<TOwner>
    {
        readonly TOwner _owner;

        public IState<TOwner> Current { get; private set; }

        public StateMachine(TOwner owner)
        {
            _owner = owner;
        }

        public void Set(IState<TOwner> next)
        {
            if (ReferenceEquals(next, Current)) return;
            Current?.Exit(_owner);
            Current = next;
            Current?.Enter(_owner);
        }

        public void Tick(float deltaTime) => Current?.Tick(_owner, deltaTime);
    }
}

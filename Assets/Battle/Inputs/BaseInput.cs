namespace Battle.Inputs
{
    public abstract class BaseInput
    {
        public virtual float Horizontal { get; }
        public virtual float Vertical { get;  }

        public abstract void Update(float deltaTime);
    }
}
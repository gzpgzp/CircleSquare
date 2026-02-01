namespace Battle.Inputs
{
    public abstract class BaseInput
    {
        public virtual float Horizontal { get; protected set; }
        public virtual float Vertical { get; protected set; }

        public abstract void Update(float deltaTime);
    }
}
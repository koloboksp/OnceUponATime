namespace Assets.Scripts.Core
{
    public interface IOrderedFixedUpdate
    {
        int Order { get; }
        void OrderedFixedUpdate();
    }
}
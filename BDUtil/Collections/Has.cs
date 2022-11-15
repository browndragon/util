namespace BDUtil
{
    public interface IHas
    {
        object Value { get; }
    }
    public interface IHas<out T> : IHas
    {
        new T Value { get; }
    }
    public interface ISet
    {
        object Value { set; }
    }
    public interface ISet<T> : ISet
    {
        new T Value { set; }
    }
}

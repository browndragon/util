namespace BDUtil
{
    /// Just so nobody uses these by mistake. They're NOT serializable.
    /// But they do have iteration-order constraints, which is nice.
    public class Set<T> : Collection<T, T>
    {
        protected override T GetKey(T t) => t;
    }
}
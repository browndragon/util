namespace BDUtil.Raw
{
    // An iteration-order hashset collection.
    public class Set<T> : Collection<T, T>
    {
        protected override T GetKey(T t) => t;
    }
}
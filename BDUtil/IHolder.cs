namespace BDUtil
{
    /// A generic type similar to Nullable. The main purpose is a mixin for unity types like ScriptableObject and MonoBehaviour to delegate their implementations to some concrete other type stored in Value.
    public interface IHolder<out T> { T Value { get; } }
}
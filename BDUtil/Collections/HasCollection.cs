using System.Collections;

namespace BDUtil
{
    /// Marker interface for something which has a collection.
    /// Particularly needed because if we're observing something, we want to know it's got an `IList<T>`,
    /// not that it's a `SerializableCollection<ObservableDeque<T>, T, ByteCompatible<T>>` or something...
    public interface IHasCollection
    {
        IEnumerable Collection { get; }
    }
    public interface IHasCollection<out TColl> : IHasCollection
    where TColl : IEnumerable
    {
        new TColl Collection { get; }
    }
}

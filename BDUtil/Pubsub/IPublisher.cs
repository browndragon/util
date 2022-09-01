namespace BDUtil.Pubsub
{
    /// Write/send view of a topic. See Topic for the read side.
    public interface IPublisher { void Invoke(); }
    public interface IPublisher<T> : IPublisher { void Publish(T value); }
}
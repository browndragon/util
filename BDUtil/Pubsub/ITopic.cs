using System;

namespace BDUtil.Pubsub
{
    /// Subscribe/read view of a topic. See publisher for the send view.
    public interface ITopic { IDisposable Subscribe(ISubscriber observer); }
    public interface ITopic<T> : ITopic { IDisposable Subscribe(ISubscriber<T> observer); }
    public interface ISelfTopic<T> : ITopic<T>
    where T : ISelfTopic<T>
    { }
}
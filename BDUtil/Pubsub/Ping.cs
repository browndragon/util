using System;
using System.Collections.Generic;

namespace BDUtil.Pubsub
{
    /// A topic which can ping its members arbitrarily.
    // Because it's definitionally made of Actions, the pingInstance.Invoke (using extensions) is legal & easy.
    public class Ping : Topic<HashSet<Action>, Action> { }

    /// Just like a ping, but the ping has some payload...
    public class Ping<T1> : Topic<HashSet<Action<T1>>, Action<T1>> { }
}
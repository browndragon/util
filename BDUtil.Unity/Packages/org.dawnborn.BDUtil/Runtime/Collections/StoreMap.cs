using System.Collections.Generic;

namespace BDUtil
{
    public abstract class StoreMap<TImpl, K, V> : Store<TImpl, KeyValuePair<K, V>, KVP.Entry<K, V>>
    where TImpl : ICollection<KeyValuePair<K, V>>, new()
    {
        protected override KVP.Entry<K, V> Internal(KeyValuePair<K, V> t) => t;
        protected override KeyValuePair<K, V> External(KVP.Entry<K, V> t) => t;
    }
}

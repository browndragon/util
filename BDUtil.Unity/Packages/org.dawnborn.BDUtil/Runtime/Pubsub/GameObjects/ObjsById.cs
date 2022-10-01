using System.Collections.Generic;
using System.Collections.ObjectModel;
using BDUtil.Raw;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Objs/ById", order = +4)]
    [Tooltip("Topic of InstanceId->GameObject; useful for set membership")]
    public class ObjsById : CollectionTopic<Observable.Dictionary<int, GameObject>, int, GameObject>
    {
        public bool TryGetValue(int id, out GameObject go) => Collection.TryGetValue(id, out go);
        public bool Contains(GameObject go) => Collection.ContainsKey(go.GetInstanceID());
        public bool Contains(Component c) => Collection.ContainsKey(c?.gameObject?.GetInstanceID() ?? 0);
    }
}
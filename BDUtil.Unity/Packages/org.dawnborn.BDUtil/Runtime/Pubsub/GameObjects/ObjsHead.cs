using System;
using System.Collections.Generic;
using BDUtil.Raw;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Objs/Head")]
    public class ObjsHead : HeadTopic<GameObject>
    {
        /// If you just want a stable ordering...?
        [Serializable]
        protected struct IdCmp : IComparer<GameObject>
        {
            static readonly IComparer<int?> IntCmp = Comparer<int?>.Default;
            public int Compare(GameObject x, GameObject y) => IntCmp.Compare(x?.GetInstanceID(), y?.GetInstanceID());
        }
        public interface ISortComponent : IComparable<ISortComponent> { }
        /// If you *know* you're going to substitute some other ordering...
        [Serializable]
        protected struct SortComponentCmp : IComparer<GameObject>
        {
            public int Compare(GameObject x, GameObject y) => (x?.GetComponent<ISortComponent>(), y?.GetComponent<ISortComponent>()) switch
            {
                (null, null) => 0,
                (_, null) => -1,
                (null, _) => +1,
                (ISortComponent xsc, ISortComponent ysc) => xsc.CompareTo(ysc),
            };
        }
    }
}
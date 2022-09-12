using System;
using System.Collections;
using System.Collections.Generic;
using BDUtil.Raw;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Actions/Head")]
    public class ActionsHead : HeadTopic<Action>
    {
        [Serializable]
        protected struct DelegateCmp : IComparer<Action>
        {
            static readonly IComparer<object> ObjCmp = Comparer<object>.Default;
            public int Compare(Action x, Action y) => ObjCmp.Compare(x?.Target, y?.Target);
        }
    }
}
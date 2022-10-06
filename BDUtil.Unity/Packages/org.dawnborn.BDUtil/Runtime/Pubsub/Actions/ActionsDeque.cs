using System;
using BDUtil.Bind;
using BDUtil.Raw;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Actions/Deque", order = +3)]
    [Impl(typeof(CollectionTopic<Observable.Deque<Action>>))]
    public class ActionsDeque : CollectionTopic<Observable.Deque<Action>, Action> { }
}
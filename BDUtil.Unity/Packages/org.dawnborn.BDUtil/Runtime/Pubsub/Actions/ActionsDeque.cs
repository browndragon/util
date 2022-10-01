using System;
using BDUtil.Raw;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Actions/Deque", order = +3)]
    public class ActionsDeque : CollectionTopic<Observable.Deque<Action>, Action> { }
}
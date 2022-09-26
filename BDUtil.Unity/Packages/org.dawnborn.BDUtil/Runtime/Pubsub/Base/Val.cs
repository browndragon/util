// using System;
// using UnityEngine;

// namespace BDUtil.Pubsub
// {
//     /// Lets a type have a valuetopic slotted in if it needs it, or else function "locally" without it.
//     [Serializable]
//     public class Val<T>
//     {
//         [SerializeField] ValueTopic<T> topic;
//         public ValueTopic<T> Topic => topic ??= MakeNewTopic();

//         ValueTopic<T> MakeNewTopic()
//         {
//             Type bestType = Bind.Bindings<Bind.ImplAttribute>.Default.GetBestType(typeof(ValueTopic<T>)).OrThrowInternal("Can't instantiate {0}", typeof(ValueTopic<T>));
//             ValueTopic<T> valueTopic = (ValueTopic<T>)ScriptableObject.CreateInstance(bestType);
//             valueTopic.DefaultValue = DefaultValue;
//             return valueTopic;
//         }
//         public T Value
//         {
//             get => Topic.Value;
//             set => Topic.Value = value;
//         }
//         public T DefaultValue;
//     }
// }
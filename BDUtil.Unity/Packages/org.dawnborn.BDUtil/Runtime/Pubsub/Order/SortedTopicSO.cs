// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Diagnostics.CodeAnalysis;
// using BDUtil.Pubsub.Ping;
// using BDUtil.Raw;
// using UnityEngine;

// namespace BDUtil.Pubsub.Orderly
// {
//     public abstract class SortedTopicSO<TMember> : ScriptableObject<TMember>, ITopic<TMember>
//     where TMember : IMember<ITopic<TMember>>, IComparable<TMember>
//     {
//         protected override ICollection<TMember> MakeSet() => new SortedList<TMember>();
//         protected SortedList<TMember> List => (SortedList<TMember>)Set;

//         [SerializeField, SuppressMessage("IDE", "IDE0044")] IPing<ITopic> OnMemberInvoke;
//         IPing<ITopic> ITopic<TMember>.OnMemberIterate => OnMemberInvoke;
//         public int CurrentIndex { get; private set; }
//         public TMember Current { get; private set; } = default;

//         public TMember this[int index]
//         {
//             get => List[index];
//             set
//             {
//                 if (IsLocked) throw new ITopic.LockedException();
//                 List[index] = value;
//             }
//         }

//         public int IndexOf(TMember item)
//         {
//             if (IsLocked) throw new ITopic.LockedException();
//             return List.IndexOf(item);
//         }
//         public void Insert(int index, TMember item)
//         {
//             if (IsLocked) throw new ITopic.LockedException();
//             List.Insert(index, item);
//         }
//         public void RemoveAt(int index)
//         {
//             if (IsLocked) throw new ITopic.LockedException();
//             List.RemoveAt(index);
//         }

//         public IEnumerator Invoke()
//         {
//             CurrentIndex = -1;
//             foreach (TMember o in this)  // Get that gooood locking.
//             {
//                 CurrentIndex++;
//                 Current = o;
//                 if (o == null) { Debug.Log($"Dead member {o}", this); continue; }
//                 o.IsCanceled = false;
//                 OnMemberInvoke?.Invoke(this);
//                 /// Sure, we started their turn, but them someone yanked the steering wheel and canceled pre-run!
//                 if (o.IsCanceled) continue;
//                 IEnumerator theyDelay = o.Invoke(this);
//                 if (theyDelay != null) yield return theyDelay;
//             }
//             CurrentIndex = -1;
//             Current = default;
//         }

//         public void Sort(IComparer<TMember> comparer)
//         {
//             if (IsLocked) throw new ITopic.LockedException();
//             List.Comparer = comparer;
//             if (comparer != null) List.Sort();
//         }
//     }
// }
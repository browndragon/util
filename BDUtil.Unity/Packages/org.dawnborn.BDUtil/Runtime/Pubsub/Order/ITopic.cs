// using System.Collections;
// using System.Collections.Generic;
// using BDUtil.Pubsub.Ping;

// namespace BDUtil.Pubsub.Orderly
// {
//     public interface ITopic<TMember> : Pubsub.ITopic<TMember>, IList<TMember>, IReadOnlyList<TMember>
//     {
//         /// Go through all members and call their Invoke, delaying between each for Begin/End.
//         /// Might be coroutine-y IEnumerable<IEnumerable>...
//         /// This is the same idea as the Invoke extension method on ienumerable<Action>, but of course
//         /// this isn't Action so that wouldn't quite work here...
//         IEnumerator Invoke();

//         /// Invoked with the new present value of each Current (before their execution).
//         IPing<ITopic> OnMemberIterate { get; }

//         /// During an iteration, we have a specific
//         int CurrentIndex { get; }
//         TMember Current { get; }

//         /// Change the underlying sort scheme between members & re-sort. I imagine this is rare?
//         /// You _can_ sort under null, which makes the whole thing dirty.
//         void Sort(IComparer<TMember> comparer);
//     }
// }
// using System;
// using System.Collections;

// namespace BDUtil.Pubsub.Orderly
// {
//     /// A member which is choosy about the types of topics into which it can be entered.
//     /// For the most part you don't need this; the Topic is already choosy about the member.
//     public interface IMember<in TTopic>
//     where TTopic : ITopic
//     {
//         // Not required to be respected! But: request cancel.
//         public bool IsCanceled { get; set; }
//         IEnumerator Invoke(TTopic topic);
//     }
// }
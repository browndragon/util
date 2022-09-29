using UnityEngine;

namespace BDUtil.Serialization
{
    /// The tagged attribute will display as a popup, with options from the return value of `methodName`.
    /// The method is invoked reflectively on the type containing the attribute.
    /// This method must return IEnumerable.
    /// Each entry in the enumerable can be:
    ///   * If the field is of int type and T is not, selection results in its positional assignment.
    ///   * T: display is .ToString & (onSelect) set to the field
    ///   * object: Cast & as T.
    ///   * string: Cast & as T
    ///   * IEnumerable: #0 .ToString displayed, #1 onSelect as above.
    ///   * ITuple: As IEnumerable.
    /// If those objects then return ITuple, it's assumed that the 0th element is the string label, and the 1th element the value to return; if it's a Delegate, it will first be invoked!
    /// These have to be equals-able; that's how we detect which value is currently selected.
    /// They're *reference assigned*; how will the serialization even work -- a problem for _you_, not me!
    public class ChoiceAttribute : PropertyAttribute
    {
        public string MethodName;
        public ChoiceAttribute(string methodName) => MethodName = methodName;
    }
}
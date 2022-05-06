# BDUtil

A collection of utilities for C# and Unity.

## Installation
### Unity3d
If you're coming from Unity3d, you can install this into your project using the
[package manager add git](https://docs.unity3d.com/Manual/upm-ui-giturl.html) and specifying this project's:
[git folder query parameter](https://forum.unity.com/threads/some-feedback-on-package-manager-git-support.743345/#post-5425311) (deep breath!):
```
https://github.com/browndragon/util.git?path=/BDUtil.Unity/Packages/net.dundrago.BDUtil
```
the package path is (or should be!) `net.dundrago.bdutil`.

### C#
TODO: implement nuget.

## API guide
* `BDUtil.Raw`: Collections API extensions based on Raw.Set and Raw.Map.
  All collections implement `Add()`-ordered traversal, such that even multiply indexed collections like multimap iterate as `KeyValuePair<,>` instances in the order in which they were added to the collection.
  > Unity Note: ALL of these types are them mirrored one level up in `BDUtil` with no additional documentation. Those versions are serializable in Unity (on serialize, they copy to a list; on deserialize, they maintain an additional list of errors)!
  * `BiMap`: A bidirectional map (the `Reverse` direction is a `Multimap`).
  * `Collection`: Some additional interfaces for extensions, and the base Collection implementation.
    * `IContainer<T>`: an `IReadOnlyCollection<>` with the `bool Contains(T)` method.
    * `IReadOnlyMultiMap<,>` and `-BiMap<,>` are provided for your code's safety. I didn't provide mutable interfaces, because I didn't need them; you might.
    * `Collection<K, T>` implements the insertion order semantics of some arbitrary `T` which can derive an insertion key `K` (so for instance, in a set `K` _is_ `T`, in a map `T` is actually `KeyValuePair<K, V>`, etc). Your code shouldn't need to look at this.
    * `Collection<K, T>.Entry`: an internal insertion location reference in case I decide to move away from linkedlist.
    * `ITryGetValue<,>` advertises the presence of that method (which is enough to implement things like `GetValueOrDefault` or `Contains`)
      > Note: I _don't_ implement `GetValueOrDefault`, because it's very difficult to write it usefully in the C# type system.
    * `IRemoveKey<,>` is similar, provides a per-key mutation method.
      > Note: No ~~`AddKey<K, V>`~~ or bulk `Clear()`? Yeah: that's just `ICollection<T>.Add()` and `.Clear()` for `T` a `KeyValuePair<K, V>`.
  * `FKeySet` is a functional `KeySet` (its constructor takes a `delegate`).
  * `KeySet` is an abstract set of types `T` which can derive a `K`. Unlike a `Map`, iteration order assumes you know how to derive `K` yourself, and so rather than forcing you into `KeyValuePair`, it iterates as `T`.
  * `Map`: an `IDictionary` and `IReadOnlyDictionary` whose iteration order is insertion order.
  * `Multimap`: A "`Map`" where `KeyValuePair<K, V>` with the same `Key` do *not* collide, but instead are allowable as long as the `V`s are different. Because it's maintained in `KVP` order, it isn't super efficient in swapping specific elements or anything; if you want that behavior, consider just using a `Map<K, Set<V>>` explicitly; there are extension methods `Add` and `Remove` that make that easier than you'd think!
  * `Set`: See `Map`; just the keys and none of the values. Perversely, this doesn't support C#'s `ISet` interface, just `ICollection` and my `IContainer` (thus `IReadOnlyCollection`).
  * `Table`: A `Map` whose keys are tuples `(R,C)` with additional indexes maintained on R & C.
* `Arrays`: Extensions for working with `Array`s (particularly the irritating `ICollection.CopyTo` reuired implementations).
* `Collections`: Extension methods for my exotic collections, so I don't have to keep implementing the derivable operations.
* `EnumArray<U, T>`: An `IDictionary` with a compact enum-position-defined layout.
  > Note: No guarantees are made for flag enums or other large gaps. This is not very clever.
  > Unity note: It's serilizable! in fact, it looks like a struct, which might be the best of both worlds.
* `EnumData<U>`: Extensions making it easier to reflectively work with enums.
  > TODO: Is there a better way to do this?
* `Enumerables`: Extensions for `IEnumerator`s to make it slightly more pleasant to manually drive them (but why...), and to `IEnumerable<>` to get a succinct ToString that's space-limited.
* `HashCode`: A little struct to make it easier to correctly implement `GetHashCode()` on your types.
* `KVP`: `KeyValuePair` `Key` and `Value` collections (particulary for implementing dictionaries).
  * `KVP.Keys` assumes your enumerator returns `KeyValuePairs` and extracts just the keys. For this library's implementation of `Collection`, that's very helpful, since the native storage of e.g. `Map` **is** `KVP<K, V>`. And it supports `Contains`.
  * `KVP.Values` assumes you have a working `this[K]` indexer and have implemented `Keys`, and uses that to derive `Values`. And I didn't bother making it support `Contains`, but you can use Linq's `IEnumerable.Contains` extension if you insist.
* `Legacy` wraps an `IReadOnlyCollection` or `IContainer` up into an unmodifiable `ICollection`
* `None` and `None<>` and `None<,>` support various useful collection interfaces and present an unmodifiable empty singletonish structy collection.
* `Objects` has extensions for throwing exceptions on false or null (`OrThrow()`), true or nonnull (`AndThrow()`), and implementing equality & comparison overrides.
* `Types` has an extension for walking the type tree of an object.

## Care and feeding
Doing a build stamps a new BDUtil.dll into the unity package, so it's important to explicitly build before pushing.
Otherwise, the unity lib won't be able to see your changes!


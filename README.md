# BDUtil

A collection of utilities for C# and Unity.

## Installation
### Unity3d
If you're coming from Unity3d, you can install this into your project using the
[package manager add git](https://docs.unity3d.com/Manual/upm-ui-giturl.html) and specifying this project's:
[git folder query parameter](https://forum.unity.com/threads/some-feedback-on-package-manager-git-support.743345/#post-5425311) (deep breath!):
```
https://github.com/browndragon/util.git?path=/BDUtil.Unity/Packages/org.dawnborn.BDUtil
```
the package path is (or should be!) `org.dawnborn.bdutil`.

### C#
TODO: implement nuget.

## API guide

This library does a lot. Sorry, notsorry, labor of love, used it in my own games.

High level, it provides untuned serializable collections & object extensions in the top level namespace, the base implementation collections under `Raw`, reflective utilities under `Bind`, and math wonkery under `Math`.
The justification for this quixotic organization is 1) C#/Unity should have included these better collections types out of the box and 2) I'm currently the only user, and haven't complained yet.

* `BDUtil.Raw`: Collections API extensions based on Raw.Set and Raw.Map.
  All collections implement `Add()`-ordered traversal, such that even multiply indexed collections like multimap iterate as `KeyValuePair<,>` instances in the order in which they were added to the collection.
  > Unity Note: ALL of these types are them mirrored one level up in `BDUtil` with no additional documentation. Those versions are serializable in Unity (on serialize, they copy to a list; on deserialize, they maintain an additional list of errors)!
  * `BiMap`: A bidirectional map (the `Reverse` direction is a `Multimap`).
  * `Collection`: Some additional interfaces for extensions, and the base Collection implementation.
    * `IContainer<T>`: an `IReadOnlyCollection<>` with the `bool Contains(T)` method.
    * `IReadOnlyMultiMap<,>` and `-BiMap<,>` are provided for your code's safety. I didn't provide mutable interfaces, because I didn't need them; you might.
    * `Collection<K, T>` implements the insertion order semantics of some arbitrary `T` which can derive an insertion key `K` (so for instance, in a set `K` _is_ `T`, in a map `T` is actually `KeyValuePair<K, V>`, etc). Your code shouldn't need to look at this.
    * `Collection<K, T>.Entry`: an internal insertion location reference in case I decide to move away from linkedlist.
  * `FKeySet` is a functional `KeySet` (its constructor takes a `delegate`).
  * `KeySet` is an abstract set of types `T` which can derive a `K`. Unlike a `Map`, iteration order assumes you know how to derive `K` yourself, and so rather than forcing you into `KeyValuePair`, it iterates as `T`.
  * `Map`: an `IDictionary` and `IReadOnlyDictionary` whose iteration order is insertion order.
  * `Multimap`: A "`Map`" where `KeyValuePair<K, V>` with the same `Key` do *not* collide, but instead are allowable as long as the `V`s are different. Because it's maintained in `KVP` order, it isn't super efficient in swapping specific elements or anything; if you want that behavior, consider just using a `Map<K, Set<V>>` explicitly; there are extension methods `Add` and `Remove` that make that easier than you'd think!
  * `Set`: See `Map`; just the keys and none of the values. Perversely, this doesn't support C#'s `ISet` interface, just `ICollection` and my `IContainer` (thus `IReadOnlyCollection`).
  * `Table`: A `Map` whose keys are tuples `(R,C)` with additional indexes maintained on R & C.
  * `RingBuffer<T>`: A fixed sized deque; adding after it's full swamps the other end.
* `BDUtil.Bind` has utilities for reflectively exploring your code
  * `AssemblyDeps` builds the set of assemblies which are loaded and which depend on some set of seed dependencies. This is great if you want to find everything that could possibly be tagged with one of your own annotations -- they have to transitively depend on your library!
  * `BindAttribute` generically indicates "I'm doing some binding here between this type and one or more identifiers". Because C# Attributes can't be generic, the identifiers are just generic objects.
    * `ImplAttribute` declares the tagged type `Impl`ements some or all of its interfaces in a way you can later discover with a `Bindings<Impl>`. For example, if you have some `interface IPrinter { void Print(string); }`, you might also declare `[Impl(typeof(IPrinter)] struct UnityPrinter : IPrinter { void Print(string s) => Debug.Log(s); }`. If your types are simple, you don't need to provide the argument to Impl; it'll walk the tree and find all types and interfaces you inherit, ignoring any in a `System` or `Unity` namespace and declare them automatically.
    * `NamedAttribute` declares the tagged type is `Named` some nicknames. This is respected later in `SubclassAttribute`, so bear it in mind.
  * `Bindings<TBindAttribute>` maintains the reverse mapping that the given `TBindAttribute : BindAttribute` declares.
* `BDUtil.Math` provides mathematical utilities.
  * `Arith` provides arithmetic operators generic by type for ints, floats, radians, degrees (and `UnityArith` provides additional `Vector2/3/4`, `Color`, etc). It works by analogy with `EqualityComparator` -- there's an `IArith<T>`, an `Arith<T>.Default` which implements the same interface, and you can always implement your own. All operations are provided in terms of a small number of core arithmetic operations (`Add`, `Scale`, `Dot`, and a bool "NaN-detecting" `IsValid`).
  * `Easings` provides standard easing functions as cached `Func<float, float>`, `enum`, and of course the actual methods. It also provides `Serializable` wrappers.
  * `PID` provides a [PID controller](wikipedia.org/wiki/PID_controller). This lets you e.g. determine a target speed, and then let it figure out how to tune physics forces to reach that speed.
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

## Unity extensions in `BDUtil.Unity`
### Structure
Unity completely owns this directory, so its `sln` and `csproj` should not be manually edited.
The outer `.sln` and in particular `../BDUtil/BDUtil.csproj`'s build step will put a copy of the built `.dll` & `.pdb` into `BDUtil.Unity/Packages/org.dawnborn.BDUtil/BDUtil/*`, which is how Unity gets access to the rest of this library.
The Unity package is implemented at `BDUtil.Unity/Packages/org.dawnborn.BDUtil/package.json`.

### API
All of the datatypes in `Raw` are extended (same name, but in the root namespace; implementations in `Collections`) to be Unity-serializable assuming that their underlying types are unity serializable (figuring out how to make that happen is your problem!).

But wait, there's more!
* `Pool/` supports `Pooled` objects which can be added to a `Registry` while active or a `Pool` while inactive, both of which can be queried.
* `CameraExt` has utility extensions.
* `ComponentsExt` has extensions for working with collections of `GameObjects` or `Components`, translating between them, searching nearby, etc. It's basically Linq for components.
* `EditorUtils` solves some very specific editor-interactive problems which make it easier to write inspector scripts. They're also safe to call from live code.
  * `InstantiateWithLink` creates instances of a prefab & remembers the link (without this, they're flattened out and changes to the prefab no longer cascade).
  * `CloneInactive` creates instances of a prefab with the prefab object set inactive, so that you can modify the child before its `Awake` method is called.
  * `DestroyChildrenByPlaystate` does an immediate destroy from the editor, or a standard end of frame destroy if called during play.
* `GameControllers` makes use of the `GameController` unity reserved tag -- which doesn't do anything! -- to build a parallel concept to `Camera.main`.
* `VectorExt` contains math extensions on `Vector2` and `3` and both of their `Int` types and `Bound`s and `Rectangle`s.

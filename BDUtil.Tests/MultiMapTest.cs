using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTI.Tests
{
    public class RawMultiMapTests
    {
        [Test]
        public void EmptyBehaviors()
        {
            Raw.MultiMap<string, int> map = new();
            Assert.AreEqual(0, map.Count);
            Assert.IsNotNull(map["never added"]);
            Assert.AreEqual(0, map["never added"].Count);
            Assert.That(!map.TryGetValue("never added", out var none));
            Assert.IsNotNull(none);
            Assert.AreEqual(0, none.Count);
        }
        [Test]
        public void IterationAndIndex()
        {
            Raw.MultiMap<string, int> map = new() { { "a", 1 }, { "b", 2 }, { "c", 3 } };
            Assert.AreEqual(3, map.Count);
            map.Add("a", 4);
            map.Add("a", 5);
            map.Add("c", 6);
            map.Add("d", 7);
            Assert.AreEqual(7, map.Count);
            CollectionAssert.AreEquivalent(
                new KeyValuePair<string, int>[] { new("a", 1), new("b", 2), new("c", 3), new("a", 4), new("a", 5), new("c", 6), new("d", 7) },
                map
            );
            CollectionAssert.AreEquivalent(
                new string[] { "a", "b", "c", "d" },
                map.Keys
            );
            IDictionary<string, List<int>> expecteds = new Dictionary<string, List<int>>
            {
                { "a", new() { 1, 4, 5 } },
                { "b", new() { 2 } },
                { "c", new() { 3, 6 } },
                { "d", new() { 7 } }
            };
            foreach (string key in map.Keys)
            {
                Assert.Less(0, expecteds[key]?.Count ?? 0);
                CollectionAssert.AreEquivalent(expecteds[key], map[key]);
            }
            expecteds.Remove("b");
            expecteds["a"].Remove(4);
            Assert.That(map.RemoveKey("b", out var bHad));
            CollectionAssert.AreEquivalent(new int[] { 2 }, bHad);
            Assert.That(!map.ContainsKey("b"));
            Assert.AreEqual(0, map["b"].Count);

            Assert.That(map.Remove("a", 4));
            Assert.That(!map.Remove("a", 17));
            Assert.That(map.ContainsKey("a"));
            CollectionAssert.AreEquivalent(new[] { 1, 5 }, map["a"]);
            map.Add("a", 8);
            CollectionAssert.AreEquivalent(new[] { 1, 5, 8 }, map["a"]);
            CollectionAssert.AreEquivalent(
                new KeyValuePair<string, int>[] { new("a", 1), new("c", 3), new("a", 5), new("c", 6), new("d", 7), new("a", 8), },
                map
            );
        }
        [Test]
        public void Swap()
        {
            Raw.MultiMap<string, int> map = new() {
                new("a", 1), new("a", 2), new("a", 3),
                new("b", 4), new("b", 5),
                new("c", 6)
            };
            Assert.AreEqual(3, map["a"].Count);
            Assert.AreEqual(2, map["b"].Count);
            Assert.AreEqual(1, map["c"].Count);
            Assert.AreEqual(0, map["d"].Count);
            CollectionAssert.AreEquivalent(new KeyValuePair<string, int>[] {
                new("a", 1), new("a", 2), new("a", 3),
                new("b", 4), new("b", 5),
                new("c", 6)
            }, map);
            map.Swap("a", "d");
            map.Swap("b", "c");
            Assert.AreEqual(3, map["d"].Count);
            Assert.AreEqual(2, map["c"].Count);
            Assert.AreEqual(1, map["b"].Count);
            Assert.AreEqual(0, map["a"].Count);
            CollectionAssert.AreEquivalent(new KeyValuePair<string, int>[] {
                new("d", 1), new("d", 2), new("d", 3),
                new("c", 4), new("c", 5),
                new("b", 6)
            }, map);

        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UTI.Raw;

namespace UTI.Tests
{
    public class RawLookupTests
    {
        [Test]
        public void EmptyBehaviors()
        {
            Raw.Lookup<string, int> map = new();
            Assert.AreEqual(0, map.Count);
            Assert.IsNotNull(map["never added"]);
            Assert.AreEqual(0, map["never added"].Count);
            Assert.That(!map.TryGetValue("never added", out var none));
            Assert.IsNull(none);
        }
        [Test]
        public void IterationAndIndex()
        {
            Raw.Lookup<string, int> map = new() {
                new("a") { 1 },
                new("b") { 2 },
                new("c") { 3 },
            };
            Assert.AreEqual(3, map.Count);
            map.GetOrPut("a").Add(4);
            map.GetOrPut("a").Add(5);
            map.GetOrPut("c").Add(6);
            map.GetOrPut("d").Add(7);
            // The counts are just the keys, not the values.
            Assert.AreEqual(4, map.Count);
            CollectionAssert.AreEquivalent(
                new string[] { "a", "b", "c", "d" },
                map.Keys
            );
            Dictionary<string, List<int>> expecteds = new()
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

            Assert.That(map.GetValueOrDefault("a")?.Remove(4) ?? false);
            Assert.False(map.GetValueOrDefault("a")?.Remove(17) ?? false);
            Assert.That(map.ContainsKey("a"));
            CollectionAssert.AreEquivalent(new[] { 1, 5 }, map["a"]);
            map.GetOrPut("a").Add(8);
            CollectionAssert.AreEquivalent(new[] { 1, 5, 8 }, map["a"]);
        }
        [Test]
        public void Swap()
        {
            Raw.Lookup<string, int> map = new()
            {
                new("a") { 1, 2, 3 },
                new("b") { 4, 5 },
                new("c") { 6 },
            };
            Assert.AreEqual(3, map["a"].Count);
            Assert.AreEqual(2, map["b"].Count);
            Assert.AreEqual(1, map["c"].Count);
            Assert.AreEqual(0, map["d"].Count);
            CollectionAssert.AreEquivalent(new int[][] {
                new[] {1, 2, 3},
                new[] {4, 5},
                new[] {6},
            }, map);
            map.Swap("a", "d");
            map.Swap("b", "c");
            Assert.AreEqual(3, map["d"].Count);
            Assert.AreEqual(2, map["c"].Count);
            Assert.AreEqual(1, map["b"].Count);
            Assert.AreEqual(0, map["a"].Count);
            CollectionAssert.AreEquivalent(new int[][] {
                new[] {6},
                new[] {4, 5},
                new[] {1, 2, 3},
            }, map);

        }
    }
}
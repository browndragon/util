using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTI.Tests
{
    public class RawMapTest
    {
        [Test]
        public void Iteration()
        {
            Raw.Map<string, int> order = new() {
                new("a", 1), new("b", 2), new("c", 3)
            };
            Assert.AreEqual(3, order.Count);
            CollectionAssert.AreEquivalent(
                new KeyValuePair<string, int>[] { new("a", 1), new("b", 2), new("c", 3) },
                order
            );
        }
        [Test]
        public void AddRemove()
        {
            Raw.Map<string, int> order = new() {
                { "a", 1 }, { "b", 2 }, { "c", 3 }
            };
            Assert.Throws<Exception>(() => order.Add("a", 1));
            Assert.Throws<Exception>(() => order.Add("b", 3));
            Assert.AreEqual(3, order.Count);
            Assert.That(order.Remove("b"));
            Assert.That(!order.Remove("b"));
            order["a"] = 4;
            order.Add("b", 5);
            CollectionAssert.AreEquivalent(
                new KeyValuePair<string, int>[] { new("a", 4), new("c", 3), new("b", 5) },
                order
            );
        }
        [Test]
        public void Clear()
        {
            Raw.Map<string, int> order = new() {
                { "a", 1 }, { "b", 2 }, { "c", 3}
            };
            order.Clear();
            Assert.AreEqual(0, order.Count);
        }
        [Test]
        public void DictEquivalentIndexAndTryGet()
        {
            Raw.Map<string, int> map = new() { { "a", 1 } };
            Dictionary<string, int> dict = new() { { "a", 1 } };
            Assert.AreEqual(dict["a"], map["a"]);
            Assert.AreEqual(
                dict.TryGetValue("a", out var dvar),
                map.TryGetValue("a", out var mvar)
            );
            Assert.AreEqual(dvar, mvar);

            Assert.Throws<KeyNotFoundException>(() => _ = dict["b"]);
            Assert.Throws<KeyNotFoundException>(() => _ = map["b"]);
            // Assert.AreEqual(dict["b"], map["b"]);
            Assert.AreEqual(
                dict.TryGetValue("b", out dvar),
                map.TryGetValue("b", out mvar)
            );
            Assert.AreEqual(dvar, mvar);

            dict["a"] = map["a"] = 5;
            Assert.AreEqual(dict["a"], map["a"]);
            Assert.AreEqual(
                dict.TryGetValue("a", out dvar),
                map.TryGetValue("a", out mvar)
            );
            Assert.AreEqual(dvar, mvar);
            Assert.AreEqual(5, dvar);
        }
    }
}
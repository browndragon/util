using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTI.Tests
{
    public class RawBiMapTest
    {
        [Test]
        public void ForwardUniqueValue()
        {
            Raw.BiMap<string, int> map = new() { { "a", 1 } };
            Assert.Throws<Exception>(() => map.Add(new("a", 1)));
            Assert.AreEqual(1, map["a"]);
            CollectionAssert.AreEquivalent(new[] { "a" }, map.Reverse[1]);
        }
        [Test]
        public void ForwardUniqueKey()
        {
            Raw.BiMap<string, int> map = new() { { "a", 1 } };
            Assert.Throws<Exception>(() => map.Add(new("a", 2)));
            Assert.AreEqual(1, map["a"]);
            CollectionAssert.AreEquivalent(new[] { "a" }, map.Reverse[1]);
        }
        [Test]
        public void BackwardNonUnique()
        {
            Raw.BiMap<string, int> map = new() { { "a", 1 }, { "b", 1 } };
            Assert.AreEqual(1, map["a"]);
            Assert.AreEqual(1, map["b"]);
            CollectionAssert.AreEquivalent(new[] { "a", "b" }, map.Reverse[1]);
        }
    }
}
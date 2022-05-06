using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTI.Tests
{
    public class RawKeySetTests
    {
        class HashSet : Raw.KeySet<int, string>
        {
            protected override int GetKey(string item) => item.GetHashCode();
        }
        [Test]
        public void IterationAndIndexOperations()
        {
            HashSet byHash = new() { "a", "b", "c" };
            CollectionAssert.AreEquivalent(
                new string[] { "a", "b", "c" }, byHash
            );
            CollectionAssert.AreEquivalent(
                new int[] { "a".GetHashCode(), "b".GetHashCode(), "c".GetHashCode() }, byHash.Keys
            );
            Assert.AreEqual("b", byHash["b".GetHashCode()]);
            Assert.Throws<KeyNotFoundException>(() => _ = byHash[12]);

            Assert.That(byHash.Remove("b"));
            Assert.AreEqual(2, byHash.Count);
            byHash.Add("b");
            Assert.AreEqual(3, byHash.Count);
            Assert.That(byHash.TryGetValue("b".GetHashCode(), out var str));
            Assert.AreEqual("b", str);
        }
    }
}
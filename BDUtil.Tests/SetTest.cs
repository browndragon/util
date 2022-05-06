using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTI.Tests
{
    public class RawSetTest
    {
        [Test]
        public void Iteration()
        {
            Raw.Set<string> order = new();
            CollectionAssert.IsEmpty(order);
            order.Add("a");
            CollectionAssert.AreEquivalent(new[] { "a" }, order);

            order.Add("b");
            CollectionAssert.AreEquivalent(new[] { "a", "b" }, order);

            order.Add("c");
            order.Remove("b");
            order.Add("d");
            CollectionAssert.AreEquivalent(new[] { "a", "c", "d" }, order);
            CollectionAssert.AreNotEquivalent(order, None.Default);
            order.Clear();
            CollectionAssert.AreEquivalent(order, None.Default);
        }
        [Test]
        public void AddRemoveContains()
        {
            Raw.Set<string> order = new() { "a", "b" };

            Assert.AreEqual(2, order.Count);
            Assert.Throws<Exception>(() => order.Add("a"));
            order.Add("c");
            Assert.AreEqual(3, order.Count);
            Assert.That(order.Contains("a"));
            Assert.That(!order.Contains("d"));
            Assert.That(order.Remove("a"));
            Assert.AreEqual(2, order.Count);
            Assert.That(!order.Remove("a"));
            Assert.AreEqual(2, order.Count);
            Assert.That(!order.Contains("a"));
            Assert.That(order.Contains("b"));
            order.Clear();
            Assert.AreEqual(0, order.Count);
        }
    }
}
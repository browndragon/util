using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTI.Tests
{
    public class RawTableTest
    {
        [Test]
        public void AddContains()
        {
            Raw.Table<string, int, byte> map = new() { { ("a", 1), 2 } };
            Assert.Throws<Exception>(() => map.Add("a", 1, (byte)3));
            map.Add("a", 2, (byte)3);
            map.Add("c", 3, (byte)4);
            map.Add("b", 1, (byte)5);
            CollectionAssert.AreEquivalent(
                new KeyValuePair<(string, int), byte>[] {
                    new(("a", 1), 2), new(("a", 2), 3),new(("c", 3), 4),new(("b", 1), 5),
                },
                map
            );
            CollectionAssert.AreEquivalent(
                new[] {
                    ("a", 1), ("a", 2), ("c", 3), ("b", 1),
                },
                map.Keys
            );
            CollectionAssert.AreEquivalent(
                new[] { "a", "c", "b" },
                map.Rows.Keys
            );
            CollectionAssert.AreEquivalent(
                new[] { 1, 2, 3 },
                map.Cols.Keys
            );
        }
    }
}
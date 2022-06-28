using System;
using System.Collections.Generic;
using Xunit;

namespace BDUtil.Raw
{
    public class MapTest
    {
        [Fact]
        public void Iteration()
        {
            Map<string, int> order = new() {
                new("a", 1), new("b", 2), new("c", 3)
            };
            Assert.Equal(3, order.Count);
            Assert.Equal(
                new KeyValuePair<string, int>[] { new("a", 1), new("b", 2), new("c", 3) },
                order
            );
        }
        [Fact]
        public void AddRemove()
        {
            Map<string, int> order = new() {
                { "a", 1 }, { "b", 2 }, { "c", 3 }
            };
            Assert.Throws<Exception>(() => order.Add("a", 1));
            Assert.Throws<Exception>(() => order.Add("b", 3));
            Assert.Equal(3, order.Count);
            Assert.True(order.RemoveKey("b"));
            Assert.True(!order.RemoveKey("b"));
            order["a"] = 4;
            order.Add("b", 5);
            Assert.Equal(
                new KeyValuePair<string, int>[] { new("a", 4), new("c", 3), new("b", 5) },
                order
            );
        }
        [Fact]
        public void Clear()
        {
            Map<string, int> order = new() {
                { "a", 1 }, { "b", 2 }, { "c", 3}
            };
            order.Clear();
            Assert.Equal(0, (int)order.Count);
            Assert.Empty(order);
        }
        [Fact]
        public void DictEquivalentIndexAndTryGet()
        {
            Map<string, int> map = new() { { "a", 1 } };
            Dictionary<string, int> dict = new() { { "a", 1 } };
            Assert.Equal(dict["a"], map["a"]);
            Assert.Equal(
                dict.TryGetValue("a", out var dvar),
                map.TryGetValue("a", out var mvar)
            );
            Assert.Equal(dvar, mvar);

            Assert.Throws<KeyNotFoundException>(() => _ = dict["b"]);
            Assert.Throws<KeyNotFoundException>(() => _ = map["b"]);
            // Assert.Equal(dict["b"], map["b"]);
            Assert.Equal(
                dict.TryGetValue("b", out dvar),
                map.TryGetValue("b", out mvar)
            );
            Assert.Equal(dvar, mvar);

            dict["a"] = map["a"] = 5;
            Assert.Equal(dict["a"], map["a"]);
            Assert.Equal(
                dict.TryGetValue("a", out dvar),
                map.TryGetValue("a", out mvar)
            );
            Assert.Equal(dvar, mvar);
            Assert.Equal(5, dvar);
        }
    }
}
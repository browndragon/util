using System.Collections.Generic;
using Xunit;

namespace BDUtil.Raw
{

    public class MultiMapTest
    {
        [Fact]
        public void EmptyBehaviors()
        {
            MultiMap<string, int> map = new();
            Assert.Equal(0, (int)map.Count);
            Assert.Empty(map);
            Assert.NotNull(map["never added"]);
            Assert.Equal(0, map["never added"].Count);
            Assert.True(!map.TryGetValue("never added", out var none));
            Assert.NotNull(none);
            Assert.Equal(0, none.Count);
        }
        [Fact]
        public void IterationAndIndex()
        {
            MultiMap<string, int> map = new() { { "a", 1 }, { "b", 2 }, { "c", 3 } };
            Assert.Equal(3, map.Count);
            map.Add("a", 4);
            map.Add("a", 5);
            map.Add("c", 6);
            map.Add("d", 7);
            Assert.Equal(7, map.Count);
            Assert.Equal(
                new KeyValuePair<string, int>[] { new("a", 1), new("b", 2), new("c", 3), new("a", 4), new("a", 5), new("c", 6), new("d", 7) },
                map
            );
            Assert.Equal(
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
                Assert.NotNull(expecteds[key]);
                Assert.True(0 < expecteds[key].Count);
                Assert.Equal(expecteds[key], map[key]);
            }
            expecteds.Remove("b");
            expecteds["a"].Remove(4);
            Assert.True(map.RemoveKey("b", out var bHad));
            Assert.Equal(new int[] { 2 }, bHad);
            Assert.True(!map.ContainsKey("b"));
            Assert.Equal(0, map["b"].Count);

            Assert.True(map.Remove("a", 4));
            Assert.False(map.Remove("a", 17));
            Assert.True(map.ContainsKey("a"));
            Assert.Equal(new[] { 1, 5 }, map["a"]);
            map.Add("a", 8);
            Assert.Equal(new[] { 1, 5, 8 }, map["a"]);
            Assert.Equal(
                new KeyValuePair<string, int>[] { new("a", 1), new("c", 3), new("a", 5), new("c", 6), new("d", 7), new("a", 8), },
                map
            );
        }
        [Fact]
        public void Swap()
        {
            MultiMap<string, int> map = new() {
                new("a", 1), new("a", 2), new("a", 3),
                new("b", 4), new("b", 5),
                new("c", 6)
            };
            Assert.Equal(3, map["a"].Count);
            Assert.Equal(2, map["b"].Count);
            Assert.Equal(1, map["c"].Count);
            Assert.Equal(0, map["d"].Count);
            Assert.Equal(new KeyValuePair<string, int>[] {
                new("a", 1), new("a", 2), new("a", 3),
                new("b", 4), new("b", 5),
                new("c", 6)
            }, map);
            map.Swap("a", "d");
            map.Swap("b", "c");
            Assert.Equal(3, map["d"].Count);
            Assert.Equal(2, map["c"].Count);
            Assert.Equal(1, map["b"].Count);
            Assert.Equal(0, map["a"].Count);
            Assert.Equal(new KeyValuePair<string, int>[] {
                new("d", 1), new("d", 2), new("d", 3),
                new("c", 4), new("c", 5),
                new("b", 6)
            }, map);
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace BDUtil.Raw
{

    public class MultiMapTest
    {
        [Fact]
        public void EmptyBehaviors()
        {
            Multi.Map<string, int> map = new();
            Assert.Equal(0, (int)map.Count);
            Assert.Empty(map);
            Assert.NotNull(map["never added"]);
            Assert.Equal(0, map["never added"].Count);
            Assert.True(!map.TryGetValue("never added", out var none));
            Assert.Null(none);
        }
        [Fact]
        public void IterationAndIndex()
        {
            Multi.Map<string, int> map = new() { { "a", 1 }, { "b", 2 }, { "c", 3 } };
            Assert.Equal(3, map.Count);
            map.Add("a", 4);
            map.Add("a", 5);
            map.Add("c", 6);
            map.Add("d", 7);
            Assert.Equal(7, map.Count);
            Assert.Equal(
                KVP.Of<string, int>(
                    new("a", 1), new("a", 4), new("a", 5), new("b", 2), new("c", 3), new("c", 6), new("d", 7)
                ),
                map.AsKVPs.ToArray()
            );
            Assert.Equal(
                Iter.Of("a", "b", "c", "d"),
                map.Keys
            );
            IDictionary<string, int[]> expecteds = new Dictionary<string, int[]>
            {
                { "a", Iter.Of(1, 4, 5) },
                { "b", Iter.Of(2) },
                { "c", Iter.Of(3, 6) },
                { "d", Iter.Of(7) }
            };
            foreach (string key in map.Keys)
            {
                Assert.NotEmpty(expecteds[key]);
                Assert.Equal(expecteds[key], map[key]);
            }
            expecteds.Remove("b");
            expecteds["a"] = Iter.Of(1, 5);
            Assert.Equal(Iter.Of(2), map.RemoveKey("b"));
            Assert.True(!map.ContainsKey("b"));
            Assert.Empty(map["b"]);

            Assert.True(map.Remove("a", 4));
            Assert.False(map.Remove("a", 17));
            Assert.True(map.ContainsKey("a"));
            Assert.Equal(Iter.Of(1, 5), map["a"]);
            map.Add("a", 8);
            Assert.Equal(Iter.Of(1, 8, 5), map["a"]);
            Assert.Equal(
                KVP.Of<string, int>(
                    new("a", 1), new("a", 8), new("a", 5), new("c", 3), new("c", 6), new("d", 7)
                ),
                map.AsKVPs.ToArray()
            );
        }
    }
}
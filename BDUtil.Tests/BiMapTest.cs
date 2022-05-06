using System;
using Xunit;

namespace BDUtil.Raw
{
    public class BiMapTest
    {
        [Fact]
        public void ForwardUniqueValue()
        {
            BiMap<string, int> map = new() { { "a", 1 } };
            Assert.Throws<Exception>(() => map.Add(new("a", 1)));
            Assert.Equal(1, map["a"]);
            Assert.Equal(new[] { "a" }, map.Reverse[1]);
        }
        [Fact]
        public void ForwardUniqueKey()
        {
            BiMap<string, int> map = new() { { "a", 1 } };
            Assert.Throws<Exception>(() => map.Add(new("a", 2)));
            Assert.Equal(1, map["a"]);
            Assert.Equal(new[] { "a" }, map.Reverse[1]);
        }
        [Fact]
        public void BackwardNonUnique()
        {
            BiMap<string, int> map = new() { { "a", 1 }, { "b", 1 } };
            Assert.Equal(1, map["a"]);
            Assert.Equal(1, map["b"]);
            Assert.Equal(new[] { "a", "b" }, map.Reverse[1]);
        }
    }
}
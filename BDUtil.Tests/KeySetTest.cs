using System.Collections.Generic;
using Xunit;

namespace BDUtil.Raw
{

    public class KeySetTest
    {
        class HashSet : KeySet<int, string>
        {
            protected override int GetKey(string item) => item.GetHashCode();
        }
        [Fact]
        public void IterationAndIndexOperations()
        {
            HashSet byHash = new() { "a", "b", "c" };
            Assert.Equal(
                new string[] { "a", "b", "c" }, byHash
            );
            Assert.Equal(
                new int[] { "a".GetHashCode(), "b".GetHashCode(), "c".GetHashCode() }, byHash.Keys
            );
            Assert.Equal("b", byHash["b".GetHashCode()]);
            Assert.Throws<KeyNotFoundException>(() => _ = byHash[12]);

            Assert.True(byHash.Remove("b"));
            Assert.Equal(2, byHash.Count);
            byHash.Add("b");
            Assert.Equal(3, byHash.Count);
            Assert.True(byHash.TryGetValue("b".GetHashCode(), out var str));
            Assert.Equal("b", str);
        }
    }
}
using System;
using Xunit;

namespace BDUtil.Raw
{
    public class SetTest
    {
        [Fact]
        public void Iteration()
        {
            Set<string> order = new();
            Assert.Empty(order);
            order.Add("a");
            Assert.Equal(new[] { "a" }, order);

            order.Add("b");
            Assert.Equal(new[] { "a", "b" }, order);

            order.Add("c");
            order.Remove("b");
            order.Add("d");
            Assert.Equal(new[] { "a", "c", "d" }, order);
            Assert.NotEqual(order, None<string>.Default);
            order.Clear();
            Assert.Equal(order, None<string>.Default);
        }
        [Fact]
        public void AddRemoveContains()
        {
            Set<string> order = new() { "a", "b" };

            Assert.Equal(2, order.Count);
            Assert.Throws<Exception>(() => order.Add("a"));
            order.Add("c");
            Assert.Equal(3, order.Count);
            Assert.Contains("a", order);
            Assert.DoesNotContain("d", order);
            Assert.True(order.Remove("a"));
            Assert.Equal(2, order.Count);
            Assert.True(!order.Remove("a"));
            Assert.Equal(2, order.Count);
            Assert.DoesNotContain("a", order);
            Assert.Contains("b", order);
            order.Clear();
            Assert.Empty(order);
        }
    }
}
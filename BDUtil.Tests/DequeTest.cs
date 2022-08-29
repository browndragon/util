using System;
using Xunit;

namespace BDUtil.Raw
{
    public class Deque
    {
        [Fact]
        public void Empty()
        {
            DequeThrows<string> order = new(3);
            Assert.Equal(3, order.Capacity);
            Assert.Empty(order);
            Assert.Equal(Array.Empty<string>(), order);
        }
        [Fact]
        public void Insertion()
        {
            DequeThrows<string> order = new(3);
            order.PushBack("hello");
            Assert.Equal(3, order.Capacity);
            Assert.Single(order);
            Assert.Equal("hello", order[0]);
            Assert.Equal(Iter.Of("hello"), order);
        }
        [Fact]
        public void Iteration()
        {
            DequeThrows<string> order = new(3) {
                "a", "b", "c"
            };
            Assert.Equal(3, order._Count);
            Assert.Equal(Iter.Of("a", "b", "c"), order);
        }
        [Fact]
        public void RemoveOneElement()
        {
            DequeThrows<string> order = new(1);
            order.PushBack("a");
            Assert.Equal(Iter.Of("a"), order);
            order.RemoveAt(0);
            Assert.Equal(Array.Empty<string>(), order);
            order.PushFront("a");
            Assert.Equal(Iter.Of("a"), order);
            order.RemoveAt(0);
            Assert.Equal(Array.Empty<string>(), order);
        }
        [Fact]
        public void Remove()
        {
            DequeThrows<string> order = new(3);
            order.PushBack("a");
            order.PushBack("b");
            Assert.Equal(2, order._Count);
            Assert.Equal(Iter.Of("a", "b"), order);
            order.RemoveAt(1);
            Assert.Single(order);
            Assert.Equal("a", order[0]);
            order.PushBack("b2");
            Assert.Equal(Iter.Of("a", "b2"), order);
            order.PushFront("c");
            Assert.Equal(3, order._Count);
            Assert.Equal(Iter.Of("c", "a", "b2"), order);
        }
        [Fact]
        public void OverflowOneElement()
        {
            DequePops<string> order = new(1);
            order.PushBack("a");
            Assert.Equal(Iter.Of("a"), order);
            order.PushBack("b");
            Assert.Equal(Iter.Of("b"), order);
            order.PushBack("c");
            Assert.Equal(Iter.Of("c"), order);
        }
        [Fact]
        public void IncrementalOverflow()
        {
            DequePops<string> order = new(3) { "a", "b", "c" };
            Assert.Equal(3, order._Count);
            Assert.Equal(Iter.Of("a", "b", "c"), order);
            order.RemoveAt(order._Count - 1);
            Assert.Equal(Iter.Of("a", "b"), order);
            order.Add("c'");
            Assert.Equal(Iter.Of("a", "b", "c'"), order);
            order.RemoveAt(0);
            Assert.Equal(Iter.Of("b", "c'"), order);
            order.Add("d");
            Assert.Equal(Iter.Of("b", "c'", "d"), order);
            order.Add("e");
            Assert.Equal(Iter.Of("c'", "d", "e"), order);
            // Assert.Equal(Iter.Of("b", "c", "d"), order);
        }
        [Fact]
        public void Overflow()
        {
            DequePops<string> order = new(3) {
                "a", "b", "c", "d", "e",
            };
            Assert.Equal(3, order._Count);
            Assert.Equal(new[] { "c", "d", "e" }, order);
            order.RemoveAt(order._Count - 1);
            Assert.Equal(2, order._Count);
            Assert.Equal(new[] { "c", "d" }, order);
        }
    }
}
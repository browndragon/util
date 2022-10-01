using System;
using BDUtil.Fluent;
using Xunit;

namespace BDUtil.Raw
{
    public class DequeTest
    {
        [Fact]
        public void Empty()
        {
            Deque<string> order = new(3) { Limit = 3 };
            Assert.Equal(3, order.Capacity);
            Assert.Empty(order);
            Assert.Equal(Array.Empty<string>(), order);
        }
        [Fact]
        public void Insertion()
        {
            Deque<string> order = new(3) { Limit = 3 };
            order.PushBack("hello");
            Assert.Equal(3, order.Capacity);
            Assert.Single(order);
            Assert.Equal("hello", order[0]);
            Assert.Equal(Iter.Of("hello"), order);
        }
        [Fact]
        public void Iteration()
        {
            Deque<string> order = new(3) { "a", "b", "c" };
            order.Limit = 3;
            Assert.Equal(3, order.Count);
            Assert.Equal(Iter.Of("a", "b", "c"), order);
        }
        [Fact]
        public void RemoveOneElement()
        {
            Deque<string> order = new(1) { Limit = 1 };
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
            Deque<string> order = new(3) { Limit = 3 };
            order.PushBack("a");
            order.PushBack("b");
            Assert.Equal(2, order.Count);
            Assert.Equal(Iter.Of("a", "b"), order);
            order.RemoveAt(1);
            Assert.Single(order);
            Assert.Equal("a", order[0]);
            order.PushBack("b2");
            Assert.Equal(Iter.Of("a", "b2"), order);
            order.PushFront("c");
            Assert.Equal(3, order.Count);
            Assert.Equal(Iter.Of("c", "a", "b2"), order);
        }
        [Fact]
        public void IncrementalOverflow()
        {
            Deque<string> order = new(3) { "a", "b", "c" };
            order.Limit = 3;
            Assert.Equal(3, order.Count);
            Assert.Equal(Iter.Of("a", "b", "c"), order);
            order.RemoveAt(order.Count - 1);
            Assert.Equal(Iter.Of("a", "b"), order);
            order.Add("c'");
            Assert.Equal(Iter.Of("a", "b", "c'"), order);
            order.RemoveAt(0);
            Assert.Equal(Iter.Of("b", "c'"), order);
            order.Add("d");
            Assert.Equal(Iter.Of("b", "c'", "d"), order);
            Assert.ThrowsAny<Exception>(() => order.Add("e"));
            Assert.Equal(Iter.Of("b", "c'", "d"), order);
            Assert.Equal("b", order.PopFront());
            order.Add("e");
            Assert.Equal(Iter.Of("c'", "d", "e"), order);
            // Assert.Equal(Iter.Of("b", "c", "d"), order);
        }
    }
}
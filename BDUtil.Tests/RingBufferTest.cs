using System;
using System.Collections.Generic;
using Xunit;

namespace BDUtil.Raw
{
    public class RingBufferTest
    {
        [Fact]
        public void Empty()
        {
            RingBuffer<string> order = new(3);
            Assert.Equal(3, order.Capacity);
            Assert.Empty(order);
            Assert.Equal(Array.Empty<string>(), order);
        }
        [Fact]
        public void Insertion()
        {
            RingBuffer<string> order = new(3);
            order.PushBack("hello");
            Assert.Equal(3, order.Capacity);
            Assert.Single(order);
            Assert.Equal("hello", order[0]);
            Assert.Equal(new[] { "hello" }, order);
        }
        [Fact]
        public void Iteration()
        {
            RingBuffer<string> order = new(3) {
                "a", "b", "c"
            };
            Assert.Equal(3, order.Count);
            Assert.Equal(new[] { "a", "b", "c" }, order);
        }
        [Fact]
        public void Remove()
        {
            RingBuffer<string> order = new(3);
            order.PushBack("a");
            order.PushBack("b");
            Assert.Equal(2, order.Count);
            Assert.Equal(new[] { "a", "b" }, order);
            order.RemoveIndex(1);
            Assert.Single(order);
            Assert.Equal("a", order[0]);
            order.PushBack("b2");
            order.PushFront("c");
            Assert.Equal(3, order.Count);
            Assert.Equal(new[] { "c", "a", "b2" }, order);
        }
        [Fact]
        public void Overflow()
        {
            RingBuffer<string> order = new(3) {
                "a", "b", "c", "d", "e",
            };
            Assert.Equal(3, order.Count);
            Assert.Equal(new[] { "c", "d", "e" }, order);
            order.RemoveIndex(-1);
            Assert.Equal(2, order.Count);
            Assert.Equal(new[] { "c", "d" }, order);
        }
    }
}
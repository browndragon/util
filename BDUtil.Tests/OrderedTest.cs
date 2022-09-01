using System;
using System.Collections.Generic;
using BDUtil.Raw;
using Xunit;

namespace BDUtil
{
    public class OrderedTest
    {
        [Fact]
        public void OrderedDequeExists()
        {
            OrderedDeque<string> test = new() { "banana", "apple", "cherry" };
            Assert.Equal(Iter.Of("apple", "banana", "cherry"), test);
        }
        [Fact]
        public void OrderedDequeAllowsDupes()
        {
            OrderedDeque<int> test = new() { 1, 2, 1, 2 };
            Assert.Equal(Iter.Of(1, 1, 2, 2), test);
        }

        [Fact]
        public void DirtyDequeDegrades()
        {
            OrderedDeque<int> test = new() { 5, 3, 1, 2 };
            test.IsDirty = true;
            test.Add(4);
            Assert.True(test.IsDirty);
            Assert.Equal(Iter.Of(1, 2, 3, 5, 4), test);
        }
        [Fact]
        public void DirtyDequeRestores()
        {
            OrderedDeque<int> test = new() { 5, 3, 1 };
            test.IsDirty = true;
            test.Add(2);
            test.Sort();
            test.Add(4);
            Assert.False(test.IsDirty);
            Assert.Equal(Iter.Of(1, 2, 3, 4, 5), test);
        }
        [Fact]
        public void InsertCleanThrows()
        {
            OrderedDeque<int> test = new() { 1, 3, 5 };
            Assert.Throws<InvalidOperationException>(() => test.Insert(1, 2));  // Look, it's even @ the correct slot!
            Assert.Equal(Iter.Of(1, 3, 5), test);
            test.IsDirty = true;
            test.Insert(1, 2);
            Assert.Equal(Iter.Of(1, 2, 3, 5), test);
        }
    }
}
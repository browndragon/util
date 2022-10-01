using System;
using System.Collections.Generic;
using BDUtil.Fluent;
using BDUtil.Raw;
using Xunit;

namespace BDUtil
{
    public class OrderedTest
    {
        [Fact]
        public void OrderedDequeExists()
        {
            Deque<string> test = new();
            foreach (string insert in Iter.Of("banana", "apple", "cherry")) test.BinaryInsert(insert);
            Assert.Equal(Iter.Of("apple", "banana", "cherry"), test);
        }
        [Fact]
        public void OrderedDequeAllowsDupes()
        {
            Deque<int> test = new();
            foreach (int insert in Iter.Of(1, 2, 1, 2)) test.BinaryInsert(insert);
            Assert.Equal(Iter.Of(1, 1, 2, 2), test);
        }

        [Fact]
        public void DirtyDequeDegrades()
        {
            Deque<int> test = new();
            foreach (int insert in Iter.Of(5, 3, 1, 2)) test.BinaryInsert(insert);
            test.Add(4);
            Assert.Equal(Iter.Of(1, 2, 3, 5, 4), test);
        }
        [Fact]
        public void DirtyDequeRestores()
        {
            Deque<int> test = new();
            foreach (int insert in Iter.Of(5, 3, 1)) test.BinaryInsert(insert);
            test.Add(2);
            test.BinarySort();
            test.BinaryInsert(4);
            Assert.Equal(Iter.Of(1, 2, 3, 4, 5), test);
        }
    }
}
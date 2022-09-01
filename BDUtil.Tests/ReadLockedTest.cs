using System;
using System.Collections.Generic;
using Xunit;

namespace BDUtil.Raw
{
    public class ReadLockedTest
    {
        [Fact]
        public void NormalListDies()
        {
            List<string> data = new() { "hello", "world", "!" };
            int i = -1;
            Assert.Throws<InvalidOperationException>(() =>
            {
                foreach (string s in data)
                {
                    i++;
                    if (s == "world") data.RemoveAt(i);
                }
            });
            // Died on "world", before fetching "!"
            Assert.Equal(1, i);
            // But the modification *does* happen, it dies during enumeration.
            Assert.Equal(Iter.Of("hello", "!"), data);
        }

        [Fact]
        public void ReadLockedMakesCopyOnce()
        {
            ReadLocked<string, List<string>, IReadOnlyList<string>, IEnumerable<string>> data = new();
            object oldValue = data.Write;
            object newValue;
            using (data.GetScoped(out IEnumerable<string> values))
            {
                Assert.True(data.IsReadLocked);
                Assert.Same(oldValue, data.Read);
                Assert.Same(oldValue, values);
                newValue = data.Write;
                Assert.NotSame(oldValue, newValue);

                Assert.Same(newValue, data.Read);
                Assert.Same(oldValue, values);  // it's scoped and doesn't change!
                Assert.Same(newValue, data.Write);  // Even if we do another write op!
            }
            Assert.Same(newValue, data.Read);
            Assert.Same(newValue, data.Write);
        }

        [Fact]
        public void ReadLockedListSurvives()
        {
            ReadLocked<string, List<string>, IReadOnlyList<string>, IEnumerable<string>> data = new();
            data.Write.AddRange(Iter.Of("hello", "world", "!"));

            int i = -1;
            foreach (string s in data.EnumerateScope())
            {
                i++;
                if (s == "world") data.Write.RemoveAt(i);
            }
            Assert.Equal(2, i);
            Assert.False(data.IsReadLocked);
            Assert.Equal(Iter.Of("hello", "!"), data.Read);
        }
    }
}
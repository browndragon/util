using System;
using Xunit;

namespace BDUtil.Raw
{
    public class DisposesTest
    {
        [Fact]
        public void DisposesOne()
        {
            var setter = Funcs.MakeSetter(out var getter);
            using (Dispose.One one = new(setter)) { Assert.False(getter()); }
            Assert.True(getter());
        }
        [Fact]
        public void DisposesAll()
        {
            bool a = false;
            Dispose.One bDisp = Funcs.MakeSetter(out var b);
            Func<bool> c;
            using (Dispose.All d = new() { () => a = true, bDisp })
            {
                d.Add(Funcs.MakeSetter(out c));
                Assert.False(a);
                Assert.False(b());
                Assert.False(c());
            }
            Assert.True(a);
            Assert.True(b());
            Assert.True(c());
        }
        [Fact]
        public void DisposesAllLoopsSafely()
        {
            Dispose.All all = new();
            int[] runs = new int[10];
            Assert.True(runs.Length >= 2);

            for (int i = 0; i < runs.Length; ++i)
            {
                all.Add(() => runs[i] = runs[i] + i);
                all.Dispose();
                // IF this weren't discarded, we'd expect it to be called again in the future, causing its element to overflow.
                // This does mean the 0th cell isn't doing much to catch correctness (0*a=0...) but Ah Well.
            }
            int runningSum = 0;
            for (int i = 0; i < runs.Length; ++i)
            {
                Assert.Equal(i, runs[i]);
                runningSum += runs[i];
            }
            Assert.Equal((runs.Length * (runs.Length - 1)) / 2, runningSum);
        }
    }
}
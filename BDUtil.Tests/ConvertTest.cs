using System;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace BDUtil
{
    public class ConvertTest
    {
        class TL : TraceListener
        {
            readonly ITestOutputHelper output;
            public TL(ITestOutputHelper helper) => output = helper;

            public override void Write(string message)
            => output.WriteLine(message);

            public override void WriteLine(string message)
            => output.WriteLine(message);
        }
        public ConvertTest(ITestOutputHelper testOutputHelper)
        => System.Diagnostics.Trace.Listeners.Add(new TL(testOutputHelper));
        public struct Parsey
        {
            public string value;
            public static explicit operator float(Parsey thiz) => thiz.value switch
            {
                null => default,
                "" => default,
                _ => float.Parse(thiz.value),
            };
            public static explicit operator Parsey(float thiz) => new() { value = "" + thiz };
            public static implicit operator Parsey(string thiz) => new() { value = "" + thiz };
            public static implicit operator string(Parsey thiz) => thiz.value;
            public static implicit operator Unparsey(Parsey thiz) => new() { value = thiz };
        }
        public struct Unparsey
        {
            public object value;
        }
        [Fact]
        public void ConvertIntLong()
        {
            Assert.NotNull(Converter<int, long>.Default);
            Assert.Equal(typeof(long), Converter<int, long>.Default.Convert(12).GetType());
            Assert.Equal(12L, Converter<int, long>.Default.Convert(12));
        }
        [Fact]
        public void ConvertLongInt()
        {
            Assert.NotNull(Converter<long, int>.Default);
            Assert.Equal(typeof(int), Converter<long, int>.Default.Convert(12L).GetType());
            Assert.Equal(12, Converter<long, int>.Default.Convert(12L));
        }
        [Fact]
        public void ConvertCustomExists()
        {
            Assert.NotNull(Converter<Parsey, Unparsey>.Default);
        }
        [Fact]
        public void ConvertCustomDoesntNecessarilyExist()
        {
            Assert.Null(Converter<Unparsey, float>.Default);
        }
        [Fact]
        public void ConvertCustomExplicitFloat()
        {
            Parsey parsey = new() { value = "12" };
            Assert.Equal(12f, Converter<Parsey, float>.Default.Convert(parsey));
        }
        [Fact]
        public void ConvertCustomFromImplicitString()
        {
            string value = "hello";
            Parsey parsey = Converter<string, Parsey>.Default.Convert(value);
            Assert.Equal("hello", parsey.value);
        }
        delegate void delegateA();
        delegate void delegateB();
        [Fact]
        public void ConvertWorksWithEquality()
        {
            string lastCalled = null;
            delegateA a1 = () => lastCalled = "a1";
            void a2() => lastCalled = "a2";
            delegateA a3 = a2;
            delegateA a4 = a2;
            delegateB b = a1.Invoke;
            Assert.Same(a1, a1);
            Assert.Equal(a1, a1);
            Assert.NotEqual(a1, a2);
            Assert.NotEqual(a1, Converter<delegateB, delegateA>.Default.Convert(b));
            Assert.Equal(a1, Converter<delegateB, delegateA>.Default.Convert(Converter<delegateA, delegateB>.Default.Convert(a1)));
            a1();
            Assert.Equal("a1", lastCalled);
            lastCalled = null;
            b();
            Assert.Equal("a1", lastCalled);
            Assert.NotSame(a3, a4);
            Assert.Equal(a3, a4);
            Assert.Equal(a4, Converter<delegateB, delegateA>.Default.Convert(Converter<delegateA, delegateB>.Default.Convert(a3)));
        }
    }
}
using System;
using Xunit;

namespace BDUtil
{
    public class ConvertTest
    {
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
    }
}
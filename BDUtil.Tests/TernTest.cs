using Xunit;

namespace BDUtil
{
    public class TernTest
    {
        static readonly bool? bTrue = true;
        static readonly bool? bNull = null;
        static readonly bool? bFalse = false;

        [Fact]
        public void BasicConversions()
        {
            Assert.Equal(tern.@true, (tern)bTrue);
            Assert.Equal(tern.@true, (tern)true);
            Assert.Equal(tern.@null, (tern)bNull);
            Assert.Equal(tern.@false, (tern)bFalse);
            Assert.Equal(tern.@false, (tern)false);
            Assert.Equal(tern.@true, (tern)(int)+1);
            Assert.Equal(tern.@null, (tern)0);
            Assert.Equal(tern.@false, (tern)(int)-1);
        }
        [Fact]
        public void TBDFirstOr()
        {
            Assert.Equal(true, bNull | bTrue);
            Assert.Null(bNull | bFalse);
            Assert.Equal(tern.@true, tern.@null | tern.@true);
            Assert.Equal(tern.@null, tern.@null | tern.@false);
            // Assert.Equal(???, bNull || bTrue);  // Doesn't compile. Proof!
            // Assert.Equal(???, bNull || bFalse);  // Doesn't compile. Proof!
            Assert.Equal(tern.@null, tern.@null || tern.@true);
            Assert.Equal(tern.@null, tern.@null || tern.@false);
        }
        [Fact]
        public void TBDFirstAnd()
        {
            Assert.Null(bNull & bTrue);
            Assert.Equal(false, bNull & bFalse);
            Assert.Equal(tern.@null, tern.@null & tern.@true);
            Assert.Equal(tern.@false, tern.@null & tern.@false);
            // Assert.Equal(???, bNull && bTrue);  // Doesn't compile. Proof!
            // Assert.Equal(???, bNull && bFalse);  // Doesn't compile. Proof!
            Assert.Equal(tern.@null, tern.@null && tern.@true);
            Assert.Equal(tern.@null, tern.@null && tern.@false);
        }
        [Fact]
        public void TBDSecondOr()
        {
            Assert.Equal(true, bTrue | bNull);
            Assert.Null(bFalse | bNull);
            Assert.Equal(tern.@true, tern.@true | tern.@null);
            Assert.Equal(tern.@null, tern.@false | tern.@null);

            // Assert.Equal(???, bNull || bTrue);  // Doesn't compile. Proof!
            // Assert.Equal(???, bNull || bFalse);  // Doesn't compile. Proof!
            Assert.Equal(tern.@true, tern.@true || tern.@null);
            Assert.Equal(tern.@null, tern.@false || tern.@null);
        }
        [Fact]
        public void TBDSecondAnd()
        {
            Assert.Null(bTrue & bNull);
            Assert.Equal(false, bFalse & bNull);
            Assert.Equal(tern.@null, tern.@true & tern.@null);
            Assert.Equal(tern.@false, tern.@false & tern.@null);

            // Assert.Equal(???, bNull && bTrue);  // Doesn't compile. Proof!
            // Assert.Equal(???, bNull && bFalse);  // Doesn't compile. Proof!
            Assert.Equal(tern.@null, tern.@true && tern.@null);
            Assert.Equal(tern.@false, tern.@false && tern.@null);
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, false, true)]
        [InlineData(false, true, false)]
        [InlineData(true, true, true)]
        public void TestBoolCasedAnds(bool expect, bool x, bool y)
        {
            Assert.Equal(expect, x & y);
            Assert.Equal(expect, x && y);
            tern tx = x, ty = y, texpect = expect;
            Assert.Equal(texpect, tx & ty);
            Assert.Equal(texpect, tx && ty);
        }
        [Theory]
        [InlineData(false, false, false)]
        [InlineData(true, false, true)]
        [InlineData(true, true, false)]
        [InlineData(true, true, true)]
        public void TestBoolCasedOrs(bool expect, bool x, bool y)
        {
            Assert.Equal(expect, x | y);
            Assert.Equal(expect, x || y);
            tern tx = x, ty = y, texpect = expect;
            Assert.Equal(texpect, tx | ty);
            Assert.Equal(texpect, tx || ty);
        }

        [Theory]
        [InlineData(true, false, true)]
        [InlineData(false, false, false)]
        [InlineData(false, true, true)]
        [InlineData(false, null, true)]
        [InlineData(false, null, null)]
        [InlineData(false, null, false)]
        public void TestStarkOperator(bool expect, bool? xValue, bool? yValue)
        {
            tern x = xValue, y = yValue;
            Assert.Equal(expect, x ^ y);
            Assert.Equal(expect, y ^ x);
        }

        [Theory]
        [InlineData(false, false, true)]
        [InlineData(true, false, false)]
        [InlineData(true, true, true)]
        [InlineData(true, null, true)]
        [InlineData(true, null, null)]
        [InlineData(true, null, false)]
        public void TestFuzzyOperator(bool expect, bool? xValue, bool? yValue)
        {
            tern x = xValue, y = yValue;
            Assert.Equal(expect, tern.Fuzzy(x, y));
            Assert.Equal(expect, tern.Fuzzy(y, x));
        }

        [Theory]
        [InlineData(false, false, false, false)]
        [InlineData(false, true, false, false)]
        [InlineData(true, true, false, true)]
        [InlineData(false, true, true, false)]
        public void TestLetOperatorAandBletC(bool? expect, bool? aValue, bool? bValue, bool? cValue)
        {
            bool wasCalled = false;
            Assert.Equal(
                (tern)expect,
                (tern)aValue && ((wasCalled = true) && (tern)bValue) % (tern)cValue
            );
            Assert.Equal(-(tern)aValue, wasCalled);
        }

        [Fact]
        public void TestShortCircuitSequenceAndSelector()
        {
            int calls = 0, first = 0, second = 0;

            tern incrToLimit(ref int v, int limit)
            => v >= limit || ++v % tern.@false || v >= limit | tern.@null;

            tern Tree(int l1 = 2, int l2 = 2)
            => calls++ | tern.@true
                && incrToLimit(ref first, l1)
                && incrToLimit(ref second, l2);

            Assert.Equal((0, 0, 0), (calls, first, second));
            Assert.Equal((0, tern.@null), (calls, Tree()));
            Assert.Equal((1, 1, 0), (calls, first, second));
            Assert.Equal((1, tern.@null), (calls, Tree()));
            Assert.Equal((2, 2, 1), (calls, first, second));
            Assert.Equal((2, tern.@true), (calls, Tree()));
            Assert.Equal((3, 2, 2), (calls, first, second));
        }
    }
}
using System;
using System.Drawing;
using Xunit;

namespace DotNext.Runtime
{
    public class IntrinsicsTests : Assert
    {
        [Fact]
        public static void IsNullable()
        {
            True(Intrinsics.IsNullable<string>());
            True(Intrinsics.IsNullable<ValueType>());
            True(Intrinsics.IsNullable<int?>());
            False(Intrinsics.IsNullable<int>());
            False(Intrinsics.IsNullable<IntPtr>());
        }

        [Fact]
        public static void RefTypeDefaultTest()
        {
            True(Intrinsics.IsDefault<string>(default));
            False(Intrinsics.IsDefault(""));
        }

        [Fact]
        public static void StructTypeDefaultTest()
        {
            var value = default(Guid);
            True(Intrinsics.IsDefault(value));
            value = Guid.NewGuid();
            False(Intrinsics.IsDefault(value));
        }

        [Fact]
        public static void SmallStructDefault()
        {
            True(Intrinsics.IsDefault(default(long)));
            False(Intrinsics.IsDefault(42L));
            True(Intrinsics.IsDefault(default(int)));
            False(Intrinsics.IsDefault(42));
            True(Intrinsics.IsDefault(default(byte)));
            False(Intrinsics.IsDefault((byte)42));
            True(Intrinsics.IsDefault(default(short)));
            False(Intrinsics.IsDefault((short)42));
        }

        [Fact]
        public static void Bitcast()
        {
            var point = new Point { X = 40, Y = 100 };
            Intrinsics.Bitcast(point, out decimal dec);
            Intrinsics.Bitcast(dec, out point);
            Equal(40, point.X);
            Equal(100, point.Y);
            Intrinsics.Bitcast<uint, int>(2U, out var i);
            Equal(2, i);
        }

        [Fact]
        public static void BitcastToLargerValueType()
        {
            var point = new Point { X = 40, Y = 100 };
            Intrinsics.Bitcast(point, out Guid g);
            False(g == Guid.Empty);
        }

        [Fact]
        public static void LightweightTypeOf()
        {
            var handle = Intrinsics.TypeOf<string>();
            Equal(typeof(string).TypeHandle, handle);
            NotEqual(typeof(StringComparer).TypeHandle, handle);
        }
    }
}
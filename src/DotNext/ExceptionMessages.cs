using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Resources;

namespace DotNext
{
    [SuppressMessage("Globalization", "CA1304", Justification = "This is culture-specific resource strings")]
    [SuppressMessage("Globalization", "CA1305", Justification = "This is culture-specific resource strings")]
    [ExcludeFromCodeCoverage]
    internal static class ExceptionMessages
    {
        private static readonly ResourceManager Resources = new ResourceManager("DotNext.ExceptionMessages", Assembly.GetExecutingAssembly());

        internal static string OptionalNoValue => Resources.GetString("OptionalNoValue");

        internal static string OptionalNullValue => Resources.GetString("OptionalNullValue");

        internal static string InvalidUserDataSlot => Resources.GetString("InvalidUserDataSlot");

        internal static string IndexShouldBeZero => Resources.GetString("IndexShouldBeZero");

        internal static string CastNullToValueType => Resources.GetString("CastNullToValueType");

        internal static string UnsupportedLockAcquisition => Resources.GetString("UnsupportedLockAcquisition");

        internal static string InvalidMethodSignature => Resources.GetString("CannotMakeMethodPointer");

        internal static string UnsupportedMethodPointerType => Resources.GetString("UnsupportedMethodPointerType");

        internal static string UnreachableCodeDetected => Resources.GetString("UnreachableCodeDetected");

        internal static string ConcreteDelegateExpected => Resources.GetString("ConcreteDelegateExpected");

        internal static string InvalidExpressionTree => Resources.GetString("InvalidExpressionTree");

        internal static string UnknownHashAlgorithm => Resources.GetString("UnknownHashAlgorithm");

        internal static string NotEnoughMemory => Resources.GetString("NotEnoughMemory");

        internal static string BoxedValueTypeExpected<T>()
            where T : struct
            => string.Format(Resources.GetString("BoxedValueTypeExpected"), typeof(T));

        internal static string UndefinedResult => Resources.GetString("UndefinedResult");
    }
}
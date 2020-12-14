﻿using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace DotNext.ComponentModel
{
    [SuppressMessage("Usage", "CA1812", Justification = "This class is instantiated implicitly via Register method")]
    internal sealed class PathStringConverter : TypeConverter
    {
        internal static void Register()
            => TypeDescriptor.AddAttributes(typeof(PathString), new TypeConverterAttribute(typeof(PathStringConverter)));

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            => sourceType == typeof(string);

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            switch (value)
            {
                case string path:
                    return new PathString(path);
                default:
                    throw new NotSupportedException();
            }
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            => destinationType == typeof(string);

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            switch (value)
            {
                case PathString path:
                    return path.Value!;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}

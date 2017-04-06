﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using AutoFixture.Extensions.Converters;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Dsl;

namespace AutoFixture.Extensions
{
    public static class AutoFixtureExtensions
    {
        private static readonly Func<Type, Type, string> GetConverterKey =
            (Type source, Type target) => $"{source.FullName}->{target.FullName}";

        private static readonly IDictionary<string, IValueConverter> Converters =
            new Dictionary<string, IValueConverter>
            {
                {GetConverterKey(typeof(int), typeof(string)), new ObjectToStringConverter()},
                {GetConverterKey(typeof(long), typeof(string)), new ObjectToStringConverter()},
                {GetConverterKey(typeof(float), typeof(string)), new ObjectToStringConverter()},
                {GetConverterKey(typeof(double), typeof(string)), new ObjectToStringConverter()},
                {GetConverterKey(typeof(string), typeof(byte[])), new StringToByteArrayConverter(Encoding.UTF8)}
            };

        public static IPostprocessComposer<T> With<T, TProperty, TValue>(
            this IPostprocessComposer<T> composer,
            Expression<Func<T, TProperty>> propertyPicker,
            TValue value)
            where TProperty : class
        {
            return composer.With(propertyPicker, Convert<TValue, TProperty>(value));
        }

        public static void Extend(this IFixture fixture, Type source, Type target, IValueConverter converter)
        {
            Converters[GetConverterKey(source, target)] = converter;
        }

        private static TTarget Convert<TSource, TTarget>(TSource value)
            where TTarget : class
        {
            if (value is TTarget)
            {
                return value as TTarget;
            }

            var key = GetConverterKey(typeof(TSource), typeof(TTarget));
            if (!Converters.ContainsKey(key))
            {
                throw new InvalidOperationException(
                    $"Conversion from {typeof(TSource).FullName} to {typeof(TTarget).FullName} is not defined.");
            }

            return (TTarget) Converters[key].Convert(value);
        }
    }
}
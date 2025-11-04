/*
 * ThrowHelper.Debug
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace ThrowHelper.Debug;

/// <summary>
/// Provides helper methods for throwing exceptions in debug builds.
/// </summary>
public static class DebugThrowHelper
{
    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if <paramref name="condition"/> is true.
    /// </summary>
    /// <param name="condition">A boolean value indicating whether to throw the exception.</param>
    /// <param name="paramName">The name of the parameter that caused the exception.</param>
    /// <exception cref="ArgumentNullException"><paramref name="condition"/> is true.</exception>
    [Conditional("DEBUG")]
    public static void ThrowArgumentNullException(
        bool condition, 
        string? paramName)
    {
        if (condition)
        {
            throw new ArgumentNullException(
                paramName: paramName);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if <paramref name="condition"/> is true.
    /// </summary>
    /// <param name="condition">A boolean value indicating whether to throw the exception.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    /// <exception cref="ArgumentNullException"><paramref name="condition"/> is true.</exception>
    [Conditional("DEBUG")]
    public static void ThrowArgumentNullException(
        bool condition, 
        string? message, 
        Exception? innerException)
    {
        if (condition)
        {
            throw new ArgumentNullException(
                message: message, 
                innerException: innerException);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if <paramref name="condition"/> is true.
    /// </summary>
    /// <param name="condition">A boolean value indicating whether to throw the exception.</param>
    /// <param name="paramName">The name of the parameter that caused the exception.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <exception cref="ArgumentNullException"><paramref name="condition"/> is true.</exception>
    [Conditional("DEBUG")]
    public static void ThrowArgumentNullException(
        bool condition, 
        string? paramName, 
        string? message)
    {
        if (condition)
        {
            throw new ArgumentNullException(
                paramName: paramName, 
                message: message);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if <paramref name="condition"/> is true.
    /// </summary>
    /// <param name="condition">A boolean value indicating whether to throw the exception.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <exception cref="ArgumentException"><paramref name="condition"/> is true.</exception>
    [Conditional("DEBUG")]
    public static void ThrowArgumentException(
        bool condition, 
        string? message)
    {
        if (condition)
        {
            throw new ArgumentException(
                message: message);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if <paramref name="condition"/> is true.
    /// </summary>
    /// <param name="condition">A boolean value indicating whether to throw the exception.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    /// <exception cref="ArgumentException"><paramref name="condition"/> is true.</exception>
    [Conditional("DEBUG")]
    public static void ThrowArgumentException(
        bool condition, 
        string? message, 
        Exception? innerException)
    {
        if (condition)
        {
            throw new ArgumentException(
                message: message, 
                innerException: innerException);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if <paramref name="condition"/> is true.
    /// </summary>
    /// <param name="condition">A boolean value indicating whether to throw the exception.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="paramName">The name of the parameter that caused the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    /// <exception cref="ArgumentException"><paramref name="condition"/> is true.</exception>
    [Conditional("DEBUG")]
    public static void ThrowArgumentException(
        bool condition, 
        string? message, 
        string? paramName, 
        Exception? innerException)
    {
        if (condition)
        {
            throw new ArgumentException(
                message: message, 
                paramName: paramName, 
                innerException: innerException);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if <paramref name="condition"/> is true.
    /// </summary>
    /// <param name="condition">A boolean value indicating whether to throw the exception.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="paramName">The name of the parameter that caused the exception.</param>
    /// <exception cref="ArgumentException"><paramref name="condition"/> is true.</exception>
    [Conditional("DEBUG")]
    public static void ThrowArgumentException(
        bool condition, 
        string? message, 
        string? paramName)
    {
        if (condition)
        {
            throw new ArgumentException(
                message: message, 
                paramName: paramName);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="condition"/> is true.
    /// </summary>
    /// <param name="condition">A boolean value indicating whether to throw the exception.</param>
    /// <param name="paramName">The name of the parameter that caused the exception.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="condition"/> is true.</exception>
    [Conditional("DEBUG")]
    public static void ThrowArgumentOutOfRangeException(
        bool condition, 
        string? paramName)
    {
        if (condition)
        {
            throw new ArgumentOutOfRangeException(
                paramName: paramName);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="condition"/> is true.
    /// </summary>
    /// <param name="condition">A boolean value indicating whether to throw the exception.</param>
    /// <param name="paramName">The name of the parameter that caused the exception.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="condition"/> is true.</exception>
    [Conditional("DEBUG")]
    public static void ThrowArgumentOutOfRangeException(
        bool condition, 
        string? paramName, 
        string? message)
    {
        if (condition)
        {
            throw new ArgumentOutOfRangeException(
                paramName: paramName, 
                message: message);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="condition"/> is true.
    /// </summary>
    /// <param name="condition">A boolean value indicating whether to throw the exception.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="condition"/> is true.</exception>
    [Conditional("DEBUG")]
    public static void ThrowArgumentOutOfRangeException(
        bool condition, 
        string? message, 
        Exception? innerException)
    {
        if (condition)
        {
            throw new ArgumentOutOfRangeException(
                message: message, 
                innerException: innerException);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="condition"/> is true.
    /// </summary>
    /// <param name="condition">A boolean value indicating whether to throw the exception.</param>
    /// <param name="paramName">The name of the parameter that caused the exception.</param>
    /// <param name="actualValue">The actual value of the parameter that caused the exception.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="condition"/> is true.</exception>
    [Conditional("DEBUG")]
    public static void ThrowArgumentOutOfRangeException(
        bool condition, 
        string? paramName, 
        object? actualValue, 
        string? message)
    {
        if (condition)
        {
            throw new ArgumentOutOfRangeException(
                paramName: paramName, 
                actualValue: actualValue, 
                message: message);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.
    /// </summary>
    /// <param name="argument">The reference type argument to validate as non-null.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is null.</exception>
    [Conditional("DEBUG")]
    public static void ThrowIfNull(
        [NotNull] object? argument, 
        [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(
            argument: argument,
            paramName: paramName);
    }

    /// <summary>
    /// Throws an exception if <paramref name="argument"/> is null or empty.
    /// </summary>
    /// <param name="argument">The string argument to validate as non-null and non-empty.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="argument"/> is empty.</exception>
    [Conditional("DEBUG")]
    public static void ThrowIfNullOrEmpty(
        [NotNull] string? argument, 
        [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(
            argument: argument,
            paramName: paramName);
    }

    /// <summary>
    /// Throws an exception if <paramref name="argument"/> is null, empty, or consists only of white-space characters.
    /// </summary>
    /// <param name="argument">The string argument to validate as non-null, non-empty, and not consisting only of white-space characters.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="argument"/> is empty or consists only of white-space characters.</exception>
    [Conditional("DEBUG")]
    public static void ThrowIfNullOrWhiteSpace(
        [NotNull] string? argument, 
        [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            argument: argument,
            paramName: paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is zero.
    /// </summary>
    /// <param name="value">The argument to validate as non-zero.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is zero.</exception>
    [Conditional("DEBUG")]
    public static void ThrowIfZero<
        T>(
        T value, 
        [CallerArgumentExpression(nameof(value))] string? paramName = null) 
        where T : INumberBase<T>
    {
        ArgumentOutOfRangeException.ThrowIfZero(
            value: value,
            paramName: paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is negative.
    /// </summary>
    /// <param name="value">The argument to validate as non-negative.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative.</exception>
    [Conditional("DEBUG")]
    public static void ThrowIfNegative<
        T>(
        T value, 
        [CallerArgumentExpression(nameof(value))] string? paramName = null) 
        where T : INumberBase<T>
    {
        ArgumentOutOfRangeException.ThrowIfNegative(
            value: value,
            paramName: paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is negative or zero.
    /// </summary>
    /// <param name="value">The argument to validate as non-zero or non-negative.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative or zero.</exception>
    [Conditional("DEBUG")]
    public static void ThrowIfNegativeOrZero<
        T>(
        T value, 
        [CallerArgumentExpression(nameof(value))] string? paramName = null) 
        where T : INumberBase<T>
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            value: value,
            paramName: paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is equal to <paramref name="other"/>.
    /// </summary>
    /// <param name="value">The argument to validate as not equal to <paramref name="other"/>.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is equal to <paramref name="other"/>.</exception>
    [Conditional("DEBUG")]
    public static void ThrowIfEqual<
        T>(
        T value, 
        T other, 
        [CallerArgumentExpression(nameof(value))] string? paramName = null) 
        where T : IEquatable<T>?
    {
        ArgumentOutOfRangeException.ThrowIfEqual(
            value: value,
            other: other,
            paramName: paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is not equal to <paramref name="other"/>.
    /// </summary>
    /// <param name="value">The argument to validate as equal to <paramref name="other"/>.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not equal to <paramref name="other"/>.</exception>
    [Conditional("DEBUG")]
    public static void ThrowIfNotEqual<
        T>(
        T value, 
        T other, 
        [CallerArgumentExpression(nameof(value))] string? paramName = null) 
        where T : IEquatable<T>?
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(
            value: value,
            other: other,
            paramName: paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is greater than <paramref name="other"/>.
    /// </summary>
    /// <param name="value">The argument to validate as less than or equal to <paramref name="other"/>.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is greater than <paramref name="other"/>.</exception>
    [Conditional("DEBUG")]
    public static void ThrowIfGreaterThan<
        T>(
        T value, 
        T other, 
        [CallerArgumentExpression(nameof(value))] string? paramName = null) 
        where T : IComparable<T>
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(
            value: value,
            other: other,
            paramName: paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is greater than or equal to <paramref name="other"/>.
    /// </summary>
    /// <param name="value">The argument to validate as less than <paramref name="other"/>.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is greater than or equal to <paramref name="other"/>.</exception>
    [Conditional("DEBUG")]
    public static void ThrowIfGreaterThanOrEqual<
        T>(
        T value, 
        T other, 
        [CallerArgumentExpression(nameof(value))] string? paramName = null) 
        where T : IComparable<T>
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(
            value: value,
            other: other,
            paramName: paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is less than <paramref name="other"/>.
    /// </summary>
    /// <param name="value">The argument to validate as greater than or equal to <paramref name="other"/>.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is less than <paramref name="other"/>.</exception>
    [Conditional("DEBUG")]
    public static void ThrowIfLessThan<
        T>(
        T value, 
        T other, 
        [CallerArgumentExpression(nameof(value))] string? paramName = null) 
        where T : IComparable<T>
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(
            value: value,
            other: other,
            paramName: paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is less than or equal to <paramref name="other"/>.
    /// </summary>
    /// <param name="value">The argument to validate as greater than <paramref name="other"/>.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is less than or equal to <paramref name="other"/>.</exception>
    [Conditional("DEBUG")]
    public static void ThrowIfLessThanOrEqual<
        T>(
        T value, 
        T other, 
        [CallerArgumentExpression(nameof(value))] string? paramName = null) 
        where T : IComparable<T>
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(
            value: value,
            other: other,
            paramName: paramName);
    }
}

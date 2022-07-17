﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

// This implementation is intended to reflect the public function set of double exactly.
// It should be possible to check floating point algorithms for precision, epsilon and robustness issues
// simply by replacing double with BigRational. Therfore some overhead, functions like Clamp, CopySign etc.

namespace System.Numerics
{
#if NET7_0 

  unsafe partial struct BigRational :
    INumber<BigRational>, ISignedNumber<BigRational>, ISpanParsable<BigRational>, //IConvertible, //todo: check IConvertible, does it makes much sens for non system types?
    IPowerFunctions<BigRational>, IRootFunctions<BigRational>, IExponentialFunctions<BigRational>,
    ILogarithmicFunctions<BigRational>, ITrigonometricFunctions<BigRational>, IHyperbolicFunctions<BigRational>
  {
    // INumberBase 
    /// <summary>Gets the radix, or base, for the type.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    public static int Radix => 1; //todo: check, Radix for rational?
    /// <summary>Gets the value <c>0</c> for the type.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    public static BigRational Zero => 0;
    /// <summary>Gets the value <c>1</c> for the type.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    public static BigRational One => 1u;
    /// <summary>Represents the number negative one (-1).</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    public static BigRational NegativeOne => -1;
    // IAdditiveIdentity
    /// <summary>Gets the additive identity of the current type.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    public static BigRational AdditiveIdentity => 0;
    // IMultiplicativeIdentity
    /// <summary>Gets the multiplicative identity of the current type.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    public static BigRational MultiplicativeIdentity => 1u;
    /// <summary>Determines if a value is zero.</summary>
    /// <param name="value">The value to be checked.</param>
    /// <returns><c>true</c> if <paramref name="value" /> is zero; otherwise, <c>false</c>.</returns>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    public static bool IsZero(BigRational value) => value.p == null;
    /// <summary>Determines whether the specified value is negative.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    public static bool IsNegative(BigRational value) => Sign(value) < 0;
    /// <summary>Determines if a value is positive.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    /// <param name="value">The value to be checked.</param>
    /// <returns><c>true</c> if <paramref name="value" /> is positive; otherwise, <c>false</c>.</returns>
    public static bool IsPositive(BigRational value) => Sign(value) > 0;
    /// <summary>Determines if a value represents an even integral value.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    /// <param name="value">The value to be checked.</param>
    /// <returns><c>true</c> if <paramref name="value" /> is an even integer; otherwise, <c>false</c>.</returns>
    public static bool IsEvenInteger(BigRational value)
    {
      return value.p == null || IsInteger(value) && (value.p[1] & 1) == 0;
    }
    /// <summary>Determines if a value represents an odd integral value.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    /// <param name="value">The value to be checked.</param>
    /// <returns><c>true</c> if <paramref name="value" /> is an odd integer; otherwise, <c>false</c>.</returns>
    public static bool IsOddInteger(BigRational value)
    {
      return value.p != null && IsInteger(value) && (value.p[1] & 1) == 1;
    }
    /// <summary>Determines if a value is in its canonical representation.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    /// <param name="value">The value to be checked.</param>
    /// <returns><c>true</c> if <paramref name="value" /> is in its canonical representation; otherwise, <c>false</c>.</returns>
    public static bool IsCanonical(BigRational value) => true;
    /// <summary>Determines if a value represents a complex value.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    /// <param name="value">The value to be checked.</param>
    /// <returns><c>true</c> if <paramref name="value" /> is a complex number; otherwise, <c>false</c>.</returns>
    /// <remarks>This function returns <c>false</c> for a complex number <c>a + bi</c> where <c>b</c> is zero.</remarks>
    public static bool IsComplexNumber(BigRational value) => true;
    /// <summary>Determines if a value is finite.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    /// <param name="value">The value to be checked.</param>
    /// <returns><c>true</c> if <paramref name="value" /> is finite; otherwise, <c>false</c>.</returns>
    public static bool IsFinite(BigRational value) => !IsNaN(value);
    /// <summary>Determines if a value represents an imaginary value.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    /// <param name="value">The value to be checked.</param>
    /// <returns><c>true</c> if <paramref name="value" /> is an imaginary number; otherwise, <c>false</c>.</returns>
    public static bool IsImaginaryNumber(BigRational value) => false;
    /// <summary>Determines if a value is infinite.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    /// <param name="value">The value to be checked.</param>
    /// <returns><c>true</c> if <paramref name="value" /> is infinite; otherwise, <c>false</c>.</returns>
    public static bool IsInfinity(BigRational value) => false;
    /// <summary>Determines if a value is negative infinity.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    /// <param name="value">The value to be checked.</param>
    /// <returns><c>true</c> if <paramref name="value" /> is negative infinity; otherwise, <c>false</c>.</returns>
    public static bool IsNegativeInfinity(BigRational value) => false;
    /// <summary>Determines if a value is positive infinity.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    /// <param name="value">The value to be checked.</param>
    /// <returns><c>true</c> if <paramref name="value" /> is positive infinity; otherwise, <c>false</c>.</returns>
    public static bool IsPositiveInfinity(BigRational value) => false;
    /// <summary>Determines if a value represents a real value.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    /// <param name="value">The value to be checked.</param>
    /// <returns><c>true</c> if <paramref name="value" /> is a real number; otherwise, <c>false</c>.</returns>
    public static bool IsRealNumber(BigRational value) => true;
    /// <summary>Determines if a value is normal.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    /// <param name="value">The value to be checked.</param>
    /// <returns><c>true</c> if <paramref name="value" /> is normal; otherwise, <c>false</c>.</returns>
    public static bool IsNormal(BigRational value) => true;
    /// <summary>Determines if a value is subnormal.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    /// <param name="value">The value to be checked.</param>
    /// <returns><c>true</c> if <paramref name="value" /> is subnormal; otherwise, <c>false</c>.</returns>
    public static bool IsSubnormal(BigRational value) => false;

    //INumber
    /// <summary>Clamps a value to an inclusive minimum and maximum value.</summary>
    /// <remarks>
    /// Part of the new NET 7 number type system see <see cref="INumber{TSelf}.Clamp(TSelf,TSelf,TSelf)"/>.<br/>
    /// </remarks>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The inclusive minimum to which <paramref name="value" /> should clamp.</param>
    /// <param name="max">The inclusive maximum to which <paramref name="value" /> should clamp.</param>
    /// <returns>The result of clamping <paramref name="value" /> to the inclusive range of <paramref name="min" /> and <paramref name="max" />.</returns>
    /// <exception cref="ArgumentException"><paramref name="min" /> is greater than <paramref name="max" />.</exception>
    public static BigRational Clamp(BigRational value, BigRational min, BigRational max)
    {
      if (min > max) throw new ArgumentException($"{nameof(min)} {nameof(max)}");
      if (value < min) return min;
      if (value > max) return max;
      return value;
    }
    /// <summary>Copies the sign of a value to the sign of another value..</summary>
    /// Part of the new NET 7 number type system see <see cref="INumber{TSelf}.CopySign(TSelf,TSelf)"/>.<br/>
    /// <param name="value">The value whose magnitude is used in the result.</param>
    /// <param name="sign">The value whose sign is used in the result.</param>
    /// <returns>A value with the magnitude of <paramref name="value" /> and the sign of <paramref name="sign" />.</returns>
    public static BigRational CopySign(BigRational value, BigRational sign)
    {
      int a, b; return (a = Sign(value)) != 0 && (b = Sign(sign) < 0 ? -1 : +1) != 0 && a != b ? -value : value;
    }

    static int cmpa(BigRational x, BigRational y)
    {
      var cpu = task_cpu; cpu.push(x); cpu.push(y);
      var i = cpu.cmpa(); cpu.pop(2); return i;
    }
    /// <summary>Compares two values to compute which is greater.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    /// <param name="x">The value to compare with <paramref name="y" />.</param>
    /// <param name="y">The value to compare with <paramref name="x" />.</param>
    /// <returns><paramref name="x" /> if it is greater than <paramref name="y" />; otherwise, <paramref name="y" />.</returns>
    public static BigRational MaxMagnitude(BigRational x, BigRational y)
    {
      return IsNaN(x) ? x : IsNaN(y) ? y : cmpa(x, y) <= 0 ? x : y;
    }
    /// <summary>Compares two values to compute which has the greater magnitude and returning the other value if an input is <c>NaN</c>.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    /// <param name="x">The value to compare with <paramref name="y" />.</param>
    /// <param name="y">The value to compare with <paramref name="x" />.</param>
    /// <returns><paramref name="x" /> if it is greater than <paramref name="y" />; otherwise, <paramref name="y" />.</returns>
    public static BigRational MaxMagnitudeNumber(BigRational x, BigRational y)
    {
      return IsNaN(x) ? y : IsNaN(x) ? x : cmpa(x, y) <= 0 ? x : y;
    }
    /// <summary>Compares two values to compute which is lesser.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    /// <param name="x">The value to compare with <paramref name="y" />.</param>
    /// <param name="y">The value to compare with <paramref name="x" />.</param>
    /// <returns><paramref name="x" /> if it is less than <paramref name="y" />; otherwise, <paramref name="y" />.</returns>
    public static BigRational MinMagnitude(BigRational x, BigRational y)
    {
      return IsNaN(x) ? x : IsNaN(y) ? y : cmpa(x, y) >= 0 ? x : y;
    }
    /// <summary>Compares two values to compute which has the lesser magnitude and returning the other value if an input is <c>NaN</c>.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    /// <param name="x">The value to compare with <paramref name="y" />.</param>
    /// <param name="y">The value to compare with <paramref name="x" />.</param>
    /// <returns><paramref name="x" /> if it is less than <paramref name="y" />; otherwise, <paramref name="y" />.</returns>
    public static BigRational MinMagnitudeNumber(BigRational x, BigRational y)
    {
      return IsNaN(x) ? y : IsNaN(x) ? x : cmpa(x, y) >= 0 ? x : y;
    }

    /// <summary>Parses a span of characters into a value.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    /// <param name="s">The span of characters to parse.</param>
    /// <param name="style">A bitwise combination of number styles that can be present in <paramref name="s" />.</param>
    /// <param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
    /// <returns>The result of parsing <paramref name="s" />.</returns>
    /// <exception cref="ArgumentException"><paramref name="style" /> is not a supported <see cref="NumberStyles" /> value.</exception>
    /// <exception cref="FormatException"><paramref name="s" /> is not in the correct format.</exception>
    /// <exception cref="OverflowException"><paramref name="s" /> is not representable by result.</exception>
    public static BigRational Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
    {
      var f = style & NumberStyles.HexNumber; if (f != 0) throw new ArgumentException($"{nameof(s)} {f}"); //todo: hex parse 
      var r = Parse(s, provider); if (IsNaN(r)) throw new ArgumentException(nameof(s)); return r;
    }
    /// <summary>Parses a string into a value.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    /// <param name="s">The string to parse.</param>
    /// <param name="style">A bitwise combination of number styles that can be present in <paramref name="s" />.</param>
    /// <param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
    /// <returns>The result of parsing <paramref name="s" />.</returns>
    /// <exception cref="ArgumentException"><paramref name="style" /> is not a supported <see cref="NumberStyles" /> value.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="s" /> is <c>null</c>.</exception>
    /// <exception cref="FormatException"><paramref name="s" /> is not in the correct format.</exception>
    /// <exception cref="OverflowException"><paramref name="s" /> is not representable by result.</exception>
    public static BigRational Parse(string s, NumberStyles style, IFormatProvider? provider)
    {
      return Parse(s.AsSpan(), style, provider);
    }
    /// <summary>Parses a string into a value.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
    /// <returns>The result of parsing <paramref name="s" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="s" /> is <c>null</c>.</exception>
    /// <exception cref="FormatException"><paramref name="s" /> is not in the correct format.</exception>
    /// <exception cref="OverflowException"><paramref name="s" /> is not representable by result.</exception>
    public static BigRational Parse(string s, IFormatProvider? provider)
    {
      var r = Parse(s.AsSpan(), provider);
      if (IsNaN(r)) throw new ArgumentException(nameof(s)); return r;
    }

    /// <summary>Tries to parses a span of characters into a value.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    /// <param name="s">The span of characters to parse.</param>
    /// <param name="style">A bitwise combination of number styles that can be present in <paramref name="s" />.</param>
    /// <param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
    /// <param name="result">On return, contains the result of succesfully parsing <paramref name="s" /> or an undefined value on failure.</param>
    /// <returns><c>true</c> if <paramref name="s" /> was successfully parsed; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentException"><paramref name="style" /> is not a supported <see cref="NumberStyles" /> value.</exception>
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out BigRational result)
    {
      var f = style & NumberStyles.HexNumber; if (f != 0) { result = default; return false; } //todo: hex parse 
      return !IsNaN(result = Parse(s, provider));
    }
    /// <summary>Tries to parses a string into a value.</summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="style">A bitwise combination of number styles that can be present in <paramref name="s" />.</param>
    /// <param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
    /// <param name="result">On return, contains the result of succesfully parsing <paramref name="s" /> or an undefined value on failure.</param>
    /// <returns><c>true</c> if <paramref name="s" /> was successfully parsed; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentException"><paramref name="style" /> is not a supported <see cref="NumberStyles" /> value.</exception>
    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, out BigRational result)
    {
      return !IsNaN(result = Parse(s.AsSpan(), provider));
    }

    //ISpanParsable
    /// <summary>Parses a span of characters into a value.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    /// <param name="s">The span of characters to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
    /// <param name="result">The result.</param>
    /// <returns>The result of parsing <paramref name="s" />.</returns>
    /// <exception cref="FormatException"><paramref name="s" /> is not in the correct format.</exception>
    /// <exception cref="OverflowException"><paramref name="s" /> is not representable by result.</exception>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out BigRational result)
    {
      return !IsNaN(result = Parse(s, provider));
    }
    /// <summary>Tries to parses a span of characters into a value.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    /// <param name="s">The span of characters to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
    /// <param name="result">On return, contains the result of succesfully parsing <paramref name="s" /> or an undefined value on failure.</param>
    /// <returns><c>true</c> if <paramref name="s" /> was successfully parsed; otherwise, <c>false</c>.</returns>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out BigRational result)
    {
      if (s == null) { result = default; return false; }
      return !IsNaN(result = Parse(s, provider));
    }

    //INumberBase //todo: test !!!
    static bool INumberBase<BigRational>.TryConvertFromChecked<TOther>(TOther value, out BigRational result) //where TOther : INumberBase<TOther>
    {
      return TryConvertFrom<TOther>(value, out result);
    }
    static bool INumberBase<BigRational>.TryConvertFromSaturating<TOther>(TOther value, out BigRational result) //where TOther : INumberBase<TOther>
    {
      return TryConvertFrom<TOther>(value, out result);
    }
    static bool INumberBase<BigRational>.TryConvertFromTruncating<TOther>(TOther value, out BigRational result) //where TOther : INumberBase<TOther>
    {
      return TryConvertFrom<TOther>(value, out result);
    }
    static bool TryConvertFrom<TOther>(TOther value, out BigRational result) where TOther : INumberBase<TOther>
    {
      if (typeof(TOther) == typeof(Half)) { result = (Half)(object)value; return true; }
      if (typeof(TOther) == typeof(short)) { result = (short)(object)value; return true; }
      if (typeof(TOther) == typeof(int)) { result = (int)(object)value; return true; }
      if (typeof(TOther) == typeof(long)) { result = (long)(object)value; return true; }
      if (typeof(TOther) == typeof(Int128)) { result = (Int128)(object)value; return true; }
      if (typeof(TOther) == typeof(nint)) { result = (nint)(object)value; return true; }
      if (typeof(TOther) == typeof(sbyte)) { result = (sbyte)(object)value; return true; }
      if (typeof(TOther) == typeof(float)) { result = (float)(object)value; return true; }
      result = default; return false;
    }
    static bool INumberBase<BigRational>.TryConvertToChecked<TOther>(BigRational value, [NotNullWhen(true)] out TOther? result) where TOther : default
    {
      if (typeof(TOther) == typeof(byte)) { result = (TOther)(object)checked((byte)value); return true; }
      if (typeof(TOther) == typeof(char)) { result = (TOther)(object)checked((char)value); return true; }
      if (typeof(TOther) == typeof(decimal)) { result = (TOther)(object)checked((decimal)value); return true; }
      if (typeof(TOther) == typeof(ushort)) { result = (TOther)(object)checked((ushort)value); return true; }
      if (typeof(TOther) == typeof(uint)) { result = (TOther)(object)checked((uint)value); return true; }
      if (typeof(TOther) == typeof(ulong)) { result = (TOther)(object)checked((ulong)value); return true; }
      if (typeof(TOther) == typeof(UInt128)) { result = (TOther)(object)checked((UInt128)value); return true; }
      if (typeof(TOther) == typeof(nuint)) { result = (TOther)(object)checked((nuint)value); return true; }
      result = default!; return false;
    }
    static bool INumberBase<BigRational>.TryConvertToSaturating<TOther>(BigRational value, [NotNullWhen(true)] out TOther? result) where TOther : default
    {
      return TryConvertTo<TOther>(value, out result);
    }
    static bool INumberBase<BigRational>.TryConvertToTruncating<TOther>(BigRational value, [NotNullWhen(true)] out TOther? result) where TOther : default
    {
      return TryConvertTo<TOther>(value, out result);
    }
    static bool TryConvertTo<TOther>(BigRational value, [NotNullWhen(true)] out TOther result) where TOther : INumberBase<TOther>
    {
      if (typeof(TOther) == typeof(byte))
      {
        byte x = (value >= byte.MaxValue) ? byte.MaxValue : (value <= byte.MinValue) ? byte.MinValue : (byte)value;
        result = (TOther)(object)x; return true;
      }
      if (typeof(TOther) == typeof(char))
      {
        char x = (value >= char.MaxValue) ? char.MaxValue : (value <= char.MinValue) ? char.MinValue : (char)value;
        result = (TOther)(object)x; return true;
      }
      if (typeof(TOther) == typeof(decimal))
      {
        decimal x = (value >= +79228162514264337593543950336.0) ? decimal.MaxValue :
                               (value <= -79228162514264337593543950336.0) ? decimal.MinValue :
                               IsNaN(value) ? 0.0m : (decimal)value;
        result = (TOther)(object)x; return true;
      }
      if (typeof(TOther) == typeof(ushort))
      {
        ushort x = (value >= ushort.MaxValue) ? ushort.MaxValue : (value <= ushort.MinValue) ? ushort.MinValue : (ushort)value;
        result = (TOther)(object)x; return true;
      }
      if (typeof(TOther) == typeof(uint))
      {
        uint x = (value >= uint.MaxValue) ? uint.MaxValue : (value <= uint.MinValue) ? uint.MinValue : (uint)value;
        result = (TOther)(object)x; return true;
      }
      if (typeof(TOther) == typeof(ulong))
      {
        ulong x = (value >= ulong.MaxValue) ? ulong.MaxValue : (value <= ulong.MinValue) ? ulong.MinValue : IsNaN(value) ? 0 : (ulong)value;
        result = (TOther)(object)x; return true;
      }
      if (typeof(TOther) == typeof(UInt128))
      {
        UInt128 x = (value >= 340282366920938463463374607431768211455.0) ? UInt128.MaxValue : (value <= 0.0) ? UInt128.MinValue : (UInt128)value;
        result = (TOther)(object)x; return true;
      }
      if (typeof(TOther) == typeof(nuint))
      {
#if TARGET_64BIT
        nuint actualResult = (value >= ulong.MaxValue) ? unchecked((nuint)ulong.MaxValue) :
                             (value <= ulong.MinValue) ? unchecked((nuint)ulong.MinValue) : (nuint)value;
        result = (TOther)(object)actualResult;
        return true;
#else
        nuint actualResult = (value >= uint.MaxValue) ? uint.MaxValue :
                             (value <= uint.MinValue) ? uint.MinValue : (nuint)value;
        result = (TOther)(object)actualResult;
        return true;
#endif
      }
      result = default!; return false;
    }
    static BigRational INumberBase<BigRational>.CreateChecked<TOther>(TOther value) //where TOther : INumberBase<TOther>
    {
      if (typeof(TOther) == typeof(BigRational)) return (BigRational)(object)value;
      if (!TryConvertFrom<TOther>(value, out BigRational r) && !TOther.TryConvertToChecked(value, out r))
        throw new NotSupportedException(typeof(TOther).Name);
      return r;
    }
    static BigRational INumberBase<BigRational>.CreateSaturating<TOther>(TOther value) // where TOther : INumberBase<TOther>
    {
      TryConvertFrom<TOther>(value, out var r); return r;
    }
    static BigRational INumberBase<BigRational>.CreateTruncating<TOther>(TOther value) //where TOther : INumberBase<TOther>
    {
      TryConvertFrom<TOther>(value, out var r); return r;
    }

    //for NET 7 all possible conversions explicitly, which was previously mapped automatically.
    /// <summary>
    /// Defines an implicit conversion of a <see cref="byte"/> object to a <see cref="BigRational"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="BigRational"/>.</param>
    /// <returns>A <see cref="BigRational"/> number that is equivalent to the number specified in the value parameter.</returns>
    public static implicit operator BigRational(byte value)
    {
      return (uint)value;
    }
    /// <summary>
    /// Defines an implicit conversion of a <see cref="sbyte"/> object to a <see cref="BigRational"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="BigRational"/>.</param>
    /// <returns>A <see cref="BigRational"/> number that is equivalent to the number specified in the value parameter.</returns>
    public static implicit operator BigRational(sbyte value)
    {
      return (int)value;
    }
    /// <summary>
    /// Defines an implicit conversion of a <see cref="ushort"/> object to a <see cref="BigRational"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="BigRational"/>.</param>
    /// <returns>A <see cref="BigRational"/> number that is equivalent to the number specified in the value parameter.</returns>
    public static implicit operator BigRational(ushort value)
    {
      return (uint)value;
    }
    /// <summary>
    /// Defines an implicit conversion of a <see cref="char"/> object to a <see cref="BigRational"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="BigRational"/>.</param>
    /// <returns>A <see cref="BigRational"/> number that is equivalent to the number specified in the value parameter.</returns>
    public static implicit operator BigRational(char value)
    {
      return (uint)value;
    }
    /// <summary>
    /// Defines an implicit conversion of a <see cref="short"/> object to a <see cref="BigRational"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="BigRational"/>.</param>
    /// <returns>A <see cref="BigRational"/> number that is equivalent to the number specified in the value parameter.</returns>
    public static implicit operator BigRational(short value)
    {
      return (int)value;
    }
    /// <summary>
    /// Defines an implicit conversion of a <see cref="nint"/> object to a <see cref="BigRational"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="BigRational"/>.</param>
    /// <returns>A <see cref="BigRational"/> number that is equivalent to the number specified in the value parameter.</returns>
    public static implicit operator BigRational(nint value)
    {
      return (long)value;
    }
    /// <summary>
    /// Defines an implicit conversion of a <see cref="nuint"/> object to a <see cref="BigRational"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="BigRational"/>.</param>
    /// <returns>A <see cref="BigRational"/> number that is equivalent to the number specified in the value parameter.</returns>
    public static implicit operator BigRational(nuint value)
    {
      return (ulong)value;
    }
    /// <summary>
    /// Defines an implicit conversion of a <see cref="Half"/> object to a <see cref="BigRational"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="BigRational"/>.</param>
    /// <returns>A <see cref="BigRational"/> number that is equivalent to the number specified in the value parameter.</returns>
    public static implicit operator BigRational(Half value)
    {
      return (float)value;
    }
    /// <summary>
    /// Defines an implicit conversion of a <see cref="Int128"/> object to a <see cref="BigRational"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="BigRational"/>.</param>
    /// <returns>A <see cref="BigRational"/> number that is equivalent to the number specified in the value parameter.</returns>
    public static implicit operator BigRational(Int128 value)
    {
      var p = (ulong*)&value; var s = (p[1] >> 63) != 0; if (s) value = -value;
      var cpu = task_cpu; cpu.push(p[0]); if (p[1] != 0) { cpu.push(p[1]); cpu.shl(64); cpu.or(); }
      if (s) cpu.neg(); return cpu.popr();
    }
    /// <summary>
    /// Defines an implicit conversion of a <see cref="UInt128"/> object to a <see cref="BigRational"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="BigRational"/>.</param>
    /// <returns>A <see cref="BigRational"/> number that is equivalent to the number specified in the value parameter.</returns>
    public static implicit operator BigRational(UInt128 value)
    {
      var cpu = task_cpu; var p = (ulong*)&value;
      cpu.push(p[0]); if (p[1] != 0) { cpu.push(p[1]); cpu.shl(64); cpu.or(); }
      return cpu.popr();
    }
    /// <summary>
    /// Defines an implicit conversion of a <see cref="NFloat"/> object to a <see cref="BigRational"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="BigRational"/>.</param>
    /// <returns>A <see cref="BigRational"/> number that is equivalent to the number specified in the value parameter.</returns>
    public static implicit operator BigRational(NFloat value)
    {
      // return nint.Size == 4 : (float)Math.Round(value.Value, 6) : value.Value; //todo: digits check for cases 5, 6, 7, regions ?
      return value.Value;
    }

    /// <summary>
    /// Defines an explicit conversion of a <see cref="BigRational"/> number to a <see cref="byte"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="byte"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="byte"/>.</returns>
    public static explicit operator byte(BigRational value)
    {
      return (byte)(uint)value;
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="BigRational"/> number to a <see cref="sbyte"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="sbyte"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="sbyte"/>.</returns>
    public static explicit operator sbyte(BigRational value)
    {
      return (sbyte)(int)value;
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="BigRational"/> number to a <see cref="short"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="short"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="short"/>.</returns>
    public static explicit operator short(BigRational value)
    {
      return (short)(int)value;
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="BigRational"/> number to a <see cref="char"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="char"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="char"/>.</returns>
    public static explicit operator char(BigRational value)
    {
      return (char)(uint)value;
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="BigRational"/> number to a <see cref="Half"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="Half"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="Half"/>.</returns>
    public static explicit operator Half(BigRational value)
    {
      return (Half)(double)value;
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="BigRational"/> number to a <see cref="nint"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="nint"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="nint"/>.</returns>
    public static explicit operator nint(BigRational value)
    {
      return (nint)(long)value;
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="BigRational"/> number to a <see cref="nuint"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="nuint"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="nuint"/>.</returns>
    public static explicit operator nuint(BigRational value)
    {
      return (nuint)(ulong)value;
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="BigRational"/> number to a <see cref="Int128"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="Int128"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="Int128"/>.</returns>
    public static explicit operator Int128(BigRational value)
    {
      var a = default(Int128); if (value.p == null || IsNaN(value)) return a; //NaN like double
      var cpu = task_cpu; cpu.push(value); var s = cpu.sign();
      if (!cpu.isi()) { cpu.mod(); cpu.swp(); cpu.pop(); } // trunc
      var b = cpu.msb(); if (b > 127) { cpu.pop(); return s < 0 ? Int128.MinValue : Int128.MaxValue; } // like double
      cpu.get(cpu.mark() - 1, out ReadOnlySpan<uint> sp);
      sp.Slice(1, sp.Length - 3).CopyTo(new Span<uint>(&a, 4));
      cpu.pop(); if (s < 0) a = -a; return a;
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="BigRational"/> number to a <see cref="UInt128"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="UInt128"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="UInt128"/>.</returns>
    public static explicit operator UInt128(BigRational value)
    {
      var a = default(UInt128); if (Sign(value) <= 0) return a; //NaN incl. like double
      var cpu = task_cpu; cpu.push(value);
      if (!cpu.isi()) { cpu.mod(); cpu.swp(); cpu.pop(); } // trunc
      var b = cpu.msb(); if (b > 128) { cpu.pop(); return UInt128.MaxValue; } // like double
      cpu.get(cpu.mark() - 1, out ReadOnlySpan<uint> sp);
      sp.Slice(1, sp.Length - 3).CopyTo(new Span<uint>(&a, 4));
      cpu.pop(); return a;
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="BigRational"/> number to a <see cref="NFloat"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="NFloat"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="NFloat"/>.</returns>
    public static explicit operator NFloat(BigRational value)
    {
      return nint.Size == 4 ? new NFloat((float)value) : new NFloat((double)value);
    }

    // }
    // #endif // NET 7
    //
    // // general public 
    // unsafe partial struct BigRational
    // {

    //IPowerFunctions
    /// <summary>Computes a value raised to a given power.</summary>
    /// <remarks>
    /// Part of the new NET 7 number type system see <see cref="IPowerFunctions{TSelf}.Pow(TSelf,TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The value which is raised to the power of <paramref name="x" />.</param>
    /// <param name="y">The power to which <paramref name="x" /> is raised.</param>
    /// <returns><paramref name="x" /> raised to the power of <paramref name="y" />.</returns>
    public static BigRational Pow(BigRational x, BigRational y)
    {
      return Pow(x, y, DefaultDigits); //todo: opt. cpu
    }

    //IRootFunctions
    /// <summary>Computes the square-root of a value.</summary>
    /// <remarks>
    /// Part of the new NET 7 number type system see <see cref="IRootFunctions{TSelf}.Sqrt(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The value whose square-root is to be computed.</param>
    /// <returns>The square-root of <paramref name="x" />.</returns>
    public static BigRational Sqrt(BigRational x)
    {
      return Sqrt(x, DefaultDigits); //todo: opt. cpu
    }
    /// <summary>Computes the cube-root of a value.</summary>
    /// <remarks>
    /// Part of the new NET 7 number type system see <see cref="IRootFunctions{TSelf}.Cbrt(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The value whose cube-root is to be computed.</param>
    /// <returns>The cube-root of <paramref name="x" />.</returns>
    public static BigRational Cbrt(BigRational x)
    {
      return Pow(x, (BigRational)1 / 3); //todo: opt. cpu
    }
    /// <summary>Computes the hypotenuse given two values representing the lengths of the shorter sides in a right-angled triangle.</summary>
    /// <remarks>
    /// Part of the new NET 7 number type system see <see cref="IRootFunctions{TSelf}.Hypot(TSelf,TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The value to square and add to <paramref name="y" />.</param>
    /// <param name="y">The value to square and add to <paramref name="x" />.</param>
    /// <returns>The square root of <paramref name="x" />-squared plus <paramref name="y" />-squared.</returns>
    public static BigRational Hypot(BigRational x, BigRational y)
    {
      //return Sqrt(x * x + y * y);
      var cpu = task_cpu; var d = DefaultDigits; var c = prec(d);
      cpu.push(x); cpu.sqr(); cpu.push(y); cpu.sqr(); cpu.add(); //todo: lim x^2, y^2 and check
      cpu.sqrt(c); cpu.rnd(d); return cpu.popr();
    }
    /// <summary>Computes the n-th root of a value.</summary>
    /// <remarks>
    /// Part of the new NET 7 number type system see <see cref="IRootFunctions{TSelf}.Root(TSelf,int)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The value whose <paramref name="n" />-th root is to be computed.</param>
    /// <param name="n">The degree of the root to be computed.</param>
    /// <returns>The <paramref name="n" />-th root of <paramref name="x" />.</returns>
    public static BigRational Root(BigRational x, int n)
    {
      return Pow(x, (BigRational)1 / n); //todo: opt. cpu
    }

    //IExponentialFunctions
    /// <summary>Computes <c>E</c> raised to a given power.</summary>
    /// <remarks>
    /// Part of the new NET 7 number type system see <see cref="IExponentialFunctions{TSelf}.Exp(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The power to which <c>E</c> is raised.</param>
    /// <returns><c>E<sup><paramref name="x" /></sup></c></returns>
    public static BigRational Exp(BigRational x)
    {
      return Exp(x, DefaultDigits); //todo: opt. cpu
    }
    /// <summary>Computes <c>2</c> raised to a given power.</summary>
    /// <remarks>
    /// Part of the new NET 7 number type system see <see cref="IExponentialFunctions{TSelf}.Exp2(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The power to which <c>2</c> is raised.</param>
    /// <returns><c>2<sup><paramref name="x" /></sup></c></returns>
    public static BigRational Exp2(BigRational x)
    {
      return Pow(2, x, DefaultDigits); //todo: impl
    }
    /// <summary>Computes <c>10</c> raised to a given power.</summary>
    /// <remarks>
    /// Part of the new NET 7 number type system see <see cref="IExponentialFunctions{TSelf}.Exp10(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The power to which <c>10</c> is raised.</param>
    /// <returns><c>10<sup><paramref name="x" /></sup></c></returns>
    public static BigRational Exp10(BigRational x)
    {
      return Pow(10, x, DefaultDigits); //todo: opt. cpu
    }
    /// <summary>Computes <c>E</c> raised to a given power and subtracts one.</summary>
    /// <remarks>
    /// Part of the new NET 7 number type system see <see cref="IExponentialFunctions{TSelf}.ExpM1(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The power to which <c>E</c> is raised.</param>
    /// <returns><c>E<sup><paramref name="x" /></sup> - 1</c></returns>
    public static BigRational ExpM1(BigRational x)
    {
      return Exp(x, DefaultDigits) - 1; //todo: opt. cpu
    }
    /// <summary>Computes <c>2</c> raised to a given power and subtracts one.</summary>
    /// <remarks>
    /// Part of the new NET 7 number type system see <see cref="IExponentialFunctions{TSelf}.Exp2M1(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The power to which <c>2</c> is raised.</param>
    /// <returns><c>2<sup><paramref name="x" /></sup> - 1</c></returns>
    public static BigRational Exp2M1(BigRational x)
    {
      return Pow(2, x, DefaultDigits) - 1; //todo: opt. cpu
    }
    /// <summary>Computes <c>10</c> raised to a given power and subtracts one.</summary>
    /// <remarks>
    /// Part of the new NET 7 number type system see <see cref="IExponentialFunctions{TSelf}.Exp10M1(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The power to which <c>10</c> is raised.</param>
    /// <returns><c>10<sup><paramref name="x" /></sup> - 1</c></returns>
    public static BigRational Exp10M1(BigRational x)
    {
      return Exp10(x) - 1; //todo: opt. cpu
    }

    //ILogarithmicFunctions
    /// <summary>Computes the natural (<c>base-E</c>) logarithm of a value.</summary>
    /// <remarks>
    /// Part of the new NET 7 number type system see <see cref="ILogarithmicFunctions{TSelf}.Log(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The value whose natural logarithm is to be computed.</param>
    /// <returns><c>log<sub>e</sub>(<paramref name="x" />)</c></returns>
    public static BigRational Log(BigRational x)
    {
      return Log(x, DefaultDigits); //todo: opt. cpu
    }
    /// <summary>Computes the logarithm of a value in the specified base.</summary>
    /// <remarks>
    /// Part of the new NET 7 number type system see <see cref="ILogarithmicFunctions{TSelf}.Log(TSelf,TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The value whose logarithm is to be computed.</param>
    /// <param name="newBase">The base in which the logarithm is to be computed.</param>
    /// <returns><c>log<sub><paramref name="newBase" /></sub>(<paramref name="x" />)</c></returns>
    public static BigRational Log(BigRational x, BigRational newBase) //todo: <--> Log(x, digits)
    {
      return Round(Log(x) / Log(newBase), DefaultDigits); //todo: opt. cpu
    }
    /// <summary>Computes the base-2 logarithm of a value.</summary>
    /// <remarks>
    /// Part of the new NET 7 number type system see <see cref="ILogarithmicFunctions{TSelf}.Log2(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The value whose base-2 logarithm is to be computed.</param>
    /// <returns><c>log<sub>2</sub>(<paramref name="x" />)</c></returns>
    public static BigRational Log2(BigRational x)
    {
      return Log2(x, DefaultDigits); //todo: opt. cpu
    }
    /// <summary>Computes the base-10 logarithm of a value.</summary>
    /// <remarks>
    /// Part of the new NET 7 number type system see <see cref="ILogarithmicFunctions{TSelf}.Log10(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The value whose base-10 logarithm is to be computed.</param>
    /// <returns><c>log<sub>10</sub>(<paramref name="x" />)</c></returns>
    public static BigRational Log10(BigRational x)
    {
      return Log10(x, DefaultDigits); //todo: opt. cpu
    }
    /// <summary>Computes the natural (<c>base-E</c>) logarithm of a value plus one.</summary>
    /// <remarks>
    /// Part of the new NET 7 number type system see <see cref="ILogarithmicFunctions{TSelf}.LogP1(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The value to which one is added before computing the natural logarithm.</param>
    /// <returns><c>log<sub>e</sub>(<paramref name="x" /> + 1)</c></returns>
    public static BigRational LogP1(BigRational x)
    {
      return Log(x + 1); //todo: opt. cpu
    }
    /// <summary>Computes the base-10 logarithm of a value plus one.</summary>
    /// <remarks>
    /// Part of the new NET 7 number type system see <see cref="ILogarithmicFunctions{TSelf}.Log10P1(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The value to which one is added before computing the base-10 logarithm.</param>
    /// <returns><c>log<sub>10</sub>(<paramref name="x" /> + 1)</c></returns>
    public static BigRational Log10P1(BigRational x)
    {
      return Log10(x + 1); //todo: opt. cpu
    }
    /// <summary>Computes the base-2 logarithm of a value plus one.</summary>
    /// <remarks>
    /// Part of the new NET 7 number type system see <see cref="ILogarithmicFunctions{TSelf}.Log2P1(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The value to which one is added before computing the base-2 logarithm.</param>
    /// <returns><c>log<sub>2</sub>(<paramref name="x" /> + 1)</c></returns>
    public static BigRational Log2P1(BigRational x)
    {
      return Log2(x + 1); //todo: opt. cpu
    }

    //ITrigonometricFunctions
    /// <summary>Computes the sine of a value.</summary>
    /// <remarks>
    /// This computes <c>sin(x)</c>.<br/>
    /// Part of the new NET 7 number type system see <see cref="ITrigonometricFunctions{TSelf}.Sin(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The value, in radians, whose sine is to be computed.</param>
    /// <returns>The sine of <paramref name="x" />.</returns>
    public static BigRational Sin(BigRational x)
    {
      return Sin(x, DefaultDigits); //todo: opt. cpu
    }
    /// <summary>Computes the cosine of a value.</summary>
    /// <remarks>
    /// This computes <c>cos(x)</c>.<br/>
    /// Part of the new NET 7 number type system see <see cref="ITrigonometricFunctions{TSelf}.Cos(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The value, in radians, whose cosine is to be computed.</param>
    /// <returns>The cosine of <paramref name="x" />.</returns>
    public static BigRational Cos(BigRational x)
    {
      return Cos(x, DefaultDigits); //todo: opt. cpu
    }
    /// <summary>Computes the tangent of a value.</summary>
    /// <remarks>
    /// This computes <c>tan(x)</c>.<br/>
    /// Part of the new NET 7 number type system see <see cref="ITrigonometricFunctions{TSelf}.Tan(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The value, in radians, whose tangent is to be computed.</param>
    /// <returns>The tangent of <paramref name="x" />.</returns>
    public static BigRational Tan(BigRational x)
    {
      return Tan(x, DefaultDigits); //todo: opt. cpu
    }
    /// <summary>Computes the arc-sine of a value.</summary>
    /// <remarks>
    /// This computes <c>arcsin(x)</c> in the interval <c>[-π / 2, +π / 2]</c> radians.<br/>
    /// Part of the new NET 7 number type system see <see cref="ITrigonometricFunctions{TSelf}.Asin(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The value whose arc-sine is to be computed.</param>
    /// <returns>The arc-sine of <paramref name="x" />.</returns>
    public static BigRational Asin(BigRational x)
    {
      return Asin(x, DefaultDigits); //todo: opt. cpu
    }
    /// <summary>Computes the arc-cosine of a value.</summary>
    /// <remarks>
    /// This computes <c>arccos(x)</c> in the interval <c>[+0, +π]</c> radians.<br/>
    /// Part of the new NET 7 number type system see <see cref="ITrigonometricFunctions{TSelf}.Acos(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The value whose arc-cosine is to be computed.</param>
    /// <returns>The arc-cosine of <paramref name="x" />.</returns>
    public static BigRational Acos(BigRational x)
    {
      return Acos(x, DefaultDigits); //todo: opt. cpu
    }
    /// <summary>Computes the arc-tangent of a value.</summary>
    /// <remarks>
    /// This computes <c>arctan(x)</c> in the interval <c>[-π / 2, +π / 2]</c> radians.<br/>
    /// Part of the new NET 7 number type system see <see cref="ITrigonometricFunctions{TSelf}.Atan(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The value whose arc-tangent is to be computed.</param>
    /// <returns>The arc-tangent of <paramref name="x" />.</returns>
    public static BigRational Atan(BigRational x)
    {
      return Atan(x, DefaultDigits); //todo: opt. cpu
    }
    /// <summary>Computes the sine and cosine of a value.</summary>
    /// <remarks>
    /// This computes <c>(sin(x), cos(x))</c>.<br/>
    /// Part of the new NET 7 number type system see <see cref="ITrigonometricFunctions{TSelf}.SinCos(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The value, in radians, whose sine and cosine are to be computed.</param>
    /// <returns>The sine and cosine of <paramref name="x" />.</returns>
    public static (BigRational Sin, BigRational Cos) SinCos(BigRational x)
    {
      return (Sin(x, DefaultDigits), Cos(x, DefaultDigits)); //todo: opt. cpu
    }
    /// <summary>Computes the arc-cosine of a value and divides the result by <c>pi</c>.</summary>
    /// <remarks>
    /// This computes <c>arccos(x) / π</c> in the interval <c>[-0.5, +0.5]</c>.<br/>
    /// Part of the new NET 7 number type system see <see cref="ITrigonometricFunctions{TSelf}.AcosPi(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The value whose arc-cosine is to be computed.</param>
    /// <returns>The arc-cosine of <paramref name="x" />, divided by <c>pi</c>.</returns>
    public static BigRational AcosPi(BigRational x)
    {
      return Acos(x, DefaultDigits) / Pi(DefaultDigits); //todo: opt. cpu
    }
    /// <summary>Computes the arc-sine of a value and divides the result by <c>pi</c>.</summary>
    /// <remarks>
    /// This computes <c>arcsin(x) / π</c> in the interval <c>[-0.5, +0.5]</c>.<br/>
    /// Part of the new NET 7 number type system see <see cref="ITrigonometricFunctions{TSelf}.AsinPi(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The value whose arc-sine is to be computed.</param>
    /// <returns>The arc-sine of <paramref name="x" />, divided by <c>pi</c>.</returns>
    public static BigRational AsinPi(BigRational x)
    {
      return Asin(x, DefaultDigits) / Pi(DefaultDigits); //todo: opt. cpu
    }
    /// <summary>Computes the arc-tangent of a value and divides the result by pi.</summary>
    /// <remarks>
    /// This computes <c>arctan(x) / π</c> in the interval <c>[-0.5, +0.5]</c>.<br/>
    /// Part of the new NET 7 number type system see <see cref="ITrigonometricFunctions{TSelf}.AtanPi(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The value whose arc-tangent is to be computed.</param>
    /// <returns>The arc-tangent of <paramref name="x" />, divided by <c>pi</c>.</returns>
    public static BigRational AtanPi(BigRational x)
    {
      return Atan(x, DefaultDigits) / Pi(DefaultDigits); //todo: opt. cpu
    }
    /// <summary>Computes the cosine of a value that has been multipled by <c>pi</c>.</summary>
    /// <remarks>
    /// This computes <c>cos(x * π)</c>.<br/>
    /// Part of the new NET 7 number type system see <see cref="ITrigonometricFunctions{TSelf}.CosPi(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The value, in half-revolutions, whose cosine is to be computed.</param>
    /// <returns>The cosine of <paramref name="x" /> multiplied-by <c>pi</c>.</returns>
    public static BigRational CosPi(BigRational x)
    {
      return Cos(x * Pi(DefaultDigits), DefaultDigits); //todo: opt. cpu
    }
    /// <summary>Computes the sine of a value that has been multiplied by <c>pi</c>.</summary>
    /// <remarks>
    /// This computes <c>sin(x * π)</c>.<br/>
    /// Part of the new NET 7 number type system see <see cref="ITrigonometricFunctions{TSelf}.SinPi(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The value, in half-revolutions, that is multipled by <c>pi</c> before computing its sine.</param>
    /// <returns>The sine of <paramref name="x" /> multiplied-by <c>pi</c>.</returns>
    public static BigRational SinPi(BigRational x)
    {
      return Sin(x * Pi(DefaultDigits), DefaultDigits); //todo: opt. cpu
    }
    /// <summary>Computes the tangent of a value that has been multipled by <c>pi</c>.</summary>
    /// <remarks>
    /// Part of the new NET 7 number type system see <see cref="ITrigonometricFunctions{TSelf}.TanPi(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The value, in half-revolutions, that is multipled by <c>pi</c> before computing its tangent.</param>
    /// <returns>The tangent of <paramref name="x"/> multiplied-by <c>pi</c>.</returns>
    /// <remarks>This computes <c>tan(x * π)</c>.</remarks>
    public static BigRational TanPi(BigRational x)
    {
      return Tan(x * Pi(DefaultDigits), DefaultDigits); //todo: opt. cpu
    }
    /// <summary>Computes the arc-tangent of the quotient of two values.</summary>
    /// <remarks>
    /// Part of the new NET 7 number type system see <see cref="ITrigonometricFunctions{TSelf}.Atan2(TSelf,TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="y">The y-coordinate of a point.</param>
    /// <param name="x">The x-coordinate of a point.</param>
    /// <returns>The arc-tangent of y divided by x.</returns>
    public static BigRational Atan2(BigRational y, BigRational x)
    {
      return Atan2(y, x, DefaultDigits); //todo: opt. cpu
    }
    /// <summary>
    /// Computes the arc-tangent of the quotient of two values and divides the result by pi.
    /// </summary>
    /// <remarks>
    /// Part of the new NET 7 number type system see <see cref="ITrigonometricFunctions{TSelf}.Atan2Pi(TSelf,TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="y">The y-coordinate of a point.</param>
    /// <param name="x">The x-coordinate of a point.</param>
    /// <returns>The arc-tangent of y divided by x divided by pi.</returns>
    public static BigRational Atan2Pi(BigRational y, BigRational x)
    {
      return Atan2(y, x, DefaultDigits) / Pi(DefaultDigits); //todo: opt. cpu
    }

    // IFloatingPointIeee754 (double compat.)
    /// <summary>Computes the integer logarithm of a value.</summary>
    /// <remarks>
    /// Part of the new NET 7 number type system see <see cref="IFloatingPointIeee754{TSelf}.ILogB(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The value whose integer logarithm is to be computed.</param>
    /// <returns>The integer logarithm of <paramref name="x" />.</returns>
    public static BigRational ILogB(BigRational x)
    {
      return (int)Log2(x); //todo: ILog2 alg
    }

    // IHyperbolicFunctions
    /// <summary>Computes the hyperbolic arc-sine of a value.</summary>
    /// <param name="x">The value, in radians, whose hyperbolic arc-sine is to be computed.</param>
    /// <remarks>
    /// Part of the new NET 7 number type system see <see cref="IHyperbolicFunctions{TSelf}.Asinh(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <returns>The hyperbolic arc-sine of <paramref name="x" />.</returns>
    public static BigRational Asinh(BigRational x)
    {
      return Log(x + Sqrt(x * x + 1)); //todo: opt. cpu
    }
    /// <summary>Computes the hyperbolic arc-cosine of a value.</summary>
    /// <param name="x">The value, in radians, whose hyperbolic arc-cosine is to be computed.</param>
    /// <remarks>
    /// Part of the new NET 7 number type system see <see cref="IHyperbolicFunctions{TSelf}.Acosh(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <returns>The hyperbolic arc-cosine of <paramref name="x" />.</returns>
    public static BigRational Acosh(BigRational x)
    {
      return Log(x + Sqrt(x * x - 1)); //todo: opt. cpu
    }
    /// <summary>Computes the hyperbolic arc-tangent of a value.</summary>
    /// <param name="x">The value, in radians, whose hyperbolic arc-tangent is to be computed.</param>
    /// <remarks>
    /// Part of the new NET 7 number type system see <see cref="IHyperbolicFunctions{TSelf}.Atanh(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <returns>The hyperbolic arc-tangent of <paramref name="x" />.</returns>
    public static BigRational Atanh(BigRational x)
    {
      return Log((1 + x) / (1 - x)) / 2; //todo: opt. cpu
    }
    /// <summary>Computes the hyperbolic sine of a value.</summary>
    /// <param name="x">The value, in radians, whose hyperbolic sine is to be computed.</param>
    /// <remarks>
    /// Part of the new NET 7 number type system see <see cref="IHyperbolicFunctions{TSelf}.Sinh(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <returns>The hyperbolic sine of <paramref name="x" />.</returns>
    public static BigRational Sinh(BigRational x)
    {
      return (Exp(x) - Exp(-x)) / 2; //todo: opt. cpu
    }
    /// <summary>Computes the hyperbolic cosine of a value.</summary>
    /// <param name="x">The value, in radians, whose hyperbolic cosine is to be computed.</param>
    /// <remarks>
    /// Part of the new NET 7 number type system see <see cref="IHyperbolicFunctions{TSelf}.Cosh(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <returns>The hyperbolic cosine of <paramref name="x" />.</returns>
    public static BigRational Cosh(BigRational x)
    {
      return (Exp(x) + Exp(-x)) / 2; //todo: opt. cpu
    }
    /// <summary>Computes the hyperbolic tangent of a value.</summary>
    /// <remarks>
    /// Part of the new NET 7 number type system see <see cref="IHyperbolicFunctions{TSelf}.Tanh(TSelf)"/>.<br/>
    /// The desired precision can preset by <see cref="DefaultDigits"/>
    /// </remarks>
    /// <param name="x">The value, in radians, whose hyperbolic tangent is to be computed.</param>
    /// <returns>The hyperbolic tangent of <paramref name="x" />.</returns>
    public static BigRational Tanh(BigRational x)
    {
      return 1 - 2 / (Exp(x * 2) + 1); //todo: opt. cpu
    }
  }

#endif // NET 7

}



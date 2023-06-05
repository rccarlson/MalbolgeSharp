using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malbolge;

internal static class Word
{
	public const int WordSize = 10;
	public const int MaxValue = 59048;

	public static int ReadInt(Span<Trit> data)
	{
		int cumulative = 0;

		for (int i = 0; i < WordSize; i++)
		{
			var idx = WordSize - 1 - i;
			cumulative += Pow3(i) * (int)data[idx];
		}
		return cumulative;

		/// Raise 3 to the specified power
		static int Pow3(int power) =>
			power switch
			{
				0 => 1,
				1 => 3,
				2 => 9,
				3 => 27,
				4 => 81,
				5 => 243,
				6 => 729,
				7 => 2187,
				8 => 6561,
				9 => 19683,
				_ => Pow3(9) * Pow3(power - 9)
			};
	}

	public static void WriteInt(Span<Trit> data, int value)
	{
		var cumulative = value;
		for (int idx = WordSize - 1; idx >= 0; idx--)
		{
			var remainder = cumulative % 3;
			data[idx] = (Trit)remainder;
			cumulative /= 3;
		}
	}

	public static void Add(Span<Trit> addend1, Span<Trit> addend2, Span<Trit> writeLocation)
	{
		int carry = 0;
		for (int i = 0; i < WordSize; i++)
		{
			var idx = WordSize - 1 - i;
			var sum = (int)addend1[idx] + (int)addend2[idx] + carry;
			writeLocation[idx] = (Trit)(sum % 3);
			carry = sum / 3;
		}
	}

	public static bool Equals(Span<Trit> a, Span<Trit> b)
	{
		for (int i = 0; i < WordSize; i++)
		{
			if (a[i] != b[i]) return false;
		}
		return true;
	}

	public static Trit Crazy(Trit a, Trit d)
	{
		return d switch
		{
			Trit.Zero => a switch
			{
				Trit.Zero => Trit.One,
				Trit.One => Trit.Zero,
				Trit.Two => Trit.Zero,
				_ => throw new NotImplementedException(a.ToString())
			},
			Trit.One => a switch
			{
				Trit.Zero => Trit.One,
				Trit.One => Trit.Zero,
				Trit.Two => Trit.Two,
				_ => throw new NotImplementedException(a.ToString())
			},
			Trit.Two => a switch
			{
				Trit.Zero => Trit.Two,
				Trit.One => Trit.Two,
				Trit.Two => Trit.One,
				_ => throw new NotImplementedException(a.ToString())
			},
			_ => throw new NotImplementedException(a.ToString())
		};
	}
	public static void Crazy(Span<Trit> a, Span<Trit> d, Span<Trit> destination)
	{
		for (int i = 0; i < WordSize; i++)
		{
			destination[i] = Crazy(a[i], d[i]);
		}
	}
	public static int Crazy(int a, int d)
	{
		Span<Trit> aSpan = stackalloc Trit[WordSize];
		Span<Trit> dSpan = stackalloc Trit[WordSize];
		Span<Trit> result = stackalloc Trit[WordSize];
		WriteInt(aSpan, a);
		WriteInt(dSpan, d);
		Crazy(aSpan, dSpan, result);
		return ReadInt(result);
	}
	public static int Rotr(int value)
	{
		return (value % 3) switch
		{
			0 => value / 3,
			1 => value / 3 + 19683, // 1/3 of 59049
			2 => value / 3 + 39366, // 2/3 of 59049
			_ => throw new NotImplementedException()
		};
	}

	public static void Rotr(Span<Trit> span)
	{
		Trit temp = span[WordSize - 1];
		span[..(WordSize - 1)].CopyTo(span[1..]);
		span[0] = temp;
	}
}
public enum Trit { Zero = 0, One = 1, Two = 2 }

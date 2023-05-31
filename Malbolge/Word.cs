using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malbolge;

[DebuggerDisplay("{Value, nq}")]
public struct Word
{
	public static Word MaxValue => new Word(new[] { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 });
	private const int WordSize = 10;
	public Word(char c): this((int)c) { }
	public Word(int i)
	{
		_data = new Trit[WordSize];
		Value = i;
	}
	public Word(Trit[] trits)
	{
		if (trits.Length != WordSize) throw new ArgumentException($"Words must be of length {WordSize}");
		_data = trits;
	}
	public Word(int[] trits)
	{
		if (trits.Length != WordSize) throw new ArgumentException($"Words must be of length {WordSize}");
		_data = new Trit[WordSize];
		for(int i = 0; i< WordSize; i++)
		{
			Data[i] = (Trit)trits[i];
		}
	}

	public Word Rotr()
	{
		var temp = ArrayPool<Trit>.Shared.Rent(WordSize - 1);
		Array.Copy(Data, temp, WordSize - 1);
		Data[0] = Data[WordSize - 1];
		Array.Copy(temp, 0, Data, 1, WordSize - 1);
		ArrayPool<Trit>.Shared.Return(temp);
		return this;
	}

	public int Value
	{
		get
		{
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

			int cumulative = 0;

			for(int i = 0; i < WordSize; i++)
			{
				var idx = WordSize - 1 - i;
				cumulative += Pow3(i) * (int)Data[idx];
			}
			return cumulative;
		}
		set
		{
			var cumulative = value;
			for(int i = 0; i < WordSize; i++)
			{
				var idx = WordSize - 1 - i;
				var remainder = cumulative % 3;
				Data[idx] = (Trit)remainder;
				cumulative /= 3;
			}
		}
	}

	private Trit[] _data;
	internal Trit[] Data {
		get => _data;
		set {
			if (value.Length != WordSize) throw new ArgumentException($"All data must be of size {WordSize}");
			_data = value;
		}
	}

	private static Trit Crazy(Trit a, Trit d)
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
	public static Trit[] TritwiseOp(Word a, Word d)
	{
		var data = new Trit[WordSize];
		for (int i = 0; i < WordSize; i++)
		{
			data[i] = Crazy(a.Data[i], d.Data[i]);
		}
		return data;
	}

	public static Word operator +(Word left, Word right)
	{
		int carry = 0;
		var newData = new Trit[WordSize];
		for (int i = 0; i < WordSize; i++)
		{
			var idx = WordSize - 1 - i;
			var sum = (int)left.Data[idx] + (int)right.Data[idx] + carry;
			newData[idx] = (Trit)(sum % 3);
			carry = sum / 3;
		}
		var result = new Word(newData);
		return result;
	}
	public static Word operator -(Word left, Word right) => new Word(left.Value - right.Value);
	public static Word operator *(Word left, Word right) => new Word(left.Value * right.Value);
	public static Word operator /(Word left, Word right) => new Word(left.Value / right.Value);
	public static Word operator %(Word left, Word right) => new Word(left.Value % right.Value);
	public static bool operator !=(Word left, Word right) => !(left == right);
	public static bool operator ==(Word left, Word right)
	{
		for (int i = 0; i < WordSize; i++)
		{
			if (left.Data[i] != right.Data[i]) return false;
		}
		return true;
	}
	public override string ToString() => $"{Value}";

}
public enum Trit { Zero = 0, One = 1, Two = 2 }

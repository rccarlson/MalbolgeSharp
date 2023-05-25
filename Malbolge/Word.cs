using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malbolge;

[DebuggerDisplay("{Value, nq}")]
public class Word
{
	public const int MaxValue = 59049;
	private const int WordSize = 10;
	public Word(char c): this((int)c) { }
	public Word(int i)
	{
		data = new Trit[WordSize];
		Value = i;
	}
	public Word(int[] trits)
	{
		if (trits.Length != WordSize) throw new ArgumentException($"Words must be of length {WordSize}");
		data = new Trit[WordSize];
		for(int i = 0; i< WordSize; i++)
		{
			data[i] = (Trit)trits[i];
		}
	}

	public Word Rotr()
	{
		var temp = data[0..(WordSize-1)];
		data[0] = data[WordSize - 1];
		Array.Copy(temp, 0, data, 1, WordSize - 1);
		return this;
	}
	public Word Rotl()
	{
		var temp = data[1..WordSize];
		data[WordSize - 1] = data[0];
		Array.Copy(temp, 0, data, 0, WordSize - 1);
		return this;
	}

	public int Value
	{
		get
		{
			int cumulative = 0;

			for(int i = 0; i < WordSize; i++)
			{
				var idx = WordSize - 1 - i;
				cumulative += (int)Math.Pow(3, i) * (int)data[idx];
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
				data[idx] = (Trit)remainder;
				cumulative /= 3;
			}
		}
	}

	private readonly Trit[] data;

	private static int Crazy(int a, int d)
	{
		return d switch
		{
			0 => a switch
			{
					0 => 1,
					1 => 0,
					2 => 0,
					_ => throw new NotImplementedException(a.ToString())
				},
			1 => a switch
			{
					0 => 1,
					1 => 0,
					2 => 2,
					_ => throw new NotImplementedException(a.ToString())
				},
			2 => a switch
			{
					0 => 2,
					1 => 2,
					2 => 1,
					_ => throw new NotImplementedException(a.ToString())
				},
				_ => throw new NotImplementedException(a.ToString())
			};
	}
	public static Word TritwiseOp(Word a, Word d)
	{
		var data = new int[WordSize];
		for(int i= 0; i < WordSize; i++)
		{
			data[i] = Crazy((int)a.data[i], (int)d.data[i]);
		}
		return new Word(data);
	}

	public static Word operator +(Word left, Word right) => new Word(left.Value + right.Value);
	public static Word operator -(Word left, Word right) => new Word(left.Value - right.Value);
	public static Word operator *(Word left, Word right) => new Word(left.Value * right.Value);
	public static Word operator /(Word left, Word right) => new Word(left.Value / right.Value);
	public static Word operator %(Word left, Word right) => new Word(left.Value % right.Value);
	public override string ToString() => $"{Value}";

	public static implicit operator int(Word word) => word.Value;
	public static implicit operator Word(int value) => new Word(value);
}
enum Trit { Zero = 0, One = 1, Two = 2 }

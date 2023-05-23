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
		Value = i;
	}
	public Word(int[] trits)
	{
		data = trits.Select(ToTrit).ToArray();
	}

	public void Rotr()
	{
		var temp = data[0..(WordSize-1)];
		data[0] = data[WordSize - 1];
		Array.Copy(temp, 0, data, 1, WordSize - 1);
	}

	public int Value
	{
		get
		{
			int cumulative = 0;

			for(int i = 0; i < 10; i++)
			{
				var idx = WordSize - 1 - i;
				cumulative += (int)Math.Pow(3, i) * (int)data[idx];
			}
			return cumulative;
		}
		set
		{
			var cumulative = value;
			for(int i = 0; i < 10; i++)
			{
				var idx = WordSize - 1 - i;
				var remainder = cumulative % 3;
				data[idx] = ToTrit(remainder);
				cumulative /= 3;
			}
		}
	}

	private Trit[] data = new Trit[WordSize];

	public static Word TritwiseOp(Word a, Word d)
	{
		var data = a.data.Zip(d.data).Select(tuple =>
		{
			var (a, d) = tuple;
			return (int)d switch
			{
				0 => (int)a switch {
					0 => 1,
					1 => 0,
					2 => 0,
					_ => throw new NotImplementedException(a.ToString())
				},
				1 => (int)a switch {
					0 => 1,
					1 => 0,
					2 => 2,
					_ => throw new NotImplementedException(a.ToString())
				},
				2 => (int)a switch {
					0 => 2,
					1 => 2,
					2 => 1,
					_ => throw new NotImplementedException(a.ToString())
				},
				_ => throw new NotImplementedException(a.ToString())
			};
		}).ToArray();
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

	private static Trit ToTrit(int i)
	{
		return i switch
		{
			0 => Trit.Zero,
			1 => Trit.One,
			2 => Trit.Two,
			_ => throw new NotImplementedException(i.ToString())
		};
	}
}
enum Trit { Zero = 0, One = 1, Two = 2 }

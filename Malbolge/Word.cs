﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malbolge;

[DebuggerDisplay("{Value, nq}")]
public class Word
{
	public static Word MaxValue => new Word(new[] { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 });
	private const int WordSize = 10;
	public Word(char c): this((int)c) { }
	public Word(int i)
	{
		data = new Trit[WordSize];
		Value = i;
	}
	public Word(Trit[] trits)
	{
		if (trits.Length != WordSize) throw new ArgumentException($"Words must be of length {WordSize}");
		data = trits;
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

	internal Trit[] data;

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
		for(int i= 0; i < WordSize; i++)
		{
			data[i] = Crazy(a.data[i], d.data[i]);
		}
		return data;
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
public enum Trit { Zero = 0, One = 1, Two = 2 }

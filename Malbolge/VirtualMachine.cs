using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malbolge;

public static class VirtualMachine
{
	private const int MemorySize = 59049;

	public static ExecutionReport Execute(MalbolgeFlavor flavor, string program, string input = "", int maxIterations = -1)
	{
		var memory = ArrayPool<int>.Shared.Rent(MemorySize);
		PopulateMemory(memory, program);

		// Run
		ExitReason? exitReason = null;
		int iteration = 0;
		int inputIndex = 0;
		StringBuilder output = new();
		int memoryReads = 0,
			memoryWrites = 0;
		int a = 0, c = 0, d = 0;

		while (exitReason is null)
		{
			var instructionValue = (memory[c] + c) % 94;
			switch (instructionValue)
			{
				case 4: c = memory[d]; memoryReads++; break;

				case 5 when flavor is MalbolgeFlavor.Specification:
				case 23 when flavor is MalbolgeFlavor.Implementation:
					output.Append((char)(a % 256)); break; // out a

				case 5 when flavor is MalbolgeFlavor.Implementation:
				case 23 when flavor is MalbolgeFlavor.Specification:
					if (inputIndex < input.Length)
					{
						var rawInput = input[inputIndex++];
						a = rawInput switch
						{
							'\r' or '\n' => 10,
							_ => rawInput
						};
					}
					else
					{
						a = Word.MaxValue;
					}
					break;

				case 39: a = memory[d] = Word.Rotr(memory[d]); memoryReads++; memoryWrites++; break; // rotr [d]
				case 40: d = memory[d]; memoryReads++; break; // mov d, [d]
				case 62: a = memory[d] = Word.Crazy(a, memory[d]); memoryReads++; memoryWrites++; break; // crz
				case 68: /* nop */ break;
				case 81: exitReason = ExitReason.ProgramComplete; break; // end
				default: /* nop iff not the first instruction */ if (iteration == 0) exitReason = ExitReason.InvalidProgram; break;
			}
			c = (c + 1) % MemorySize;
			d = (d + 1) % MemorySize;
			iteration++;

			if (maxIterations >= 0 && iteration >= maxIterations)
				exitReason = ExitReason.MaxIteration;
		}

		ArrayPool<int>.Shared.Return(memory);

		return new ExecutionReport()
		{
			Iterations = iteration,
			MemoryReads = memoryReads,
			MemoryWrites = memoryWrites,
			Program = program,
			ExitReason = exitReason ?? ExitReason.Unknown,
			Result = output.ToString()
		};
	}
	private static void PopulateMemory(Span<int> memory, string program)
	{
		var progLen = WriteProgramToMemory(memory, program);

		WriteRepeatingCrazyNumbers(memory[progLen - 2], memory[progLen - 1], memory.Slice(progLen, MemorySize - progLen));
	}

	public static int[] GetRepeatingCrazyNumbers(int seed1, int seed2, int maxLength)
	{
		int a = Word.Crazy(seed1, seed2);
		int b = Word.Crazy(seed2, a);
		int c = Word.Crazy(a, b);
		int d = Word.Crazy(b, c);

		switch (maxLength)
		{
			case 0: return Array.Empty<int>();
			case 1: return new[] { a };
			case 2: return new[] { a, b };
			case 3: return new[] { a, b, c };
		}

		List<int> values = new(maxLength) { a, b };
		for (int i = values.Count; i < maxLength; i++)
		{
			var newValue = Word.Crazy(c, d);
			(b, c, d) = (c, d, newValue);
			values.Add(b);
			if (ValuePairExists(c, d)) return values.ToArray();
		}
		return values.ToArray();

		bool ValuePairExists(int first, int second)
		{
			for (int i = 0; i < values.Count - 1; i++)
			{
				if (values[i] == first && values[i + 1] == second) return true;
			}
			return false;
		}
	}

	public static void WriteRepeatingCrazyNumbers(int seed1, int seed2, Span<int> writeLocation)
	{
		// populate cache
		const int MaxCacheSize = 12;
		Span<int> cache = stackalloc int[MaxCacheSize];
		int cacheSize = -1;
		var a = cache[0] = Word.Crazy(seed1, seed2);
		var b = cache[1] = Word.Crazy(seed2, a);
		var c = Word.Crazy(a, b);
		var d = Word.Crazy(b, c);
		for (int i = 2; i < MaxCacheSize; i++)
		{
			var newValue = Word.Crazy(c, d);
			(b, c, d) = (c, d, newValue);
			cache[i] = b;
			if (ValuePairExists(c, d, cache[..i]))
			{
				cacheSize = i + 1;
				break;
			}
		}
		if (cacheSize < 1) throw new InvalidOperationException("Failed to build cache");
		var validCache = cache[..cacheSize];
		static bool ValuePairExists(int first, int second, Span<int> values)
		{
			for (int i = 0; i < values.Length - 1; i++)
			{
				if (values[i] == first && values[i + 1] == second) return true;
			}
			return false;
		}

		// populate memory from cache
		int writeDest;
		for (writeDest = 0; writeDest < writeLocation.Length - cacheSize; writeDest += cacheSize)
		{
			validCache.CopyTo(writeLocation.Slice(writeDest, cacheSize));
		}
		var finalCopySize = writeLocation.Length - writeDest;
		var finalCopyDest = writeLocation[^finalCopySize..];
		var finalCopySource = cache[..finalCopySize];
		finalCopySource.CopyTo(finalCopyDest);
	}

	private static int WriteProgramToMemory(Span<int> memory, string program) // TODO: scoped?
	{
		int pointer = 0;
		for (int i = 0; i < program.Length; i++)
		{
			char c = program[i];
			if (char.IsWhiteSpace(c)) continue;
			memory[pointer++] = c;
		}
		return pointer;
	}
}

public enum ExitReason
{
	/// <summary> This is an error state and should not be legitimately reachable </summary>
	Unknown,
	ProgramComplete,
	MaxIteration,
	InvalidProgram
}

public struct ExecutionReport
{
	public ExitReason ExitReason;
	public int Iterations;
	public string Result;
	public string Program;
	public int MemoryReads, MemoryWrites;

	public override string ToString() => $"'{Result}' <= '{Program}'";
}

public enum MalbolgeFlavor
{
	Specification,
	Implementation
}
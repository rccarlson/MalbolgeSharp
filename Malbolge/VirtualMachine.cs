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

		int progLen = 0;
		if (program.Length >= MemorySize) goto error;
		for (int i = 0; i < program.Length; i++)
		{
			char x = program[i];
			if (char.IsWhiteSpace(x)) continue;
			if (IsGraphicalAscii(x))
				if(!CharIsAllowed(x, progLen))
					goto error;
			memory[progLen++] = x;
		}

		if (progLen < 2) return new ExecutionReport() { ExitReason = ExitReason.InvalidProgram, Program = program };
		WriteRepeatingCrazyNumbers(memory[progLen - 2], memory[progLen - 1], memory.AsSpan()[progLen..MemorySize]);

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
			if (!IsGraphicalAscii(memory[c] % 256))
			{
				exitReason = ExitReason.InvalidProgram; break;
			}
			char instruction = xlat1[(memory[c] - 33 + c) % 94];

			switch (instruction)
			{
				case 'j': d = memory[d]; memoryReads++; break;
				case 'i': c = memory[d]; memoryReads++; break;
				case '*': a = memory[d] = memory[d] / 3 + memory[d] % 3 * 19683; memoryReads++; memoryWrites++; break;
				case 'p': a = memory[d] = Word.Crazy2(a, memory[d]); memoryReads++; memoryWrites++; break;
				case '/' when flavor is MalbolgeFlavor.Specification:
				case '<' when flavor is MalbolgeFlavor.Implementation:
					// Input
					if (input.Length < inputIndex)
						a = memory[d] = input[inputIndex++];
					else
						a = memory[d] = 59048; //eof
					memoryWrites++;
					break;
				case '<' when flavor is MalbolgeFlavor.Specification:
				case '/' when flavor is MalbolgeFlavor.Implementation:
					// Output
					output.Append((char)(a % 256)); break;
				case 'v': exitReason = ExitReason.ProgramComplete; break;
					//case 'o': /*nop*/ break;
			}

			if (memory[c] is >= 33 and <= 126)
				memory[c] = xlat2[memory[c] - 33];
			else
				exitReason = ExitReason.InvalidProgram;
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

		error:
		ArrayPool<int>.Shared.Return(memory);
		return new ExecutionReport() { ExitReason = ExitReason.InvalidProgram, Program = program };
	}

	private static readonly char[] xlat1 = """+b(29e*j1VMEKLyC})8&m#~W>qxdRp0wkrUo[D7,XTcA"lI.v%{gJh4G\-=O@5`_3i<?Z';FNQuY]szf$!BS/|t:Pn6^Ha""".ToCharArray();
	private static readonly char[] xlat2 = """5z]&gqtyfr$(we4{WP)H-Zn,[%\3dL+Q;>U!pJS72FhOA1CB6v^=I_0/8|jsb9m<.TVac`uY*MK'X~xDl}REokN:#?G"i@""".ToCharArray();

	public static void WriteRepeatingCrazyNumbers(int seed1, int seed2, Span<int> writeLocation)
	{
		// populate cache
		const int MaxCacheSize = 12;
		Span<int> cache = stackalloc int[MaxCacheSize];
		int cacheSize = -1;
		var a = cache[0] = Word.Crazy2(seed1, seed2);
		var b = cache[1] = Word.Crazy2(seed2, a);
		var c = Word.Crazy2(a, b);
		var d = Word.Crazy2(b, c);
		for (int i = 2; i < MaxCacheSize; i++)
		{
			var newValue = Word.Crazy2(c, d);
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

	private static int WriteProgramToMemory(Span<int> memory, string program)
	{
		int pointer = 0;
		if (program.Length >= MemorySize) throw new ArgumentException($"Input too long");
		for (int i = 0; i < program.Length; i++)
		{
			char x = program[i];
			if (char.IsWhiteSpace(x)) continue;
			if (IsGraphicalAscii(x))
				if (AllowableInstructions.Contains(xlat1[(x - 33 + pointer) % 94]))
					throw new ArgumentException($"Invalid character in source file");
			memory[pointer++] = x;
		}
		return pointer;
	}
	public static bool CharIsAllowed(char x, int memoryIndex) => AllowableInstructions.Contains(xlat1[(x - 33 + memoryIndex) % 94]);
	public static IEnumerable<char> GetAllowableChars(int memoryIndex)
	{
		for(int i = 0; i <= 256; i++)
		{
			char c = (char)i;
			if (IsGraphicalAscii(c))
				if (!CharIsAllowed(c, memoryIndex))
					continue;
			if (!IsGraphicalAscii(c % 256)) continue;
			yield return c;
		}
	}
	public const string AllowableInstructions = "ji*p</vo";

	private static bool IsGraphicalAscii(int c) => c is >= 33 and <= 126;

	public static string Normalize(string program)
	{
		var sb = new StringBuilder(program.Length);
		int memoryIndex = 0;
		for(int i=0; i<program.Length; i++)
		{
			char x = program[i];
			if (char.IsWhiteSpace(x)) continue;
			char c = xlat1[(x - 33 + memoryIndex) % 94];
			sb.Append(c);
			memoryIndex++;
		}
		var result = sb.ToString();
		return result;
	}
	public static string Denormalize(string program)
	{
		var sb = new StringBuilder(program.Length);
		for(int memoryIndex = 0; memoryIndex < program.Length; memoryIndex++)
		{
			char c = program[memoryIndex];
			var idx = xlat1.IndexOf(c);
			int xAscii = idx - memoryIndex + 33;
			if (xAscii < 32) xAscii += 94;
			sb.Append((char)xAscii);
		}
		var result = sb.ToString();
		return result;
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

	public override string ToString() => $"'{Result.Replace("\0","")}' <= '{VirtualMachine.Normalize(Program)}'";
}

public enum MalbolgeFlavor
{
	Specification,
	Implementation
}
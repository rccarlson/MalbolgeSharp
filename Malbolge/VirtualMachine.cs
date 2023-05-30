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
		if (program.Length < 2) return new ExecutionReport() { Program = program, ExitReason = ExitReason.InvalidProgram };

		// Construct
		var memory = ArrayPool<Word>.Shared.Rent(MemorySize);
		var programAsMemory = program.Where(c => !char.IsWhiteSpace(c)).Select(c => new Word(c)).ToList();
		Array.Copy(programAsMemory.ToArray(), memory, programAsMemory.Count);
		var progLen = programAsMemory.Count;

		var first = memory[progLen - 2];
		var second = memory[progLen - 1];
		var third = new Word(Word.TritwiseOp(first, second));
		var fourth = new Word(Word.TritwiseOp(second, third));
		var wordSeq = new List<Word>() { third, fourth };

		while (wordSeq.Count < 2 || (
			first != wordSeq[^2] &&
			second != wordSeq[^1]
			))
		{
			var newTrits = Word.TritwiseOp(wordSeq[^2], wordSeq[^1]);
			var newWord = new Word(newTrits);
			wordSeq.Add(newWord);
		}
		var wordSeqArr = wordSeq.ToArray();

		for (int i = progLen; i < MemorySize; i += wordSeqArr.Length)
		{
			var copySize = Math.Min(wordSeqArr.Length, MemorySize - i);
			Array.Copy(wordSeqArr, 0, memory, i, copySize);
		}

		// Run
		ExitReason? exitReason = null;
		int iteration = 0;
		int inputIndex = 0;
		StringBuilder output = new();
		int memoryReads = 0,
			memoryWrites = 0;
		Word a = new(0),
			c = new(0),
			d = new(0);

		while (exitReason is null)
		{
			var instructionValue = (memory[c.Value] + c).Value % 94;

			switch (instructionValue)
			{
				case 4: c = memory[d.Value]; memoryReads++; break; // jmp [d]

				case 5 when flavor is MalbolgeFlavor.Specification:
				case 23 when flavor is MalbolgeFlavor.Implementation:
					output.Append((char)(a.Value % 256)); break; // out a

				case 5 when flavor is MalbolgeFlavor.Implementation:
				case 23 when flavor is MalbolgeFlavor.Specification:
					if (inputIndex < input.Length)
					{
						var rawInput = input[inputIndex++];
						a = rawInput switch
						{
							'\r' or '\n' => new Word(10),
							_ => new Word(rawInput)
						};
					}
					else
					{
						a = Word.MaxValue; // 59048
					}
					break;
				case 39: a = memory[d.Value].Rotr(); memoryReads++; break; // rotr [d]
				case 40: d = memory[d.Value]; memoryReads++; break; // mov d, [d]
				case 62: a.Data = memory[d.Value].Data = Word.TritwiseOp(a, memory[d.Value]); memoryReads++; memoryWrites++; break; // crz
				case 68: /* nop */ break;
				case 81: exitReason = ExitReason.ProgramComplete; break; // end
				default: /* nop iff not the first instruction */ if (iteration == 0) exitReason = ExitReason.InvalidProgram; break;
			}
			c = new Word((c.Value + 1) % MemorySize);
			d = new Word((d.Value + 1) % MemorySize);
			iteration++;

			if (maxIterations >= 0 && iteration >= maxIterations)
				exitReason = ExitReason.MaxIteration;

			//if (i % 1 == 0)
			//{
			//	var memHash = HashMemory();
			//	var output =
			//		 new string(OutputQueue.ToArray()).ReplaceLineEndings().Replace(Environment.NewLine, @"\n");
			//	//"[REMOVED]";
			//	Console.Write($"\riter: {i,-8} memory hash: {HashMemory(),-12} output: {output}");
			//}
		}

		// Deconstruct
		ArrayPool<Word>.Shared.Return(memory);

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malbolge;

public class VirtualMachine
{
	private const int MemorySize = 59049;

	public VirtualMachine(MalbolgeFlavor flavor, string program)
	{
		this.Flavor = flavor;
		this.InitialProgram = program;
		var programAsMemory = program.Where(c => !char.IsWhiteSpace(c)).Select(c => new Word(c)).ToArray();
		Array.Copy(programAsMemory, memory, programAsMemory.Length);
		var progLen = programAsMemory.Length;

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

		for (int i = progLen; i < memory.Length; i += wordSeqArr.Length)
		{
			var copySize = Math.Min(wordSeqArr.Length, MemorySize - i);
			Array.Copy(wordSeqArr, 0, memory, i, copySize);
		}
	}

	public readonly string InitialProgram;
	private Word[] memory = new Word[MemorySize];
	public int HashMemory() => memory.Aggregate(0, (state, word) => HashCode.Combine(state, word.Value));

	/// <summary> accumulator </summary>
	internal Word a = new(0);
	/// <summary> code pointer </summary>
	internal Word c = new(0);
	/// <summary> data pointer </summary>
	internal Word d = new(0);

	public Queue<char> InputQueue { get; } = new Queue<char>();
	public Queue<char> OutputQueue { get; } = new Queue<char>();
	public int MemoryReads { get; private set; } = 0;
	public int MemoryWrites { get; private set; } = 0;

	public MalbolgeFlavor Flavor { get; }
	public int MaxIterations { get; set; } = -1;

	Dictionary<int, int> hitcount = new();
	int iteration = 0;

	/// <summary>
	/// Returns true while the program can continue
	/// </summary>
	public bool ExecuteSingle()
	{
		var instructionValue = (memory[c] + c) % 94;

		if (hitcount.TryGetValue(instructionValue, out int hits)) hitcount[instructionValue] = hits + 1;
		else hitcount[instructionValue] = 1;

		switch (instructionValue)
		{
			case 4: c = memory[d]; MemoryReads++; break; // jmp [d]

			case 5 when Flavor is MalbolgeFlavor.Specification:
			case 23 when Flavor is MalbolgeFlavor.Implementation:
				OutputQueue.Enqueue((char)(a % 256)); break; // out a

			case 5 when Flavor is MalbolgeFlavor.Implementation:
			case 23 when Flavor is MalbolgeFlavor.Specification:
				if (!InputQueue.TryDequeue(out char result))
				{
					a = Word.MaxValue; // 59048
				}
				else
				{
					a = result switch
					{
						'\r' or '\n' => 10,
						_ => result
					};
				}
				break;
			case 39: a = memory[d].Rotr(); MemoryReads++; break; // rotr [d]
			case 40: d = memory[d]; MemoryReads++; break; // mov d, [d]
			case 62: a.Data = memory[d].Data = Word.TritwiseOp(a, memory[d]); MemoryReads++; MemoryWrites++; break; // crz
			case 68: /* nop */ break;
			case 81: return false; // end
			default: /* nop iff not the first instruction */ if (iteration == 0) return false; break;
		}
		c = (c.Value + 1) % MemorySize;
		d = (d.Value + 1) % MemorySize;
		iteration++;
		return true;
	}

	public ExecutionReport Execute()
	{
		for(int i=0; MaxIterations < 0 || iteration < MaxIterations; i++)
		{
			var single = ExecuteSingle();
			if (!single)
			{
				return new ExecutionReport(this)
				{
					RanToCompletion = true,
					Iterations = iteration,
				};
			}
			//if (i % 1 == 0)
			//{
			//	var memHash = HashMemory();
			//	var output =
			//		 new string(OutputQueue.ToArray()).ReplaceLineEndings().Replace(Environment.NewLine, @"\n");
			//	//"[REMOVED]";
			//	Console.Write($"\riter: {i,-8} memory hash: {HashMemory(),-12} output: {output}");
			//}
		}
		return new ExecutionReport(this)
		{
			RanToCompletion = false,
			Iterations = iteration,
		};
	}
}

public struct ExecutionReport
{
	public ExecutionReport(VirtualMachine vm)
	{
		Result = new string(vm.OutputQueue.ToArray());
		Program = vm.InitialProgram;
		MemoryReads = vm.MemoryReads;
		MemoryWrites = vm.MemoryWrites;
	}
	public bool RanToCompletion;
	public int Iterations;
	public string Result;
	public string Program;
	public int MemoryReads, MemoryWrites;
}

public enum MalbolgeFlavor
{
	Specification,
	Implementation
}
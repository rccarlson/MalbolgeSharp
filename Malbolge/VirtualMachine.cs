using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malbolge;
/// <summary>
/// 
/// </summary>
/// <remarks>
/// Based on http://www.lscheffer.com/malbolge_spec.html
/// </remarks>
public class VirtualMachine
{
	private const int MemorySize = 59049;

	public VirtualMachine(MalbolgeFlavor flavor, string program)
	{
		this.Flavor = flavor;
		var programAsMemory = program.Where(c => !char.IsWhiteSpace(c)).Select(c => new Word(c)).ToArray();
		Array.Copy(programAsMemory, memory, programAsMemory.Length);
		for (int i = programAsMemory.Length; i < MemorySize; i++)
		{
			memory[i] = Word.TritwiseOp(memory[i-2], memory[i-1]);
		}
	}

	private readonly Word[] memory = new Word[MemorySize];
	public int HashMemory() => memory.Aggregate(0, (state, word) => HashCode.Combine(state, word.Value));

	/// <summary> accumulator </summary>
	internal Word a = new(0);
	/// <summary> code pointer </summary>
	internal Word c = new(0);
	/// <summary> data pointer </summary>
	internal Word d = new(0);

	public Queue<char> InputQueue { get; } = new Queue<char>();
	public Queue<char> OutputQueue { get; } = new Queue<char>();

	public MalbolgeFlavor Flavor { get; }

	public void Execute()
	{
		Dictionary<int, int> hitcount = new();
		for(int i=0; ; i++)
		{
			var instructionValue = (memory[c] + c) % 94;

			if(hitcount.TryGetValue(instructionValue, out int hits)) hitcount[instructionValue] = hits + 1;
			else hitcount[instructionValue] = 1;

			switch(instructionValue)
			{
				case 4: c = memory[d]; break; // jmp [d]

				case 5 when Flavor is MalbolgeFlavor.Specification:
				case 23 when Flavor is MalbolgeFlavor.Implementation:
					OutputQueue.Enqueue((char)(a % 256)); break; // out a

				case 5 when Flavor is MalbolgeFlavor.Implementation:
				case 23 when Flavor is MalbolgeFlavor.Specification:
					if (!InputQueue.TryDequeue(out char result))
					{
						a = new Word(new[] { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 }); // 59048
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
				case 39: memory[d].Rotr(); a = memory[d]; break; // rotr [d]
				case 40: d = memory[d]; break; // mov d, [d]
				case 62: a = memory[d] = Word.TritwiseOp(a, memory[d]); break; // crz
				case 68: /* nop */ break;
				case 81: return; // end
				default: /* nop iff not the first instruction */ if (i == 0) return; break;
			}
			c = (c.Value + 1) % MemorySize;
			d = (d.Value + 1) % MemorySize;
			if(i%1 == 0) Console.Write($"\riter: {i,-12} hash: {HashMemory(),-12}");
		}
	}
}

public enum MalbolgeFlavor
{
	Specification,
	Implementation
}
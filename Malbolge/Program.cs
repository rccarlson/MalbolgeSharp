using Malbolge;
static void PrintQueue(Queue<char> queue) { while (queue.TryDequeue(out var output)) { Console.Write(output); } Console.WriteLine(); }
static void PopulateQueue(Queue<char> queue, string str) { foreach (var c in str) queue.Enqueue(c); }
static string SimpleExecute(MalbolgeFlavor flavor, string program)
{
	var vm = new VirtualMachine(flavor, program);
	vm.Execute();
	var result = new string(vm.OutputQueue.ToArray());
	Console.WriteLine(result);
	return result;
}


var a = new Word(256);
a.Rotl();

var helloWorldProgram = """
b'BA@?>=<;:987654321r`oo,llH('&%
ed"c~w|{z9'Z%utsrqponmlkjihgfedc
ba`_^]\[ZYXWVUTSRQPONMLKJIHGFEDC
BA@#>~~;|z8xwvuts10/.nm+*)i'&%fd
"ba`_^]yxwvXWsrqSonmPNjLKJIHGcba
`BA]\[=YXW:8T654321MLKJ,+GFE'CBA
$">~}|{zy7654ts10/o-,+lj(hgfedc!
~}|^]yxwYutsVTpRQPONMihgfHGcbaC_
^]@>Z<;:987SRQP21MLK-IHG*(D&%$#"
!=<;:zy765u321r/.-,+*)iX&%$dS!~}
|{zy\wvutsUDConmlkjihgfedcFa`B1@
/[ZYXWVUTSRQPONM0K-zHGFEDCBA@?>=
<;{j87x543sb0/.-,+*)('&%$#"!b`O{
zyxZIutsrqSBQ@lkjihgIIdcba`B1j
"""; // http://www2.latech.edu/~acm/helloworld/malbolge.html
SimpleExecute(MalbolgeFlavor.Implementation, helloWorldProgram);

var helloWorldProgram2 = """
(=<`#9]~6ZY327Uv4-QsqpMn&+Ij"'E%e{Ab~w=_:]Kw%o44Uqp0/Q?xNvL:`H%c#DD2^WV>gY;dts76qKJImZkj
"""; // https://gist.github.com/kspalaiologos/a1fe6913aaff8edea515b4af385368fe
SimpleExecute(MalbolgeFlavor.Specification, helloWorldProgram2);
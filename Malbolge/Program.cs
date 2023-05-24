using Malbolge;
void PrintQueue(Queue<char> queue) { while (queue.TryDequeue(out var output)) { Console.Write(output); } }
void PopulateQueue(Queue<char> queue, string str) { foreach (var c in str) queue.Enqueue(c); }


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
""";
var machine1 = new VirtualMachine(MalbolgeFlavor.Implementation, helloWorldProgram);
machine1.Execute();
Console.WriteLine();
PrintQueue(machine1.OutputQueue);
Console.WriteLine();
PrintQueue(machine2.OutputQueue);

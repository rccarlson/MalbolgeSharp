﻿using Malbolge;
static double Profile(int iterations, Action action)
{
	var sw = System.Diagnostics.Stopwatch.StartNew();
	for (int i = 0; i < iterations; i++)
		action();
	sw.Stop();
	return sw.Elapsed.TotalMicroseconds / iterations;
}

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
VirtualMachine.Execute(MalbolgeFlavor.Implementation, helloWorldProgram);

var helloWorldProgram2 = """
(=<`#9]~6ZY327Uv4-QsqpMn&+Ij"'E%e{Ab~w=_:]Kw%o44Uqp0/Q?xNvL:`H%c#DD2^WV>gY;dts76qKJImZkj
"""; // https://gist.github.com/kspalaiologos/a1fe6913aaff8edea515b4af385368fe
VirtualMachine.Execute(MalbolgeFlavor.Specification, helloWorldProgram2);

VirtualMachine.Execute(MalbolgeFlavor.Implementation, helloWorldProgram);

var time = Profile(1_000, () =>
{
	VirtualMachine.Execute(MalbolgeFlavor.Implementation, helloWorldProgram);
});
Console.WriteLine(time);
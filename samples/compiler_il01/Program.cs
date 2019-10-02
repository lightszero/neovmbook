using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.IO;

namespace compiler_il01
{
    class Program
    {
        static void Main(string[] args)
        {
            string srccode = @"
static class Program
{
    static int Main()
    {
        int a=1;
        int b=2;
        return a+b;
    }
}
";
            //step01 get a  c# dll;

            byte[] dll = GetILDLL(srccode);
            DumpILDLL(dll);

            //step02 compile AST->ASMLProject
            Neo.ASML.Node.ASMProject proj = Compiler.Compile(dll);
            proj.Dump((str) => Console.WriteLine(str));


            //step03 link ASML->machinecode
            var module = Neo.ASML.Linker.Linker.CreateModule(proj);
            module.Dump((str) => Console.WriteLine(str));
            var machinecode = Neo.ASML.Linker.Linker.Link(module);
            DumpAVM(machinecode);


            //run machinecode with neovm
            var engine = new Neo.VM.ExecutionEngine();
            engine.LoadScript(machinecode);
            engine.Execute();

            //show result
            var calcstack = engine.ResultStack;

            var v = calcstack.Peek();
            Console.WriteLine("retvalue=" + v.GetBigInteger());
        }
        static byte[] GetILDLL(string srccode, bool Optimization = false)
        {
            byte[] dll = null;
            byte[] pdb = null;
            var ast = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(srccode);
            var op = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            if (Optimization)
                op = op.WithOptimizationLevel(OptimizationLevel.Release);
            var comp = CSharpCompilation.Create("aaa.dll", new[] { ast }, new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            }, op);
            using (var streamDll = new MemoryStream())
            {
                using (var streamPdb = new MemoryStream())
                {
                    var result = comp.Emit(streamDll, streamPdb);
                    if (!result.Success)
                    {
                        foreach (var d in result.Diagnostics)
                        {
                            Console.WriteLine(d.ToString());
                        }
                        throw new Exception("build error.");
                    }
                    dll = streamDll.ToArray();
                    pdb = streamPdb.ToArray();
                    return dll;
                }
            }

        }
        static void DumpAVM(byte[] bytes)
        {
            Console.WriteLine("==Dump AVM:");

            string outstr = "";
            foreach (var b in bytes)
            {
                outstr += b.ToString("X02");
            }
            Console.WriteLine(outstr);

        }
        static void DumpILDLL(byte[] bytes)
        {
            Console.WriteLine("==DUMP IL");
            using (var ms = new System.IO.MemoryStream(bytes))
            {
                var md = Mono.Cecil.ModuleDefinition.ReadModule(ms);
                foreach (var t in md.Types)
                {
                    Console.WriteLine("class " + t.Name);
                    Console.WriteLine("{");
                    foreach (var m in t.Methods)
                    {
                        Console.WriteLine("    method:" + m.Name);
                        Console.WriteLine("    {");
                        Console.WriteLine("        VariablesTable Count=" + m.Body.Variables.Count);
                        foreach(var c in m.Body.Variables)
                        {
                            Console.WriteLine("         v=" + c.VariableType + "[" + c.Index + "]");
                        }
                        foreach (var o in m.Body.Instructions)
                        {
                            Console.WriteLine("        IL_" + o.Offset.ToString("x04") + ":" + o.OpCode + " : " + o.Operand);
                        }
                        Console.WriteLine("    }");

                    }
                    Console.WriteLine("}");

                }

            }
        }
    }
}

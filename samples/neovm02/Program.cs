using System;
using System.Collections.Generic;

namespace neovm01
{
    class Program
    {
        public interface ISyntaxNode
        {
            ISyntaxNode[] subnodes
            {
                get;
            }
        }
        class SyntaxNode_Add : ISyntaxNode
        {
            public ISyntaxNode[] subnodes
            {
                get
                {
                    return new ISyntaxNode[] { left, right };
                }
            }
            public ISyntaxNode left;
            public ISyntaxNode right;
            public override string ToString()
            {
                return "[+]";
            }
        }
        class SyntaxNode_Num : ISyntaxNode
        {
            public ISyntaxNode[] subnodes => null;
            public uint num;
            public override string ToString()
            {
                return "[" + num + "]";
            }
        }

        public static ISyntaxNode ParseSyntaxNode(string srccode)
        {
            var words = srccode.Split('+');
            return ParseWords(new ArraySegment<string>(words, 0, words.Length));
        }
        static ISyntaxNode ParseWords(ArraySegment<string> words)
        {
            if (words.Count == 1)
            {
                return new SyntaxNode_Num { num = uint.Parse(words[0]) };
            }
            else
            {
                var right = ParseWords(words.Slice(words.Count - 1, 1));
                var left = ParseWords(words.Slice(0, words.Count - 1));
                var add = new SyntaxNode_Add() { left = left, right = right };
                return add;
            }

        }
        static void DumpAST(ISyntaxNode node,string space="")
        {            
            Console.WriteLine(space + node.ToString());
            if (node.subnodes != null)
            {
                for(var i=0;i<node.subnodes.Length;i++)
                {
                  
                    var subspace = "";
                    if (space == "")
                        subspace = "├─ ";
                    else
                        subspace = "│   " + space;

                    if (i == node.subnodes.Length - 1)
                        subspace = subspace.Replace("├─ ", "└─ ");

                    DumpAST(node.subnodes[i], subspace);
                }
            }
        }
        static void EmitCode(Neo.VM.ScriptBuilder builder,ISyntaxNode ast)
        {
            if(ast is SyntaxNode_Num)
            {
                builder.EmitPush((ast as SyntaxNode_Num).num);
            }
            else if(ast is SyntaxNode_Add)
            {
                foreach(var sn in ast.subnodes)
                {
                    EmitCode(builder, sn);
                }
                builder.Emit(Neo.VM.OpCode.ADD);
            }
        }

        static void Main(string[] args)
        {

            var srccode = " 1 + 2 + 4 + 5";
            var ast = ParseSyntaxNode(srccode);
            DumpAST(ast);


            Neo.VM.ScriptBuilder builder = new Neo.VM.ScriptBuilder();
            EmitCode(builder, ast);
            builder.Emit(Neo.VM.OpCode.RET);

            var machinecode = builder.ToArray();
            var hexstr = "0x";
            foreach (var m in machinecode)
            {
                hexstr += m.ToString("X02");
            }
            Console.WriteLine("machinecode=" + hexstr);

            //run machinecode with neovm
            var engine = new Neo.VM.ExecutionEngine();
            engine.LoadScript(machinecode);
            engine.Execute();

            //show result
            var calcstack = engine.ResultStack;

            var v = calcstack.Peek();
            Console.WriteLine("retvalue=" + v.GetBigInteger());

            Console.ReadLine();

        }
    }
}

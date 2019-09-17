using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;

namespace neovm01
{
    class Program
    {
        class Scanner
        {
            public static List<string> SplitWords(string srccode)
            {
                string numbers = "1234567890";
                string ops = "+-*/()";

                List<string> words = new List<string>();
                string tempnum = "";
                bool lastop = true;
                for (var i = 0; i < srccode.Length; i++)
                {
                    var c = srccode[i];
                    if (ops.Contains(c))
                    {

                        if (lastop)
                        {
                            tempnum += c;
                        }
                        else
                        {
                            words.Add(tempnum);
                            tempnum = null;

                            words.Add(c.ToString());
                            lastop = true;
                        }
                    }
                    else if(numbers.Contains(c))
                    {
                        tempnum += c;
                        lastop = false;
                    }
                }
                if (tempnum != null)
                    words.Add(tempnum);

                return words;
            }
        }
        class SyntaxNode
        {
            public SyntaxNode(Type type,int value)
            {
                this.type = type;
                this.value = value;
            }
            public enum Type
            {
                Number,
                Math,
            }
            public Type type;
            public int value;

            public List<SyntaxNode> subnodes;
            public override string ToString()
            {
                if (type == Type.Number)
                    return "<num>" + value;
                if (type == Type.Math)
                {
                    switch (value)
                    {
                        case 1:
                            return "<math>+";
                        case 2:
                            return "<math>-";
                    }

                }
                throw new Exception("error format");
            }
            public static SyntaxNode Parse(string srccode)
            {
                var words = Scanner.SplitWords(srccode);
                if (words.Count == 1)
                    return new SyntaxNode(Type.Number,int.Parse(words[0]));
                {

                }
                return null;
            }
        }
        static void DumpAST(SyntaxNode node, int layer = 0)
        {
            var space = "";
            for (var i = 0; i < layer; i++)
                space += "   ";
            Console.WriteLine(space + node.ToString());
            if (node.subnodes != null)
            {
                foreach (var subnode in node.subnodes)
                {
                    DumpAST(subnode, layer + 1);
                }
            }
        }


        static void Main(string[] args)
        {

            var srccode = " -41 + -2 ";
            var ast = SyntaxNode.Parse(srccode);
            DumpAST(ast);


            Neo.VM.ScriptBuilder builder = new Neo.VM.ScriptBuilder();

            builder.Emit(Neo.VM.OpCode.NOP);
            builder.EmitPush(1);
            builder.EmitPush(2);
            builder.Emit(Neo.VM.OpCode.ADD);
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

using System;
using System.Collections.Generic;

namespace compiler_csharp01
{
    class Program
    {
        static void Main(string[] args)
        {
            string srccode = @"
class Program
{
    static void Main()
    {
        int a=1;
        int b=2;
        return a+b;
    }
}
";
            //step01 srccode -> AST
            var ast = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(srccode);
            var root = ast.GetRoot();
            DumpAst(root);

            //step02 compile AST->ASMLProject
            Neo.ASML.Node.ASMProject proj = Compiler.Compile(root);

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

        static void DumpAVM(byte[] bytes)
        {
            Console.WriteLine("==Dump AVM:");

            string outstr = "";
            foreach(var b in bytes)
            {
                outstr += b.ToString("X02");
            }
            Console.WriteLine(outstr);

        }
        static void DumpAst(Microsoft.CodeAnalysis.SyntaxNode node)
        {
            Console.WriteLine("==Dump AST:");

            DumpAstNode(node, "");
        }

        static void DumpAstNode(Microsoft.CodeAnalysis.SyntaxNode node, string space)
        {

            if (node is Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax)
            {

            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax)
            {
                var cl = node as Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax;

                var outtxt = space + "<class define>:" + cl.Identifier.ValueText;
                Console.WriteLine(outtxt);
            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.PredefinedTypeSyntax)
            {
                var pt = node as Microsoft.CodeAnalysis.CSharp.Syntax.PredefinedTypeSyntax;
                var outtxt = space + "<type>:" + pt.Keyword.ValueText;
                Console.WriteLine(outtxt);

            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax)
            {
                var vd = node as Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax;
                var outtxt = space + "<variable>:" + vd.Identifier.ValueText;
                Console.WriteLine(outtxt);
            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.EqualsValueClauseSyntax)
            {
                var eq = node as Microsoft.CodeAnalysis.CSharp.Syntax.EqualsValueClauseSyntax;
                var outtxt = space + "<Equals>:" + eq.EqualsToken.ValueText;
                Console.WriteLine(outtxt);
            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax)
            {
                var lt = node as Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax;
                var outtxt = space + "<value>:" + lt.Token.ValueText;
                Console.WriteLine(outtxt);

            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax)
            {
                var define = (node as Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax).Declaration;
                var subv = define.Variables;
                Console.WriteLine(space + "<local value>");
                foreach (var sv in subv)
                {
                    DumpAstNode(sv, "    " + space);
                }
                return;
            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.BinaryExpressionSyntax)
            {
                var bin = node as Microsoft.CodeAnalysis.CSharp.Syntax.BinaryExpressionSyntax;
                var outtxt = space + "<binary>:" + bin.OperatorToken.ValueText;
                Console.WriteLine(outtxt);
            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax)
            {
                var id = node as Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax;
                var outtxt = space + "<id>:" + id.Identifier.ValueText;
                Console.WriteLine(outtxt);
            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.ReturnStatementSyntax)
            {
                Console.WriteLine(space + "<return>");
            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)
            {
                var me = node as Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax;

                var outtxt = space + "<method define>:" + me.Identifier.ValueText;
                Console.WriteLine(outtxt);

                var subbody = me.Body.ChildNodes();
                foreach (var sn in subbody)
                {
                    DumpAstNode(sn, "    " + space);
                }
                return;
            }
            else
            {
                var outtxt = space + "node=" + node.GetType().ToString();
                Console.WriteLine(outtxt);
            }
            var subnode = node.ChildNodes();
            foreach (var sn in subnode)
            {
                DumpAstNode(sn, "    " + space);
            }
        }
    }
}

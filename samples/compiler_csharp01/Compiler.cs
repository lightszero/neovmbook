using System;
using System.Collections.Generic;
using System.Text;

namespace compiler_csharp01
{
    class Compiler
    {

        public static Neo.ASML.Node.ASMProject Compile(Microsoft.CodeAnalysis.SyntaxNode node)
        {
            var proj = new Neo.ASML.Node.ASMProject();
            CompileContext context = new CompileContext();
            CompileNode(context, proj, node);
            return proj;
        }
        class CompileContext
        {
            public List<string> variables = new List<string>();
        }
        static void CompileExpression(CompileContext context, Neo.ASML.Node.ASMFunction func, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, string comment = null)
        {
            if (expression is Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax)
            {
                var lit = expression as Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax;
                var v = lit.Token.ValueText;
                func.nodes.Add(new Neo.ASML.Node.ASMInstruction() { opcode = Neo.ASML.Node.ASMOpCode.CreatePush(), valuetext = v, commentRight = comment });
                return;
            }
            else if (expression is Microsoft.CodeAnalysis.CSharp.Syntax.BinaryExpressionSyntax)
            {
                var binary = expression as Microsoft.CodeAnalysis.CSharp.Syntax.BinaryExpressionSyntax;
                if (comment != null)
                    func.nodes.Add(new Neo.ASML.Node.ASMComment() { text = comment });

                CompileExpression(context, func, binary.Left);
                CompileExpression(context, func, binary.Right);

                Neo.VM.OpCode opcode = Neo.VM.OpCode.NOP;
                if (binary.OperatorToken.ValueText == "+")
                    opcode = Neo.VM.OpCode.ADD;
                else if (binary.OperatorToken.ValueText == "-")
                    opcode = Neo.VM.OpCode.DEC;
                if (binary.OperatorToken.ValueText == "*")
                    opcode = Neo.VM.OpCode.MUL;
                if (binary.OperatorToken.ValueText == "/")
                    opcode = Neo.VM.OpCode.DIV;

                func.nodes.Add(new Neo.ASML.Node.ASMInstruction() { opcode = Neo.ASML.Node.ASMOpCode.Create(opcode) });

            }
            else if (expression is Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax)
            {
                var id = expression as Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax;
                var varname = id.Identifier.ValueText;
                var varindex = context.variables.IndexOf(varname);
                if (varindex >= 0)
                {
                    //this is a LDLoc op
                    func.nodes.Add(new Neo.ASML.Node.ASMComment() { text = "//" + varname });
                    //push setitem
                    func.nodes.Add(new Neo.ASML.Node.ASMInstruction() { opcode = Neo.ASML.Node.ASMOpCode.Create(Neo.VM.OpCode.DUPFROMALTSTACK), commentRight = "//variables array" });
                    func.nodes.Add(new Neo.ASML.Node.ASMInstruction() { opcode = Neo.ASML.Node.ASMOpCode.CreatePush(), valuetext = varindex.ToString(), commentRight = "//index" });
                    func.nodes.Add(new Neo.ASML.Node.ASMInstruction() { opcode = Neo.ASML.Node.ASMOpCode.Create(Neo.VM.OpCode.PICKITEM) });

                }

            }
            else
            {
                var type = expression.GetType().ToString();
                throw new Exception("not support type:" + type);
            }
        }
        static void CompileNode(CompileContext context, Neo.ASML.Node.ASMProject project, Microsoft.CodeAnalysis.SyntaxNode node)
        {
            if (node is Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)
            {
                var func = new Neo.ASML.Node.ASMFunction();
                var srcmethod = node as Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax;

                func.Name = srcmethod.Identifier.ValueText;
                project.nodes.Add(func);

                context.variables = new List<string>();
                foreach (var op in srcmethod.Body.Statements)
                {
                    if (op is Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax)
                    {
                        var localvar = op as Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax;
                        var vars = localvar.Declaration.Variables;
                        foreach (var _var in vars)
                        {
                            context.variables.Add(_var.Identifier.ValueText);
                            var index = context.variables.IndexOf(_var.Identifier.ValueText);
                            if (_var.Initializer != null)
                            {
                                var v = _var.Initializer.Value.ToString();
                                func.nodes.Add(new Neo.ASML.Node.ASMComment() { text = "//" + _var.ToString() });
                                //push setitem
                                func.nodes.Add(new Neo.ASML.Node.ASMInstruction() { opcode = Neo.ASML.Node.ASMOpCode.Create(Neo.VM.OpCode.DUPFROMALTSTACK), commentRight = "//variables array" });
                                func.nodes.Add(new Neo.ASML.Node.ASMInstruction() { opcode = Neo.ASML.Node.ASMOpCode.CreatePush(), valuetext = index.ToString(), commentRight = "//index" });
                                //push value
                                CompileExpression(context, func, _var.Initializer.Value, "//value");
                                func.nodes.Add(new Neo.ASML.Node.ASMInstruction() { opcode = Neo.ASML.Node.ASMOpCode.Create(Neo.VM.OpCode.SETITEM) });
                            }
                        }
                        //define a local value

                    }
                    if (op is Microsoft.CodeAnalysis.CSharp.Syntax.ReturnStatementSyntax)
                    {
                        var ret = op as Microsoft.CodeAnalysis.CSharp.Syntax.ReturnStatementSyntax;
                        if (ret.Expression != null)
                        {
                            CompileExpression(context, func, ret.Expression, "//" + ret.ToString());
                        }
                        func.nodes.Add(new Neo.ASML.Node.ASMComment() { text = "//clear and return" });

                        func.nodes.Add(new Neo.ASML.Node.ASMInstruction() { opcode = Neo.ASML.Node.ASMOpCode.Create(Neo.VM.OpCode.FROMALTSTACK) });
                        func.nodes.Add(new Neo.ASML.Node.ASMInstruction() { opcode = Neo.ASML.Node.ASMOpCode.Create(Neo.VM.OpCode.DROP) });
                        func.nodes.Add(new Neo.ASML.Node.ASMInstruction() { opcode = Neo.ASML.Node.ASMOpCode.Create(Neo.VM.OpCode.RET) });
                    }
                }

                var variablecount = context.variables.Count;

                func.nodes.Insert(0, new Neo.ASML.Node.ASMInstruction() { opcode = Neo.ASML.Node.ASMOpCode.CreatePush(), valuetext = variablecount.ToString(), commentRight = "//insert varlist code" });
                func.nodes.Insert(1, new Neo.ASML.Node.ASMInstruction() { opcode = Neo.ASML.Node.ASMOpCode.Create(Neo.VM.OpCode.NEWARRAY) });
                func.nodes.Insert(2, new Neo.ASML.Node.ASMInstruction() { opcode = Neo.ASML.Node.ASMOpCode.Create(Neo.VM.OpCode.TOALTSTACK) });
            }
            else
            {
                var subnodes = node.ChildNodes();
                foreach (var sn in subnodes)
                {
                    CompileNode(context, project, sn);
                }
            }
        }
    }
}

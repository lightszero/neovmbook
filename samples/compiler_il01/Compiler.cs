using System;
using System.Collections.Generic;
using System.Text;

namespace compiler_il01
{
    class Compiler
    {
        public static Neo.ASML.Node.ASMProject Compile(byte[] dll)
        {
            var proj = new Neo.ASML.Node.ASMProject();
            CompileContext context = new CompileContext();
            context.OpenDLL(dll);
            foreach (var t in context.module.Types)
            {
                foreach (var m in t.Methods)
                {
                    CompileMethod(context, proj, m);
                }
            }
            return proj;

        }
        class CompileContext
        {
            public System.IO.MemoryStream ms;
            public Mono.Cecil.ModuleDefinition module;
            public void OpenDLL(byte[] bytes)
            {
                ms = new System.IO.MemoryStream(bytes);
                module = Mono.Cecil.ModuleDefinition.ReadModule(ms);
            }
        }
        static void CompileMethod(CompileContext context, Neo.ASML.Node.ASMProject proj, Mono.Cecil.MethodDefinition method)
        {
            var func = new Neo.ASML.Node.ASMFunction();
            func.Name = method.Name;

            proj.nodes.Add(func);

            //insert varibletable
            func.nodes.Insert(0, new Neo.ASML.Node.ASMInstruction() { opcode = Neo.ASML.Node.ASMOpCode.CreatePush(), valuetext = method.Body.Variables.Count.ToString(), commentRight = "//insert varlist code" });
            func.nodes.Insert(1, new Neo.ASML.Node.ASMInstruction() { opcode = Neo.ASML.Node.ASMOpCode.Create(Neo.VM.OpCode.NEWARRAY) });
            func.nodes.Insert(2, new Neo.ASML.Node.ASMInstruction() { opcode = Neo.ASML.Node.ASMOpCode.Create(Neo.VM.OpCode.TOALTSTACK) });

            foreach (var inst in method.Body.Instructions)
            {
                switch (inst.OpCode.Code)
                {
                    case Mono.Cecil.Cil.Code.Nop:
                        func.nodes.Add(new Neo.ASML.Node.ASMInstruction() { opcode = Neo.ASML.Node.ASMOpCode.Create(Neo.VM.OpCode.NOP) });
                        break;
                    case Mono.Cecil.Cil.Code.Ldc_I4_0:
                        PUSH(func, 0);
                        break;
                    case Mono.Cecil.Cil.Code.Ldc_I4_1:
                        PUSH(func, 1);
                        break;
                    case Mono.Cecil.Cil.Code.Ldc_I4_2:
                        PUSH(func, 2);
                        break;
                    case Mono.Cecil.Cil.Code.Ldloc_0:
                        LDLOC(func, 0);
                        break;
                    case Mono.Cecil.Cil.Code.Ldloc_1:
                        LDLOC(func, 1);
                        break;
                    case Mono.Cecil.Cil.Code.Ldloc_2:
                        LDLOC(func, 2);
                        break;
                    case Mono.Cecil.Cil.Code.Stloc_0:
                        STLOC(func, 0);
                        break;
                    case Mono.Cecil.Cil.Code.Stloc_1:
                        STLOC(func, 1);
                        break;
                    case Mono.Cecil.Cil.Code.Stloc_2:
                        STLOC(func, 2);
                        break;
                    case Mono.Cecil.Cil.Code.Add:
                        func.nodes.Add(new Neo.ASML.Node.ASMInstruction() { opcode = Neo.ASML.Node.ASMOpCode.Create(Neo.VM.OpCode.ADD) });
                        break;
                    case Mono.Cecil.Cil.Code.Br:
                    case Mono.Cecil.Cil.Code.Br_S:
                        continue;
                    case Mono.Cecil.Cil.Code.Ret:
                        func.nodes.Add(new Neo.ASML.Node.ASMInstruction() { opcode = Neo.ASML.Node.ASMOpCode.Create(Neo.VM.OpCode.FROMALTSTACK),commentRight="clear variblelist" });
                        func.nodes.Add(new Neo.ASML.Node.ASMInstruction() { opcode = Neo.ASML.Node.ASMOpCode.Create(Neo.VM.OpCode.DROP) });

                        func.nodes.Add(new Neo.ASML.Node.ASMInstruction() { opcode = Neo.ASML.Node.ASMOpCode.Create(Neo.VM.OpCode.RET) });

                        continue;
                    default:
                        throw new Exception("lost IL:" + inst.OpCode.Code);

                }
            }

        }

        private static void STLOC(Neo.ASML.Node.ASMFunction func, int index)
        {
            func.nodes.Add(new Neo.ASML.Node.ASMComment() { text = "//" + "STLOC " + index });
            //push setitem
            func.nodes.Add(new Neo.ASML.Node.ASMInstruction() { opcode = Neo.ASML.Node.ASMOpCode.Create(Neo.VM.OpCode.DUPFROMALTSTACK), commentRight = "//variables array" });
            func.nodes.Add(new Neo.ASML.Node.ASMInstruction() { opcode = Neo.ASML.Node.ASMOpCode.CreatePush(), valuetext = index.ToString(), commentRight = "//index" });
            //push value
            //value is before STLOC, move it to here.
            func.nodes.Add(new Neo.ASML.Node.ASMInstruction() { opcode = Neo.ASML.Node.ASMOpCode.CreatePush(), valuetext = "2", commentRight = "//move value" });
            func.nodes.Add(new Neo.ASML.Node.ASMInstruction() { opcode = Neo.ASML.Node.ASMOpCode.Create(Neo.VM.OpCode.ROLL) });

            func.nodes.Add(new Neo.ASML.Node.ASMInstruction() { opcode = Neo.ASML.Node.ASMOpCode.Create(Neo.VM.OpCode.SETITEM) });

        }
        private static void LDLOC(Neo.ASML.Node.ASMFunction func, int index)
        {
            func.nodes.Add(new Neo.ASML.Node.ASMComment() { text = "//" + "LDLOC " + index });

            func.nodes.Add(new Neo.ASML.Node.ASMInstruction() { opcode = Neo.ASML.Node.ASMOpCode.Create(Neo.VM.OpCode.DUPFROMALTSTACK), commentRight = "//variables array" });
            func.nodes.Add(new Neo.ASML.Node.ASMInstruction() { opcode = Neo.ASML.Node.ASMOpCode.CreatePush(), valuetext = index.ToString(), commentRight = "//index" });
            func.nodes.Add(new Neo.ASML.Node.ASMInstruction() { opcode = Neo.ASML.Node.ASMOpCode.Create(Neo.VM.OpCode.PICKITEM) });
        }

        private static void PUSH(Neo.ASML.Node.ASMFunction func, int num)
        {
            func.nodes.Add(new Neo.ASML.Node.ASMInstruction() { opcode = Neo.ASML.Node.ASMOpCode.CreatePush(), valuetext = num.ToString() });
        }
    }
}

using System;

namespace neovm01
{
    class Program
    {
        static void Main(string[] args)
        {
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

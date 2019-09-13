using System;
using System.Collections.Generic;

namespace turing01
{
    class Program
    {

        public class Head
        {
            public int pos;
        }
        public enum OP
        {
            NOP,
            PUSH,
            ADD,
            RET
        }
        public class OPCode
        {
            public OP op;
            public int v;
        }
        public class SimNeoTuringMachine
        {
            Head head;
            List<OPCode> tape;
            public bool stop
            {
                get;
                private set;
            }
            public Stack<int> calcstack
            {
                get;
                private set;
            }
            public void SetCodes(IList<OPCode> codes)
            {
                tape = new List<OPCode>(codes);
                head = new Head();
                head.pos = 0;
                calcstack = new Stack<int>();
                this.stop = false;
            }
            public void StepOne()
            {
                var code = tape[head.pos];
                switch (code.op)
                {
                    case OP.NOP:
                        break;
                    case OP.PUSH:
                        calcstack.Push(code.v);
                        break;
                    case OP.ADD:
                        {
                            var a = calcstack.Pop();
                            var b = calcstack.Pop();
                            var v = a + b;
                            calcstack.Push(v);
                            break;
                        }
                    case OP.RET:
                        this.stop = true;
                        return;
                }

                //scroll tape
                head.pos++;
            }
            public void Run()
            {
                while (!this.stop)
                {
                    StepOne();

                }
            }
        }
        static void Main(string[] args)
        {
            Console.WriteLine("use turing to calc 1+2");

            //prepare tape
            var codes = new OPCode[]{
                new OPCode(){op=OP.NOP},
                new OPCode(){op=OP.PUSH,v=1},
                new OPCode(){op=OP.PUSH,v=2},
                new OPCode(){op=OP.ADD},
                new OPCode(){op=OP.RET},
            };
            var vm = new SimNeoTuringMachine();
            vm.SetCodes(codes);

            //scroll the tape
            while (!vm.stop)
            {
                vm.StepOne();
            }

            //watch calcstack
            var retvalue = vm.calcstack.Peek();
            Console.WriteLine("return value = " + retvalue);

            Console.ReadLine();
        }
    }
}

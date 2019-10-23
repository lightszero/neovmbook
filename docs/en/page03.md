Reference source code location  
https://github.com/lightszero/neovmbook/tree/master/samples/neovm02

# Where does AVM come from - modularity and assembler

I’ve been talking about the assembler before, but in the process description, we didn’t seriously discuss this issue.

## Modularity

We know that NEOVM is an implementation of a Turing machine, but a Turing machine is a tape machine, and standard tape drives are not modular.

You think about the previous tape drive listening to music, can you simply jump to the next song with one click.

Organizing data by song is modularity. CDS are ok, tapes are not.

But the first important topic in software engineering practice is modularity.

High-level languages were, of course, modular, and functions were the most popular modular unit, and later classes came along with oop's popularity.

But long before high-level languages became popular, software engineers worked on modularity.

## How is modularity implemented in machine language

In the machine, there is only one instruction area, and the modularity in the instruction area is defined by the memory area. If you have studied the oldest Basic language, there is only one code file, and there is no function support, We use

```
goto [linenum]
```

this way to achieve modularity, so that different areas of the code can achieve different functions.

For neovm, we use JMP and CALL instructions to modularize the code.


## CALL instruction

Let's consider a piece of AVM code
```
    0x00    PUSH 1
    0x01    PUSH 2
    0x03    CALL +4
    0x06    RET
    0x07    ADD
    0x08    RET
```
In fact, it is divided into two modules.
    0x00 to 0x06 is Main module，0x07 to 0x08 is ADD module.

Without the aid of modularization tools, engineers must plan how to divide modules in memory by themselves, which is a very tedious work. Only with modularization can there be software engineering.


## Linker

Since modularity is so important, it is natural to have a modularity aid.

Now we have an assembler project.

https://github.com/neo-project/neo-vm/tree/Branch_neoasm/src/neo-asm

If we use our definition of ASML language with modularity it is

```
Main()
{
    PUSH 1//push 1 number
    PUSH 2
    CALL method1
    RET;
}
method1()
{
    ADD
    RET
}
```

Engineers think and write code module by module, regardless of which block of memory is which module.

The work of considering the relationship between modules and address translation is often referred to as Link.

The c++ language, for example, has a very explicitly separate Link process.

The CALL instruction is used for function-level modularity.

The JMP instruction is used for modularity inside functions.

Our assembler has the function of a Linker that automatically connects the two modules, assigns them the appropriate address segment, and makes the CALL instruction automatically point to the right place.

Now assembly, the next step is the high-level language, the process is the same, the only way.

The compiler's final job is address translation, which consists of assigning an address area to a module and providing the correct address to the CALL instruction to generate the final AVM byte[].

Since our assembler has modularity and Linker's work, the next step in explaining the compiler process is two parts.

High-level language -> AVML -> byte[]

Or other virtual machine intermediate languages like IL -> AVML -> byte[]

I won't go into details about how other compilers handle Linker's work.

## JMP instruction

Let's talk about the JMP instruction.

Think about code like this


```
int a=1;
if(a)
{
    //aaa
}
else
{
    //bbb
}

```

aaa and bbb are the two submodules inside the function.

If we don't have a modular representation, that's it, we still have to deal with addresses.

```
0x00 PUSH 1
0x01 JMPIF +3
0x02 PUSH 1
0x03 RET
0x04 PUSH 8
0x05 RET
```

If we use the modular ASML defined by us to express:

```
Main()
{
    PUSH 1
    JMPIF label1
    PUSH 1
    RET
label1:
    PUSH 8
    RET    
}
```

Never mind the address, a label has been introduced as a jump location.

In the process of switching from high-level language to assembly language, 80% of the work is the process of turning loops into JMP.

Ignore the address translation of the JMP and CALL instructions, which is left to Linker. In the next article, we will discuss how to compile high-level languages into NEOVM instructions.

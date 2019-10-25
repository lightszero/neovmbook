Reference source code location  
https://github.com/lightszero/neovmbook/tree/master/samples/compiler_csharp01

# Compile AVM - High-Level Language - Variables

We've already discussed address translation, so this article is only about how to convert high-level languages into AVM.

## How to compile variables

The first problem are the variables. In the high-level language, we are accustomed to variables. In the NEOVM instruction sequence, there is obviously no variable.

```
int a=0;
```

So such a high-level language instruction, obviously cannot be converted.

```
   int a=1;
   int b=2;
   return a + b;
```

We need to think about what we have in NEOVM, we have a CalcStack and a AltStack,
The CalcStack is easy to use to calculate a simple evaluation expression, such as "1+2+3+4", but once the variable appears, it seems more complicated.


Constant computation is very easy to express with the CalcStackstack:
```
    const int a=0;
    const int b=2;
    return a+b;

    ==>

    PUSH 0
    PUSH 2
    ADD
    RET
```
The trouble with variables is that they can change, so they should come from a location, not a specific value.

For example, a list of variables, let's take a look at this program with the idea of a list of variables.

Suppose we have a global list of variables
```
    //we have a List<int> values;
    int a=1; //a is values[0]
    //values[0] = 1;
    int b=2; //b is values[1]
    //values[1] = 2;
    return a + b;
    //return values[0]+values[1]
```

In fact, to compile this code, we need to create a variable list.
Let's first design two pseudocodes to operate our variable list.

STLOC  Put the values into the variable list.

LDLOC  Take the values from the variable list.

Using pseudocode to represent this program is:
```
    //int a=1
    PUSH 1
    STLOC 0
    //int b=2
    PUSH 2
    STLOC 1
    //return a+b
    LDLOC 0
    LDLOC 1
    ADD

    RET
```

Then we write the variable list directly in the form of code：

NEWARRAY  
PICKITEM  
SETITEM  

These operations are available to NEOVM. We create a variable list on the AltStack when the function is started, and remove the variable list when the instruction is RET.

```
    //CreateArray size=2
    PUSH 2
    NEWARRAY
    TOALTSTACK

    //int a=1
    DUPFROMALTSTACK //getarray
    PUSH 0//index
    PUSH 1//value
    SETITEM

    //int b=2
    DUPFROMALTSTACK //getarray
    PUSH 1//index
    PUSH 2//value
    SETITEM

    //get value a
    DUPFROMALTSTACK //getarray
    PUSH 0//index
    PICKITEM

    //get value b
    DUPFROMALTSTACK //getarray
    PUSH 1//index
    PICKITEM

    //add
    ADD

    //return
    //cleararray
    FROMALTSTACK
    DROP
    RET
```

You can find this program under samples/compiler_csharp01


![](../imgs/compiler06.png)

The code is divided into distinct parts.

Step01 is to interpret the C# source code as an abstract syntax tree (AST). Here we can call rosyln directly to solve this problem. Whatever high-level language you are going to compile, there are a lot of tools that you can use to interpret it as an AST.

Step02 is the part that compiles the AST into assembly
This part is the main job of the compiler.

Step03 is the job of the Linker, and it's always the same regardless of what you compile and from.
In the next article we will discuss the code that does the same thing from IL to AVM, where you will find that step03 is still the same code.

Then you test with NEOVM, and no doubt you'll get result 3.

```
class Program
{
    static void Main()
    {
        int a=1;
        int b=2;
        return a+b;
    }
}
//result 3
```

This is the output:

![](../imgs/compiler05.png)

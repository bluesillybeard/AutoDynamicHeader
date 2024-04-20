
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace AutoDynamicHeader;

public sealed class Program {
    public static void Main(string[] args) {
        // THE GOAL: Recreate what XDL does
        // weird #ifdef extern "C"{} #endif stuff to make compiling on C++ work correctly
        // typedef all function pointers
        // declarations of all functions pointers (so they are stored somewhere)
        // using #define to replace previous functions to use the new pointers
        // a function that loads all of the functions from the library

        if(args.Length < 3) {
            Console.WriteLine(@"
            Usage: autodynamicheader [source] [destination] [name of load function]
            The source and destination should be clear.
            The name of the load function is just what the function to load all the functions is called.");
            return;
        }

        // Load the source file as a big string
        var source = File.ReadAllText(args[0]);
        // Remove comments, because they tend to have things that look like functions
        // (My favorite "function" in pinc.h is "lower will(generally);")
        source = RemoveComments(source);

        // The only thing we care about is functions. All other syntax is useless.
        // We don't need an AST either. Each part of a function is a string.
        var functions = ParseFunctionDecls(source);
        Console.WriteLine($"Found functions: {string.Join("\n", functions)}");
    }

    static FunctionDecl[] ParseFunctionDecls(string source) {
        var bits = source.Split(';');
        var decls = new List<FunctionDecl>();
        foreach(var bit in bits) {
            var func = ParseFunction(bit);
            if(func.HasValue) {
                decls.Add(func.Value);
            }
        }
        return decls.ToArray();
    }

    static FunctionDecl? ParseFunction(string bit) {
        // try to figure out if this is a function declaration or not
        // [keywords][\w*][type][\w*][name][\w*]([arguments])[anything else I guess]
        // * is not part of the identifier but as far as we're concerned it is.
        var identifierChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890*_";
        var whitespaceChars = " \t\n";

        // Find the '(' in the declaration
        var argStart = bit.LastIndexOf('(');
        if(argStart == -1) return null;
        var argEnd = bit.LastIndexOf(')');
        if(argEnd == -1) return null;

        // Collect the args
        var args = bit[argStart..(argEnd+1)];
        if(args == null) return null;
        var nameBuilder = new StringBuilder();
        var typeBuilder = new StringBuilder();
        // State machine iterating everything before the args backwards
        // 0 -> whitespace before args
        // 1 -> name
        // 2 -> whitespace before name
        // 3 -> type
        int state = 0;
        for(var i = argStart-1; i>0; --i) {
            var character = bit[i];
            switch(state) {
                case 0:
                    if(whitespaceChars.Contains(character)) {
                        break;
                    }
                    if(identifierChars.Contains(character)) {
                        state = 1;
                        nameBuilder.Append(character);
                        break;
                    }
                    return null;
                case 1:
                    if(whitespaceChars.Contains(character)) {
                        state = 2;
                        break;
                    }
                    if(identifierChars.Contains(character)) {
                        nameBuilder.Append(character);
                        break;
                    }
                    return null;
                case 2:
                    if(whitespaceChars.Contains(character)) {
                        break;
                    }
                    if(identifierChars.Contains(character)) {
                        state = 3;
                        typeBuilder.Append(character);
                        break;
                    }
                    return null;
                case 3:
                    if(whitespaceChars.Contains(character)) {
                        state = 4;
                        break;
                    }
                    if(identifierChars.Contains(character)) {
                        typeBuilder.Append(character);
                        break;
                    }
                    return null;
                default:
                    // THIS is why goto is useful - breaking out of multiple things at once
                    goto BreakCharLoop;
            }
        }
        // Admittedly the goto could just be replaced with the return
        BreakCharLoop:
        // how the actual frick does String not have a reverse function?
        // I am concerningly frequently dissapointed by C#'s standard library
        // Thankfully I can convert it to a list and reverse that.
        var typeList = typeBuilder.ToString().ToList();
        typeList.Reverse();
        // then use string.join because reasons
        var typeString = string.Join(null, typeList);
        // I'm starting to think C# isn't actually that good at string processing,
        // Either that or I'm just dumb.
        var nameList = nameBuilder.ToString().ToList();
        nameList.Reverse();
        var nameString = string.Join(null, nameList);
        return new FunctionDecl(typeString, nameString, args);
    }
    static string RemoveComments(string source) {
        var b = new StringBuilder();
        // This is a state machine
        // 0 -> reading source code
        // 1 -> inside of a single line comment (post "//", pre '\n')
        // 2 -> inside of a multi line comment (post "/*, pre "*/")
        int state = 0;
        // Current index into the string.
        // Why the actual freaking heck is an index into an array a signed integer in C#?
        int index = 0;
        while(index < source.Length) {
            switch(state) {
                case 0:
                    if(source[index] == '/'){
                        // "//" -> move forward 2 and state is 1
                        if(index+1 < source.Length && source[index+1] == '/') {
                            index += 2;
                            state = 1;
                            break;
                        }
                        // "/*" -> move forward 2 and state is 2
                        if(index+1 < source.Length && source[index+1] == '*') {
                            index+=2;
                            state = 2;
                            break;
                        }
                    }
                    // Not a '/', this is code and should be added to the output
                    b.Append(source[index]);
                    ++index;
                    break;
                case 1:
                    // This single-line comment reached a newline, comment ends here
                    if(source[index] == '\n') {
                        state = 0;
                    }
                    ++index;
                    break;
                case 2:
                    // '*/ -> this might be the end of the comment
                    if(source[index] == '*') {
                        // "*/" -> comment ends
                        if(index+1 < source.Length && source[index+1] == '/') {
                            index += 2;
                            state = 0;
                            break;
                        }
                    }
                    // This was not the end of the comment, keep going
                    ++index;
                    break;
                default:
                    // This shouldn't happen, but keep going anyway
                    ++index;
                    break;
            }
        }
        return b.ToString();
    }
}

struct FunctionDecl {
    public FunctionDecl(string type, string nam, string arg) {
        returnType = type;
        name = nam;
        args = arg;
    }
    // The return type
    readonly string returnType;
    // the name of the function
    readonly string name;
    // the arguments of a function, parenthesis included
    readonly string args;
    // TODO: calling convention
    public override string ToString()
    {
        var b = new StringBuilder();
        b.Append("Return type: \"");
        b.Append(returnType);
        b.Append("\", ");
        b.Append("Name: \"");
        b.Append(name);
        b.Append("\", ");
        b.Append("Args: ");
        b.Append(args);
        b.Append(')');
        return b.ToString();
    }

    /// <summary>
    /// Returns the function as a C declaration
    /// </summary>
    public string ToDeclString() {
        var b = new StringBuilder();
        b.Append(returnType);
        b.Append(' ');
        b.Append(name);
        b.Append(args);
        b.Append(';');
        return b.ToString();
    }
}

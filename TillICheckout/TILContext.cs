using TillICheckout.Nodes;

namespace TillICheckout;

/// <summary>
/// Context container
/// </summary>
public class TILContext(
    Dictionary<string, TILType> DeclaredVariables
)
{
    public TILType this[string name]
    {
        get
        {
            if (DeclaredVariables != null) return DeclaredVariables[name];
            throw new ArgumentException($"Variable {{{name}}} not declared");
        }
    }

    public TILType GetType(IExpr expr) => expr.GetType(this);

    public void Declare(string name, TILType type)
    {
        DeclaredVariables[name] = TILType.Construction(type);
    }

    public void Declare(string sentence)
    {
        var parts = sentence.Split("/").Select(x => x.Trim()).ToArray();
        DeclaredVariables[parts[0]] = TILType.Construction(TILParser.ParseType(parts[1]));
    }

    public void Proof(string sentence)
    {
        IExpr expr;
        try
        {
            expr = TILParser.Parse(sentence);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Sentence {{{sentence}}}: cannot be parsed");
            throw e;
        }

        Console.WriteLine("====================================");
        foreach (var keyValuePair in DeclaredVariables)
        {
            Console.WriteLine($"{keyValuePair.Key} / {keyValuePair.Value}");
        }
        Console.WriteLine("------------------------------------");


        try
        {
            Console.WriteLine(expr.Visualize(this));
        }
        catch (Exception e)
        {

            Console.WriteLine($"Sentence {{{sentence}}}: cannot be proven");
            throw;
        }

    }


    public TILContext() : this(new Dictionary<string, TILType>())
    {
    }
}
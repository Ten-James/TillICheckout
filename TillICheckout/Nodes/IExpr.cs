namespace TillICheckout.Nodes;

public interface IExpr
{
    public string ToString();

    public string DebugString();

    public TILType GetType(TILContext context);

    public string Visualize(TILContext context);
}

public class Variable(
        string name
    ) : IExpr
{
    public TILType GetType(TILContext context)
    {
        return context[name];
    }

    public override string ToString()
    {
        return name;
    }

    public string DebugString()
    {
        return $"Variable {name}";
    }

    public string Visualize(TILContext context)
    {
        return $"{this}\n|\n{GetType(context)}";
    }
}

public class Trivialization(
    IExpr expr
) : IExpr
{
    public TILType GetType(TILContext context)
    {
        var type = context.GetType(expr);
        if (type.type is not TILTypeEnum.Construction || type.metadata is not TillTypeConstructionMetadata metadata)
            throw new ArgumentException();

        return metadata.ToTrivialization;
    }

    public override string ToString()
    {
        return $"'{expr}";
    }

    public string DebugString()
    {
        var cnt = expr.DebugString();
        return $"Trivialization\n  {cnt.Replace("\n", "\n  ")}";
    }

    public string Visualize(TILContext context)
    {
        return $"{this}\n|\n{GetType(context)}";
    }
}

public class Composition(
    IExpr function,
    IExpr[] arguments
): IExpr
{
    public TILType GetType(TILContext context)
    {
        var type = context.GetType(function);

        if (type.type is not TILTypeEnum.Function || type.metadata is not TillTypeFunctionMetadata metadata)
            throw new ArgumentException();

        var argtypes = arguments.Select(context.GetType).ToArray();

        if (argtypes.Length != metadata.Args.Length)
            throw new ArgumentException();

        var not_same = metadata.Args.Zip(argtypes).Select((a, _) => a.First == a.Second).Any(x => x == false);

        if (not_same)
            throw new ArgumentException();

        return metadata.Result;

    }

    public override string ToString()
    {
        return $"[{function} {string.Join(" ", arguments.Select(x => x.ToString()))}]";
    }

    public string DebugString()
    {
        var cnt = function.DebugString();
        var args_cnt = string.Join("\n  ",arguments.Select(x => x.DebugString().Replace("\n", "\n  ")));
        return $"Composition\n  {cnt.Replace("\n", "\n  ")}\n  {args_cnt}";
    }

    public string Visualize(TILContext context)
    {
        var left = new StringUtils.StringBox("[\n ");
        var right = new StringUtils.StringBox(function.Visualize(context));
        right.AddRow("|");
        left.Join(right);
        foreach (var argument in arguments)
        {
            left.AddColumn(" ");
            var inner = new StringUtils.StringBox(argument.Visualize(context));
            left.Join(inner);
        }
        left.AddRow(" " +GetType(context).ToString().PadRight(left[^1].TrimEnd().Length-2,'-') + "/");
        left.AddColumn("]", " ");
        return left.String();
    }
}

public class IntensionalDescent(
    IExpr expr,
    string name): IExpr
{
    public TILType GetType(TILContext context)
    {
        var type = context.GetType(expr);

        if (type.type is not TILTypeEnum.Function || type.metadata is not TillTypeFunctionMetadata metadata)
            throw new ArgumentException();

        if (metadata.Args.Length != 1)
            throw new ArgumentException();

        return (metadata.Result);
    }

    public override string ToString()
    {
        return expr is IntensionalDescent ? $"{expr},{name}" : $"{expr}@{name}";
    }

    public string DebugString()
    {
        var cnt = expr.DebugString();
        return $"IntensionalDescent\n  {cnt.Replace("\n", "\n  ")}";
    }

    public string Visualize(TILContext context)
    {
        var left = new StringUtils.StringBox(expr.Visualize(context));
        left.ResizeWidth(expr.ToString().Length);
        left.AddRow("|");
        left.AddColumn( expr is IntensionalDescent ? $",{name}" : $"@{name}"," |");
        var type = this.GetType(context).ToString();
        var missing = left.Width - 1 - type.Length;
        return $"{left.String()}\n{type}{"".PadRight(missing, '-')}/";

    }
}
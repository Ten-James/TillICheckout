namespace TillICheckout;

public enum TILTypeEnum
{
    Bool,
    Individual,
    Time,
    World,
    Construction,
    Function
}

public class TILTypeMetadata
{
}

public class TillTypeFunctionMetadata : TILTypeMetadata
{
    public required TILType Result;
    public required TILType[] Args;
}

public class TillTypeConstructionMetadata : TILTypeMetadata
{
    public required TILType ToTrivialization;
}

public record TILType(
    TILTypeEnum type,
    TILTypeMetadata metadata
)
{
    public override string ToString() =>
        this switch
        {
            (TILTypeEnum.Bool, _) => "o",
            (TILTypeEnum.Individual, _) => "i",
            (TILTypeEnum.Time, _) => "t",
            (TILTypeEnum.World, _) => "w",
            (TILTypeEnum.Construction, TillTypeConstructionMetadata metadata) => $"*n[{metadata.ToTrivialization}]",
            (TILTypeEnum.Function, TillTypeFunctionMetadata metadata) =>
                $"({metadata.Result}{string.Join("", metadata.Args.Select(x => x.ToString()))})",
            _ => throw new ArgumentException()
        };

    public virtual bool Equals(TILType? other)
    {
        return type == other.type && metadata switch
        {
            TillTypeConstructionMetadata constructionMetadata when
                other.metadata is TillTypeConstructionMetadata otherConstructionMetadata => constructionMetadata
                    .ToTrivialization.Equals(otherConstructionMetadata.ToTrivialization),
            TillTypeFunctionMetadata functionMetadata when other.metadata is TillTypeFunctionMetadata otherMetadata =>
                functionMetadata.Result.Equals(otherMetadata.Result) &&
                functionMetadata.Args.Length == otherMetadata.Args.Length && functionMetadata.Args
                    .Zip(otherMetadata.Args)
                    .Aggregate(true, (a, x) => a && x.First.Equals(x.Second)),
            _ => true
        };
    }

    public TILType AtTauOmega() => new TILType(
        TILTypeEnum.Function,
        new TillTypeFunctionMetadata
        {
            Result = new TILType(TILTypeEnum.Function,
                new TillTypeFunctionMetadata()
                {
                    Result = this,
                    Args = [Time()]
                }),
            Args = [World()]
        }
    );


    public static TILType Bool() => new TILType(TILTypeEnum.Bool, new TILTypeMetadata());
    public static TILType Individual() => new TILType(TILTypeEnum.Individual, new TILTypeMetadata());
    public static TILType Time() => new TILType(TILTypeEnum.Time, new TILTypeMetadata());
    public static TILType World() => new TILType(TILTypeEnum.World, new TILTypeMetadata());

    public static TILType Construction(
        TILType of
    ) => new TILType(TILTypeEnum.Construction, new TillTypeConstructionMetadata
    {
        ToTrivialization = of
    });

    public static TILType Function(
        TILType result,
        TILType[] args
    ) => new TILType(TILTypeEnum.Function, new TillTypeFunctionMetadata
    {
        Result = result,
        Args = args
    });
}
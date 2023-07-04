namespace NRete;

public interface ICondition
{
    ValueTask<ConditionResult> EvaluateAsync(CancellationToken cancellationToken = default);
}

public abstract class Condition : ICondition
{
    public abstract ValueTask<ConditionResult> EvaluateAsync(CancellationToken cancellationToken = default);

    protected ConditionResult Success(bool condition = true) => new(this, condition);

    protected ConditionResult Failure(bool condition = true) => new(this, !condition);
}

public record ConditionResult(ICondition Condition, bool IsPassed);

public interface IRule
{
    ValueTask<RuleResult> EvaluateAsync(CancellationToken cancellationToken = default);
}

public record RuleResult(IReadOnlyCollection<ConditionResult> ConditionResults, bool IsPassed);

public abstract class Rule : IRule
{
    private readonly IReadOnlyCollection<ICondition> _conditions;

    protected Rule(IEnumerable<ICondition> conditions)
    {
        _conditions = conditions.ToArray();
    }

    public async ValueTask<RuleResult> EvaluateAsync(CancellationToken cancellationToken = default)
    {
        var conditionResults = await EvaluateConditionsAsync(cancellationToken);
        return new RuleResult(conditionResults, IsPassed(conditionResults));
    }

    private async Task<IReadOnlyCollection<ConditionResult>> EvaluateConditionsAsync(CancellationToken cancellationToken)
    {
        var conditionResults = new List<ConditionResult>(_conditions.Count);
        foreach (var condition in _conditions)
        {
            var conditionResult = await condition.EvaluateAsync(cancellationToken);
            conditionResults.Add(conditionResult);
        }

        return conditionResults.ToArray();
    }

    protected abstract bool IsPassed(IReadOnlyCollection<ConditionResult> results);
}

public class BreakOnFailRule
{

}

public class BreakOnPassRule
{

}

public class EvaluaeAllFailOnAnyRule : Rule
{
    public EvaluaeAllFailOnAnyRule(IEnumerable<ICondition> conditions)
        : base(conditions)
    {
    }

    protected override bool IsPassed(IReadOnlyCollection<ConditionResult> results) => results.All(r => r.IsPassed);
}

public class EvaluaeAllPassOnAnyRule : Rule
{
    public EvaluaeAllPassOnAnyRule(IEnumerable<ICondition> conditions)
        : base(conditions)
    {
    }

    protected override bool IsPassed(IReadOnlyCollection<ConditionResult> results) => results.Any(r => r.IsPassed);
}


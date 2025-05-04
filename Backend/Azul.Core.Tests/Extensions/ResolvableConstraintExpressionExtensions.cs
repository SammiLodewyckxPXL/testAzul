using NUnit.Framework.Constraints;

namespace Azul.Core.Tests.Extensions;

public static class ResolvableConstraintExpressionExtensions
{
    public static Constraint ContainsOne(this ResolvableConstraintExpression expression, params string[] substrings)
    {
        return expression.Matches(new ContainsOneConstraint(substrings));
    }

    private class ContainsOneConstraint : Constraint
    {
        private readonly IEnumerable<string> _substrings;

        public ContainsOneConstraint(IEnumerable<string> substrings)
        {
            _substrings = substrings ?? throw new ArgumentNullException(nameof(substrings));
        }

        public override ConstraintResult ApplyTo<TActual>(TActual actual)
        {
            if (actual is string input)
            {
                foreach (string substring in _substrings)
                {
                    if (input.Contains(substring, StringComparison.OrdinalIgnoreCase))
                    {
                        return new ConstraintResult(this, actual, isSuccess: true);
                    }
                }
            }

            return new ConstraintResult(this, actual, isSuccess: false);
        }

        public override string Description => $"contains one of the substrings ({string.Join(", ", _substrings)})";
    }
}
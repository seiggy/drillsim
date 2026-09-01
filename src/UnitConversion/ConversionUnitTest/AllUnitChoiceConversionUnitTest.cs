using System.Globalization;
using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class AllUnitChoiceConversionUnitTest
    {
        private static readonly double[] SiSamples = [-123.456789, -0.125, 0.0, 0.125, 123.456789];

        public static IEnumerable<TestCaseData> AllUnitChoices()
        {
            foreach (BasePhysicalQuantity quantity in BasePhysicalQuantity.AvailableBasePhysicalQuantities)
            {
                foreach (UnitChoice choice in quantity.UnitChoices)
                {
                    yield return new TestCaseData(quantity.Name, choice)
                        .SetName($"{quantity.Name}: {choice.UnitName}");
                }
            }
        }

        [TestCaseSource(nameof(AllUnitChoices))]
        public void ConversionMatchesIndependentlyEvaluatedSymbolicDefinition(
            string quantityName,
            UnitChoice choice)
        {
            Assert.That(choice.ConversionFactorFromSIFormula, Is.Not.Null.And.Not.Empty,
                $"{quantityName}: {choice.UnitName} must define its conversion symbolically");

            double referenceFactor = SymbolicReferenceEvaluator.Evaluate(choice.ConversionFactorFromSIFormula!);
            double referenceBias = string.IsNullOrWhiteSpace(choice.ConversionBiasFromSIFormula)
                ? 0.0
                : SymbolicReferenceEvaluator.Evaluate(choice.ConversionBiasFromSIFormula);

            Assert.That(double.IsFinite(referenceFactor) && referenceFactor > 0.0, Is.True,
                $"{quantityName}: {choice.UnitName} reference factor");
            AssertClose(choice.ConversionFactorFromSI, referenceFactor,
                Math.Abs(referenceFactor), $"{quantityName}: {choice.UnitName} stored factor");
            AssertClose(choice.ConversionBiasFromSI, referenceBias,
                Math.Abs(referenceBias), $"{quantityName}: {choice.UnitName} stored bias");

            foreach (double siValue in SiSamples)
            {
                double referenceUnitValue = referenceFactor * siValue + referenceBias;
                double arithmeticScale = Math.Abs(referenceFactor * siValue) + Math.Abs(referenceBias);

                AssertClose(choice.FromSI(siValue), referenceUnitValue, arithmeticScale,
                    $"{quantityName}: {choice.UnitName}, FromSI({siValue:R})");
                AssertClose(choice.ToSI(referenceUnitValue), siValue, Math.Abs(siValue),
                    $"{quantityName}: {choice.UnitName}, ToSI({referenceUnitValue:R})");
            }
        }

        private static void AssertClose(double actual, double expected, double calculationScale, string context)
        {
            // A few ulps cover the different evaluation paths. The 2e-12 relative
            // allowance also accommodates constants explicitly classified as
            // approximate without making very small conversions pass absolutely.
            double scale = Math.Max(Math.Max(Math.Abs(expected), calculationScale), 1e-300);
            double tolerance = Math.Max(32.0 * Math.Abs(Math.BitIncrement(expected) - expected), 2e-12 * scale);
            Assert.That(actual, Is.EqualTo(expected).Within(tolerance), context);
        }

        /// <summary>
        /// Evaluates the restricted arithmetic language used by conversion formulas.
        /// Factor symbols are recursively expanded from their symbolic descriptions;
        /// the already-computed static Factor values are deliberately not used.
        /// </summary>
        private sealed class SymbolicReferenceEvaluator
        {
            private static readonly Dictionary<string, double> FactorCache = new(StringComparer.Ordinal);
            private static readonly HashSet<string> ActiveFactors = new(StringComparer.Ordinal);
            private readonly string expression_;
            private int position_;

            private SymbolicReferenceEvaluator(string expression)
            {
                expression_ = expression;
            }

            public static double Evaluate(string expression)
            {
                var parser = new SymbolicReferenceEvaluator(expression);
                double value = parser.ParseExpression();
                parser.SkipWhiteSpace();
                if (parser.position_ != expression.Length)
                {
                    throw new FormatException($"Unexpected conversion formula text at position {parser.position_}: {expression}");
                }
                return value;
            }

            private double ParseExpression()
            {
                double value = ParseTerm();
                while (true)
                {
                    if (Consume('+')) value += ParseTerm();
                    else if (Consume('-')) value -= ParseTerm();
                    else return value;
                }
            }

            private double ParseTerm()
            {
                double value = ParseUnary();
                while (true)
                {
                    if (Consume('*')) value *= ParseUnary();
                    else if (Consume('/')) value /= ParseUnary();
                    else return value;
                }
            }

            private double ParseUnary()
            {
                if (Consume('+')) return ParseUnary();
                if (Consume('-')) return -ParseUnary();
                return ParsePrimary();
            }

            private double ParsePrimary()
            {
                if (Consume('('))
                {
                    double value = ParseExpression();
                    Expect(')');
                    return value;
                }

                SkipWhiteSpace();
                if (position_ < expression_.Length &&
                    (char.IsDigit(expression_[position_]) || expression_[position_] == '.'))
                {
                    return ParseNumber();
                }

                string identifier = ParseIdentifier();
                if (identifier == "System.Math.PI") return Math.PI;
                if (identifier == "System.Math.Sqrt")
                {
                    Expect('(');
                    double value = ParseExpression();
                    Expect(')');
                    return Math.Sqrt(value);
                }
                if (identifier.StartsWith("Factors.", StringComparison.Ordinal))
                {
                    return ResolveFactor(identifier["Factors.".Length..]);
                }

                throw new FormatException($"Unsupported identifier '{identifier}' in conversion formula: {expression_}");
            }

            private static double ResolveFactor(string name)
            {
                lock (FactorCache)
                {
                    if (FactorCache.TryGetValue(name, out double value)) return value;
                    if (!Factors.Descriptions.TryGetValue(name, out FactorDescription description))
                    {
                        throw new KeyNotFoundException($"Conversion formula refers to unknown factor '{name}'.");
                    }
                    if (!ActiveFactors.Add(name))
                    {
                        throw new InvalidOperationException($"Circular symbolic factor definition involving '{name}'.");
                    }

                    try
                    {
                        value = Evaluate(description.Definition);
                        FactorCache.Add(name, value);
                        return value;
                    }
                    finally
                    {
                        ActiveFactors.Remove(name);
                    }
                }
            }

            private double ParseNumber()
            {
                SkipWhiteSpace();
                int start = position_;
                while (position_ < expression_.Length &&
                       (char.IsDigit(expression_[position_]) || expression_[position_] == '.'))
                {
                    position_++;
                }
                if (position_ < expression_.Length &&
                    (expression_[position_] == 'e' || expression_[position_] == 'E'))
                {
                    position_++;
                    if (position_ < expression_.Length &&
                        (expression_[position_] == '+' || expression_[position_] == '-')) position_++;
                    while (position_ < expression_.Length && char.IsDigit(expression_[position_])) position_++;
                }
                return double.Parse(expression_[start..position_], NumberStyles.Float, CultureInfo.InvariantCulture);
            }

            private string ParseIdentifier()
            {
                SkipWhiteSpace();
                int start = position_;
                while (position_ < expression_.Length &&
                       (char.IsLetterOrDigit(expression_[position_]) ||
                        expression_[position_] == '.' || expression_[position_] == '_'))
                {
                    position_++;
                }
                if (start == position_)
                {
                    throw new FormatException($"Expected a value at position {position_}: {expression_}");
                }
                return expression_[start..position_];
            }

            private bool Consume(char token)
            {
                SkipWhiteSpace();
                if (position_ >= expression_.Length || expression_[position_] != token) return false;
                position_++;
                return true;
            }

            private void Expect(char token)
            {
                if (!Consume(token))
                {
                    throw new FormatException($"Expected '{token}' at position {position_}: {expression_}");
                }
            }

            private void SkipWhiteSpace()
            {
                while (position_ < expression_.Length && char.IsWhiteSpace(expression_[position_])) position_++;
            }
        }
    }
}

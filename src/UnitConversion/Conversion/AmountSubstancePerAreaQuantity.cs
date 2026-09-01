using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents amount of substance per area.
    /// </summary>
    public partial class AmountSubstancePerAreaQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "n/A";
        public override string SIUnitName { get; } = "mole per square metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{mol}{m^2}";
        public override double AmountSubstanceDimension { get; } = 1;
        public override double LengthDimension { get; } = -2;

        private static AmountSubstancePerAreaQuantity instance_ = null;
        public static AmountSubstancePerAreaQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new AmountSubstancePerAreaQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "mole per square metre",
                UnitLabel = "mol/m^2",
                ID = new Guid("d146aedf-876f-4f5c-a766-378d77ebad03"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "millimole per square metre",
                UnitLabel = "mmol/m^2",
                ID = new Guid("126b36a2-550d-4eaf-9170-cf95bbf5e7f1"),
                ConversionFactorFromSIFormula = "1.0/Factors.Milli",
            }
        };

        public AmountSubstancePerAreaQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "amount of substance per area" };
            ID = new Guid("4d6cfb0d-27ce-42df-a46e-4555268608b8");
            DescriptionMD = "**amount of substance per area** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is mole per square metre with unit label $\\frac{mol}{m^2}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}

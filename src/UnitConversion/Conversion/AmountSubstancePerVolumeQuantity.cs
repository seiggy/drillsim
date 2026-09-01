using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents amount of substance per volume.
    /// </summary>
    public partial class AmountSubstancePerVolumeQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "c";
        public override string SIUnitName { get; } = "mole per cubic metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{mol}{m^3}";
        public override double AmountSubstanceDimension { get; } = 1;
        public override double LengthDimension { get; } = -3;

        private static AmountSubstancePerVolumeQuantity instance_ = null;
        public static AmountSubstancePerVolumeQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new AmountSubstancePerVolumeQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "mole per cubic metre",
                UnitLabel = "mol/m^3",
                ID = new Guid("fd710071-6493-47fa-84a3-565ea9284423"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "mole per litre",
                UnitLabel = "mol/L",
                ID = new Guid("2835bc29-d482-4737-9d6c-a2e90d40e748"),
                ConversionFactorFromSIFormula = "Factors.Litre",
            },
            new UnitChoice
            {
                UnitName = "millimole per litre",
                UnitLabel = "mmol/L",
                ID = new Guid("3229ec79-0e78-43a0-b725-76f2694338a7"),
                ConversionFactorFromSIFormula = "Factors.Litre/Factors.Milli",
            }
        };

        public AmountSubstancePerVolumeQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "amount of substance per volume", "amount concentration" };
            ID = new Guid("0ee126c6-c0e5-46cf-8427-b4d2eddf0d62");
            DescriptionMD = "**amount of substance per volume** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is mole per cubic metre with unit label $\\frac{mol}{m^3}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}

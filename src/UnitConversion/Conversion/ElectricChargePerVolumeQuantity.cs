using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents electric charge per volume.
    /// </summary>
    public partial class ElectricChargePerVolumeQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "rho_q";
        public override string SIUnitName { get; } = "coulomb per cubic metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{C}{m^3}";
        public override double LengthDimension { get; } = -3;
        public override double TimeDimension { get; } = 1;
        public override double ElectricCurrentDimension { get; } = 1;

        private static ElectricChargePerVolumeQuantity instance_ = null;
        public static ElectricChargePerVolumeQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ElectricChargePerVolumeQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "coulomb per cubic metre",
                UnitLabel = "C/m^3",
                ID = new Guid("eadd7d6b-71e9-4df2-8498-19ba03c2fd5b"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "microcoulomb per cubic metre",
                UnitLabel = "uC/m^3",
                ID = new Guid("0a112bca-5e8a-4e13-8bdc-63ee39085081"),
                ConversionFactorFromSIFormula = "1.0/Factors.Micro",
            },
            new UnitChoice
            {
                UnitName = "coulomb per litre",
                UnitLabel = "C/L",
                ID = new Guid("0cc2230a-9b24-471c-a0d8-21171aa78291"),
                ConversionFactorFromSIFormula = "Factors.Litre",
            }
        };

        public ElectricChargePerVolumeQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "electric charge per volume", "volume charge density" };
            ID = new Guid("f58d46c1-bef9-41df-9273-62b3894c0e27");
            DescriptionMD = "**electric charge per volume** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is coulomb per cubic metre with unit label $\\frac{C}{m^3}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}

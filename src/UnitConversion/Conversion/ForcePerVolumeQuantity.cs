using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents force per volume.
    /// </summary>
    public partial class ForcePerVolumeQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "f_V";
        public override string SIUnitName { get; } = "newton per cubic metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{N}{m^3}";
        public override double LengthDimension { get; } = -2;
        public override double TimeDimension { get; } = -2;
        public override double MassDimension { get; } = 1;

        private static ForcePerVolumeQuantity instance_ = null;
        public static ForcePerVolumeQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ForcePerVolumeQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "newton per cubic metre",
                UnitLabel = "N/m^3",
                ID = new Guid("130f8eaf-2abd-4da7-8c47-aa34aa0f1948"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "kilonewton per cubic metre",
                UnitLabel = "kN/m^3",
                ID = new Guid("ee9d7c2e-f962-452c-ab1e-664c44cbf144"),
                ConversionFactorFromSIFormula = "1.0/Factors.Kilo",
            },
            new UnitChoice
            {
                UnitName = "pound force per cubic foot",
                UnitLabel = "lbf/ft^3",
                ID = new Guid("ff272cc7-1157-440d-869b-c3183dbc2c90"),
                ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot*Factors.Foot/Factors.PoundForce",
            }
        };

        public ForcePerVolumeQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "force per volume", "body force density" };
            ID = new Guid("595b57c6-2b90-4090-a9ee-0f3a1481edf4");
            DescriptionMD = "**force per volume** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is newton per cubic metre with unit label $\\frac{N}{m^3}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}

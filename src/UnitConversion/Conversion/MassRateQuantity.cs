using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class MassRateQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "kilogram per second";
        public override string SIUnitLabelLatex { get; } = "\\frac{kg}{s}";
        public override double MassDimension { get; } = 1;
        public override double TimeDimension { get; } = -1;
        private static MassRateQuantity instance_ = null;

        public static MassRateQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MassRateQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
        new UnitChoice
        {
          UnitName = "kilogram per second",
          UnitLabel = "kg/s",
          ID = new Guid("a2daceb8-7705-4c97-9945-b354ea1ff78d"),
          ConversionFactorFromSIFormula = "1.0/Factors.Unit",
          IsSI = true
        },
        new UnitChoice
        {
          UnitName = "kilogram per minute",
          UnitLabel = "kg/min",
          ID = new Guid("b776ae6f-5b86-462c-b815-2608d7e98192"),
          ConversionFactorFromSIFormula = "Factors.Minute/Factors.Unit"
        },
        new UnitChoice
        {
          UnitName = "kilogram per hour",
          UnitLabel = "kg/h",
          ID = new Guid("736e4fcd-434f-4442-b025-a480a1532543"),
          ConversionFactorFromSIFormula = "Factors.Hour/Factors.Unit"
        },
        new UnitChoice
        {
          UnitName = "kilogram per year",
          UnitLabel = "kg/y",
          ID = new Guid("0ce50feb-a755-4a62-a50b-4af417bc2702"),
          ConversionFactorFromSIFormula = "Factors.YearJulian/Factors.Unit"
        },
        new UnitChoice
        {
          UnitName = "pound per second",
          UnitLabel = "lb/s",
          ID = new Guid("48ac7515-ce4e-4ed6-a198-fe3ed3451a38"),
          ConversionFactorFromSIFormula = "Factors.Unit/Factors.Pound"
        },
        new UnitChoice
        {
          UnitName = "pound per minute",
          UnitLabel = "lb/min",
          ID = new Guid("92d18443-9357-42cf-86d2-fa78996c838a"),
          ConversionFactorFromSIFormula = "Factors.Minute/Factors.Pound"
        },
        new UnitChoice
        {
          UnitName = "pound per hour",
          UnitLabel = "lb/h",
          ID = new Guid("d4e0791c-eb4c-47a4-9e71-af3ad1b707cc"),
          ConversionFactorFromSIFormula = "Factors.Hour/Factors.Pound"
        },
        new UnitChoice
        {
          UnitName = "pound per year",
          UnitLabel = "lb/y",
          ID = new Guid("a461e40b-48ea-49b1-8a55-8e75b26fbb8e"),
          ConversionFactorFromSIFormula = "Factors.YearJulian/Factors.Pound"
        }
        };

        public MassRateQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "mass rate" };
            ID = new Guid("3dd05c4c-3d6d-49ae-a878-5a5e4a6e7acf");
            DescriptionMD = string.Empty;
            DescriptionMD += @"A mass rate is the time derivative of a mass: $\frac{dm}{dt}$, where $m$ is a mass and $t$ is time." + Environment.NewLine;
            DescriptionMD += @"The dimension of mass rate is:" + Environment.NewLine;
            DescriptionMD += "$" + GetDimensionsEnclosed() + "$." + Environment.NewLine;
            if (!string.IsNullOrEmpty(SIUnitLabelLatex) && !string.IsNullOrEmpty(SIUnitName) && UsualNames != null && UsualNames.Count > 0)
            {
                DescriptionMD += Environment.NewLine;
                DescriptionMD += @"The SI unit for **" + UsualNames.First() + "** is: " + SIUnitName + " with the associated unit label $" + SIUnitLabelLatex + "$" + Environment.NewLine;
            }
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}

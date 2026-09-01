using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class CurvatureQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "κ";
        public override string SIUnitName { get; } = "radian per metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{rad}{m}";
        public override double LengthDimension { get; } = -1;
        public override double PlaneAngleDimension { get; } = 1;
        private static CurvatureQuantity instance_ = null;

        public static CurvatureQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new CurvatureQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
        {
          UnitName = "radian per metre",
          UnitLabel = "rad/m",
          ID = new Guid("cbba2f64-9914-4c00-a3fe-3c2aef560225"),
          ConversionFactorFromSIFormula = "1.0/Factors.Unit",
          IsSI = true
        },
        new UnitChoice
        {
          UnitName = "degree per 10m",
          UnitLabel = "°/10m",
          ID = new Guid("c62408d6-c3c5-47bd-8cb0-4fa9faf51598"),
          ConversionFactorFromSIFormula = "Factors.Degree*10.0"
        },
        new UnitChoice
        {
          UnitName = "degree per 30m",
          UnitLabel = "°/30m",
          ID = new Guid("98a4d593-c959-49fb-ba1a-b1aa61830cd7"),
          ConversionFactorFromSIFormula = "Factors.Degree*30.0"
        },
        new UnitChoice
        {
          UnitName = "degree per 30ft",
          UnitLabel = "°/30ft",
          ID = new Guid("284dca7d-b1fc-4e40-ab08-a3e0fa2ef9d0"),
          ConversionFactorFromSIFormula = "Factors.Degree*30.0*Factors.Foot"
        },
        new UnitChoice
        {
          UnitName = "degree per 100ft",
          UnitLabel = "°/100ft",
          ID = new Guid("5aa20ab5-6800-48db-abb1-4c3538b0972d"),
          ConversionFactorFromSIFormula = "Factors.Degree*100.0*Factors.Foot"
        },
        new UnitChoice
        {
          UnitName = "degree per foot",
          UnitLabel = "°/ft",
          ID = new Guid("363a6781-5829-4046-95d8-ce1e844343fc"),
          ConversionFactorFromSIFormula = "Factors.Degree*Factors.Foot"
        },
        new UnitChoice
        {
          UnitName = "radian per foot",
          UnitLabel = "rad/ft",
          ID = new Guid("1428743e-927c-4f7a-9e15-62d37790ad51"),
          ConversionFactorFromSIFormula = "Factors.Foot"
        },
        new UnitChoice
        {
          UnitName = "degree per metre",
          UnitLabel = "°/m",
          ID = new Guid("7c47f046-0499-4108-937d-abb504883259"),
          ConversionFactorFromSIFormula = "Factors.Degree"
        },
        new UnitChoice
        {
          UnitName = "degree per decimeter",
          UnitLabel = "°/dm",
          ID = new Guid("ed1db27f-5f42-4678-8d0b-8e4e91208a83"),
          ConversionFactorFromSIFormula = "Factors.Degree*Factors.Deci"
        },
        new UnitChoice
        {
          UnitName = "degree per centimeter",
          UnitLabel = "°/cm",
          ID = new Guid("0440cec4-b070-4b03-a065-453c2fafbf24"),
          ConversionFactorFromSIFormula = "Factors.Degree*Factors.Centi"
        },
        new UnitChoice
        {
          UnitName = "degree per millimeter",
          UnitLabel = "°/mm",
          ID = new Guid("c6e0bc05-2ee5-4dd1-85fa-71de7a235ef4"),
          ConversionFactorFromSIFormula = "Factors.Degree*Factors.Milli"
        },
        new UnitChoice
        {
          UnitName = "degree per micrometer",
          UnitLabel = "°/mm",
          ID = new Guid("996e89d6-3b4a-4893-81f5-e4f539e93cf8"),
          ConversionFactorFromSIFormula = "Factors.Degree*Factors.Micro"
        },
        new UnitChoice
        {
          UnitName = "degree per nanometer",
          UnitLabel = "°/mm",
          ID = new Guid("e15c78f0-7a6c-47e5-bd3f-4f02571fabe5"),
          ConversionFactorFromSIFormula = "Factors.Degree*Factors.Nano"
        },
        new UnitChoice
        {
          UnitName = "degree per decameter",
          UnitLabel = "°/dam",
          ID = new Guid("970e611b-87ee-4006-aee7-c6d500c33ff0"),
          ConversionFactorFromSIFormula = "Factors.Degree*Factors.Deca"
        },
        new UnitChoice
        {
          UnitName = "degree per hectometer",
          UnitLabel = "°/hm",
          ID = new Guid("5cca6250-fd0e-47a7-a64f-01b8025950ad"),
          ConversionFactorFromSIFormula = "Factors.Degree*Factors.Hecto"
        },
        new UnitChoice
        {
          UnitName = "degree per kilometer",
          UnitLabel = "°/km",
          ID = new Guid("91ef582e-ee8b-4f48-a1f6-c5d96eb634ca"),
          ConversionFactorFromSIFormula = "Factors.Degree*Factors.Kilo"
        },
        new UnitChoice
        {
          UnitName = "radian per decimeter",
          UnitLabel = "rad/dm",
          ID = new Guid("c6d2c02d-83a6-4043-9a7d-392415a74b12"),
          ConversionFactorFromSIFormula = "Factors.Deci"
        },
        new UnitChoice
        {
          UnitName = "radian per centimeter",
          UnitLabel = "rad/cm",
          ID = new Guid("f101a0b8-f710-4010-b9a5-6ced681bcf0a"),
          ConversionFactorFromSIFormula = "Factors.Centi"
        },
        new UnitChoice
        {
          UnitName = "radian per millimeter",
          UnitLabel = "rad/mm",
          ID = new Guid("80e5d9d6-c1fa-4273-8134-33ff4bc46b40"),
          ConversionFactorFromSIFormula = "Factors.Milli"
        },
        new UnitChoice
        {
          UnitName = "radian per micrometer",
          UnitLabel = "rad/mm",
          ID = new Guid("777ca6bc-48da-4353-a00d-0bf0931a4328"),
          ConversionFactorFromSIFormula = "Factors.Micro"
        },
        new UnitChoice
        {
          UnitName = "radian per nanometer",
          UnitLabel = "rad/mm",
          ID = new Guid("eceb51ee-d3ad-4a42-a877-6b20f927fc01"),
          ConversionFactorFromSIFormula = "Factors.Nano"
        },
        new UnitChoice
        {
          UnitName = "radian per decameter",
          UnitLabel = "rad/dam",
          ID = new Guid("2e8079f1-260e-4524-be2f-cee1195c7fdd"),
          ConversionFactorFromSIFormula = "Factors.Deca"
        },
        new UnitChoice
        {
          UnitName = "radian per hectometer",
          UnitLabel = "rad/hm",
          ID = new Guid("cb2c22fb-ac24-4c65-96d3-2437c8942662"),
          ConversionFactorFromSIFormula = "Factors.Hecto"
        },
        new UnitChoice
        {
          UnitName = "radian per kilometer",
          UnitLabel = "rad/km",
          ID = new Guid("e9a7a647-092a-4349-bb58-0d096a1477c7"),
          ConversionFactorFromSIFormula = "Factors.Kilo"
        },
        new UnitChoice
        {
          UnitName = "degree per yard",
          UnitLabel = "°/yd",
          ID = new Guid("9cb7c45f-56d8-4b03-b514-6b5fc687919c"),
          ConversionFactorFromSIFormula = "Factors.Degree*Factors.Yard"
        },
        new UnitChoice
        {
          UnitName = "degree per mile",
          UnitLabel = "°/mi",
          ID = new Guid("d155bdde-da22-4f3a-9050-9f5f7e69b57d"),
          ConversionFactorFromSIFormula = "Factors.Degree*Factors.Mile"
        },
        new UnitChoice
        {
          UnitName = "radian per yard",
          UnitLabel = "rad/yd",
          ID = new Guid("7409e987-f5cb-4d24-93a8-50fc993fe949"),
          ConversionFactorFromSIFormula = "Factors.Yard"
        },
        new UnitChoice
        {
          UnitName = "radian per mile",
          UnitLabel = "rad/mi",
          ID = new Guid("59e78258-2739-41b4-aa50-8e077bad6678"),
          ConversionFactorFromSIFormula = "Factors.Mile"
        }
        };
        public CurvatureQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "curvature" };
            ID = new Guid("bbfe7349-8cf5-4ca0-8a84-ffe66d7f33d0");
            DescriptionMD = string.Empty;
            DescriptionMD += @"A curvature is the second derivative of a position compared to the curvilinear abscissa: $\frac{d^2x}{ds^2}$, where $x$ is the position and $s$ is the curvilinear abscissa." + Environment.NewLine;
            DescriptionMD += @"The dimension of curvature is:" + Environment.NewLine;
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

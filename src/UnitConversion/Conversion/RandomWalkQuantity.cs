using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;

namespace OSDC.UnitConversion.Conversion
{
    public partial class RandomWalkQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "radian per square root second";
        public override string SIUnitLabelLatex { get; } = "\\frac{rad}{\\sqrt{s}}";
        public override double PlaneAngleDimension { get; } = 1;
        public override double TimeDimension { get; } = -1.0/2.0;
        private static RandomWalkQuantity instance_ = null;

        public static RandomWalkQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new RandomWalkQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
        new UnitChoice
        {
          UnitName = "radian per square root second",
          UnitLabel = "rad/√s",
          ID = new Guid("557ea59e-a1da-438b-b04d-ccfc5539f87f"),
          ConversionFactorFromSIFormula = "1.0/Factors.Unit",
          IsSI = true
        },
        new UnitChoice
        {
          UnitName = "radian per square root minute",
          UnitLabel = "rad/√min",
          ID = new Guid("ccc41b4e-2efb-4760-969f-94614248374f"),
          ConversionFactorFromSIFormula = "System.Math.Sqrt(Factors.Minute)"
        },
        new UnitChoice
        {
          UnitName = "radian per square root hour",
          UnitLabel = "rad/√h",
          ID = new Guid("e296c410-e278-4586-af95-bae6fe4f0673"),
          ConversionFactorFromSIFormula = "System.Math.Sqrt(Factors.Hour)"
        },
        new UnitChoice
        {
          UnitName = "radian per square root day",
          UnitLabel = "rad/√d",
          ID = new Guid("fb4a74f9-a648-4310-a424-9c85036bbc41"),
          ConversionFactorFromSIFormula = "System.Math.Sqrt(Factors.Day)"
        },
        new UnitChoice
        {
          UnitName = "degree per square root second",
          UnitLabel = "°/√s",
          ID = new Guid("87a0a4e3-a2f5-4f84-b845-c7e6276e1655"),
          ConversionFactorFromSIFormula = "Factors.Degree"
        },
        new UnitChoice
        {
          UnitName = "degree per square root minute",
          UnitLabel = "°/√min",
          ID = new Guid("e8e3a988-4219-44a5-ae89-ce115a239d04"),
          ConversionFactorFromSIFormula = "System.Math.Sqrt(Factors.Minute)*Factors.Degree"
        },
        new UnitChoice
        {
          UnitName = "degree per square root hour",
          UnitLabel = "°/√h",
          ID = new Guid("ab6b85cf-54e5-4c3b-a330-f65d7e3bb926"),
          ConversionFactorFromSIFormula = "System.Math.Sqrt(Factors.Hour)*Factors.Degree"
        },
        new UnitChoice
        {
          UnitName = "degree per square root day",
          UnitLabel = "°/√d",
          ID = new Guid("8f806d0f-3741-4aa8-9f37-54b4f80e307c"),
          ConversionFactorFromSIFormula = "System.Math.Sqrt(Factors.Day)*Factors.Degree"
        }
        };
        public RandomWalkQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "random walk" };
            ID = new Guid("e3d17133-1c98-4ef2-8b1b-f0d935a4c1e4");
            DescriptionMD = string.Empty;
            DescriptionMD += @"A random walk is a mathematical concept describing a path consisting of a series of random steps or movements, where each step's direction or magnitude is determined probabilistically. It is used to model various phenomena in fields like physics, finance, and computer science." + Environment.NewLine;
            DescriptionMD += @"The dimension of random walk is:" + Environment.NewLine;
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

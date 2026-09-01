using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class AngleGradientPerLengthQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "radian per metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{rad}{m}";
        public override double LengthDimension { get; } = -1;
        public override double PlaneAngleDimension { get; } = 1;
        private static AngleGradientPerLengthQuantity instance_ = null;

        public static AngleGradientPerLengthQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new AngleGradientPerLengthQuantity();
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
          ID = new Guid("5d9782b6-c4c7-47ca-a86b-dce3f63c3747"),
          ConversionFactorFromSIFormula = "1.0/Factors.Unit",
          IsSI = true
        },
        new UnitChoice
        {
          UnitName = "degree per metre",
          UnitLabel = "°/m",
          ID = new Guid("2fcd4219-8879-4494-9563-5173ec2292fa"),
          ConversionFactorFromSIFormula = "Factors.Degree"
        },
         new UnitChoice
        {
          UnitName = "degree per centimetre",
          UnitLabel = "°/cm",
          ID = new Guid("7f4f63d6-5ea8-4c6b-8be4-81f52b7060c7"),
          ConversionFactorFromSIFormula = "Factors.Degree*Factors.Centi"
        },
        new UnitChoice
        {
          UnitName = "degree per foot",
          UnitLabel = "°/ft",
          ID = new Guid("23bf7716-5779-4607-aef7-1e0eeb7f201b"),
          ConversionFactorFromSIFormula = "Factors.Degree*Factors.Foot"
        },
        new UnitChoice
        {
          UnitName = "degree per inch",
          UnitLabel = "°/in",
          ID = new Guid("271db65d-2a9f-4fec-a52a-21e13e106dd4"),
          ConversionFactorFromSIFormula = "Factors.Degree*Factors.Inch"
        },
        new UnitChoice
        {
          UnitName = "degree per decimetre",
          UnitLabel = "°/dm",
          ID = new Guid("452edd17-d501-487b-8cc1-90c08f7b1417"),
          ConversionFactorFromSIFormula = "Factors.Degree*Factors.Deci"
        },
        new UnitChoice
        {
          UnitName = "degree per millimetre",
          UnitLabel = "°/mm",
          ID = new Guid("5cc72a73-70c0-4ccf-83ae-38e8a45391b4"),
          ConversionFactorFromSIFormula = "Factors.Degree*Factors.Milli"
        },
        new UnitChoice
        {
          UnitName = "radian per millimetre",
          UnitLabel = "rad/mm",
          ID = new Guid("dbd20525-128b-43c5-9de4-a8e604cbf6bf"),
          ConversionFactorFromSIFormula = "Factors.Milli"
        },
         new UnitChoice
        {
          UnitName = "radian per centimetre",
          UnitLabel = "rad/cm",
          ID = new Guid("5552abca-e21b-48ca-aedc-4518a32b8de3"),
          ConversionFactorFromSIFormula = "Factors.Centi"
        },
          new UnitChoice
        {
          UnitName = "radian per decimetre",
          UnitLabel = "rad/dm",
          ID = new Guid("47e72ab7-444d-4d4b-8cd2-01d2fb8efa2d"),
          ConversionFactorFromSIFormula = "Factors.Deci"
        },
        new UnitChoice
        {
          UnitName = "radian per foot",
          UnitLabel = "rad/ft",
          ID = new Guid("e1ab7dd2-48c7-4ac8-ac5e-bc50fdcae5df"),
          ConversionFactorFromSIFormula = "Factors.Foot"
        },
        new UnitChoice
        {
          UnitName = "radian per inch",
          UnitLabel = "rad/in",
          ID = new Guid("c36cf9c1-d4f2-4654-99eb-5d84eac21c66"),
          ConversionFactorFromSIFormula = "Factors.Inch"
        }
        };
        public AngleGradientPerLengthQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "angle gradient per length" };
            ID = new Guid("aed9c464-1073-448b-be62-a6a0c2a53dbc");
            DescriptionMD = string.Empty;
            DescriptionMD += @"Consider a situation where you have a length $L$ along which an angle $\theta$ changes. The angle variation gradient is defined as the **rate of change of the angle** per unit change in length. Mathematically, it can be expressed as: " + Environment.NewLine;
            DescriptionMD += @"$$\text{ Angle Variation Gradient} = \frac{ d\theta}{ dL}$$" + Environment.NewLine;
            DescriptionMD += @"where: " + Environment.NewLine;
            DescriptionMD += @"- **$d\theta$** is the infinitesimal change in the angle. " + Environment.NewLine;
            DescriptionMD += @"- **$dL$** is the infinitesimal change in the length along the direction of interest. " + Environment.NewLine;
            DescriptionMD += @"1. **Dimension**: The angle $\theta$ has the dimension of **plane angle**, and the length $L$ is one of the fundamental dimensions. Therefore, the dimension of **angle variation gradient** is: " + Environment.NewLine;
            DescriptionMD += "$" + GetDimensionsEnclosed() + "$ " + Environment.NewLine;
            DescriptionMD += @"2. **Interpretation**: This gradient describes how quickly the angle changes as you move along the length. For example, in fields like physics or engineering, this could describe the bending of a beam(where the angle describes the deflection) or the rate of turning along a curved path. " + Environment.NewLine;
            DescriptionMD += @"3. **Applications**: This concept is common in areas like differential geometry, mechanics (bending beams or wires), and in the analysis of curvature in space (where curvature can be described as the rate of change of the angle with respect to the arc length). " + Environment.NewLine;
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

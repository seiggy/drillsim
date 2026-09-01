using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class AngularVelocityQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "ω";
        public override string SIUnitName { get; } = "radian per second";
        public override string SIUnitLabelLatex { get; } = "\\frac{rad}{s}";
        public override double PlaneAngleDimension { get; } = 1;
        public override double TimeDimension { get; } = -1;
        private static AngularVelocityQuantity instance_ = null;

        public static AngularVelocityQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new AngularVelocityQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
                {
                  UnitName = "radian per second",
                  UnitLabel = "rad/s",
                  ID = new Guid("8889fafb-cc58-4c4e-a52d-696219dfcf4a"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                  IsSI = true
                },
                new UnitChoice
                {
                  UnitName = "degree per second",
                  UnitLabel = "°/s",
                  ID = new Guid("c6c0676e-c8a6-407d-be44-1691fd6b5d9e"),
                  ConversionFactorFromSIFormula = "Factors.Degree"
                },
                new UnitChoice
                {
                  UnitName = "radian per day",
                  UnitLabel = "rad/d",
                  ID = new Guid("53f2a55b-beb6-4e4d-b696-e817c5b0eaed"),
                  ConversionFactorFromSIFormula = "Factors.Day"
                },
                new UnitChoice
                {
                  UnitName = "radian per hour",
                  UnitLabel = "rad/h",
                  ID = new Guid("c2f0e82b-236b-4eb5-9c77-b2c500ab60ab"),
                  ConversionFactorFromSIFormula = "Factors.Hour"
                },
                new UnitChoice
                {
                  UnitName = "radian per minute",
                  UnitLabel = "rad/min",
                  ID = new Guid("5267eb8c-36bc-4ab9-bd62-26f05f852c35"),
                  ConversionFactorFromSIFormula = "Factors.Minute"
                },
                new UnitChoice
                {
                  UnitName = "degree per day",
                  UnitLabel = "°/d",
                  ID = new Guid("ec049b3d-134b-44a3-9746-0419a368beef"),
                  ConversionFactorFromSIFormula = "Factors.Degree*Factors.Day"
                },
                new UnitChoice
                {
                  UnitName = "degree per hour",
                  UnitLabel = "°/h",
                  ID = new Guid("44cc8b17-2f87-49c9-8ab7-7f4ed36d9eb6"),
                  ConversionFactorFromSIFormula = "Factors.Degree*Factors.Hour"
                },
                new UnitChoice
                {
                  UnitName = "degree per minute",
                  UnitLabel = "°/min",
                  ID = new Guid("0a3ad50c-bbfa-456c-9613-84f37b4a80f1"),
                  ConversionFactorFromSIFormula = "Factors.Degree*Factors.Minute"
                },
                new UnitChoice
                {
                UnitName = "revolution per second",
                UnitLabel = "rps",
                ID = new Guid("00a96665-b967-4982-8b6b-1ca3671f8c9a"),
                ConversionFactorFromSIFormula = "Factors.Unit / Factors.Revolution"
                },
                new UnitChoice
                {
                UnitName = "revolution per minute",
                UnitLabel = "rpm",
                ID = new Guid("fe846c15-ddba-480f-93f0-16af45f7b9ce"),
                ConversionFactorFromSIFormula = "Factors.Minute / Factors.Revolution"
                },
                 new UnitChoice
                {
                UnitName = "revolution per hour",
                UnitLabel = "rph",
                ID = new Guid("950585c6-bc50-4f51-becb-2f840e217c4f"),
                ConversionFactorFromSIFormula = "Factors.Hour / Factors.Revolution"
                },
                new UnitChoice
                {
                UnitName = "thousand revolution per second",
                UnitLabel = "1000xrps",
                ID = new Guid("4c7c26e6-874c-4da0-b26d-095e59938bf0"),
                ConversionFactorFromSIFormula = "Factors.Unit / (Factors.Kilo*Factors.Revolution)"
                },
                new UnitChoice
                {
                UnitName = "thousand revolution per minute",
                UnitLabel = "1000xrpm",
                ID = new Guid("4ac0f148-241b-42bd-b55e-88a27d12f860"),
                ConversionFactorFromSIFormula = "Factors.Minute / (Factors.Kilo*Factors.Revolution)"
                },
                new UnitChoice
                {
                UnitName = "thousand revolution per hour",
                UnitLabel = "1000xrph",
                ID = new Guid("ee395b6a-95bc-4fd1-b6fe-0eb26046d595"),
                ConversionFactorFromSIFormula = "Factors.Hour / (Factors.Kilo*Factors.Revolution)"
                },
                new UnitChoice
                {
                UnitName = "stroke per second",
                UnitLabel = "sps",
                ID = new Guid("23fd9599-d210-4050-8c61-fc18a5087db3"),
                ConversionFactorFromSIFormula = "Factors.Unit / Factors.Revolution"
                },
                new UnitChoice
                {
                UnitName = "stroke per minute",
                UnitLabel = "spm",
                ID = new Guid("979b3170-be8b-42ee-a7d5-ecf9d9f1869d"),
                ConversionFactorFromSIFormula = "Factors.Minute / Factors.Revolution"
                },
                new UnitChoice
                {
                UnitName = "stroke per hour",
                UnitLabel = "sph",
                ID = new Guid("114d2d67-d080-4c46-85c6-9047cd0e2d7a"),
                ConversionFactorFromSIFormula = "Factors.Hour / Factors.Revolution"
                },
                new UnitChoice
                {
                UnitName = "thousand stroke per second",
                UnitLabel = "1000xsps",
                ID = new Guid("edb3d772-8ed9-499a-a2db-e5835176fb1b"),
                ConversionFactorFromSIFormula = "Factors.Unit / (Factors.Kilo*Factors.Revolution)"
                },
                new UnitChoice
                {
                UnitName = "thousand stroke per minute",
                UnitLabel = "1000xspm",
                ID = new Guid("bf130c38-70bb-4bd1-8e21-ae3b3043bc96"),
                ConversionFactorFromSIFormula = "Factors.Minute / (Factors.Kilo*Factors.Revolution)"
                },
                new UnitChoice
                {
                UnitName = "thousand stroke per hour",
                UnitLabel = "1000xsph",
                ID = new Guid("756ac21d-94c9-4501-947f-0bf275528fb5"),
                ConversionFactorFromSIFormula = "Factors.Hour / (Factors.Kilo*Factors.Revolution)"
                }

        };
        public AngularVelocityQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "angular velocity" };
            ID = new Guid("688ccd2b-6a30-4ccc-8580-a80c3a5803fa");
            DescriptionMD = string.Empty;
            DescriptionMD += @"An angular velocity is the first derivative compared to time of a plan angle: $\frac{d\theta}{dt}$." + Environment.NewLine;
            DescriptionMD += @"The dimension of angular velocity is:" + Environment.NewLine;
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

using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class AngularAccelerationQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "ω̇";
        public override string SIUnitName { get; } = "radian per second squared";
        public override string SIUnitLabelLatex { get; } = "\\frac{rad}{s^{2}}";
        public override double PlaneAngleDimension { get; } = 1;
        public override double TimeDimension { get; } = -2;
        private static AngularAccelerationQuantity instance_ = null;

        public static AngularAccelerationQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new AngularAccelerationQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
                {
                  UnitName = "radian per second squared",
                  UnitLabel = "rad/s²",
                  ID = new Guid("15d7ab2b-c0c3-4d33-8242-670ba2f937ff"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                  IsSI = true
                },
                new UnitChoice
                {
                  UnitName = "degree per second squared",
                  UnitLabel = "°/s²",
                  ID = new Guid("6fcc944b-fd7e-4368-baa4-3bb8eeba63a2"),
                  ConversionFactorFromSIFormula = "Factors.Degree"
                },
                new UnitChoice
                {
                  UnitName = "radian per day per second",
                  UnitLabel = "rad/d/s",
                  ID = new Guid("76b421a7-87e5-4fdf-a280-8e5aea80e0d0"),
                  ConversionFactorFromSIFormula = "Factors.Day"
                },
                new UnitChoice
                {
                  UnitName = "radian per hour per second",
                  UnitLabel = "rad/h/s",
                  ID = new Guid("70b2f838-b8d2-425f-bea1-0a841c520ba8"),
                  ConversionFactorFromSIFormula = "Factors.Hour"
                },
                new UnitChoice
                {
                  UnitName = "radian per minute per second",
                  UnitLabel = "rad/min/s",
                  ID = new Guid("fdf50c96-cef9-466a-80d6-747b99dad734"),
                  ConversionFactorFromSIFormula = "Factors.Minute"
                },
                new UnitChoice
                {
                  UnitName = "degree per day per second",
                  UnitLabel = "°/d/s",
                  ID = new Guid("838a73aa-fd19-42ac-9c72-c62573cda91b"),
                  ConversionFactorFromSIFormula = "Factors.Day*Factors.Degree"
                },
                new UnitChoice
                {
                  UnitName = "degree per hour per second",
                  UnitLabel = "°/h/s",
                  ID = new Guid("cbfff738-14e6-47ad-8b21-41eeeea41439"),
                  ConversionFactorFromSIFormula = "Factors.Hour*Factors.Degree"
                },
                new UnitChoice
                {
                  UnitName = "degree per minute per second",
                  UnitLabel = "°/min/s",
                  ID = new Guid("f6b4276b-64a8-46f9-a831-fd14a61c34a0"),
                  ConversionFactorFromSIFormula = "Factors.Minute*Factors.Degree"
                },
                new UnitChoice
                {
                  UnitName = "radian per second per minute",
                  UnitLabel = "rad/s/min",
                  ID = new Guid("6bc32ba1-3a66-415a-bc0f-d9292ac77ab6"),
                  ConversionFactorFromSIFormula = "Factors.Unit * Factors.Minute"
                },
                new UnitChoice
                {
                  UnitName = "degree per second per minute",
                  UnitLabel = "°/s/min",
                  ID = new Guid("2bb42b52-ab2d-4d2f-8ebb-1294f9b35b89"),
                  ConversionFactorFromSIFormula = "Factors.Unit * Factors.Minute * Factors.Degree"
                },
                new UnitChoice
                {
                  UnitName = "radian per day per minute",
                  UnitLabel = "rad/d/min",
                  ID = new Guid("62a8e46b-3701-4375-be57-f5d53df23d87"),
                  ConversionFactorFromSIFormula = "Factors.Day*Factors.Minute"
                },
                new UnitChoice
                {
                  UnitName = "radian per hour per minute",
                  UnitLabel = "rad/h/min",
                  ID = new Guid("77f3ea1e-8280-4881-befe-08dc1065a94f"),
                  ConversionFactorFromSIFormula = "Factors.Hour*Factors.Minute"
                },
                new UnitChoice
                {
                  UnitName = "radian per minute squared",
                  UnitLabel = "rad/min²",
                  ID = new Guid("61ec8b9b-f1c7-4798-bf37-d7f697d0ea8e"),
                  ConversionFactorFromSIFormula = "Factors.Minute*Factors.Minute"
                },
                new UnitChoice
                {
                  UnitName = "degree per day per minute",
                  UnitLabel = "°/d/min",
                  ID = new Guid("22b8d910-ce73-4ce4-87b3-761aa17059d6"),
                  ConversionFactorFromSIFormula = "Factors.Day*Factors.Minute*Factors.Degree"
                },
                new UnitChoice
                {
                  UnitName = "degree per hour per minute",
                  UnitLabel = "°/h/min",
                  ID = new Guid("414785c2-4d81-472e-a020-7eca9913ebd2"),
                  ConversionFactorFromSIFormula = "Factors.Hour*Factors.Minute*Factors.Degree"
                },
                new UnitChoice
                {
                  UnitName = "degree per minute squared",
                  UnitLabel = "°/min²",
                  ID = new Guid("49b1ab89-8a0c-4bd9-b693-7db0e14b86e9"),
                  ConversionFactorFromSIFormula = "Factors.Minute*Factors.Minute*Factors.Degree"
                },
                new UnitChoice
                {
                  UnitName = "radian per second per hour",
                  UnitLabel = "rad/s/h",
                  ID = new Guid("6bb6033c-3391-4e68-9cc3-63e9d8b03eb3"),
                  ConversionFactorFromSIFormula = "Factors.Unit*Factors.Hour"
                },
                new UnitChoice
                {
                  UnitName = "degree per second per hour",
                  UnitLabel = "°/s/h",
                  ID = new Guid("cdd641f0-2b50-4cb4-88c1-256ac2f5e2b5"),
                  ConversionFactorFromSIFormula = "Factors.Unit*Factors.Hour*Factors.Degree"
                },
                new UnitChoice
                {
                  UnitName = "radian per day per hour",
                  UnitLabel = "rad/d/h",
                  ID = new Guid("1ff0952d-4395-4dd6-b101-99b890a46ee8"),
                  ConversionFactorFromSIFormula = "Factors.Day*Factors.Hour"
                },
                new UnitChoice
                {
                  UnitName = "radian per hour squared",
                  UnitLabel = "rad/h²",
                  ID = new Guid("f105faab-2b48-402a-98f2-004b99adb5a7"),
                  ConversionFactorFromSIFormula = "Factors.Hour*Factors.Hour"
                },
                new UnitChoice
                {
                  UnitName = "radian per minute per hour",
                  UnitLabel = "rad/min/h",
                  ID = new Guid("c5713507-731e-482a-bd4e-d86ef21d4ae5"),
                  ConversionFactorFromSIFormula = "Factors.Minute*Factors.Hour"
                },
                new UnitChoice
                {
                  UnitName = "degree per day per hour",
                  UnitLabel = "°/d/h",
                  ID = new Guid("7e8596f9-657f-4eb6-937d-091a55bd0e34"),
                  ConversionFactorFromSIFormula = "Factors.Day*Factors.Hour*Factors.Degree"
                },
                new UnitChoice
                {
                  UnitName = "degree per hour squared",
                  UnitLabel = "°/h²",
                  ID = new Guid("fad47c71-53d9-4a88-b720-6a953092b41b"),
                  ConversionFactorFromSIFormula = "Factors.Hour*Factors.Hour*Factors.Degree"
                },
                new UnitChoice
                {
                  UnitName = "degree per minute per hour",
                  UnitLabel = "°/min/h",
                  ID = new Guid("a4870e3b-95b7-4c8c-b86a-ffe1d2d6e524"),
                  ConversionFactorFromSIFormula = "Factors.Minute*Factors.Hour*Factors.Degree"
                },
                new UnitChoice
                {
                  UnitName = "radian per second per day",
                  UnitLabel = "rad/s/d",
                  ID = new Guid("52462419-b494-473f-a46d-270ae253f076"),
                  ConversionFactorFromSIFormula = "Factors.Unit*Factors.Day"
                },
                new UnitChoice
                {
                  UnitName = "degree per second per day",
                  UnitLabel = "°/s/d",
                  ID = new Guid("dab003cb-6906-4f9f-8551-e8f72d3a1c4d"),
                  ConversionFactorFromSIFormula = "Factors.Degree*Factors.Unit*Factors.Day"
                },
                new UnitChoice
                {
                  UnitName = "radian per day squared",
                  UnitLabel = "rad/d²",
                  ID = new Guid("6027756a-1c1e-47e8-b938-0bc4e4b25a24"),
                  ConversionFactorFromSIFormula = "Factors.Day*Factors.Day"
                },
                new UnitChoice
                {
                  UnitName = "radian per hour per day",
                  UnitLabel = "rad/h/d",
                  ID = new Guid("a31be21e-1f83-4e9b-aaa7-56a2f797c3d6"),
                  ConversionFactorFromSIFormula = "Factors.Hour*Factors.Day"
                },
                new UnitChoice
                {
                  UnitName = "radian per minute per day",
                  UnitLabel = "rad/min/d",
                  ID = new Guid("3335414d-1a69-477b-b27f-61f464a7ca13"),
                  ConversionFactorFromSIFormula = "Factors.Minute*Factors.Day"
                },
                new UnitChoice
                {
                  UnitName = "degree per day squared",
                  UnitLabel = "°/d²",
                  ID = new Guid("a77c1dea-6fe9-43cc-9f53-75d87e00e458"),
                  ConversionFactorFromSIFormula = "Factors.Day*Factors.Day*Factors.Degree"
                },
                new UnitChoice
                {
                  UnitName = "degree per hour per day",
                  UnitLabel = "°/h/d",
                  ID = new Guid("69437581-725e-4a66-b945-abaac133be0a"),
                  ConversionFactorFromSIFormula = "Factors.Hour*Factors.Day*Factors.Degree"
                },
                new UnitChoice
                {
                  UnitName = "degree per minute per day",
                  UnitLabel = "°/min/d",
                  ID = new Guid("5a8a9fe1-9866-4c71-95a5-23dcb4726235"),
                  ConversionFactorFromSIFormula = "Factors.Minute*Factors.Day*Factors.Degree"
                }
        };
        public AngularAccelerationQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "angular acceleration" };
            ID = new Guid("8b33d305-f77e-4631-9818-7ef574bd0c02");
            DescriptionMD = string.Empty;
            DescriptionMD += @"An angular acceleration is the second derivative compared to time of a plan angle: $\frac{d^2\theta}{dt^2}$." + Environment.NewLine;
            DescriptionMD += @"The dimension of angular acceleration is:" + Environment.NewLine;
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

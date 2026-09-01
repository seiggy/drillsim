using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class PlaneAngleQuantity : SymbolizedBasePhysicalQuantity
    {
        public override string DimensionSymbol { get; } = "";
        public override string SIUnitName { get; } = "radian";

        public override string SIUnitLabelLatex { get; } = "rad";

        public override double PlaneAngleDimension { get; } = 1;

        private static PlaneAngleQuantity instance_ = null;

        public static PlaneAngleQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new PlaneAngleQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
                {
                    UnitName = "radian",
                    UnitLabel = "rad",
                    ID = new Guid("a71fc712-342a-48c2-8e45-b56ee31c7ae0"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                    IsSI = true
                },
                new UnitChoice
                {
                    UnitName = "milliradian",
                    UnitLabel = "mrad",
                    ID = new Guid("34a37faf-dfb9-4a34-899c-c9fa78f295a5"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "degree",
                    UnitLabel = "°",
                    ID = new Guid("023a3393-a01e-499f-967a-a76b1a78d586"),
                    ConversionFactorFromSIFormula = "Factors.Degree"
                },
                new UnitChoice
                {
                    UnitName = "grad",
                    UnitLabel = "grad",
                    ID = new Guid("584314cf-a10f-49b6-a5e9-1cfa0ec0f355"),
                    ConversionFactorFromSIFormula = "Factors.Grad"
                },
                new UnitChoice
                {
                    UnitName = "gon",
                    UnitLabel = "gon",
                    ID = new Guid("feefeed5-2df2-4c66-84f1-0de998ba44db"),
                    ConversionFactorFromSIFormula = "Factors.Grad"
                },

                new UnitChoice
                {
                    UnitName = "circle",
                    UnitLabel = "circle",
                    ID = new Guid("e27aeec3-667d-41bb-9bd2-60bf213f8b7b"),
                    ConversionFactorFromSIFormula = "1.0/(2.0*System.Math.PI)"
                },
                new UnitChoice
                {
                    UnitName = "revolution",
                    UnitLabel = "revolution",
                    ID = new Guid("e613477b-f8bc-45c7-8ccc-391a7f33af05"),
                    ConversionFactorFromSIFormula = "1.0/(2.0*System.Math.PI)"
                },
                new UnitChoice
                {
                    UnitName = "quadrant",
                    UnitLabel = "quadrant",
                    ID = new Guid("dedbbea6-40e7-439a-9409-220fee4c148a"),
                    ConversionFactorFromSIFormula = "2.0/System.Math.PI"
                },
                new UnitChoice
                {
                    UnitName = "sextant",
                    UnitLabel = "sextant",
                    ID = new Guid("ce4197b4-6d9d-488b-a360-98899a82837e"),
                    ConversionFactorFromSIFormula = "3.0/System.Math.PI"
                },
                new UnitChoice
                {
                    UnitName = "octant",
                    UnitLabel = "octant",
                    ID = new Guid("8f78bfce-cad9-4a77-aa5f-3a5fecc89364"),
                    ConversionFactorFromSIFormula = "4.0/System.Math.PI"
                },
                 new UnitChoice
                {
                    UnitName = "arc minute",
                    UnitLabel = "\'",
                    ID = new Guid("e1ce9562-ecd0-46e2-82e2-bcec1b6ac113"),
                    ConversionFactorFromSIFormula = "60.0*Factors.Degree"
                },
                new UnitChoice
                {
                    UnitName = "arc second",
                    UnitLabel = "\"",
                    ID = new Guid("bea092da-34d6-4130-bc65-41fb7702597a"),
                    ConversionFactorFromSIFormula = "60.0*60.0*Factors.Degree"
                }
        };
        public PlaneAngleQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "plane angle" };
            ID = new Guid("751a8f44-d938-4319-a422-a753962fd91f");
            DescriptionMD = string.Empty;
            DescriptionMD += @"A plane angle is the angle formed between two intersecting lines or planes in a two-dimensional or three-dimensional space." + Environment.NewLine;
            DescriptionMD += @"The dimension of plane angle is:" + Environment.NewLine;
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

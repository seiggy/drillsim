using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class VelocityQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "v";
        public override string SIUnitName { get; } = "metre per second";
        public override string SIUnitLabelLatex { get; } = "\\frac{m}{s}";
        public override double LengthDimension { get; } = 1;
        public override double TimeDimension { get; } = -1;
        private static VelocityQuantity instance_ = null;

        public static VelocityQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new VelocityQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
                new UnitChoice
                {
                  UnitName = "metre per second",
                  UnitLabel = "m/s",
                  ID = new Guid("919ac736-9a37-45d1-8c02-54bc453d65dc"),
                  ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                  IsSI = true
                },
                new UnitChoice
                {
                  UnitName = "metre per minute",
                  UnitLabel = "m/min",
                  ID = new Guid("824d3b5b-1e51-446a-99a4-39c02377f303"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.Unit"
                },
                new UnitChoice
                {
                  UnitName = "centimetre per second",
                  UnitLabel = "cm/s",
                  ID = new Guid("d32f84d7-f076-49ab-8bd5-1ccd27e0eba6"),
                  ConversionFactorFromSIFormula = "Factors.Unit/Factors.Centi"
                },
                new UnitChoice
                {
                  UnitName = "metre per hour",
                  UnitLabel = "m/h",
                  ID = new Guid("b4867c19-0668-4043-b3b9-f666f7552b02"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.Unit"
                },
                new UnitChoice
                {
                  UnitName = "metre per day",
                  UnitLabel = "m/d",
                  ID = new Guid("aa10055f-1ef6-460d-841c-b6dc69f6a7f2"),
                  ConversionFactorFromSIFormula = "Factors.Day/Factors.Unit"
                },
                new UnitChoice
                {
                  UnitName = "foot per hour",
                  UnitLabel = "ft/h",
                  ID = new Guid("adb3b459-aa7e-4639-ad07-6d19c80f8170"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.Foot"
                },
                new UnitChoice
                {
                  UnitName = "foot per day",
                  UnitLabel = "ft/d",
                  ID = new Guid("aa58ec87-0dbc-4ed4-b8aa-8553b02c7b14"),
                  ConversionFactorFromSIFormula = "Factors.Day/Factors.Foot"
                },
                new UnitChoice
                {
                  UnitName = "foot per minute",
                  UnitLabel = "ft/m",
                  ID = new Guid("2d139d2c-1063-4f8d-99ae-bf71a98a1076"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.Foot"
                },
                new UnitChoice
                {
                  UnitName = "foot per second",
                  UnitLabel = "ft/s",
                  ID = new Guid("6c9eef39-29f0-4d6d-ae7a-f9161d8fd4fa"),
                  ConversionFactorFromSIFormula = "Factors.Unit/Factors.Foot"
                },
                new UnitChoice
                {
                  UnitName = "inch per second",
                  UnitLabel = "in/s",
                  ID = new Guid("8cd16c97-5c7a-4ee9-b59b-cbe2decd8ff9"),
                  ConversionFactorFromSIFormula = "Factors.Unit/Factors.Inch"
                },
                new UnitChoice
                {
                  UnitName = "mile per hour",
                  UnitLabel = "mph",
                  ID = new Guid("6c6d0be3-5b60-4b8a-9fd6-8b7afb261081"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.Mile"
                },
                new UnitChoice
                {
                  UnitName = "kilometre per hour",
                  UnitLabel = "km/h",
                  ID = new Guid("a1bab5e0-221c-4555-bd37-cf2b8004fd53"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.Kilo"
                },
                new UnitChoice
                {
                  UnitName = "kilometre per minute",
                  UnitLabel = "km/min",
                  ID = new Guid("b37519e1-5d78-4d34-ad7b-37bc3f0bc775"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.Kilo"
                },
                new UnitChoice
                {
                  UnitName = "kilometre per second",
                  UnitLabel = "km/s",
                  ID = new Guid("3944bb76-5675-49bf-ae2f-143d3ff8e41a"),
                  ConversionFactorFromSIFormula = "Factors.Unit/Factors.Kilo"
                },
                new UnitChoice
                {
                  UnitName = "kilometre per day",
                  UnitLabel = "km/d",
                  ID = new Guid("2d09bf7b-0f99-42c0-9732-f9923c11bde1"),
                  ConversionFactorFromSIFormula = "Factors.Day/Factors.Kilo"
                },
                new UnitChoice
                {
                  UnitName = "mile per minute",
                  UnitLabel = "mi/min",
                  ID = new Guid("959dcb48-193b-48a9-9b86-554ea6b6e755"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.Mile"
                },
                new UnitChoice
                {
                  UnitName = "mile per second",
                  UnitLabel = "mi/s",
                  ID = new Guid("5ec77a90-200b-4e6e-877b-8df0edb7adc2"),
                  ConversionFactorFromSIFormula = "Factors.Unit/Factors.Mile"
                },
                new UnitChoice
                {
                  UnitName = "mile per day",
                  UnitLabel = "mi/d",
                  ID = new Guid("340ef6b0-53c2-447c-b8dd-f8f184bce71d"),
                  ConversionFactorFromSIFormula = "Factors.Day/Factors.Mile"
                },
                new UnitChoice
                {
                  UnitName = "inch per minute",
                  UnitLabel = "in/min",
                  ID = new Guid("d6421f59-0d0f-49e3-9f2c-37590569beb4"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.Inch"
                },
                new UnitChoice
                {
                  UnitName = "inch per hour",
                  UnitLabel = "in/h",
                  ID = new Guid("06115ddb-4f51-41cd-a502-8c4f443d66b2"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.Inch"
                },
                new UnitChoice
                {
                  UnitName = "inch per day",
                  UnitLabel = "in/d",
                  ID = new Guid("38991fcc-56f6-4447-bd1e-86159681e8d0"),
                  ConversionFactorFromSIFormula = "Factors.Day/Factors.Inch"
                },
                new UnitChoice
                {
                  UnitName = "centimetre per minute",
                  UnitLabel = "cm/min",
                  ID = new Guid("b52fb69d-f8f7-4e46-9223-626e7497854d"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.Centi"
                },
                new UnitChoice
                {
                  UnitName = "centimetre per hour",
                  UnitLabel = "cm/h",
                  ID = new Guid("9a4d693e-cb18-4587-a465-48aec69369bf"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.Centi"
                },
                new UnitChoice
                {
                  UnitName = "centimetre per day",
                  UnitLabel = "cm/d",
                  ID = new Guid("d34eba86-b8e2-4f28-92bb-8a26132ccfc6"),
                  ConversionFactorFromSIFormula = "Factors.Day/Factors.Centi"
                },
                new UnitChoice
                {
                  UnitName = "millimetre per second",
                  UnitLabel = "mm/s",
                  ID = new Guid("8d787bbf-81b0-4ba4-b913-c71cfe4b7025"),
                  ConversionFactorFromSIFormula = "Factors.Unit/Factors.Milli"
                },
                new UnitChoice
                {
                  UnitName = "millimetre per minute",
                  UnitLabel = "mm/min",
                  ID = new Guid("87a2da8b-a5e8-43f4-af18-859f6e8dc822"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.Milli"
                },
                new UnitChoice
                {
                  UnitName = "millimetre per hour",
                  UnitLabel = "mm/h",
                  ID = new Guid("4628ccfb-2837-40b3-9141-222af23fa7be"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.Milli"
                },
                new UnitChoice
                {
                  UnitName = "millimetre per day",
                  UnitLabel = "mm/d",
                  ID = new Guid("c1540a11-a20e-43e2-9d1b-e173b928c94b"),
                  ConversionFactorFromSIFormula = "Factors.Day/Factors.Milli"
                },
                new UnitChoice
                {
                  UnitName = "decimetre per second",
                  UnitLabel = "dm/s",
                  ID = new Guid("0f9aa2e1-b66f-4728-bf57-79526ffce563"),
                  ConversionFactorFromSIFormula = "Factors.Unit/Factors.Deci"
                },
                new UnitChoice
                {
                  UnitName = "decimetre per minute",
                  UnitLabel = "dm/min",
                  ID = new Guid("980c51cc-a185-44a6-a69c-34f52e2b1fe2"),
                  ConversionFactorFromSIFormula = "Factors.Minute/Factors.Deci"
                },
                new UnitChoice
                {
                  UnitName = "decimetre per hour",
                  UnitLabel = "dm/h",
                  ID = new Guid("1d3b5a3c-81ff-4698-b92f-9b721f946220"),
                  ConversionFactorFromSIFormula = "Factors.Hour/Factors.Deci"
                },
                new UnitChoice
                {
                  UnitName = "decimetre per day",
                  UnitLabel = "dm/d",
                  ID = new Guid("dcb77826-7550-4681-b3ce-a59cfdb7620d"),
                  ConversionFactorFromSIFormula = "Factors.Day/Factors.Deci"
                },
                new UnitChoice
                {
                  UnitName = "furlong per fortnight",
                  UnitLabel = "furlong/14d",
                  ID = new Guid("028ad001-b80d-49d8-8d18-8e10c1f0239f"),
                  ConversionFactorFromSIFormula = "Factors.Fortnight/Factors.Furlong"
                }
        };
        public VelocityQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "velocity" };
            ID = new Guid("b3fd17fe-ce71-4ef3-ac99-cf4f5756e81a");
            DescriptionMD = string.Empty;
            DescriptionMD += @"A velocity is the time derivative of a position or a displacement: $\frac{dx}{dt}$, where $x$ is a position and $t$ is time." + Environment.NewLine;
            DescriptionMD += @"The dimension of velocity is:" + Environment.NewLine;
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

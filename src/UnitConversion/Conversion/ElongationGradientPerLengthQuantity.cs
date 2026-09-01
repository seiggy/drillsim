using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class ElongationGradientPerLengthQuantity : DerivedBasePhysicalQuantity
    {
        public override string SIUnitName { get; } = "metre per metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{m}{m}";

        private static ElongationGradientPerLengthQuantity instance_ = null;

        public static ElongationGradientPerLengthQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ElongationGradientPerLengthQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "metre per metre",
                UnitLabel = "m/m",
                ID = new Guid("cc12967b-4c7d-4c70-95cf-d2f23bd6f76b"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true
            },
            new UnitChoice
            {
                UnitName = "decimetre per metre",
                UnitLabel = "dm/m",
                ID = new Guid("337d67e2-b28c-4ab8-9817-3f9951fdb67b"),
                ConversionFactorFromSIFormula = "1.0 / Factors.Deci"
            },
            new UnitChoice
            {
                UnitName = "centimetre per metre",
                UnitLabel = "cm/m",
                ID = new Guid("4a7ce144-b35f-401f-bfbc-276b7c4ec4a9"),
                ConversionFactorFromSIFormula = "1.0 / Factors.Centi"
            },
            new UnitChoice
            {
                UnitName = "millimetre per metre",
                UnitLabel = "mm/m",
                ID = new Guid("4e241b59-388a-428f-82e7-b9971f9e1df5"),
                ConversionFactorFromSIFormula = "1.0/Factors.Milli"
            },
            new UnitChoice
            {
                UnitName = "micrometre per metre",
                UnitLabel = "µm/m",
                ID = new Guid("bbd912b2-06e2-46fe-82da-af87bab150dc"),
                ConversionFactorFromSIFormula = "1.0 / Factors.Micro"
            },
            new UnitChoice
            {
                UnitName = "metre per kilometre",
                UnitLabel = "m/km",
                ID = new Guid("5b583a4c-7838-48e7-8201-420f43eef9e1"),
                ConversionFactorFromSIFormula = "Factors.Kilo/Factors.Unit"
            },
            new UnitChoice
            {
                UnitName = "decimetre per kilometre",
                UnitLabel = "dm/km",
                ID = new Guid("7dc93254-9a25-4215-b2bc-9f2d8dc14d6e"),
                ConversionFactorFromSIFormula = "Factors.Kilo / Factors.Deci"
            },
            new UnitChoice
            {
                UnitName = "centimetre per kilometre",
                UnitLabel = "cm/km",
                ID = new Guid("f539c676-e969-4235-9524-42e860e84966"),
                ConversionFactorFromSIFormula = "Factors.Kilo / Factors.Centi"
            },
            new UnitChoice
            {
                UnitName = "millimetre per kilometre",
                UnitLabel = "mm/km",
                ID = new Guid("59f50e71-7796-4559-9e55-7fc420d9c20c"),
                ConversionFactorFromSIFormula = "Factors.Kilo / Factors.Milli"
            },
            new UnitChoice
            {
                UnitName = "micrometre per kilometre",
                UnitLabel = "µm/km",
                ID = new Guid("73d8d799-d9f5-40f9-9216-4bc0bbf1c044"),
                ConversionFactorFromSIFormula = "Factors.Kilo / Factors.Micro"
            },
            new UnitChoice
            {
                UnitName = "inch per foot",
                UnitLabel = "in/ft",
                ID = new Guid("3df1af23-afd0-41ed-9442-a3af6ae944d2"),
                ConversionFactorFromSIFormula = "Factors.Foot / Factors.Inch"
            },
            new UnitChoice
            {
                UnitName = "inch per yard",
                UnitLabel = "in/yd",
                ID = new Guid("ec1fbeee-cbf4-41f0-94d8-573e241c22b2"),
                ConversionFactorFromSIFormula = "Factors.Yard / Factors.Inch"
            },
            new UnitChoice
            {
                UnitName = "inch per mile",
                UnitLabel = "in/mi",
                ID = new Guid("998afd92-de3a-4f10-90b6-a252b8e59181"),
                ConversionFactorFromSIFormula = "Factors.Mile / Factors.Inch"
            },
            new UnitChoice
            {
                UnitName = "foot per foot",
                UnitLabel = "ft/ft",
                ID = new Guid("a53ffdb6-a2db-4984-85aa-53763ba3aabb"),
                ConversionFactorFromSIFormula = "Factors.Foot / Factors.Foot"
            },
            new UnitChoice
            {
                UnitName = "foot per yard",
                UnitLabel = "ft/yd",
                ID = new Guid("ba9062f9-68be-4b9c-ba61-57c8543ed6f9"),
                ConversionFactorFromSIFormula = "Factors.Yard / Factors.Foot"
            },
            new UnitChoice
            {
                UnitName = "foot per mile",
                UnitLabel = "ft/mi",
                ID = new Guid("89b73d98-2818-43c5-8d31-8aa1bb78d3bc"),
                ConversionFactorFromSIFormula = "Factors.Mile / Factors.Foot"
            },
            new UnitChoice
            {
                UnitName = "yard per foot",
                UnitLabel = "yd/ft",
                ID = new Guid("a6c2cf06-e21a-4387-90db-89d7d46b1b28"),
                ConversionFactorFromSIFormula = "Factors.Foot / Factors.Yard"
            },
            new UnitChoice
            {
                UnitName = "yard per yard",
                UnitLabel = "yd/yd",
                ID = new Guid("56f75af0-7213-43d9-b23f-bc74da6382e9"),
                ConversionFactorFromSIFormula = "Factors.Yard / Factors.Yard"
            },
            new UnitChoice
            {
                UnitName = "yard per mile",
                UnitLabel = "yd/mi",
                ID = new Guid("3283a57e-ec6d-4487-ab32-cdc0c5de2020"),
                ConversionFactorFromSIFormula = "Factors.Mile / Factors.Yard"
            }
        };
        public ElongationGradientPerLengthQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "elongation gradient per length" };
            ID = new Guid("3c6176f8-8f74-4fbf-bb65-207ed8b0a120");
            DescriptionMD = string.Empty;
            DescriptionMD += @"An elongation gradient per length is the first derivative of an elongation compared to a distance: $\frac{d\epsilon}{ds}$, where $\epsilon$ is an elongation and $s$ is a distance." + Environment.NewLine;
            DescriptionMD += @"It is dimensionless." + Environment.NewLine;
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

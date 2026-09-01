using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class MassQuantity : SymbolizedBasePhysicalQuantity
    {
        public override string DimensionSymbol { get; } = "M";
        public override string SIUnitName { get; } = "kilogram";

        public override string SIUnitLabelLatex { get; } = "kg";
        private static MassQuantity instance_ = null;

        public override double MassDimension { get; } = 1;

        public static MassQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MassQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
                new UnitChoice
                {
                    UnitName = "kilogram",
                    UnitLabel = "kg",
                    ID = new Guid("ef4c5fc1-8774-4aea-b772-35aeae56413d"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                    IsSI = true
                },
                new UnitChoice
                {
                    UnitName = "hectogram",
                    UnitLabel = "hg",
                    ID = new Guid("2fb79e4b-3eb5-4aa3-9f12-2c66b1784902"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Deci"
                },
                new UnitChoice
                {
                    UnitName = "decagram",
                    UnitLabel = "dag",
                    ID = new Guid("1b3f72cb-55b1-4027-b6ad-309cd7d6c1a3"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Centi"
                },
                new UnitChoice
                {
                    UnitName = "gram",
                    UnitLabel = "g",
                    ID = new Guid("049ba04e-4c70-41f5-bb29-6b54bb5b2103"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Milli"
                },
                new UnitChoice
                {
                    UnitName = "centigram",
                    UnitLabel = "cg",
                    ID = new Guid("e56aa2fa-80b7-417f-8e08-91b9b8a1198c"),
                    ConversionFactorFromSIFormula = "1.0/(0.1*Factors.Milli)"
                },
                new UnitChoice
                {
                    UnitName = "milligram",
                    UnitLabel = "mg",
                    ID = new Guid("322b0e70-c8e5-482e-a9db-682d15baacf9"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Micro"
                },
                new UnitChoice
                {
                    UnitName = "microgram",
                    UnitLabel = "µg",
                    ID = new Guid("eb831d52-2690-4b8a-a1a4-83e9bdb07dbc"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Nano"
                },
                new UnitChoice
                {
                    UnitName = "nanogram",
                    UnitLabel = "ng",
                    ID = new Guid("93db8c40-4dd0-46a4-ade6-db51bcbca66f"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Pico"
                },
                new UnitChoice
                {
                    UnitName = "atom mass unit",
                    UnitLabel = "u",
                    ID = new Guid("f470168e-1e20-458e-b6da-6bee551cb6d6"),
                    ConversionFactorFromSIFormula = "1.0/Factors.AtomicMass"
                },
                new UnitChoice
                {
                    UnitName = "tonne metric",
                    UnitLabel = "t",
                    ID = new Guid("320b99ba-3115-42f5-939c-15a04d9e7e3c"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Kilo"
                },
                new UnitChoice
                {
                    UnitName = "kilotonne",
                    UnitLabel = "kt",
                    ID = new Guid("2a767cda-fc61-4aa4-81dd-1a4f6d6af755"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Mega"
                },
                 new UnitChoice
                {
                    UnitName = "megatonne",
                    UnitLabel = "Mt",
                    ID = new Guid("92c4b624-4205-4596-aabf-1dd4aa442718"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Giga"
                },
                 new UnitChoice
                {
                    UnitName = "gigatonne",
                    UnitLabel = "Gt",
                    ID = new Guid("51cd0591-d741-4769-bd22-e36959d1adcf"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Tera"
                },
                  new UnitChoice
                {
                    UnitName = "pound",
                    UnitLabel = "lb",
                    ID = new Guid("e9e313ad-cb28-43fe-93fd-7f94dfee1878"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Pound"
                },
                new UnitChoice
                {
                   UnitName = "kilopound",
                   UnitLabel = "klb",
                   ID = new Guid("777ff8ee-edc2-46d1-ac40-f097c1e1cd69"),
                   ConversionFactorFromSIFormula = "1.0/(Factors.Kilo * Factors.Pound)"
                },
                new UnitChoice
                {
                    UnitName = "ounce",
                    UnitLabel = "oz",
                    ID = new Guid("4e64e69b-2276-46c8-a918-06ab6980178c"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Ounce"
                },
                 new UnitChoice
                {
                    UnitName = "stone",
                    UnitLabel = "st",
                    ID = new Guid("6894dc1c-21e2-42aa-9569-759c0e6e6d6e"),
                    ConversionFactorFromSIFormula = "1.0/ Factors.Stone"
                },
                 new UnitChoice
                {
                    UnitName = "ton UK",
                    UnitLabel = "LT",
                    ID = new Guid("059c7b81-ed11-410e-9466-4661011372d2"),
                    ConversionFactorFromSIFormula = "1.0 / Factors.TonUK"
                },
                  new UnitChoice
                {
                    UnitName = "ton US",
                    UnitLabel = "ST",
                    ID = new Guid("443af797-a62f-4137-a852-ad1c9163dd7b"),
                    ConversionFactorFromSIFormula = "1.0/ Factors.TonUS"
                },
                   new UnitChoice
                {
                    UnitName = "solar mass",
                    UnitLabel = "M☉",
                    ID = new Guid("432e73bf-a448-47f6-9c65-9339d5bac5a3"),
                    ConversionFactorFromSIFormula = "1.0/Factors.SolarMass"
                },
                    new UnitChoice
                {
                    UnitName = "earth mass",
                    UnitLabel = "M🜨",
                    ID = new Guid("f9303406-dfce-45c4-9a1e-299d9bac1d4e"),
                    ConversionFactorFromSIFormula = "1.0/Factors.EarthMass"
                },
                    new UnitChoice
                {
                    UnitName = "grain",
                    UnitLabel = "gr",
                    ID = new Guid("dad9b0a5-ce14-4132-b571-6365ab336bc2"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Grain"
                },
                    new UnitChoice
                {
                    UnitName = "hundred weights",
                    UnitLabel = "cwt",
                    ID = new Guid("83810f2a-b260-41b3-bc13-5ef60290f214"),
                    ConversionFactorFromSIFormula = "1.0/(100.0*Factors.Pound)"
                }
        };

        public MassQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "mass" };
            ID = new Guid("99d13248-c303-4b3d-b062-af98de701d6f");
            DescriptionMD = string.Empty;
            DescriptionMD += @"Mass can be experimentally defined as a measure of the body's inertia, meaning the resistance to acceleration (change of velocity) when a net force is applied. The object's mass also determines the strength of its gravitational attraction to other bodies." + Environment.NewLine;
            DescriptionMD += @"The dimension of mass is:" + Environment.NewLine;
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

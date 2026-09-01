using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents data storage.
    /// </summary>
    public partial class DataStorageQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "D";
        public override string SIUnitName { get; } = "bit";
        public override string SIUnitLabelLatex { get; } = "bit";

        public override double? MeaningfulPrecisionInSI { get; } = 1;

        private static DataStorageQuantity instance_ = null;
        public static DataStorageQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new DataStorageQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "bit",
                UnitLabel = "bit",
                ID = new Guid("ef371794-6550-40be-a7eb-fe4e7b6cadf4"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "byte",
                UnitLabel = "B",
                ID = new Guid("ae06779c-0ffb-45f9-a4ea-3597677b1210"),
                ConversionFactorFromSIFormula = "1.0/Factors.BitsPerByte",
            },
            new UnitChoice
            {
                UnitName = "kilobit",
                UnitLabel = "kbit",
                ID = new Guid("db6bfea5-1351-4bd8-a119-0639f8fc5e15"),
                ConversionFactorFromSIFormula = "1.0/Factors.Kilo",
            },
            new UnitChoice
            {
                UnitName = "kilobyte",
                UnitLabel = "kB",
                ID = new Guid("5309aeca-c57a-412e-9080-57708fea1cd9"),
                ConversionFactorFromSIFormula = "1.0/(Factors.BitsPerByte*Factors.Kilo)",
            }
        };

        public DataStorageQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "data storage", "information capacity" };
            ID = new Guid("62c44d7b-45ca-48d0-8715-692edadf31b7");
            DescriptionMD = "**data storage** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is bit with unit label $bit$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}

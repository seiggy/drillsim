using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class TemperatureQuantity : SymbolizedBasePhysicalQuantity
    {
        public override string DimensionSymbol { get; } = "Θ";
        public override string SIUnitName { get; } = "kelvin";

        public override string SIUnitLabelLatex { get; } = "K";
        private static TemperatureQuantity instance_ = null;

        public override double TemperatureDimension { get; } = 1;

        public static TemperatureQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new TemperatureQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
                new UnitChoice
                {
                    UnitName = "kelvin",
                    UnitLabel = "K",
                    ID = new Guid("8fc5fa10-2d89-4064-8ace-b852d9a8d31f"),
                    ConversionFactorFromSIFormula = "Factors.Unit",
                    ConversionBiasFromSIFormula = "0",
                    IsSI = true
                },
                new UnitChoice
                {
                    UnitName = "celsius",
                    UnitLabel = "°C",
                    ID = new Guid("5d69048e-fe18-4923-9341-cb80c2ccf8cc"),
                    ConversionFactorFromSIFormula = "Factors.Unit",
                    ConversionBiasFromSIFormula = "-Factors.ZeroCelsius"
                },
                new UnitChoice
                {
                    UnitName = "fahrenheit",
                    UnitLabel = "°F",
                    ID = new Guid("55c289ab-6975-439f-9b7a-fdca6d219a9f"),
                    ConversionFactorFromSIFormula = "1.0/Factors.FahrenheitSlope",
                    ConversionBiasFromSIFormula = "-Factors.FahrenheitBias"
                },
                new UnitChoice
                {
                    UnitName = "rankine",
                    UnitLabel = "°R",
                    ID = new Guid("b4d6c55d-cf05-46e1-a09b-d0b26eba634a"),
                    ConversionFactorFromSIFormula = "1.0/Factors.FahrenheitSlope",
                    ConversionBiasFromSIFormula = "0"
                },
                new UnitChoice
                {
                    UnitName = "réaumur",
                    UnitLabel = "°Ré",
                    ID = new Guid("968def6c-bc85-49b0-84a8-3ac7ad37efc6"),
                    ConversionFactorFromSIFormula = "1.0/Factors.ReaumurSlope",
                    ConversionBiasFromSIFormula = "-Factors.ReaumurBias"
                }
        };
        public TemperatureQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "temperature" };
            ID = new Guid("16130f2d-72a8-44a5-beaa-adbb5a1a7b21");
            DescriptionMD = string.Empty;
            DescriptionMD += @"Temperature is a measure of the average kinetic energy of particles in a substance, indicating how hot or cold the substance is." + Environment.NewLine;
            DescriptionMD += @"The dimension of temperature is:" + Environment.NewLine;
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

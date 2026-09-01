using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion
{
    public partial class RelativeTemperatureQuantity : TemperatureQuantity
    {

        private static RelativeTemperatureQuantity instance_ = null;

        public static new RelativeTemperatureQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new RelativeTemperatureQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            TemperatureQuantity.Instance.GetUnitChoice(TemperatureQuantity.UnitChoicesEnum.Kelvin),
            new UnitChoice
            {
                UnitName = "relative celsius",
                UnitLabel = "°C",
                ID = new Guid("10ea31a1-e661-41c9-9a3d-245904b73599"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit"
            },
            new UnitChoice()
            {
                UnitName = "rankine",
                UnitLabel = "°R",
                ID = new Guid("62f3ffbc-eda3-400a-9fb7-8d021771f0fa"),
                ConversionFactorFromSIFormula = "1.0/Factors.FahrenheitSlope"
            }
        };
        public RelativeTemperatureQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "relative temperature" };
            ID = new Guid("58dadec7-7858-414b-8d7b-66504d5c2793");
            DescriptionMD = string.Empty;
            DescriptionMD += @"Relative temperature is a measure of temperature expressed in relation to a reference point or baseline, often used to compare temperature differences rather than absolute values." + Environment.NewLine;
            DescriptionMD += @"The dimension of relative temperature is:" + Environment.NewLine;
            DescriptionMD += "$" + GetDimensionsEnclosed() + "$." + Environment.NewLine;
            if (!string.IsNullOrEmpty(SIUnitLabelLatex) && !string.IsNullOrEmpty(SIUnitName) && UsualNames != null && UsualNames.Count > 0)
            {
                DescriptionMD += Environment.NewLine;
                DescriptionMD += @"The SI unit for **" + UsualNames.First() + "** is: " + SIUnitName + " with the associated unit label $" + SIUnitLabelLatex + "$" + Environment.NewLine;
            }
            Reset();
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}

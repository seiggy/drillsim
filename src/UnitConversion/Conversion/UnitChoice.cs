using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public class UnitChoice
    {
        /// <summary>
        /// true if it is the standard SI unit
        /// </summary>
        public bool IsSI { get; set; }
        /// <summary>
        /// true if it is a unit choice of one the default unit systems
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// the name for that unit choice
        /// </summary>
        public string UnitName { get; set; } = null;
        /// <summary>
        /// the label used for that unit choice
        /// </summary>
        public string UnitLabel { get; set; }
        /// <summary>
        /// Alternative spellings and symbols accepted for this unit choice.
        /// Canonical names and labels remain in <see cref="UnitName"/> and
        /// <see cref="UnitLabel"/>.
        /// </summary>
        public HashSet<string> Synonyms { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// the SI Unit name corresponding to this unit choice
        /// </summary>
        public string? SIUnitName { get; set; } = null;
        /// <summary>
        /// a global unique identifier
        /// </summary>
        public Guid ID { get; set; } = Guid.Empty;
        /// <summary>
        /// If the formula for calculating the Conversion factor is put here, then the ConversionFactorFromSI is overwritten by the result of the calculation
        /// </summary>
        public string? ConversionFactorFromSIFormula { get; set; } = null;
        /// <summary>
        /// If the formula for calculating the Conversion bias is put here, then the ConversionBiasFromSI is overwritten by the result of the calculation
        /// </summary>
        public string? ConversionBiasFromSIFormula { get; set; } = null;
        /// <summary>
        /// the conversion factor from SI unit: value_in_unit_choice = ConversionFactorFromSI * value_in_SI + ConversionBiasFromSI
        /// </summary>
        public double ConversionFactorFromSI { get; set; } = 1.0;
        /// <summary>
        /// the conversion bias used when converting from SI unit: value_in_unit_choice = ConversionFactorFromSI * value_in_SI + ConversionBiasFromSI
        /// </summary>
        public double ConversionBiasFromSI { get; set; } = 0.0;
        public string? ConversionDescription { get; set; } = null;
        /// <summary>
        /// default constructor
        /// </summary>
        public UnitChoice() : base()
        {
        }
        /// <summary>
        /// copy constructor. Copy only the name and conversion parametres
        /// </summary>
        /// <param name="reference"></param>
        public UnitChoice(UnitChoice reference) : base()
        {
            if (reference != null)
            {
                UnitName = reference.UnitName;
                UnitLabel = reference.UnitLabel;
                Synonyms = new HashSet<string>(reference.Synonyms ?? [], StringComparer.OrdinalIgnoreCase);
                ConversionFactorFromSIFormula = reference.ConversionFactorFromSIFormula;
                ConversionBiasFromSIFormula = reference.ConversionBiasFromSIFormula;
                ConversionFactorFromSI = reference.ConversionFactorFromSI;
                ConversionBiasFromSI = reference.ConversionBiasFromSI;
                ConversionDescription = reference.ConversionDescription;
                SIUnitName = reference.SIUnitName;
            }
        }

        internal void SupplementSynonyms()
        {
            Synonyms ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            AddSpellingVariant(UnitName, "metre", "meter");
            AddSpellingVariant(UnitName, "litre", "liter");
            AddSpellingVariant(UnitName, "UK gallon", "imperial gallon");
            AddSpellingVariant(UnitName, "UK gallon", "British gallon");
            AddSpellingVariant(UnitName, "US gallon", "US liquid gallon");

            if (!string.IsNullOrWhiteSpace(UnitLabel))
            {
                var variants = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { UnitLabel };
                Expand(variants, "²", "^2");
                Expand(variants, "³", "^3");
                Expand(variants, "⁴", "^4");
                Expand(variants, "•", "*");
                Expand(variants, "•", "·");
                Expand(variants, "µ", "u");
                Expand(variants, "µ", "μ");
                Expand(variants, "°", "deg");
                Expand(variants, "UKGal", "ImpGal");
                foreach (string variant in variants)
                {
                    if (!variant.Equals(UnitLabel, StringComparison.OrdinalIgnoreCase))
                    {
                        Synonyms.Add(variant);
                    }
                }
            }

            Synonyms.Remove(UnitName ?? string.Empty);
            Synonyms.Remove(UnitLabel ?? string.Empty);
        }

        private void AddSpellingVariant(string? value, string source, string replacement)
        {
            if (!string.IsNullOrWhiteSpace(value) && value.Contains(source, StringComparison.OrdinalIgnoreCase))
            {
                Synonyms.Add(value.Replace(source, replacement, StringComparison.OrdinalIgnoreCase));
            }
        }

        private static void Expand(HashSet<string> values, string source, string replacement)
        {
            foreach (string value in values.ToArray())
            {
                if (value.Contains(source, StringComparison.Ordinal))
                {
                    values.Add(value.Replace(source, replacement, StringComparison.Ordinal));
                }
            }
        }

        public string GetVariableName()
        {
            string varName = string.Empty;
            if (!string.IsNullOrEmpty(UnitName))
            {
                string[] names = UnitName.Replace("Å","Aa").Replace("å","aa").Replace("Ø","Oe").Replace("ø","oe").Replace("Æ","ae").Replace("æ","ae").Split(' ', '\t', '-', '_', '/', '\\', '&', '%', '(',')', '\'');
                if (names != null)
                {
                    foreach (string name in names)
                    {
                        if (name.Length > 0)
                        {
                            varName += char.ToUpper(name[0]) + name.Substring(1).ToLowerInvariant();
                        }      
                    }
                }
            }
            return varName;
        }

        /// <summary>
        /// convert a SI value into the unit choice
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public double FromSI(double val)
        {
            return val * ConversionFactorFromSI + ConversionBiasFromSI;
        }
        /// <summary>
        /// convert a value in the unit choice into a SI value
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public double ToSI(double val)
        {
            return (val - ConversionBiasFromSI) / ConversionFactorFromSI;
        }
        /// <summary>
        /// generate the truncated string (to a given meaningful precision given in SI) associated to the given SI value, to be converted into the unit choice
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public string FromSI(double val, double? meaningfulPrecisionInSI)
        {
            double valInUnit = FromSI(val);
            return ToStringInUnit(valInUnit, meaningfulPrecisionInSI);
        }

        /// <summary>
        /// generate the truncated string (to a given meaningful precision given in SI) associated to the given value, already converted into the unit choice (useful when unit conversion has already been done)
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public string ToStringInUnit(double valInUnit, double? meaningfulPrecisionInSI)
        {
            if (meaningfulPrecisionInSI != null)
            {
                int digits = DigitAccuracy(FromSI((double)meaningfulPrecisionInSI));
                return valInUnit.ToString("N" + ((digits >= 0) ? digits : 0), CultureInfo.InvariantCulture.NumberFormat);
            }
            else
            {
                return valInUnit.ToString(CultureInfo.InvariantCulture.NumberFormat);
            }
        }
        /// <summary>
        /// generate the truncated string (to a given meaningful precision given in SI) associated to the given value in the unit choice, to be converted into SI unit
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public string ToSI(double val, double? meaningfulPrecisionInSI)
        {
            double valInSI = ToSI(val);
            return ToStringInSI(valInSI, meaningfulPrecisionInSI);
        }
        /// <summary>
        /// generate the truncated string (to a given meaningful precision given in SI) associated to the given value in the unit choice, already converted in SI unit (useful when unit conversion has already been done)
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public string ToStringInSI(double valInSI, double? meaningfulPrecisionInSI)
        {
            if (meaningfulPrecisionInSI != null)
            {
                int digits = DigitAccuracy((double)meaningfulPrecisionInSI);
                return valInSI.ToString("N" + ((digits >= 0) ? digits : 0), CultureInfo.InvariantCulture.NumberFormat);
            }
            else
            {
                return valInSI.ToString(CultureInfo.InvariantCulture.NumberFormat);
            }
        }

        public string GetConversionDescription()
        {
            string desc = string.Empty;
            if (!string.IsNullOrEmpty(UnitName))
            {
                List<string> factors = new List<string>();
                if (IsSI)
                {
                    desc += "No conversion necessary as the unit choice is SI" + Environment.NewLine; ;
                }
                else
                {
                    if (!string.IsNullOrEmpty(ConversionFactorFromSIFormula) && !string.IsNullOrEmpty(ConversionBiasFromSIFormula))
                    {
                        desc += "[v] = a * [SI] + b" + Environment.NewLine;
                        desc += "where" + Environment.NewLine;
                        desc += "[v] is the value in " + UnitName + Environment.NewLine;
                        desc += "[SI] is the value in SI";
                        if (!string.IsNullOrEmpty(SIUnitName))
                        {
                            desc += ", i.e., in " + SIUnitName;
                        }
                        desc += Environment.NewLine;
                        desc += "a = " + GetFactorFormulaDescription(factors);
                        desc += "b = " + GetBiasFormulaDescription(factors);
                    }
                    else if (!string.IsNullOrEmpty(ConversionFactorFromSIFormula))
                    {
                        desc += "[v] = a * [SI]" + Environment.NewLine;
                        desc += "where" + Environment.NewLine;
                        desc += "[v] is the value in " + UnitName + Environment.NewLine;
                        desc += "[SI] is the value in SI";
                        if (!string.IsNullOrEmpty(SIUnitName))
                        {
                            desc += ", i.e., in " + SIUnitName;
                        }
                        desc += Environment.NewLine;
                        desc += "a = " + GetFactorFormulaDescription(factors);
                    }
                    else if (!string.IsNullOrEmpty(ConversionBiasFromSIFormula))
                    {
                        desc += "[v] = [SI] - b" + Environment.NewLine;
                        desc += "where" + Environment.NewLine;
                        desc += "[v] is the value in " + UnitName + Environment.NewLine;
                        desc += "[SI] is the value in SI";
                        if (!string.IsNullOrEmpty(SIUnitName))
                        {
                            desc += ", i.e., in " + SIUnitName;
                        }
                        desc += Environment.NewLine;
                        desc += "b = " + GetBiasFormulaDescription(factors);
                    }
                }
                if (factors.Count > 0)
                {
                    GetFactors(factors);
                    desc += "and" + Environment.NewLine;
                    foreach (string factor in factors)
                    {
                        desc += GetFactorDescription(factor);
                    }
                }
            }
            return desc;
        }
        private string GetFactorFormulaDescription(List<string> factors)
        {
            string desc = string.Empty;
            if (!string.IsNullOrEmpty(ConversionFactorFromSIFormula))
            {
                desc += GetDescription(ConversionFactorFromSIFormula);
                GetFactors(ConversionFactorFromSIFormula, factors);
            }
            return desc;
        }
        public string GetBiasFormulaDescription(List<string> factors)
        {
            string desc = string.Empty;
            if (!string.IsNullOrEmpty(ConversionBiasFromSIFormula))
            {
                desc += GetDescription(ConversionBiasFromSIFormula);
                GetFactors(ConversionBiasFromSIFormula, factors);
            }
            return desc;
        }

        private bool GetFactors(string desc, List<string> factors)
        {
            bool found = false;
            if (!string.IsNullOrEmpty(desc))
            {
                int pos = desc.IndexOf("Factors.");
                if (pos >= 0)
                {
                    pos += "Factors.".Length;
                    desc = desc.Substring(pos);
                    string factor = new string(desc.TakeWhile(c => Char.IsLetterOrDigit(c) || c == '_').ToArray());
                    if (!string.IsNullOrEmpty(factor))
                    {
                        pos = factor.Length;
                        factor = "Factors." + factor;
                        if (!factors.Contains(factor))
                        {
                            factors.Add(factor);
                            found = true;
                        }
                        desc = desc.Substring(pos);
                        GetFactors(desc, factors);
                    }
                }
            }
            return found;
        }

        private bool GetFactors(List<string> factors)
        {
            bool found = false;
            List<string> src = new List<string>(factors);
            foreach (var factor in src)
            {
                if (!string.IsNullOrEmpty(factor))
                {
                    string f = factor.Replace("Factors.", "");
                    if (Factors.Descriptions.ContainsKey(f))
                    {
                        string def = Factors.Descriptions[f].Definition;
                        if (!string.IsNullOrEmpty(def))
                        {
                            found |= GetFactors(def, factors);
                        }
                    }
                }
            }
            if (found)
            {
                GetFactors(factors);
            }
            return found;
        }

        private string GetFactorDescription(string factor)
        {
            string desc = string.Empty;
            if (!string.IsNullOrEmpty(factor))
            {
                factor = factor.Replace("Factors.", "");
                if (Factors.Descriptions.ContainsKey(factor))
                {
                    string def = Factors.Descriptions[factor].Definition;
                    if (!string.IsNullOrEmpty(def))
                    {
                        desc += factor + " = " + def.Replace("Factors.", "");
                        if (!string.IsNullOrEmpty(Factors.Descriptions[factor].Reference))
                        {
                            desc += " reference: " + Factors.Descriptions[factor].Reference;
                        }
                        desc += Environment.NewLine;
                    }
                }
            }
            return desc;
        }

        private string GetDescription(string desc)
        {
            string res = string.Empty;
            if (!string.IsNullOrEmpty(desc))
            {
                res += desc.Replace("Factors.", "").Replace("System.Math.", "");
                try
                {
                    var assies = AppDomain.CurrentDomain.GetAssemblies().Where(assembly => !string.IsNullOrEmpty(assembly.Location));
                    double dres = CSharpScript.EvaluateAsync<double>(desc, ScriptOptions.Default.WithReferences(assies).WithImports("OSDC.UnitConversion.Conversion", "System.Math")).GetAwaiter().GetResult();
                    if (!double.IsNaN(dres) && !double.IsInfinity(dres))
                    {
                        res += ", i.e., " + dres.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    }
                }
                catch (Exception ex)
                {
                }
                res += Environment.NewLine;
            }
            return res;
        }
        /// <summary>
        /// return the number of digits after the decimal separator that corresponds to this accuracy
        /// </summary>
        /// <param name="accuracy"></param>
        /// <returns></returns>
        private int DigitAccuracy(double accuracy)
        {
            if (accuracy <= 0)
            {
                return 0;
            }
            else
            {
                return (int)Math.Round(Math.Log10(1 / accuracy));
            }
        }
    }
}

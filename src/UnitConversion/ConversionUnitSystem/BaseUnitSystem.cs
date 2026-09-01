using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ConstrainedExecution;

namespace OSDC.UnitConversion.Conversion.UnitSystem
{
    public class BaseUnitSystem
    {
        public enum DefaultUnitSystemEnum { SI = 1, Metric = 2, Imperial = 4, US = 8 }

        private static Dictionary<Guid, BaseUnitSystem> baseUnitSystems_ = null;

        private static List<BasePhysicalQuantity> availableBasePhysicalQuantities_ = null;

        private static BaseUnitSystem SIBaseUnitSystem_ = null;
        private static BaseUnitSystem MetricBaseUnitSystem_ = null;
        private static BaseUnitSystem ImperialBaseUnitSystem_ = null;
        private static BaseUnitSystem USBaseUnitSystem_ = null;

        public static BaseUnitSystem Get(Guid ID)
        {
            if (baseUnitSystems_ == null)
            {
                Initialize();
            }
            BaseUnitSystem result;
            baseUnitSystems_.TryGetValue(ID, out result);
            return result;
        }

        public static BaseUnitSystem CreateNew()
        {
            if (baseUnitSystems_ == null)
            {
                Initialize();
            }
            BaseUnitSystem unitSystem = new BaseUnitSystem();
            unitSystem.ID = Guid.NewGuid();
            baseUnitSystems_.Add(unitSystem.ID, unitSystem);
            return unitSystem;
        }

        public static BaseUnitSystem CreateNew(Guid guid)
        {
            if (baseUnitSystems_ == null)
            {
                Initialize();
            }
            if (baseUnitSystems_.ContainsKey(guid))
            {
                BaseUnitSystem result;
                baseUnitSystems_.TryGetValue(guid, out result);
                return result;
            }
            else
            {
                BaseUnitSystem unitSystem = new BaseUnitSystem();
                unitSystem.ID = guid;
                baseUnitSystems_.Add(unitSystem.ID, unitSystem);
                return unitSystem;
            }
        }

        private static void Initialize()
        {
            if (baseUnitSystems_ == null)
            {
                baseUnitSystems_ = new Dictionary<Guid, BaseUnitSystem>();
                BaseUnitSystem SI = SIBaseUnitSystem;
                baseUnitSystems_.Add(SI.ID, SI);
                BaseUnitSystem metric = MetricBaseUnitSystem;
                baseUnitSystems_.Add(metric.ID, metric);
                BaseUnitSystem US = USBaseUnitSystem;
                baseUnitSystems_.Add(US.ID, US);
                BaseUnitSystem imperial = ImperialBaseUnitSystem;
                baseUnitSystems_.Add(imperial.ID, imperial);
            }
        }

        public static BaseUnitSystem SIBaseUnitSystem
        {
            get
            {
                if (SIBaseUnitSystem_ == null)
                {
                    SIBaseUnitSystem_ = new BaseUnitSystem(DefaultUnitSystemEnum.SI);
                }
                return SIBaseUnitSystem_;
            }
        }

        public static BaseUnitSystem MetricBaseUnitSystem
        {
            get
            {
                if (MetricBaseUnitSystem_ == null)
                {
                    MetricBaseUnitSystem_ = new BaseUnitSystem(DefaultUnitSystemEnum.Metric);
                }
                return MetricBaseUnitSystem_;
            }
        }

        public static BaseUnitSystem ImperialBaseUnitSystem
        {
            get
            {
                if (ImperialBaseUnitSystem_ == null)
                {
                    ImperialBaseUnitSystem_ = new BaseUnitSystem(DefaultUnitSystemEnum.Imperial);
                }
                return ImperialBaseUnitSystem_;
            }
        }
        public static BaseUnitSystem USBaseUnitSystem
        {
            get
            {
                if (USBaseUnitSystem_ == null)
                {
                    USBaseUnitSystem_ = new BaseUnitSystem(DefaultUnitSystemEnum.US);
                }
                return USBaseUnitSystem_;
            }
        }

        public Dictionary<string, string> Choices { get; set; } = new Dictionary<string, string>();

        protected Guid StringToGUID(string val)
        {
            Guid result = Guid.Empty;
            if (!string.IsNullOrEmpty(val))
            {
                try
                {
                    result = new Guid(val);
                }
                catch(Exception)
                {

                }
            }
            return result;
        }
        /// <summary>
        /// a MetaInfo for the base unit system
        /// </summary>
        public Guid ID { get; set; }
        /// <summary>
        /// a name for the unit system
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// a description of the unit system
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// true if it is one the default unit system
        /// </summary>
        public bool IsDefault { get; set; }
        /// <summary>
        /// true if it is the SI unit system
        /// </summary>
        public bool IsSI { get; set; }

        /// <summary>
        /// default constructor
        /// </summary>
        public BaseUnitSystem() : base()
        {

        }

        public static List<BasePhysicalQuantity>? AvailableBasePhysicalQuantities
        {
            get
            {
                if (availableBasePhysicalQuantities_ == null)
                {
                    List<Type> subclasses = GetAllSubclasses(typeof(BasePhysicalQuantity));
                    if (subclasses != null)
                    {
                        foreach (Type typ in subclasses)
                        {
                            if (typ.IsSubclassOf(typeof(BasePhysicalQuantity)))
                            {
                                MethodInfo? method = null;
                                foreach (MethodInfo meth in typ.GetMethods())
                                {
                                    if (meth.IsStatic &&
                                        meth.Name.EndsWith("Instance") &&
                                        meth.ReturnType.IsSubclassOf(typeof(BasePhysicalQuantity)))
                                    {
                                        method = meth;
                                        break;
                                    }
                                }
                                // call the method
                                if (method != null)
                                {
                                    object? obj = method.Invoke(null, null);
                                    if (obj != null)
                                    {
                                        var res = (BasePhysicalQuantity)obj;
                                        if (availableBasePhysicalQuantities_ == null)
                                        {
                                            availableBasePhysicalQuantities_ = new List<BasePhysicalQuantity>();
                                        }
                                        availableBasePhysicalQuantities_.Add(res);
                                    }
                                }
                            }
                        }
                    }
                }
                return availableBasePhysicalQuantities_;
            }
        }

        private static List<Type> GetAllSubclasses(Type baseType)
        {
            List<Type> result = new List<Type>();
            Assembly? assy = Assembly.GetAssembly(baseType);
            if (assy != null)
            {
                Queue<Type> typesToProcess = new Queue<Type>();

                typesToProcess.Enqueue(baseType);

                while (typesToProcess.Count > 0)
                {
                    Type currentType = typesToProcess.Dequeue();

                    IEnumerable<Type> subclasses = assy.GetTypes().Where(t => t.IsClass && t.IsSubclassOf(currentType));

                    foreach (var subclass in subclasses)
                    {
                        if (!result.Contains(subclass))
                        {
                            result.Add(subclass);
                            typesToProcess.Enqueue(subclass);
                        }
                    }
                }
            }
            return result;
        }

        protected virtual BasePhysicalQuantity GetQuantity(Guid quantityID)
        {
            return BasePhysicalQuantity.GetQuantity(quantityID);
        }

        protected virtual BasePhysicalQuantity GetQuantity(string quantityName)
        {
            return BasePhysicalQuantity.GetQuantity(quantityName);
        }

        protected virtual BasePhysicalQuantity GetQuantity(BasePhysicalQuantity.QuantityEnum quantityChoice)
        {
            return BasePhysicalQuantity.GetQuantity(quantityChoice);
        }

        public UnitChoice GetChoice(Guid physicalQuantityID)
        {
            UnitChoice choice = null;
            string unitChoiceID;
            BasePhysicalQuantity quantity = GetQuantity(physicalQuantityID);
            if (quantity != null && Choices.TryGetValue(physicalQuantityID.ToString(), out unitChoiceID))
            {
                Guid ID = StringToGUID(unitChoiceID);
                if (ID != Guid.Empty)
                {
                    choice = quantity.GetUnitChoice(ID);
                }
            }
            return choice;
        }
        public UnitChoice GetChoice(string physicalQuantityName)
        {
            UnitChoice choice = null;
            string unitChoiceID;
            BasePhysicalQuantity quantity = GetQuantity(physicalQuantityName);
            if (quantity != null &&  Choices.TryGetValue(quantity.ID.ToString(), out unitChoiceID))
            {
                Guid ID = StringToGUID(unitChoiceID);
                if (ID != Guid.Empty)
                {
                    choice = quantity.GetUnitChoice(ID);
                }
            }
            return choice;
        }
        public UnitChoice GetChoice(BasePhysicalQuantity.QuantityEnum physicalQuantityID)
        {
            UnitChoice choice = null;
            string unitChoiceID;
            BasePhysicalQuantity quantity = GetQuantity(physicalQuantityID);
            if (quantity != null && Choices.TryGetValue(quantity.ID.ToString(), out unitChoiceID))
            {
                Guid ID = StringToGUID(unitChoiceID);
                if (ID != Guid.Empty)
                {
                    choice = quantity.GetUnitChoice(ID);
                }
            }
            return choice;
        }

        public double? FromSI(string physicalQuantityName, double val)
        {
            UnitChoice choice = GetChoice(physicalQuantityName);
            if (choice != null)
            {
                return choice.FromSI(val);
            }
            else
            {
                return null;
            }
        }

        public double? FromSI(Guid physicalQuantityID, double val)
        {
            UnitChoice choice = GetChoice(physicalQuantityID);
            if (choice != null)
            {
                return choice.FromSI(val);
            }
            else
            {
                return null;
            }
        }

        public double? FromSI(BasePhysicalQuantity.QuantityEnum physicalQuantityID, double val)
        {
            UnitChoice choice = GetChoice(physicalQuantityID);
            if (choice != null)
            {
                return choice.FromSI(val);
            }
            else
            {
                return null;
            }
        }

        public string FromSIString(string physicalQuantityName, double val)
        {
            double? meaningfulPrecision = null;
            BasePhysicalQuantity quantity = GetQuantity(physicalQuantityName);
            if (quantity != null)
            {
                meaningfulPrecision = quantity.MeaningfulPrecisionInSI;
            }
            UnitChoice choice = GetChoice(physicalQuantityName);
            if (choice != null)
            {
                return choice.FromSI(val, meaningfulPrecision);
            }
            else
            {
                return null;
            }
        }

        public string FromSIString(Guid physicalQuantityID, double val)
        {
            double? meaningfulPrecision = null;
            BasePhysicalQuantity quantity = GetQuantity(physicalQuantityID);
            if (quantity != null)
            {
                meaningfulPrecision = quantity.MeaningfulPrecisionInSI;
            }
            UnitChoice choice = GetChoice(physicalQuantityID);
            if (choice != null)
            {
                return choice.FromSI(val, meaningfulPrecision);
            }
            else
            {
                return null;
            }
        }

        public string FromSIString(BasePhysicalQuantity.QuantityEnum physicalQuantityID, double val)
        {
            double? meaningfulPrecision = null;
            BasePhysicalQuantity quantity = GetQuantity(physicalQuantityID);
            if (quantity != null)
            {
                meaningfulPrecision = quantity.MeaningfulPrecisionInSI;
            }
            UnitChoice choice = GetChoice(physicalQuantityID);
            if (choice != null)
            {
                return choice.FromSI(val, meaningfulPrecision);
            }
            else
            {
                return null;
            }
        }


        public double? ToSI(string physicalQuantityName, double val)
        {
            UnitChoice choice = GetChoice(physicalQuantityName);
            if (choice != null)
            {
                return choice.ToSI(val);
            }
            else
            {
                return null;
            }
        }
        public double? ToSI(Guid physicalQuantityID, double val)
        {
            UnitChoice choice = GetChoice(physicalQuantityID);
            if (choice != null)
            {
                return choice.ToSI(val);
            }
            else
            {
                return null;
            }
        }

        public double? ToSI(BasePhysicalQuantity.QuantityEnum physicalQuantityID, double val)
        {
            UnitChoice choice = GetChoice(physicalQuantityID);
            if (choice != null)
            {
                return choice.ToSI(val);
            }
            else
            {
                return null;
            }
        }
        public string ToSIString(string physicalQuantityName, double val)
        {
            double? meaningfulPrecision = null;
            BasePhysicalQuantity quantity = GetQuantity(physicalQuantityName);
            if (quantity != null)
            {
                meaningfulPrecision = quantity.MeaningfulPrecisionInSI;
            }
            UnitChoice choice = GetChoice(physicalQuantityName);
            if (choice != null)
            {
                return choice.ToSI(val, meaningfulPrecision);
            }
            else
            {
                return null;
            }
        }

        public string ToSIString(Guid physicalQuantityID, double val)
        {
            double? meaningfulPrecision = null;
            BasePhysicalQuantity quantity = GetQuantity(physicalQuantityID);
            if (quantity != null)
            {
                meaningfulPrecision = quantity.MeaningfulPrecisionInSI;
            }
            UnitChoice choice = GetChoice(physicalQuantityID);
            if (choice != null)
            {
                return choice.ToSI(val, meaningfulPrecision);
            }
            else
            {
                return null;
            }
        }

        public string ToSIString(BasePhysicalQuantity.QuantityEnum physicalQuantityID, double val)
        {
            double? meaningfulPrecision = null;
            BasePhysicalQuantity quantity = GetQuantity(physicalQuantityID);
            if (quantity != null)
            {
                meaningfulPrecision = quantity.MeaningfulPrecisionInSI;
            }
            UnitChoice choice = GetChoice(physicalQuantityID);
            if (choice != null)
            {
                return choice.ToSI(val, meaningfulPrecision);
            }
            else
            {
                return null;
            }
        }

        public string GetUnitLabel(string physicalQuantityName)
        {
            UnitChoice choice = GetChoice(physicalQuantityName);
            if (choice != null)
            {
                return choice.UnitLabel;
            }
            else
            {
                return null;
            }
        }
        public string GetUnitLabel(Guid physicalQuantityID)
        {
            UnitChoice choice = GetChoice(physicalQuantityID);
            if (choice != null)
            {
                return choice.UnitLabel;
            }
            else
            {
                return null;
            }
        }

        public string GetUnitLabel(BasePhysicalQuantity.QuantityEnum physicalQuantityID)
        {
            UnitChoice choice = GetChoice(physicalQuantityID);
            if (choice != null)
            {
                return choice.UnitLabel;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="defaultUnitChoice"></param>
        protected BaseUnitSystem(DefaultUnitSystemEnum defaultUnitChoice)
        {
            List<BasePhysicalQuantity> quantities = BaseUnitSystem.AvailableBasePhysicalQuantities;
            if (quantities != null)
            {
                /*
                // generate
                foreach(PhysicalQuantity quantity in quantities)
                {
                    Console.WriteLine("Choices.Add(" + quantity.GetType().Name + ".Instance.ID, " + quantity.GetType().Name + ".Instance.GetUnitChoice(" + quantity.GetType().Name + ".UnitChoicesEnum.).ID);");
                }
                */
                if (defaultUnitChoice == DefaultUnitSystemEnum.SI)
                {
                    IsSI = true;
                    IsDefault = true;
                    ID = new Guid("a02672dc-17fc-474d-a4de-397e0d39325a");
                    Name = "SI";
                    Description = "International System of Units";
                    foreach (BasePhysicalQuantity quantity in quantities)
                    {
                        if (quantity.ID != Guid.Empty)
                        {
                            Guid unitChoiceID = Guid.Empty;
                            if (quantity.UnitChoices != null)
                            {
                                foreach (UnitChoice unitChoice in quantity.UnitChoices)
                                {
                                    if (unitChoice.IsSI)
                                    {
                                        unitChoiceID = unitChoice.ID;
                                        break;
                                    }
                                }
                            }
                            if (unitChoiceID != Guid.Empty)
                            {
                                if (!Choices.ContainsKey(quantity.ID.ToString()))
                                {
                                    Choices.Add(quantity.ID.ToString(), unitChoiceID.ToString());
                                }
                                else
                                {
                                    throw new Exception("duplicate unit choice guid (" + quantity.ID.ToString() + ", " + quantity.Name + ") for base quantity: " + quantity.Name);
                                }
                            }
                            else
                            {
                                throw new Exception("missing default unit choice for this physical quantity");
                            }
                        }
                    }
                }
                else if (defaultUnitChoice == DefaultUnitSystemEnum.Metric)
                {
                    IsDefault = true;
                    ID = new Guid("54191fdc-e88e-46db-ba05-9efce3d4fc0b");
                    Name = "Metric";
                    Description = "Metric System of Units";

                    Choices.Add(AccelerationQuantity.Instance.ID.ToString(), AccelerationQuantity.Instance.GetUnitChoice(AccelerationQuantity.UnitChoicesEnum.MetrePerSecondSquared).ID.ToString());
                    Choices.Add(AmountSubstanceQuantity.Instance.ID.ToString(), AmountSubstanceQuantity.Instance.GetUnitChoice(AmountSubstanceQuantity.UnitChoicesEnum.Mole).ID.ToString());
                    Choices.Add(AngleMagneticFluxDensityQuantity.Instance.ID.ToString(), AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.RadianTesla).ID.ToString());
                    Choices.Add(AngleGradientPerLengthQuantity.Instance.ID.ToString(), AngleGradientPerLengthQuantity.Instance.GetUnitChoice(AngleGradientPerLengthQuantity.UnitChoicesEnum.DegreePerMetre).ID.ToString());
                    Choices.Add(AngularAccelerationQuantity.Instance.ID.ToString(), AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.DegreePerSecondSquared).ID.ToString());
                    Choices.Add(AngularVelocityQuantity.Instance.ID.ToString(), AngularVelocityQuantity.Instance.GetUnitChoice(AngularVelocityQuantity.UnitChoicesEnum.DegreePerSecond).ID.ToString());
                    Choices.Add(AreaQuantity.Instance.ID.ToString(), AreaQuantity.Instance.GetUnitChoice(AreaQuantity.UnitChoicesEnum.SquareMetre).ID.ToString());
                    Choices.Add(BendingMomentQuantity.Instance.ID.ToString(), BendingMomentQuantity.Instance.GetUnitChoice(BendingMomentQuantity.UnitChoicesEnum.NewtonMetre).ID.ToString());
                    Choices.Add(CompressibilityQuantity.Instance.ID.ToString(), CompressibilityQuantity.Instance.GetUnitChoice(CompressibilityQuantity.UnitChoicesEnum.InverseBar).ID.ToString());
                    Choices.Add(CurvatureQuantity.Instance.ID.ToString(), CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.DegreePer30m).ID.ToString());
                    Choices.Add(SpecificVolumeQuantity.Instance.ID.ToString(), SpecificVolumeQuantity.Instance.GetUnitChoice(SpecificVolumeQuantity.UnitChoicesEnum.CubicCentimetrePerGram).ID.ToString());
                    Choices.Add(SpecificVolumeSquaredQuantity.Instance.ID.ToString(), SpecificVolumeSquaredQuantity.Instance.GetUnitChoice(SpecificVolumeSquaredQuantity.UnitChoicesEnum.CubicCentimetreSquaredPerGramSquared).ID.ToString());
                    Choices.Add(MassDensityGradientPerPressureQuantity.Instance.ID.ToString(), MassDensityGradientPerPressureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerGigapascal).ID.ToString());
                    Choices.Add(MassDensityGradientPerPressureSquaredQuantity.Instance.ID.ToString(), MassDensityGradientPerPressureSquaredQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerGigapascalSquared).ID.ToString());
                    Choices.Add(MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.ID.ToString(), MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerGigapascalSquaredKelvin).ID.ToString());
                    Choices.Add(MassDensityGradientPerPressureTemperatureQuantity.Instance.ID.ToString(), MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerGigapascalKelvin).ID.ToString());
                    Choices.Add(MassDensityGradientPerLengthQuantity.Instance.ID.ToString(), MassDensityGradientPerLengthQuantity.Instance.GetUnitChoice(MassDensityGradientPerLengthQuantity.UnitChoicesEnum.GramPerCubicCentimetrePer100Metre).ID.ToString());
                    Choices.Add(MassDensityGradientPerTemperatureQuantity.Instance.ID.ToString(), MassDensityGradientPerTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerTemperatureQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerCelsius).ID.ToString());
                    Choices.Add(MassDensityQuantity.Instance.ID.ToString(), MassDensityQuantity.Instance.GetUnitChoice(MassDensityQuantity.UnitChoicesEnum.GramPerCubicCentimetre).ID.ToString());
                    Choices.Add(MassDensityRateOfChangeQuantity.Instance.ID.ToString(), MassDensityRateOfChangeQuantity.Instance.GetUnitChoice(MassDensityRateOfChangeQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerHour).ID.ToString());
                    Choices.Add(DimensionlessQuantity.Instance.ID.ToString(), DimensionlessQuantity.Instance.GetUnitChoice(DimensionlessQuantity.UnitChoicesEnum.Dimensionless).ID.ToString());
                    Choices.Add(DynamicViscosityQuantity.Instance.ID.ToString(), DynamicViscosityQuantity.Instance.GetUnitChoice(DynamicViscosityQuantity.UnitChoicesEnum.PascalSecond).ID.ToString());
                    Choices.Add(EarthMagneticFluxDensityQuantity.Instance.ID.ToString(), EarthMagneticFluxDensityQuantity.Instance.GetUnitChoice(EarthMagneticFluxDensityQuantity.UnitChoicesEnum.Nanotesla).ID.ToString());
                    Choices.Add(ElectricCapacitanceQuantity.Instance.ID.ToString(), ElectricCapacitanceQuantity.Instance.GetUnitChoice(ElectricCapacitanceQuantity.UnitChoicesEnum.Microfarad).ID.ToString());
                    Choices.Add(ElectricCurrentQuantity.Instance.ID.ToString(), ElectricCurrentQuantity.Instance.GetUnitChoice(ElectricCurrentQuantity.UnitChoicesEnum.Ampere).ID.ToString());
                    Choices.Add(ElectricTensionQuantity.Instance.ID.ToString(), ElectricTensionQuantity.Instance.GetUnitChoice(ElectricTensionQuantity.UnitChoicesEnum.Volt).ID.ToString());
                    Choices.Add(ElongationGradientPerLengthQuantity.Instance.ID.ToString(), ElongationGradientPerLengthQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthQuantity.UnitChoicesEnum.MetrePerMetre).ID.ToString());
                    Choices.Add(EnergyQuantity.Instance.ID.ToString(), EnergyQuantity.Instance.GetUnitChoice(EnergyQuantity.UnitChoicesEnum.Joule).ID.ToString());
                    Choices.Add(EnergyDensityQuantity.Instance.ID.ToString(), EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.JoulePerCubicMetre).ID.ToString());
                    Choices.Add(FluidShearRateQuantity.Instance.ID.ToString(), FluidShearRateQuantity.Instance.GetUnitChoice(FluidShearRateQuantity.UnitChoicesEnum.ReciprocalSecond).ID.ToString());
                    Choices.Add(FluidShearStressQuantity.Instance.ID.ToString(), FluidShearStressQuantity.Instance.GetUnitChoice(FluidShearStressQuantity.UnitChoicesEnum.Pascal).ID.ToString());
                    Choices.Add(ForceGradientPerLengthQuantity.Instance.ID.ToString(), ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.NewtonPerMetre).ID.ToString());
                    Choices.Add(ForceQuantity.Instance.ID.ToString(), ForceQuantity.Instance.GetUnitChoice(ForceQuantity.UnitChoicesEnum.Newton).ID.ToString());
                    Choices.Add(ForceRateOfChangeQuantity.Instance.ID.ToString(), ForceRateOfChangeQuantity.Instance.GetUnitChoice(ForceRateOfChangeQuantity.UnitChoicesEnum.NewtonPerSecond).ID.ToString());
                    Choices.Add(FrequencyQuantity.Instance.ID.ToString(), FrequencyQuantity.Instance.GetUnitChoice(FrequencyQuantity.UnitChoicesEnum.Hertz).ID.ToString());
                    Choices.Add(FrequencyRateOfChangeQuantity.Instance.ID.ToString(), FrequencyRateOfChangeQuantity.Instance.GetUnitChoice(FrequencyRateOfChangeQuantity.UnitChoicesEnum.HertzPerSecond).ID.ToString());
                    Choices.Add(HeatTransferCoefficientQuantity.Instance.ID.ToString(), HeatTransferCoefficientQuantity.Instance.GetUnitChoice(HeatTransferCoefficientQuantity.UnitChoicesEnum.WattPerSquareMetrePerKelvin).ID.ToString());
                    Choices.Add(HydraulicConductivityQuantity.Instance.ID.ToString(), HydraulicConductivityQuantity.Instance.GetUnitChoice(HydraulicConductivityQuantity.UnitChoicesEnum.MetrePerSecond).ID.ToString());
                    Choices.Add(ImageScaleQuantity.Instance.ID.ToString(), ImageScaleQuantity.Instance.GetUnitChoice(ImageScaleQuantity.UnitChoicesEnum.DotPerInch).ID.ToString());
                    Choices.Add(InterfacialTensionQuantity.Instance.ID.ToString(), InterfacialTensionQuantity.Instance.GetUnitChoice(InterfacialTensionQuantity.UnitChoicesEnum.NewtonPerMetre).ID.ToString());
                    Choices.Add(VolumeLargeQuantity.Instance.ID.ToString(), VolumeLargeQuantity.Instance.GetUnitChoice(VolumeLargeQuantity.UnitChoicesEnum.CubicMetre).ID.ToString());
                    Choices.Add(LengthQuantity.Instance.ID.ToString(), LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Metre).ID.ToString());
                    Choices.Add(LuminousIntensityQuantity.Instance.ID.ToString(), LuminousIntensityQuantity.Instance.GetUnitChoice(LuminousIntensityQuantity.UnitChoicesEnum.Candela).ID.ToString());
                    Choices.Add(MagneticFluxDensityQuantity.Instance.ID.ToString(), MagneticFluxDensityQuantity.Instance.GetUnitChoice(MagneticFluxDensityQuantity.UnitChoicesEnum.Gauss).ID.ToString());
                    Choices.Add(MagneticFluxQuantity.Instance.ID.ToString(), MagneticFluxQuantity.Instance.GetUnitChoice(MagneticFluxQuantity.UnitChoicesEnum.Microweber).ID.ToString());
                    Choices.Add(MassGradientPerLengthQuantity.Instance.ID.ToString(), MassGradientPerLengthQuantity.Instance.GetUnitChoice(MassGradientPerLengthQuantity.UnitChoicesEnum.KilogramPerMetre).ID.ToString());
                    Choices.Add(MassQuantity.Instance.ID.ToString(), MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.Kilogram).ID.ToString());
                    Choices.Add(MassRateQuantity.Instance.ID.ToString(), MassRateQuantity.Instance.GetUnitChoice(MassRateQuantity.UnitChoicesEnum.KilogramPerSecond).ID.ToString());
                    Choices.Add(MaterialStrengthQuantity.Instance.ID.ToString(), MaterialStrengthQuantity.Instance.GetUnitChoice(MaterialStrengthQuantity.UnitChoicesEnum.Megapascal).ID.ToString());
                    Choices.Add(MomentOfAreaQuantity.Instance.ID.ToString(), MomentOfAreaQuantity.Instance.GetUnitChoice(MomentOfAreaQuantity.UnitChoicesEnum.MetresToTheFourthPower).ID.ToString());
                    Choices.Add(MomentOfInertiaQuantity.Instance.ID.ToString(), MomentOfInertiaQuantity.Instance.GetUnitChoice(MomentOfInertiaQuantity.UnitChoicesEnum.KilogramMetreSquared).ID.ToString());
                    Choices.Add(PorousMediumPermeabilityQuantity.Instance.ID.ToString(), PorousMediumPermeabilityQuantity.Instance.GetUnitChoice(PorousMediumPermeabilityQuantity.UnitChoicesEnum.Millidarcy).ID.ToString());
                    Choices.Add(PlaneAngleQuantity.Instance.ID.ToString(), PlaneAngleQuantity.Instance.GetUnitChoice(PlaneAngleQuantity.UnitChoicesEnum.Degree).ID.ToString());
                    Choices.Add(PlaneAngleGeodesicQuantity.Instance.ID.ToString(), PlaneAngleGeodesicQuantity.Instance.GetUnitChoice(PlaneAngleGeodesicQuantity.UnitChoicesEnum.Degree).ID.ToString());
                    Choices.Add(PlaneAngleStandardQuantity.Instance.ID.ToString(), PlaneAngleStandardQuantity.Instance.GetUnitChoice(PlaneAngleStandardQuantity.UnitChoicesEnum.Degree).ID.ToString());
                    Choices.Add(PorosityQuantity.Instance.ID.ToString(), PorosityQuantity.Instance.GetUnitChoice(PorosityQuantity.UnitChoicesEnum.Proportion).ID.ToString());
                    Choices.Add(PowerQuantity.Instance.ID.ToString(), PowerQuantity.Instance.GetUnitChoice(PowerQuantity.UnitChoicesEnum.Watt).ID.ToString());
                    Choices.Add(PowerRateOfChangeQuantity.Instance.ID.ToString(), PowerRateOfChangeQuantity.Instance.GetUnitChoice(PowerRateOfChangeQuantity.UnitChoicesEnum.WattPerSecond).ID.ToString());
                    Choices.Add(ProportionRateOfChangeQuantity.Instance.ID.ToString(), ProportionRateOfChangeQuantity.Instance.GetUnitChoice(ProportionRateOfChangeQuantity.UnitChoicesEnum.PercentPerSecond).ID.ToString());
                    Choices.Add(PressureGradientPerLengthQuantity.Instance.ID.ToString(), PressureGradientPerLengthQuantity.Instance.GetUnitChoice(PressureGradientPerLengthQuantity.UnitChoicesEnum.BarPerMetre).ID.ToString());
                    Choices.Add(PressureLossConstantQuantity.Instance.ID.ToString(), PressureLossConstantQuantity.Instance.GetUnitChoice(PressureLossConstantQuantity.UnitChoicesEnum.PressureLossConstantMetric).ID.ToString());
                    Choices.Add(PressureQuantity.Instance.ID.ToString(), PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.Bar).ID.ToString());
                    Choices.Add(PressureRateOfChangeQuantity.Instance.ID.ToString(), PressureRateOfChangeQuantity.Instance.GetUnitChoice(PressureRateOfChangeQuantity.UnitChoicesEnum.BarPerSecond).ID.ToString());
                    Choices.Add(ProportionQuantity.Instance.ID.ToString(), ProportionQuantity.Instance.GetUnitChoice(ProportionQuantity.UnitChoicesEnum.Percent).ID.ToString());
                    Choices.Add(RandomWalkQuantity.Instance.ID.ToString(), RandomWalkQuantity.Instance.GetUnitChoice(RandomWalkQuantity.UnitChoicesEnum.DegreePerSquareRootHour).ID.ToString());
                    Choices.Add(RelativeTemperatureQuantity.Instance.ID.ToString(), RelativeTemperatureQuantity.Instance.GetUnitChoice(RelativeTemperatureQuantity.UnitChoicesEnum.RelativeCelsius).ID.ToString());
                    Choices.Add(ElectricResistivityQuantity.Instance.ID.ToString(), ElectricResistivityQuantity.Instance.GetUnitChoice(ElectricResistivityQuantity.UnitChoicesEnum.OhmMetre).ID.ToString());
                    Choices.Add(ConsistencyIndexRheologyQuantity.Instance.ID.ToString(), ConsistencyIndexRheologyQuantity.Instance.GetUnitChoice(ConsistencyIndexRheologyQuantity.UnitChoicesEnum.PascalSecond).ID.ToString());
                    Choices.Add(RotationalFrequencyQuantity.Instance.ID.ToString(), RotationalFrequencyQuantity.Instance.GetUnitChoice(RotationalFrequencyQuantity.UnitChoicesEnum.Rpm).ID.ToString());
                    Choices.Add(RotationalFrequencyRateOfChangeQuantity.Instance.ID.ToString(), RotationalFrequencyRateOfChangeQuantity.Instance.GetUnitChoice(RotationalFrequencyRateOfChangeQuantity.UnitChoicesEnum.RpmPerSecond).ID.ToString());
                    Choices.Add(ShockRateQuantity.Instance.ID.ToString(), ShockRateQuantity.Instance.GetUnitChoice(ShockRateQuantity.UnitChoicesEnum.ShockPerMinute).ID.ToString());
                    Choices.Add(DiameterSmallQuantity.Instance.ID.ToString(), DiameterSmallQuantity.Instance.GetUnitChoice(DiameterSmallQuantity.UnitChoicesEnum.Millimetre).ID.ToString());
                    Choices.Add(LengthSmallQuantity.Instance.ID.ToString(), LengthSmallQuantity.Instance.GetUnitChoice(LengthSmallQuantity.UnitChoicesEnum.Millimetre).ID.ToString());
                    Choices.Add(ProportionSmallQuantity.Instance.ID.ToString(), ProportionSmallQuantity.Instance.GetUnitChoice(ProportionSmallQuantity.UnitChoicesEnum.PartPerMillion).ID.ToString());
                    Choices.Add(RotationalFrequencySmallQuantity.Instance.ID.ToString(), RotationalFrequencySmallQuantity.Instance.GetUnitChoice(RotationalFrequencySmallQuantity.UnitChoicesEnum.Rpm).ID.ToString());
                    Choices.Add(SolidAngleQuantity.Instance.ID.ToString(), SolidAngleQuantity.Instance.GetUnitChoice(SolidAngleQuantity.UnitChoicesEnum.DegreeSquared).ID.ToString());
                    Choices.Add(IsobaricSpecificHeatCapacityQuantity.Instance.ID.ToString(), IsobaricSpecificHeatCapacityQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityQuantity.UnitChoicesEnum.JoulePerKilogramKelvin).ID.ToString());
                    Choices.Add(IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance.ID.ToString(), IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.UnitChoicesEnum.JoulePerKilogramSquaredKelvin).ID.ToString());
                    Choices.Add(DimensionLessStandardQuantity.Instance.ID.ToString(), DimensionLessStandardQuantity.Instance.GetUnitChoice(DimensionLessStandardQuantity.UnitChoicesEnum.Dimensionless).ID.ToString());
                    Choices.Add(LengthStandardQuantity.Instance.ID.ToString(), LengthStandardQuantity.Instance.GetUnitChoice(LengthStandardQuantity.UnitChoicesEnum.Metre).ID.ToString());
                    Choices.Add(ProportionStandardQuantity.Instance.ID.ToString(), ProportionStandardQuantity.Instance.GetUnitChoice(ProportionStandardQuantity.UnitChoicesEnum.Percent).ID.ToString());
                    Choices.Add(StressQuantity.Instance.ID.ToString(), StressQuantity.Instance.GetUnitChoice("pascal").ID.ToString());
                    Choices.Add(StrokeFrequencyQuantity.Instance.ID.ToString(), StrokeFrequencyQuantity.Instance.GetUnitChoice(StrokeFrequencyQuantity.UnitChoicesEnum.Spm).ID.ToString());
                    Choices.Add(TemperatureGradientPerLengthQuantity.Instance.ID.ToString(), TemperatureGradientPerLengthQuantity.Instance.GetUnitChoice(TemperatureGradientPerLengthQuantity.UnitChoicesEnum.CelsiusPerMetre).ID.ToString());
                    Choices.Add(TemperatureQuantity.Instance.ID.ToString(), TemperatureQuantity.Instance.GetUnitChoice(TemperatureQuantity.UnitChoicesEnum.Celsius).ID.ToString());
                    Choices.Add(TensionQuantity.Instance.ID.ToString(), TensionQuantity.Instance.GetUnitChoice(TensionQuantity.UnitChoicesEnum.Decanewton).ID.ToString());
                    Choices.Add(ThermalConductivityQuantity.Instance.ID.ToString(), ThermalConductivityQuantity.Instance.GetUnitChoice(ThermalConductivityQuantity.UnitChoicesEnum.WattPerMetreKelvin).ID.ToString());
                    Choices.Add(ThermalConductivityGradientPerTemperatureQuantity.Instance.ID.ToString(), ThermalConductivityGradientPerTemperatureQuantity.Instance.GetUnitChoice(ThermalConductivityGradientPerTemperatureQuantity.UnitChoicesEnum.WattPerMetreKelvinPerKelvin).ID.ToString());
                    Choices.Add(TimeQuantity.Instance.ID.ToString(), TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.Second).ID.ToString());
                    Choices.Add(TorqueSmallQuantity.Instance.ID.ToString(), TorqueSmallQuantity.Instance.GetUnitChoice(TorqueSmallQuantity.UnitChoicesEnum.NewtonMillimetre).ID.ToString());
                    Choices.Add(TorqueQuantity.Instance.ID.ToString(), TorqueQuantity.Instance.GetUnitChoice(TorqueQuantity.UnitChoicesEnum.NewtonMetre).ID.ToString());
                    Choices.Add(TorqueGradientPerLengthQuantity.Instance.ID.ToString(), TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.NewtonMetrePerMetre).ID.ToString());
                    Choices.Add(TorqueRateOfChangeQuantity.Instance.ID.ToString(), TorqueRateOfChangeQuantity.Instance.GetUnitChoice(TorqueRateOfChangeQuantity.UnitChoicesEnum.NewtonMetrePerSecond).ID.ToString());
                    Choices.Add(VelocityQuantity.Instance.ID.ToString(), VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.MetrePerSecond).ID.ToString());
                    Choices.Add(VolumeQuantity.Instance.ID.ToString(), VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.Litre).ID.ToString());
                    Choices.Add(VolumetricFlowRateOfChangeQuantity.Instance.ID.ToString(), VolumetricFlowRateOfChangeQuantity.Instance.GetUnitChoice(VolumetricFlowRateOfChangeQuantity.UnitChoicesEnum.LitrePerMinutePerSecond).ID.ToString());
                    Choices.Add(VolumetricFlowRateQuantity.Instance.ID.ToString(), VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.LitrePerMinute).ID.ToString());
                    Choices.Add(WaveNumberQuantity.Instance.ID.ToString(), WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalMetre).ID.ToString());
                    Choices.Add(ElasticModulusQuantity.Instance.ID.ToString(), ElasticModulusQuantity.Instance.GetUnitChoice(ElasticModulusQuantity.UnitChoicesEnum.Megapascal).ID.ToString());
                }
                else if (defaultUnitChoice == DefaultUnitSystemEnum.US)
                {
                    IsDefault = true;
                    ID = new Guid("59043df0-ff9d-42ad-bd40-d81625fe99b4");
                    Name = "US";
                    Description = "United States of America System of Units";

                    Choices.Add(AccelerationQuantity.Instance.ID.ToString(), AccelerationQuantity.Instance.GetUnitChoice(AccelerationQuantity.UnitChoicesEnum.FootPerSecondSquared).ID.ToString());
                    Choices.Add(AmountSubstanceQuantity.Instance.ID.ToString(), AmountSubstanceQuantity.Instance.GetUnitChoice(AmountSubstanceQuantity.UnitChoicesEnum.Mole).ID.ToString());
                    Choices.Add(AngleMagneticFluxDensityQuantity.Instance.ID.ToString(), AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.RadianTesla).ID.ToString());
                    Choices.Add(AngleGradientPerLengthQuantity.Instance.ID.ToString(), AngleGradientPerLengthQuantity.Instance.GetUnitChoice(AngleGradientPerLengthQuantity.UnitChoicesEnum.DegreePerFoot).ID.ToString());
                    Choices.Add(AngularAccelerationQuantity.Instance.ID.ToString(), AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.DegreePerSecondSquared).ID.ToString());
                    Choices.Add(AngularVelocityQuantity.Instance.ID.ToString(), AngularVelocityQuantity.Instance.GetUnitChoice(AngularVelocityQuantity.UnitChoicesEnum.DegreePerSecond).ID.ToString());
                    Choices.Add(AreaQuantity.Instance.ID.ToString(), AreaQuantity.Instance.GetUnitChoice(AreaQuantity.UnitChoicesEnum.SquareFoot).ID.ToString());
                    Choices.Add(BendingMomentQuantity.Instance.ID.ToString(), BendingMomentQuantity.Instance.GetUnitChoice(BendingMomentQuantity.UnitChoicesEnum.FootPound).ID.ToString());
                    Choices.Add(CompressibilityQuantity.Instance.ID.ToString(), CompressibilityQuantity.Instance.GetUnitChoice(CompressibilityQuantity.UnitChoicesEnum.InversePoundPerSquareInch).ID.ToString());
                    Choices.Add(CurvatureQuantity.Instance.ID.ToString(), CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.DegreePer100ft).ID.ToString());
                    Choices.Add(SpecificVolumeQuantity.Instance.ID.ToString(), SpecificVolumeQuantity.Instance.GetUnitChoice(SpecificVolumeQuantity.UnitChoicesEnum.GallonUSPerPound).ID.ToString());
                    Choices.Add(SpecificVolumeSquaredQuantity.Instance.ID.ToString(), SpecificVolumeSquaredQuantity.Instance.GetUnitChoice(SpecificVolumeSquaredQuantity.UnitChoicesEnum.GallonUSSquaredPerPoundSquared).ID.ToString());
                    Choices.Add(MassDensityGradientPerPressureQuantity.Instance.ID.ToString(), MassDensityGradientPerPressureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureQuantity.UnitChoicesEnum.PoundPerGallonUSPerPoundPerSquareInch).ID.ToString());
                    Choices.Add(MassDensityGradientPerPressureSquaredQuantity.Instance.ID.ToString(), MassDensityGradientPerPressureSquaredQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredQuantity.UnitChoicesEnum.PoundPerGallonUSPerPoundPerSquareInchSquared).ID.ToString());
                    Choices.Add(MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.ID.ToString(), MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUSPerPoundPerSquareInchSquaredFahrenheit).ID.ToString());
                    Choices.Add(MassDensityGradientPerPressureTemperatureQuantity.Instance.ID.ToString(), MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUSPerPoundPerSquareInchFahrenheit).ID.ToString());
                    Choices.Add(MassDensityGradientPerLengthQuantity.Instance.ID.ToString(), MassDensityGradientPerLengthQuantity.Instance.GetUnitChoice(MassDensityGradientPerLengthQuantity.UnitChoicesEnum.PoundPerGallonUKPer100Foot).ID.ToString());
                    Choices.Add(MassDensityGradientPerTemperatureQuantity.Instance.ID.ToString(), MassDensityGradientPerTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUSPerFahrenheit).ID.ToString());
                    Choices.Add(MassDensityQuantity.Instance.ID.ToString(), MassDensityQuantity.Instance.GetUnitChoice(MassDensityQuantity.UnitChoicesEnum.PoundPerGallonUS).ID.ToString());
                    Choices.Add(MassDensityRateOfChangeQuantity.Instance.ID.ToString(), MassDensityRateOfChangeQuantity.Instance.GetUnitChoice(MassDensityRateOfChangeQuantity.UnitChoicesEnum.PoundPerGallonUSPerHour).ID.ToString());
                    Choices.Add(DimensionlessQuantity.Instance.ID.ToString(), DimensionlessQuantity.Instance.GetUnitChoice(DimensionlessQuantity.UnitChoicesEnum.Dimensionless).ID.ToString());
                    Choices.Add(DynamicViscosityQuantity.Instance.ID.ToString(), DynamicViscosityQuantity.Instance.GetUnitChoice(DynamicViscosityQuantity.UnitChoicesEnum.PoundSecondPer100SquareFoot).ID.ToString());
                    Choices.Add(EarthMagneticFluxDensityQuantity.Instance.ID.ToString(), EarthMagneticFluxDensityQuantity.Instance.GetUnitChoice(EarthMagneticFluxDensityQuantity.UnitChoicesEnum.Nanotesla).ID.ToString());
                    Choices.Add(ElectricCapacitanceQuantity.Instance.ID.ToString(), ElectricCapacitanceQuantity.Instance.GetUnitChoice(ElectricCapacitanceQuantity.UnitChoicesEnum.Microfarad).ID.ToString());
                    Choices.Add(ElectricCurrentQuantity.Instance.ID.ToString(), ElectricCurrentQuantity.Instance.GetUnitChoice(ElectricCurrentQuantity.UnitChoicesEnum.Ampere).ID.ToString());
                    Choices.Add(ElectricTensionQuantity.Instance.ID.ToString(), ElectricTensionQuantity.Instance.GetUnitChoice(ElectricTensionQuantity.UnitChoicesEnum.Volt).ID.ToString());
                    Choices.Add(ElongationGradientPerLengthQuantity.Instance.ID.ToString(), ElongationGradientPerLengthQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthQuantity.UnitChoicesEnum.InchPerFoot).ID.ToString());
                    Choices.Add(EnergyQuantity.Instance.ID.ToString(), EnergyQuantity.Instance.GetUnitChoice(EnergyQuantity.UnitChoicesEnum.Joule).ID.ToString());
                    Choices.Add(EnergyDensityQuantity.Instance.ID.ToString(), EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.JoulePerCubicFoot).ID.ToString());
                    Choices.Add(FluidShearRateQuantity.Instance.ID.ToString(), FluidShearRateQuantity.Instance.GetUnitChoice(FluidShearRateQuantity.UnitChoicesEnum.ReciprocalSecond).ID.ToString());
                    Choices.Add(FluidShearStressQuantity.Instance.ID.ToString(), FluidShearStressQuantity.Instance.GetUnitChoice(FluidShearStressQuantity.UnitChoicesEnum.PoundPer100SquareFoot).ID.ToString());
                    Choices.Add(ForceGradientPerLengthQuantity.Instance.ID.ToString(), ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.PoundPerFoot).ID.ToString());
                    Choices.Add(ForceQuantity.Instance.ID.ToString(), ForceQuantity.Instance.GetUnitChoice(ForceQuantity.UnitChoicesEnum.PoundForce).ID.ToString());
                    Choices.Add(ForceRateOfChangeQuantity.Instance.ID.ToString(), ForceRateOfChangeQuantity.Instance.GetUnitChoice(ForceRateOfChangeQuantity.UnitChoicesEnum.PoundForcePerSecond).ID.ToString());
                    Choices.Add(FrequencyQuantity.Instance.ID.ToString(), FrequencyQuantity.Instance.GetUnitChoice(FrequencyQuantity.UnitChoicesEnum.Hertz).ID.ToString());
                    Choices.Add(FrequencyRateOfChangeQuantity.Instance.ID.ToString(), FrequencyRateOfChangeQuantity.Instance.GetUnitChoice(FrequencyRateOfChangeQuantity.UnitChoicesEnum.HertzPerSecond).ID.ToString());
                    Choices.Add(HeatTransferCoefficientQuantity.Instance.ID.ToString(), HeatTransferCoefficientQuantity.Instance.GetUnitChoice(HeatTransferCoefficientQuantity.UnitChoicesEnum.BritishThermalUnitPerHourPerSquareFootPerDegreeFahrenheit).ID.ToString());
                    Choices.Add(HydraulicConductivityQuantity.Instance.ID.ToString(), HydraulicConductivityQuantity.Instance.GetUnitChoice(HydraulicConductivityQuantity.UnitChoicesEnum.FootPerSecond).ID.ToString());
                    Choices.Add(ImageScaleQuantity.Instance.ID.ToString(), ImageScaleQuantity.Instance.GetUnitChoice(ImageScaleQuantity.UnitChoicesEnum.DotPerInch).ID.ToString());
                    Choices.Add(InterfacialTensionQuantity.Instance.ID.ToString(), InterfacialTensionQuantity.Instance.GetUnitChoice(InterfacialTensionQuantity.UnitChoicesEnum.PoundPerSecondSquared).ID.ToString());
                    Choices.Add(VolumeLargeQuantity.Instance.ID.ToString(), VolumeLargeQuantity.Instance.GetUnitChoice(VolumeLargeQuantity.UnitChoicesEnum.USGallon).ID.ToString());
                    Choices.Add(LengthQuantity.Instance.ID.ToString(), LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Foot).ID.ToString());
                    Choices.Add(LuminousIntensityQuantity.Instance.ID.ToString(), LuminousIntensityQuantity.Instance.GetUnitChoice(LuminousIntensityQuantity.UnitChoicesEnum.Candela).ID.ToString());
                    Choices.Add(MagneticFluxDensityQuantity.Instance.ID.ToString(), MagneticFluxDensityQuantity.Instance.GetUnitChoice(MagneticFluxDensityQuantity.UnitChoicesEnum.Gauss).ID.ToString());
                    Choices.Add(MagneticFluxQuantity.Instance.ID.ToString(), MagneticFluxQuantity.Instance.GetUnitChoice(MagneticFluxQuantity.UnitChoicesEnum.Microweber).ID.ToString());
                    Choices.Add(MassGradientPerLengthQuantity.Instance.ID.ToString(), MassGradientPerLengthQuantity.Instance.GetUnitChoice(MassGradientPerLengthQuantity.UnitChoicesEnum.PoundPerFoot).ID.ToString());
                    Choices.Add(MassQuantity.Instance.ID.ToString(), MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.Pound).ID.ToString());
                    Choices.Add(MassRateQuantity.Instance.ID.ToString(), MassRateQuantity.Instance.GetUnitChoice(MassRateQuantity.UnitChoicesEnum.KilogramPerSecond).ID.ToString());
                    Choices.Add(MaterialStrengthQuantity.Instance.ID.ToString(), MaterialStrengthQuantity.Instance.GetUnitChoice(MaterialStrengthQuantity.UnitChoicesEnum.Psi).ID.ToString());
                    Choices.Add(MomentOfAreaQuantity.Instance.ID.ToString(), MomentOfAreaQuantity.Instance.GetUnitChoice(MomentOfAreaQuantity.UnitChoicesEnum.InchesToTheFourthPower).ID.ToString());
                    Choices.Add(MomentOfInertiaQuantity.Instance.ID.ToString(), MomentOfInertiaQuantity.Instance.GetUnitChoice(MomentOfInertiaQuantity.UnitChoicesEnum.PoundInchSquared).ID.ToString());
                    Choices.Add(PorousMediumPermeabilityQuantity.Instance.ID.ToString(), PorousMediumPermeabilityQuantity.Instance.GetUnitChoice(PorousMediumPermeabilityQuantity.UnitChoicesEnum.Millidarcy).ID.ToString());
                    Choices.Add(PlaneAngleQuantity.Instance.ID.ToString(), PlaneAngleQuantity.Instance.GetUnitChoice(PlaneAngleQuantity.UnitChoicesEnum.Degree).ID.ToString());
                    Choices.Add(PlaneAngleGeodesicQuantity.Instance.ID.ToString(), PlaneAngleGeodesicQuantity.Instance.GetUnitChoice(PlaneAngleGeodesicQuantity.UnitChoicesEnum.Degree).ID.ToString());
                    Choices.Add(PlaneAngleStandardQuantity.Instance.ID.ToString(), PlaneAngleStandardQuantity.Instance.GetUnitChoice(PlaneAngleStandardQuantity.UnitChoicesEnum.Degree).ID.ToString());
                    Choices.Add(PorosityQuantity.Instance.ID.ToString(), PorosityQuantity.Instance.GetUnitChoice(PorosityQuantity.UnitChoicesEnum.Proportion).ID.ToString());
                    Choices.Add(PowerQuantity.Instance.ID.ToString(), PowerQuantity.Instance.GetUnitChoice(PowerQuantity.UnitChoicesEnum.Watt).ID.ToString());
                    Choices.Add(PowerRateOfChangeQuantity.Instance.ID.ToString(), PowerRateOfChangeQuantity.Instance.GetUnitChoice(PowerRateOfChangeQuantity.UnitChoicesEnum.WattPerSecond).ID.ToString());
                    Choices.Add(ProportionRateOfChangeQuantity.Instance.ID.ToString(), ProportionRateOfChangeQuantity.Instance.GetUnitChoice(ProportionRateOfChangeQuantity.UnitChoicesEnum.PercentPerSecond).ID.ToString());
                    Choices.Add(PressureGradientPerLengthQuantity.Instance.ID.ToString(), PressureGradientPerLengthQuantity.Instance.GetUnitChoice(PressureGradientPerLengthQuantity.UnitChoicesEnum.PsiPerFoot).ID.ToString());
                    Choices.Add(PressureLossConstantQuantity.Instance.ID.ToString(), PressureLossConstantQuantity.Instance.GetUnitChoice(PressureLossConstantQuantity.UnitChoicesEnum.PressureLossConstantUS).ID.ToString());
                    Choices.Add(PressureQuantity.Instance.ID.ToString(), PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.PoundPerSquareInch).ID.ToString());
                    Choices.Add(PressureRateOfChangeQuantity.Instance.ID.ToString(), PressureRateOfChangeQuantity.Instance.GetUnitChoice(PressureRateOfChangeQuantity.UnitChoicesEnum.PoundPerSquareInchPerSecond).ID.ToString());
                    Choices.Add(ProportionQuantity.Instance.ID.ToString(), ProportionQuantity.Instance.GetUnitChoice(ProportionQuantity.UnitChoicesEnum.Percent).ID.ToString());
                    Choices.Add(RandomWalkQuantity.Instance.ID.ToString(), RandomWalkQuantity.Instance.GetUnitChoice(RandomWalkQuantity.UnitChoicesEnum.DegreePerSquareRootHour).ID.ToString());
                    Choices.Add(RelativeTemperatureQuantity.Instance.ID.ToString(), RelativeTemperatureQuantity.Instance.GetUnitChoice(RelativeTemperatureQuantity.UnitChoicesEnum.Rankine).ID.ToString());
                    Choices.Add(ElectricResistivityQuantity.Instance.ID.ToString(), ElectricResistivityQuantity.Instance.GetUnitChoice(ElectricResistivityQuantity.UnitChoicesEnum.OhmMetre).ID.ToString());
                    Choices.Add(ConsistencyIndexRheologyQuantity.Instance.ID.ToString(), ConsistencyIndexRheologyQuantity.Instance.GetUnitChoice(ConsistencyIndexRheologyQuantity.UnitChoicesEnum.PoundSecondPer100SquareFoot).ID.ToString());
                    Choices.Add(RotationalFrequencyQuantity.Instance.ID.ToString(), RotationalFrequencyQuantity.Instance.GetUnitChoice(RotationalFrequencyQuantity.UnitChoicesEnum.Rpm).ID.ToString());
                    Choices.Add(RotationalFrequencyRateOfChangeQuantity.Instance.ID.ToString(), RotationalFrequencyRateOfChangeQuantity.Instance.GetUnitChoice(RotationalFrequencyRateOfChangeQuantity.UnitChoicesEnum.RpmPerSecond).ID.ToString());
                    Choices.Add(ShockRateQuantity.Instance.ID.ToString(), ShockRateQuantity.Instance.GetUnitChoice(ShockRateQuantity.UnitChoicesEnum.ShockPerMinute).ID.ToString());
                    Choices.Add(DiameterSmallQuantity.Instance.ID.ToString(), DiameterSmallQuantity.Instance.GetUnitChoice(DiameterSmallQuantity.UnitChoicesEnum.InchPer32).ID.ToString());
                    Choices.Add(LengthSmallQuantity.Instance.ID.ToString(), LengthSmallQuantity.Instance.GetUnitChoice(LengthSmallQuantity.UnitChoicesEnum.Inch).ID.ToString());
                    Choices.Add(ProportionSmallQuantity.Instance.ID.ToString(), ProportionSmallQuantity.Instance.GetUnitChoice(ProportionSmallQuantity.UnitChoicesEnum.PartPerMillion).ID.ToString());
                    Choices.Add(RotationalFrequencySmallQuantity.Instance.ID.ToString(), RotationalFrequencySmallQuantity.Instance.GetUnitChoice(RotationalFrequencySmallQuantity.UnitChoicesEnum.Rpm).ID.ToString());
                    Choices.Add(SolidAngleQuantity.Instance.ID.ToString(), SolidAngleQuantity.Instance.GetUnitChoice(SolidAngleQuantity.UnitChoicesEnum.DegreeSquared).ID.ToString());
                    Choices.Add(IsobaricSpecificHeatCapacityQuantity.Instance.ID.ToString(), IsobaricSpecificHeatCapacityQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityQuantity.UnitChoicesEnum.BritishThermalUnitPerPoundDegreeFahrenheit).ID.ToString());
                    Choices.Add(IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance.ID.ToString(), IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.UnitChoicesEnum.BritishThermalUnitPerPoundSquaredDegreeFahrenheit).ID.ToString());
                    Choices.Add(DimensionLessStandardQuantity.Instance.ID.ToString(), DimensionLessStandardQuantity.Instance.GetUnitChoice(DimensionLessStandardQuantity.UnitChoicesEnum.Dimensionless).ID.ToString());
                    Choices.Add(LengthStandardQuantity.Instance.ID.ToString(), LengthStandardQuantity.Instance.GetUnitChoice(LengthStandardQuantity.UnitChoicesEnum.Foot).ID.ToString());
                    Choices.Add(ProportionStandardQuantity.Instance.ID.ToString(), ProportionStandardQuantity.Instance.GetUnitChoice(ProportionStandardQuantity.UnitChoicesEnum.Percent).ID.ToString());
                    Choices.Add(StressQuantity.Instance.ID.ToString(), StressQuantity.Instance.GetUnitChoice("pascal").ID.ToString());
                    Choices.Add(StrokeFrequencyQuantity.Instance.ID.ToString(), StrokeFrequencyQuantity.Instance.GetUnitChoice(StrokeFrequencyQuantity.UnitChoicesEnum.Spm).ID.ToString());
                    Choices.Add(TemperatureGradientPerLengthQuantity.Instance.ID.ToString(), TemperatureGradientPerLengthQuantity.Instance.GetUnitChoice(TemperatureGradientPerLengthQuantity.UnitChoicesEnum.FahrenheitPer100Foot).ID.ToString());
                    Choices.Add(TemperatureQuantity.Instance.ID.ToString(), TemperatureQuantity.Instance.GetUnitChoice(TemperatureQuantity.UnitChoicesEnum.Fahrenheit).ID.ToString());
                    Choices.Add(TensionQuantity.Instance.ID.ToString(), TensionQuantity.Instance.GetUnitChoice(TensionQuantity.UnitChoicesEnum.PoundForce).ID.ToString());
                    Choices.Add(ThermalConductivityQuantity.Instance.ID.ToString(), ThermalConductivityQuantity.Instance.GetUnitChoice(ThermalConductivityQuantity.UnitChoicesEnum.BritishThermalUnitPerHourFootDegreeFahrenheit).ID.ToString());
                    Choices.Add(ThermalConductivityGradientPerTemperatureQuantity.Instance.ID.ToString(), ThermalConductivityGradientPerTemperatureQuantity.Instance.GetUnitChoice(ThermalConductivityGradientPerTemperatureQuantity.UnitChoicesEnum.BritishThermalUnitInchPerHourSquareFootDegreeFahrenheitSquared).ID.ToString());
                    Choices.Add(TimeQuantity.Instance.ID.ToString(), TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.Second).ID.ToString());
                    Choices.Add(TorqueQuantity.Instance.ID.ToString(), TorqueQuantity.Instance.GetUnitChoice(TorqueQuantity.UnitChoicesEnum.FootPound).ID.ToString());
                    Choices.Add(TorqueGradientPerLengthQuantity.Instance.ID.ToString(), TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.FootPoundPerFoot).ID.ToString());
                    Choices.Add(TorqueSmallQuantity.Instance.ID.ToString(), TorqueSmallQuantity.Instance.GetUnitChoice(TorqueSmallQuantity.UnitChoicesEnum.InchPound).ID.ToString());
                    Choices.Add(TorqueRateOfChangeQuantity.Instance.ID.ToString(), TorqueRateOfChangeQuantity.Instance.GetUnitChoice(TorqueRateOfChangeQuantity.UnitChoicesEnum.FootPoundPerSecond).ID.ToString());
                    Choices.Add(VelocityQuantity.Instance.ID.ToString(), VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.FootPerSecond).ID.ToString());
                    Choices.Add(VolumeQuantity.Instance.ID.ToString(), VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.USGallon).ID.ToString());
                    Choices.Add(VolumetricFlowRateOfChangeQuantity.Instance.ID.ToString(), VolumetricFlowRateOfChangeQuantity.Instance.GetUnitChoice(VolumetricFlowRateOfChangeQuantity.UnitChoicesEnum.USGallonPerMinutePerSecond).ID.ToString());
                    Choices.Add(VolumetricFlowRateQuantity.Instance.ID.ToString(), VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.USGallonPerMinute).ID.ToString());
                    Choices.Add(WaveNumberQuantity.Instance.ID.ToString(), WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalMetre).ID.ToString());
                    Choices.Add(ElasticModulusQuantity.Instance.ID.ToString(), ElasticModulusQuantity.Instance.GetUnitChoice(ElasticModulusQuantity.UnitChoicesEnum.PoundPerSquareInch).ID.ToString());
                }
                else if (defaultUnitChoice == DefaultUnitSystemEnum.Imperial)
                {
                    IsDefault = true;
                    ID = new Guid("0f3f96ae-9b00-42f3-be3a-e0e9f329cb62");
                    Name = "Imperial";
                    Description = "United Kingdom System of Units";

                    Choices.Add(AccelerationQuantity.Instance.ID.ToString(), AccelerationQuantity.Instance.GetUnitChoice(AccelerationQuantity.UnitChoicesEnum.FootPerSecondSquared).ID.ToString());
                    Choices.Add(AmountSubstanceQuantity.Instance.ID.ToString(), AmountSubstanceQuantity.Instance.GetUnitChoice(AmountSubstanceQuantity.UnitChoicesEnum.Mole).ID.ToString());
                    Choices.Add(AngleMagneticFluxDensityQuantity.Instance.ID.ToString(), AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.RadianTesla).ID.ToString());
                    Choices.Add(AngleGradientPerLengthQuantity.Instance.ID.ToString(), AngleGradientPerLengthQuantity.Instance.GetUnitChoice(AngleGradientPerLengthQuantity.UnitChoicesEnum.DegreePerFoot).ID.ToString());
                    Choices.Add(AngularAccelerationQuantity.Instance.ID.ToString(), AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.DegreePerSecondSquared).ID.ToString());
                    Choices.Add(AngularVelocityQuantity.Instance.ID.ToString(), AngularVelocityQuantity.Instance.GetUnitChoice(AngularVelocityQuantity.UnitChoicesEnum.DegreePerSecond).ID.ToString());
                    Choices.Add(AreaQuantity.Instance.ID.ToString(), AreaQuantity.Instance.GetUnitChoice(AreaQuantity.UnitChoicesEnum.SquareFoot).ID.ToString());
                    Choices.Add(BendingMomentQuantity.Instance.ID.ToString(), BendingMomentQuantity.Instance.GetUnitChoice(BendingMomentQuantity.UnitChoicesEnum.FootPound).ID.ToString());
                    Choices.Add(CompressibilityQuantity.Instance.ID.ToString(), CompressibilityQuantity.Instance.GetUnitChoice(CompressibilityQuantity.UnitChoicesEnum.InversePoundPerSquareInch).ID.ToString());
                    Choices.Add(CurvatureQuantity.Instance.ID.ToString(), CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.DegreePer100ft).ID.ToString());
                    Choices.Add(SpecificVolumeQuantity.Instance.ID.ToString(), SpecificVolumeQuantity.Instance.GetUnitChoice(SpecificVolumeQuantity.UnitChoicesEnum.GallonUSPerPound).ID.ToString());
                    Choices.Add(SpecificVolumeSquaredQuantity.Instance.ID.ToString(), SpecificVolumeSquaredQuantity.Instance.GetUnitChoice(SpecificVolumeSquaredQuantity.UnitChoicesEnum.GallonUSSquaredPerPoundSquared).ID.ToString());
                    Choices.Add(MassDensityGradientPerPressureQuantity.Instance.ID.ToString(), MassDensityGradientPerPressureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureQuantity.UnitChoicesEnum.PoundPerGallonUKPerPoundPerSquareInch).ID.ToString());
                    Choices.Add(MassDensityGradientPerPressureSquaredQuantity.Instance.ID.ToString(), MassDensityGradientPerPressureSquaredQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredQuantity.UnitChoicesEnum.PoundPerGallonUKPerPoundPerSquareInchSquared).ID.ToString());
                    Choices.Add(MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.ID.ToString(), MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUKPerPoundPerSquareInchSquaredCelsius).ID.ToString());
                    Choices.Add(MassDensityGradientPerPressureTemperatureQuantity.Instance.ID.ToString(), MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUKPerPoundPerSquareInchCelsius).ID.ToString());
                    Choices.Add(MassDensityGradientPerLengthQuantity.Instance.ID.ToString(), MassDensityGradientPerLengthQuantity.Instance.GetUnitChoice(MassDensityGradientPerLengthQuantity.UnitChoicesEnum.PoundPerGallonUKPer100Foot).ID.ToString());
                    Choices.Add(MassDensityGradientPerTemperatureQuantity.Instance.ID.ToString(), MassDensityGradientPerTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUKPerCelsius).ID.ToString());
                    Choices.Add(MassDensityQuantity.Instance.ID.ToString(), MassDensityQuantity.Instance.GetUnitChoice(MassDensityQuantity.UnitChoicesEnum.PoundPerGallonUK).ID.ToString());
                    Choices.Add(MassDensityRateOfChangeQuantity.Instance.ID.ToString(), MassDensityRateOfChangeQuantity.Instance.GetUnitChoice(MassDensityRateOfChangeQuantity.UnitChoicesEnum.PoundPerGallonUKPerHour).ID.ToString());
                    Choices.Add(DimensionlessQuantity.Instance.ID.ToString(), DimensionlessQuantity.Instance.GetUnitChoice(DimensionlessQuantity.UnitChoicesEnum.Dimensionless).ID.ToString());
                    Choices.Add(DynamicViscosityQuantity.Instance.ID.ToString(), DynamicViscosityQuantity.Instance.GetUnitChoice(DynamicViscosityQuantity.UnitChoicesEnum.PoundSecondPer100SquareFoot).ID.ToString());
                    Choices.Add(EarthMagneticFluxDensityQuantity.Instance.ID.ToString(), EarthMagneticFluxDensityQuantity.Instance.GetUnitChoice(EarthMagneticFluxDensityQuantity.UnitChoicesEnum.Nanotesla).ID.ToString());
                    Choices.Add(ElectricCapacitanceQuantity.Instance.ID.ToString(), ElectricCapacitanceQuantity.Instance.GetUnitChoice(ElectricCapacitanceQuantity.UnitChoicesEnum.Microfarad).ID.ToString());
                    Choices.Add(ElectricCurrentQuantity.Instance.ID.ToString(), ElectricCurrentQuantity.Instance.GetUnitChoice(ElectricCurrentQuantity.UnitChoicesEnum.Ampere).ID.ToString());
                    Choices.Add(ElectricTensionQuantity.Instance.ID.ToString(), ElectricTensionQuantity.Instance.GetUnitChoice(ElectricTensionQuantity.UnitChoicesEnum.Volt).ID.ToString());
                    Choices.Add(ElongationGradientPerLengthQuantity.Instance.ID.ToString(), ElongationGradientPerLengthQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthQuantity.UnitChoicesEnum.InchPerFoot).ID.ToString());
                    Choices.Add(EnergyQuantity.Instance.ID.ToString(), EnergyQuantity.Instance.GetUnitChoice(EnergyQuantity.UnitChoicesEnum.Joule).ID.ToString());
                    Choices.Add(EnergyDensityQuantity.Instance.ID.ToString(), EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.JoulePerCubicFoot).ID.ToString());
                    Choices.Add(FluidShearRateQuantity.Instance.ID.ToString(), FluidShearRateQuantity.Instance.GetUnitChoice(FluidShearRateQuantity.UnitChoicesEnum.ReciprocalSecond).ID.ToString());
                    Choices.Add(FluidShearStressQuantity.Instance.ID.ToString(), FluidShearStressQuantity.Instance.GetUnitChoice(FluidShearStressQuantity.UnitChoicesEnum.PoundPer100SquareFoot).ID.ToString());
                    Choices.Add(ForceGradientPerLengthQuantity.Instance.ID.ToString(), ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.PoundPerFoot).ID.ToString());
                    Choices.Add(ForceQuantity.Instance.ID.ToString(), ForceQuantity.Instance.GetUnitChoice(ForceQuantity.UnitChoicesEnum.PoundForce).ID.ToString());
                    Choices.Add(ForceRateOfChangeQuantity.Instance.ID.ToString(), ForceRateOfChangeQuantity.Instance.GetUnitChoice(ForceRateOfChangeQuantity.UnitChoicesEnum.PoundForcePerSecond).ID.ToString());
                    Choices.Add(FrequencyQuantity.Instance.ID.ToString(), FrequencyQuantity.Instance.GetUnitChoice(FrequencyQuantity.UnitChoicesEnum.Hertz).ID.ToString());
                    Choices.Add(FrequencyRateOfChangeQuantity.Instance.ID.ToString(), FrequencyRateOfChangeQuantity.Instance.GetUnitChoice(FrequencyRateOfChangeQuantity.UnitChoicesEnum.HertzPerSecond).ID.ToString());
                    Choices.Add(HeatTransferCoefficientQuantity.Instance.ID.ToString(), HeatTransferCoefficientQuantity.Instance.GetUnitChoice(HeatTransferCoefficientQuantity.UnitChoicesEnum.BritishThermalUnitPerHourPerSquareFootPerDegreeFahrenheit).ID.ToString());
                    Choices.Add(HydraulicConductivityQuantity.Instance.ID.ToString(), HydraulicConductivityQuantity.Instance.GetUnitChoice(HydraulicConductivityQuantity.UnitChoicesEnum.FootPerSecond).ID.ToString());
                    Choices.Add(ImageScaleQuantity.Instance.ID.ToString(), ImageScaleQuantity.Instance.GetUnitChoice(ImageScaleQuantity.UnitChoicesEnum.DotPerInch).ID.ToString());
                    Choices.Add(InterfacialTensionQuantity.Instance.ID.ToString(), InterfacialTensionQuantity.Instance.GetUnitChoice(InterfacialTensionQuantity.UnitChoicesEnum.PoundPerSecondSquared).ID.ToString());
                    Choices.Add(VolumeLargeQuantity.Instance.ID.ToString(), VolumeLargeQuantity.Instance.GetUnitChoice(VolumeLargeQuantity.UnitChoicesEnum.UKGallon).ID.ToString());
                    Choices.Add(LengthQuantity.Instance.ID.ToString(), LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Foot).ID.ToString());
                    Choices.Add(LuminousIntensityQuantity.Instance.ID.ToString(), LuminousIntensityQuantity.Instance.GetUnitChoice(LuminousIntensityQuantity.UnitChoicesEnum.Candela).ID.ToString());
                    Choices.Add(MagneticFluxDensityQuantity.Instance.ID.ToString(), MagneticFluxDensityQuantity.Instance.GetUnitChoice(MagneticFluxDensityQuantity.UnitChoicesEnum.Gauss).ID.ToString());
                    Choices.Add(MagneticFluxQuantity.Instance.ID.ToString(), MagneticFluxQuantity.Instance.GetUnitChoice(MagneticFluxQuantity.UnitChoicesEnum.Microweber).ID.ToString());
                    Choices.Add(MassGradientPerLengthQuantity.Instance.ID.ToString(), MassGradientPerLengthQuantity.Instance.GetUnitChoice(MassGradientPerLengthQuantity.UnitChoicesEnum.PoundPerFoot).ID.ToString());
                    Choices.Add(MassQuantity.Instance.ID.ToString(), MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.Pound).ID.ToString());
                    Choices.Add(MassRateQuantity.Instance.ID.ToString(), MassRateQuantity.Instance.GetUnitChoice(MassRateQuantity.UnitChoicesEnum.KilogramPerSecond).ID.ToString());
                    Choices.Add(MaterialStrengthQuantity.Instance.ID.ToString(), MaterialStrengthQuantity.Instance.GetUnitChoice(MaterialStrengthQuantity.UnitChoicesEnum.Psi).ID.ToString());
                    Choices.Add(MomentOfAreaQuantity.Instance.ID.ToString(), MomentOfAreaQuantity.Instance.GetUnitChoice(MomentOfAreaQuantity.UnitChoicesEnum.InchesToTheFourthPower).ID.ToString());
                    Choices.Add(MomentOfInertiaQuantity.Instance.ID.ToString(), MomentOfInertiaQuantity.Instance.GetUnitChoice(MomentOfInertiaQuantity.UnitChoicesEnum.PoundInchSquared).ID.ToString());
                    Choices.Add(PorousMediumPermeabilityQuantity.Instance.ID.ToString(), PorousMediumPermeabilityQuantity.Instance.GetUnitChoice(PorousMediumPermeabilityQuantity.UnitChoicesEnum.Millidarcy).ID.ToString());
                    Choices.Add(PlaneAngleQuantity.Instance.ID.ToString(), PlaneAngleQuantity.Instance.GetUnitChoice(PlaneAngleQuantity.UnitChoicesEnum.Degree).ID.ToString());
                    Choices.Add(PlaneAngleGeodesicQuantity.Instance.ID.ToString(), PlaneAngleGeodesicQuantity.Instance.GetUnitChoice(PlaneAngleGeodesicQuantity.UnitChoicesEnum.Degree).ID.ToString());
                    Choices.Add(PlaneAngleStandardQuantity.Instance.ID.ToString(), PlaneAngleStandardQuantity.Instance.GetUnitChoice(PlaneAngleStandardQuantity.UnitChoicesEnum.Degree).ID.ToString());
                    Choices.Add(PorosityQuantity.Instance.ID.ToString(), PorosityQuantity.Instance.GetUnitChoice(PorosityQuantity.UnitChoicesEnum.Proportion).ID.ToString());
                    Choices.Add(PowerQuantity.Instance.ID.ToString(), PowerQuantity.Instance.GetUnitChoice(PowerQuantity.UnitChoicesEnum.Watt).ID.ToString());
                    Choices.Add(PowerRateOfChangeQuantity.Instance.ID.ToString(), PowerRateOfChangeQuantity.Instance.GetUnitChoice(PowerRateOfChangeQuantity.UnitChoicesEnum.WattPerSecond).ID.ToString());
                    Choices.Add(ProportionRateOfChangeQuantity.Instance.ID.ToString(), ProportionRateOfChangeQuantity.Instance.GetUnitChoice(ProportionRateOfChangeQuantity.UnitChoicesEnum.PercentPerSecond).ID.ToString());
                    Choices.Add(PressureGradientPerLengthQuantity.Instance.ID.ToString(), PressureGradientPerLengthQuantity.Instance.GetUnitChoice(PressureGradientPerLengthQuantity.UnitChoicesEnum.PsiPerFoot).ID.ToString());
                    Choices.Add(PressureLossConstantQuantity.Instance.ID.ToString(), PressureLossConstantQuantity.Instance.GetUnitChoice(PressureLossConstantQuantity.UnitChoicesEnum.PressureLossConstantUK).ID.ToString());
                    Choices.Add(PressureQuantity.Instance.ID.ToString(), PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.PoundPerSquareInch).ID.ToString());
                    Choices.Add(PressureRateOfChangeQuantity.Instance.ID.ToString(), PressureRateOfChangeQuantity.Instance.GetUnitChoice(PressureRateOfChangeQuantity.UnitChoicesEnum.PoundPerSquareInchPerSecond).ID.ToString());
                    Choices.Add(ProportionQuantity.Instance.ID.ToString(), ProportionQuantity.Instance.GetUnitChoice(ProportionQuantity.UnitChoicesEnum.Percent).ID.ToString());
                    Choices.Add(RandomWalkQuantity.Instance.ID.ToString(), RandomWalkQuantity.Instance.GetUnitChoice(RandomWalkQuantity.UnitChoicesEnum.DegreePerSquareRootHour).ID.ToString());
                    Choices.Add(RelativeTemperatureQuantity.Instance.ID.ToString(), RelativeTemperatureQuantity.Instance.GetUnitChoice(RelativeTemperatureQuantity.UnitChoicesEnum.Rankine).ID.ToString());
                    Choices.Add(ElectricResistivityQuantity.Instance.ID.ToString(), ElectricResistivityQuantity.Instance.GetUnitChoice(ElectricResistivityQuantity.UnitChoicesEnum.OhmMetre).ID.ToString());
                    Choices.Add(ConsistencyIndexRheologyQuantity.Instance.ID.ToString(), ConsistencyIndexRheologyQuantity.Instance.GetUnitChoice(ConsistencyIndexRheologyQuantity.UnitChoicesEnum.PoundSecondPer100SquareFoot).ID.ToString());
                    Choices.Add(RotationalFrequencyQuantity.Instance.ID.ToString(), RotationalFrequencyQuantity.Instance.GetUnitChoice(RotationalFrequencyQuantity.UnitChoicesEnum.Rpm).ID.ToString());
                    Choices.Add(RotationalFrequencyRateOfChangeQuantity.Instance.ID.ToString(), RotationalFrequencyRateOfChangeQuantity.Instance.GetUnitChoice(RotationalFrequencyRateOfChangeQuantity.UnitChoicesEnum.RpmPerSecond).ID.ToString());
                    Choices.Add(ShockRateQuantity.Instance.ID.ToString(), ShockRateQuantity.Instance.GetUnitChoice(ShockRateQuantity.UnitChoicesEnum.ShockPerMinute).ID.ToString());
                    Choices.Add(DiameterSmallQuantity.Instance.ID.ToString(), DiameterSmallQuantity.Instance.GetUnitChoice(DiameterSmallQuantity.UnitChoicesEnum.InchPer32).ID.ToString());
                    Choices.Add(LengthSmallQuantity.Instance.ID.ToString(), LengthSmallQuantity.Instance.GetUnitChoice(LengthSmallQuantity.UnitChoicesEnum.Inch).ID.ToString());
                    Choices.Add(ProportionSmallQuantity.Instance.ID.ToString(), ProportionSmallQuantity.Instance.GetUnitChoice(ProportionSmallQuantity.UnitChoicesEnum.PartPerMillion).ID.ToString());
                    Choices.Add(RotationalFrequencySmallQuantity.Instance.ID.ToString(), RotationalFrequencySmallQuantity.Instance.GetUnitChoice(RotationalFrequencySmallQuantity.UnitChoicesEnum.Rpm).ID.ToString());
                    Choices.Add(SolidAngleQuantity.Instance.ID.ToString(), SolidAngleQuantity.Instance.GetUnitChoice(SolidAngleQuantity.UnitChoicesEnum.DegreeSquared).ID.ToString());
                    Choices.Add(IsobaricSpecificHeatCapacityQuantity.Instance.ID.ToString(), IsobaricSpecificHeatCapacityQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityQuantity.UnitChoicesEnum.BritishThermalUnitPerPoundDegreeFahrenheit).ID.ToString());
                    Choices.Add(IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance.ID.ToString(), IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.UnitChoicesEnum.BritishThermalUnitPerPoundSquaredDegreeFahrenheit).ID.ToString());
                    Choices.Add(DimensionLessStandardQuantity.Instance.ID.ToString(), DimensionLessStandardQuantity.Instance.GetUnitChoice(DimensionLessStandardQuantity.UnitChoicesEnum.Dimensionless).ID.ToString());
                    Choices.Add(LengthStandardQuantity.Instance.ID.ToString(), LengthStandardQuantity.Instance.GetUnitChoice(LengthStandardQuantity.UnitChoicesEnum.Foot).ID.ToString());
                    Choices.Add(ProportionStandardQuantity.Instance.ID.ToString(), ProportionStandardQuantity.Instance.GetUnitChoice(ProportionStandardQuantity.UnitChoicesEnum.Percent).ID.ToString());
                    Choices.Add(StressQuantity.Instance.ID.ToString(), StressQuantity.Instance.GetUnitChoice("pascal").ID.ToString());
                    Choices.Add(StrokeFrequencyQuantity.Instance.ID.ToString(), StrokeFrequencyQuantity.Instance.GetUnitChoice(StrokeFrequencyQuantity.UnitChoicesEnum.Spm).ID.ToString());
                    Choices.Add(TemperatureGradientPerLengthQuantity.Instance.ID.ToString(), TemperatureGradientPerLengthQuantity.Instance.GetUnitChoice(TemperatureGradientPerLengthQuantity.UnitChoicesEnum.FahrenheitPer100Foot).ID.ToString());
                    Choices.Add(TemperatureQuantity.Instance.ID.ToString(), TemperatureQuantity.Instance.GetUnitChoice(TemperatureQuantity.UnitChoicesEnum.Fahrenheit).ID.ToString());
                    Choices.Add(TensionQuantity.Instance.ID.ToString(), TensionQuantity.Instance.GetUnitChoice(TensionQuantity.UnitChoicesEnum.PoundForce).ID.ToString());
                    Choices.Add(ThermalConductivityQuantity.Instance.ID.ToString(), ThermalConductivityQuantity.Instance.GetUnitChoice(ThermalConductivityQuantity.UnitChoicesEnum.BritishThermalUnitPerHourFootDegreeFahrenheit).ID.ToString());
                    Choices.Add(ThermalConductivityGradientPerTemperatureQuantity.Instance.ID.ToString(), ThermalConductivityGradientPerTemperatureQuantity.Instance.GetUnitChoice(ThermalConductivityGradientPerTemperatureQuantity.UnitChoicesEnum.BritishThermalUnitInchPerHourSquareFootDegreeFahrenheitSquared).ID.ToString());
                    Choices.Add(TimeQuantity.Instance.ID.ToString(), TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.Second).ID.ToString());
                    Choices.Add(TorqueQuantity.Instance.ID.ToString(), TorqueQuantity.Instance.GetUnitChoice(TorqueQuantity.UnitChoicesEnum.FootPound).ID.ToString());
                    Choices.Add(TorqueGradientPerLengthQuantity.Instance.ID.ToString(), TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.FootPoundPerFoot).ID.ToString());
                    Choices.Add(TorqueSmallQuantity.Instance.ID.ToString(), TorqueSmallQuantity.Instance.GetUnitChoice(TorqueSmallQuantity.UnitChoicesEnum.InchPound).ID.ToString());
                    Choices.Add(TorqueRateOfChangeQuantity.Instance.ID.ToString(), TorqueRateOfChangeQuantity.Instance.GetUnitChoice(TorqueRateOfChangeQuantity.UnitChoicesEnum.FootPoundPerSecond).ID.ToString());
                    Choices.Add(VelocityQuantity.Instance.ID.ToString(), VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.FootPerSecond).ID.ToString());
                    Choices.Add(VolumeQuantity.Instance.ID.ToString(), VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.UKGallon).ID.ToString());
                    Choices.Add(VolumetricFlowRateOfChangeQuantity.Instance.ID.ToString(), VolumetricFlowRateOfChangeQuantity.Instance.GetUnitChoice(VolumetricFlowRateOfChangeQuantity.UnitChoicesEnum.UKGallonPerMinutePerSecond).ID.ToString());
                    Choices.Add(VolumetricFlowRateQuantity.Instance.ID.ToString(), VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.UKGallonPerMinute).ID.ToString());
                    Choices.Add(WaveNumberQuantity.Instance.ID.ToString(), WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalMetre).ID.ToString());
                    Choices.Add(ElasticModulusQuantity.Instance.ID.ToString(), ElasticModulusQuantity.Instance.GetUnitChoice(ElasticModulusQuantity.UnitChoicesEnum.PoundPerSquareInch).ID.ToString());
                }
                if (defaultUnitChoice == DefaultUnitSystemEnum.Metric)
                {
                    AddNewMetricChoices();
                }
                else if (defaultUnitChoice == DefaultUnitSystemEnum.US)
                {
                    AddNewUSChoices();
                }
                else if (defaultUnitChoice == DefaultUnitSystemEnum.Imperial)
                {
                    AddNewImperialChoices();
                }
                CheckMissing(quantities);
            }
        }

        private void SetChoice(BasePhysicalQuantity quantity, UnitChoice choice)
        {
            Choices[quantity.ID.ToString()] = choice.ID.ToString();
        }

        private void AddNewMetricChoices()
        {
            SetChoice(BendingMomentGradientQuantity.Instance, BendingMomentGradientQuantity.Instance.GetUnitChoice(BendingMomentGradientQuantity.UnitChoicesEnum.KilonewtonMetrePerMetre));
            SetChoice(EnergyPerAreaQuantity.Instance, EnergyPerAreaQuantity.Instance.GetUnitChoice(EnergyPerAreaQuantity.UnitChoicesEnum.KilojoulePerSquareMetre));
            SetChoice(MassPerEnergyQuantity.Instance, MassPerEnergyQuantity.Instance.GetUnitChoice(MassPerEnergyQuantity.UnitChoicesEnum.KilogramPerMegajoule));
            SetChoice(PressurePerVolumeQuantity.Instance, PressurePerVolumeQuantity.Instance.GetUnitChoice(PressurePerVolumeQuantity.UnitChoicesEnum.BarPerCubicMetre));
            SetChoice(PressureTimePerVolumeQuantity.Instance, PressureTimePerVolumeQuantity.Instance.GetUnitChoice(PressureTimePerVolumeQuantity.UnitChoicesEnum.BarSecondPerCubicMetre));
            SetChoice(RelativeTemperaturePerPressureQuantity.Instance, RelativeTemperaturePerPressureQuantity.Instance.GetUnitChoice(RelativeTemperaturePerPressureQuantity.UnitChoicesEnum.KelvinPerBar));
            SetChoice(TemperatureRateOfChangeQuantity.Instance, TemperatureRateOfChangeQuantity.Instance.GetUnitChoice(TemperatureRateOfChangeQuantity.UnitChoicesEnum.RelativeCelsiusPerMinute));
            SetChoice(VolumePerEnergyQuantity.Instance, VolumePerEnergyQuantity.Instance.GetUnitChoice(VolumePerEnergyQuantity.UnitChoicesEnum.LitrePerMegajoule));
            SetChoice(WorkGradientQuantity.Instance, WorkGradientQuantity.Instance.GetUnitChoice(WorkGradientQuantity.UnitChoicesEnum.KilojoulePerMetre));
            SetChoice(AbsorbedDoseQuantity.Instance, AbsorbedDoseQuantity.Instance.GetUnitChoice(AbsorbedDoseQuantity.UnitChoicesEnum.Milligray));
            SetChoice(AmountSubstancePerAreaQuantity.Instance, AmountSubstancePerAreaQuantity.Instance.GetUnitChoice(AmountSubstancePerAreaQuantity.UnitChoicesEnum.MillimolePerSquareMetre));
            SetChoice(AmountSubstancePerAreaRateOfChangeQuantity.Instance, AmountSubstancePerAreaRateOfChangeQuantity.Instance.GetUnitChoice(AmountSubstancePerAreaRateOfChangeQuantity.UnitChoicesEnum.MillimolePerSquareMetreSecond));
            SetChoice(AmountSubstancePerVolumeQuantity.Instance, AmountSubstancePerVolumeQuantity.Instance.GetUnitChoice(AmountSubstancePerVolumeQuantity.UnitChoicesEnum.MillimolePerLitre));
            SetChoice(AmountSubstanceRateQuantity.Instance, AmountSubstanceRateQuantity.Instance.GetUnitChoice(AmountSubstanceRateQuantity.UnitChoicesEnum.MolePerMinute));
            SetChoice(AngularVelocityPerVolumetricFlowRateQuantity.Instance, AngularVelocityPerVolumetricFlowRateQuantity.Instance.GetUnitChoice(AngularVelocityPerVolumetricFlowRateQuantity.UnitChoicesEnum.RevolutionPerMinutePerLitrePerMinute));
            SetChoice(AreaPerAmountSubstanceQuantity.Instance, AreaPerAmountSubstanceQuantity.Instance.GetUnitChoice(AreaPerAmountSubstanceQuantity.UnitChoicesEnum.SquareCentimetrePerMole));
            SetChoice(AreaPerMassQuantity.Instance, AreaPerMassQuantity.Instance.GetUnitChoice(AreaPerMassQuantity.UnitChoicesEnum.SquareCentimetrePerGram));
            SetChoice(AreaPerVolumeQuantity.Instance, AreaPerVolumeQuantity.Instance.GetUnitChoice(AreaPerVolumeQuantity.UnitChoicesEnum.SquareCentimetrePerCubicCentimetre));
            SetChoice(AreaRateOfChangeQuantity.Instance, AreaRateOfChangeQuantity.Instance.GetUnitChoice(AreaRateOfChangeQuantity.UnitChoicesEnum.SquareMetrePerMinute));
            SetChoice(DataStorageQuantity.Instance, DataStorageQuantity.Instance.GetUnitChoice(DataStorageQuantity.UnitChoicesEnum.Byte));
            SetChoice(DataTransferRateQuantity.Instance, DataTransferRateQuantity.Instance.GetUnitChoice(DataTransferRateQuantity.UnitChoicesEnum.MegabitPerSecond));
            SetChoice(DiffusivityQuantity.Instance, DiffusivityQuantity.Instance.GetUnitChoice(DiffusivityQuantity.UnitChoicesEnum.SquareMillimetrePerSecond));
            SetChoice(DigitalSymbolRateQuantity.Instance, DigitalSymbolRateQuantity.Instance.GetUnitChoice(DigitalSymbolRateQuantity.UnitChoicesEnum.Kilobaud));
            SetChoice(DoseEquivalentQuantity.Instance, DoseEquivalentQuantity.Instance.GetUnitChoice(DoseEquivalentQuantity.UnitChoicesEnum.Millisievert));
            SetChoice(DoseEquivalentRateQuantity.Instance, DoseEquivalentRateQuantity.Instance.GetUnitChoice(DoseEquivalentRateQuantity.UnitChoicesEnum.MicrosievertPerHour));
            SetChoice(ElectricChargePerAreaQuantity.Instance, ElectricChargePerAreaQuantity.Instance.GetUnitChoice(ElectricChargePerAreaQuantity.UnitChoicesEnum.MicrocoulombPerSquareCentimetre));
            SetChoice(ElectricChargePerMassQuantity.Instance, ElectricChargePerMassQuantity.Instance.GetUnitChoice(ElectricChargePerMassQuantity.UnitChoicesEnum.MillicoulombPerKilogram));
            SetChoice(ElectricChargePerVolumeQuantity.Instance, ElectricChargePerVolumeQuantity.Instance.GetUnitChoice(ElectricChargePerVolumeQuantity.UnitChoicesEnum.MicrocoulombPerCubicMetre));
            SetChoice(ElectricChargeQuantity.Instance, ElectricChargeQuantity.Instance.GetUnitChoice(ElectricChargeQuantity.UnitChoicesEnum.AmpereHour));
            SetChoice(ElectricConductanceQuantity.Instance, ElectricConductanceQuantity.Instance.GetUnitChoice(ElectricConductanceQuantity.UnitChoicesEnum.Millisiemens));
            SetChoice(ElectricConductivityQuantity.Instance, ElectricConductivityQuantity.Instance.GetUnitChoice(ElectricConductivityQuantity.UnitChoicesEnum.MillisiemensPerMetre));
            SetChoice(ElectricCurrentDensityQuantity.Instance, ElectricCurrentDensityQuantity.Instance.GetUnitChoice(ElectricCurrentDensityQuantity.UnitChoicesEnum.MilliamperePerSquareCentimetre));
            SetChoice(ElectricDipoleMomentQuantity.Instance, ElectricDipoleMomentQuantity.Instance.GetUnitChoice(ElectricDipoleMomentQuantity.UnitChoicesEnum.ElementaryChargeNanometre));
            SetChoice(ElectricFieldStrengthQuantity.Instance, ElectricFieldStrengthQuantity.Instance.GetUnitChoice(ElectricFieldStrengthQuantity.UnitChoicesEnum.KilovoltPerMetre));
            SetChoice(ElectricResistanceGradientPerLengthQuantity.Instance, ElectricResistanceGradientPerLengthQuantity.Instance.GetUnitChoice(ElectricResistanceGradientPerLengthQuantity.UnitChoicesEnum.OhmPerKilometre));
            SetChoice(ElectricalMobilityQuantity.Instance, ElectricalMobilityQuantity.Instance.GetUnitChoice(ElectricalMobilityQuantity.UnitChoicesEnum.SquareCentimetrePerVoltSecond));
            SetChoice(ElectromagneticMomentQuantity.Instance, ElectromagneticMomentQuantity.Instance.GetUnitChoice(ElectromagneticMomentQuantity.UnitChoicesEnum.AmpereSquareCentimetre));
            SetChoice(ForceAreaQuantity.Instance, ForceAreaQuantity.Instance.GetUnitChoice(ForceAreaQuantity.UnitChoicesEnum.KilonewtonSquareMetre));
            SetChoice(ForcePerVolumeQuantity.Instance, ForcePerVolumeQuantity.Instance.GetUnitChoice(ForcePerVolumeQuantity.UnitChoicesEnum.KilonewtonPerCubicMetre));
            SetChoice(HeatCapacityQuantity.Instance, HeatCapacityQuantity.Instance.GetUnitChoice(HeatCapacityQuantity.UnitChoicesEnum.KilojoulePerKelvin));
            SetChoice(IlluminanceQuantity.Instance, IlluminanceQuantity.Instance.GetUnitChoice(IlluminanceQuantity.UnitChoicesEnum.Kilolux));
            SetChoice(InductanceQuantity.Instance, InductanceQuantity.Instance.GetUnitChoice(InductanceQuantity.UnitChoicesEnum.Millihenry));
            SetChoice(KinematicViscosityQuantity.Instance, KinematicViscosityQuantity.Instance.GetUnitChoice(KinematicViscosityQuantity.UnitChoicesEnum.Centistokes));
            SetChoice(LengthPerAngleQuantity.Instance, LengthPerAngleQuantity.Instance.GetUnitChoice(LengthPerAngleQuantity.UnitChoicesEnum.MetrePerDegree));
            SetChoice(LengthPerMassQuantity.Instance, LengthPerMassQuantity.Instance.GetUnitChoice(LengthPerMassQuantity.UnitChoicesEnum.MillimetrePerKilogram));
            SetChoice(LengthPerPressureQuantity.Instance, LengthPerPressureQuantity.Instance.GetUnitChoice(LengthPerPressureQuantity.UnitChoicesEnum.MillimetrePerBar));
            SetChoice(LengthPerTemperatureQuantity.Instance, LengthPerTemperatureQuantity.Instance.GetUnitChoice(LengthPerTemperatureQuantity.UnitChoicesEnum.MillimetrePerKelvin));
            SetChoice(LengthPerVolumeQuantity.Instance, LengthPerVolumeQuantity.Instance.GetUnitChoice(LengthPerVolumeQuantity.UnitChoicesEnum.MetrePerCubicMetre));
            SetChoice(LightExposureQuantity.Instance, LightExposureQuantity.Instance.GetUnitChoice(LightExposureQuantity.UnitChoicesEnum.LuxHour));
            SetChoice(LinearThermalExpansionCoefficientQuantity.Instance, LinearThermalExpansionCoefficientQuantity.Instance.GetUnitChoice(LinearThermalExpansionCoefficientQuantity.UnitChoicesEnum.MicrometrePerMetreKelvin));
            SetChoice(LuminanceQuantity.Instance, LuminanceQuantity.Instance.GetUnitChoice(LuminanceQuantity.UnitChoicesEnum.CandelaPerSquareCentimetre));
            SetChoice(LuminousEfficacyQuantity.Instance, LuminousEfficacyQuantity.Instance.GetUnitChoice(LuminousEfficacyQuantity.UnitChoicesEnum.LumenPerKilowatt));
            SetChoice(LuminousEnergyQuantity.Instance, LuminousEnergyQuantity.Instance.GetUnitChoice(LuminousEnergyQuantity.UnitChoicesEnum.LumenHour));
            SetChoice(LuminousExitanceQuantity.Instance, LuminousExitanceQuantity.Instance.GetUnitChoice(LuminousExitanceQuantity.UnitChoicesEnum.LumenPerSquareMetre));
            SetChoice(LuminousFluxQuantity.Instance, LuminousFluxQuantity.Instance.GetUnitChoice(LuminousFluxQuantity.UnitChoicesEnum.Kilolumen));
            SetChoice(MagneticFieldStrengthQuantity.Instance, MagneticFieldStrengthQuantity.Instance.GetUnitChoice(MagneticFieldStrengthQuantity.UnitChoicesEnum.AmperePerCentimetre));
            SetChoice(MagneticFluxDensityGradientPerLengthQuantity.Instance, MagneticFluxDensityGradientPerLengthQuantity.Instance.GetUnitChoice(MagneticFluxDensityGradientPerLengthQuantity.UnitChoicesEnum.MicroteslaPerMetre));
            SetChoice(MagneticPermeabilityQuantity.Instance, MagneticPermeabilityQuantity.Instance.GetUnitChoice(MagneticPermeabilityQuantity.UnitChoicesEnum.MicrohenryPerMetre));
            SetChoice(MagneticVectorPotentialQuantity.Instance, MagneticVectorPotentialQuantity.Instance.GetUnitChoice(MagneticVectorPotentialQuantity.UnitChoicesEnum.WeberPerMetre));
            SetChoice(MassLengthQuantity.Instance, MassLengthQuantity.Instance.GetUnitChoice(MassLengthQuantity.UnitChoicesEnum.GramCentimetre));
            SetChoice(MassPerAreaQuantity.Instance, MassPerAreaQuantity.Instance.GetUnitChoice(MassPerAreaQuantity.UnitChoicesEnum.GramPerSquareCentimetre));
            SetChoice(MassRateGradientPerLengthQuantity.Instance, MassRateGradientPerLengthQuantity.Instance.GetUnitChoice(MassRateGradientPerLengthQuantity.UnitChoicesEnum.KilogramPerMinuteMetre));
            SetChoice(MassRatePerAreaQuantity.Instance, MassRatePerAreaQuantity.Instance.GetUnitChoice(MassRatePerAreaQuantity.UnitChoicesEnum.KilogramPerSquareMetreHour));
            SetChoice(MobilityQuantity.Instance, MobilityQuantity.Instance.GetUnitChoice(MobilityQuantity.UnitChoicesEnum.MillidarcyPerCentipoise));
            SetChoice(MolarEnergyQuantity.Instance, MolarEnergyQuantity.Instance.GetUnitChoice(MolarEnergyQuantity.UnitChoicesEnum.KilojoulePerMole));
            SetChoice(MolarHeatCapacityQuantity.Instance, MolarHeatCapacityQuantity.Instance.GetUnitChoice(MolarHeatCapacityQuantity.UnitChoicesEnum.KilojoulePerKilomoleKelvin));
            SetChoice(MolarMassQuantity.Instance, MolarMassQuantity.Instance.GetUnitChoice(MolarMassQuantity.UnitChoicesEnum.GramPerMole));
            SetChoice(MolarVolumeQuantity.Instance, MolarVolumeQuantity.Instance.GetUnitChoice(MolarVolumeQuantity.UnitChoicesEnum.LitrePerMole));
            SetChoice(MomentumQuantity.Instance, MomentumQuantity.Instance.GetUnitChoice(MomentumQuantity.UnitChoicesEnum.NewtonSecond));
            SetChoice(PermeabilityLengthQuantity.Instance, PermeabilityLengthQuantity.Instance.GetUnitChoice(PermeabilityLengthQuantity.UnitChoicesEnum.DarcyMetre));
            SetChoice(PermittivityQuantity.Instance, PermittivityQuantity.Instance.GetUnitChoice(PermittivityQuantity.UnitChoicesEnum.PicofaradPerMetre));
            SetChoice(PlaneAnglePerVolumeQuantity.Instance, PlaneAnglePerVolumeQuantity.Instance.GetUnitChoice(PlaneAnglePerVolumeQuantity.UnitChoicesEnum.DegreePerLitre));
            SetChoice(PowerPerAreaQuantity.Instance, PowerPerAreaQuantity.Instance.GetUnitChoice(PowerPerAreaQuantity.UnitChoicesEnum.KilowattPerSquareMetre));
            SetChoice(PowerPerVolumeQuantity.Instance, PowerPerVolumeQuantity.Instance.GetUnitChoice(PowerPerVolumeQuantity.UnitChoicesEnum.KilowattPerCubicMetre));
            SetChoice(PowerRateOfChangeQuantity.Instance, PowerRateOfChangeQuantity.Instance.GetUnitChoice(PowerRateOfChangeQuantity.UnitChoicesEnum.KilowattPerSecond));
            SetChoice(ProductivityIndexQuantity.Instance, ProductivityIndexQuantity.Instance.GetUnitChoice(ProductivityIndexQuantity.UnitChoicesEnum.CubicMetrePerDayBar));
            SetChoice(ProportionRateOfChangeQuantity.Instance, ProportionRateOfChangeQuantity.Instance.GetUnitChoice(ProportionRateOfChangeQuantity.UnitChoicesEnum.PercentPerSecond));
            SetChoice(RadianceQuantity.Instance, RadianceQuantity.Instance.GetUnitChoice(RadianceQuantity.UnitChoicesEnum.KilowattPerSteradianSquareMetre));
            SetChoice(RadiantIntensityQuantity.Instance, RadiantIntensityQuantity.Instance.GetUnitChoice(RadiantIntensityQuantity.UnitChoicesEnum.KilowattPerSteradian));
            SetChoice(RadioactivityQuantity.Instance, RadioactivityQuantity.Instance.GetUnitChoice(RadioactivityQuantity.UnitChoicesEnum.Kilobecquerel));
            SetChoice(ReciprocalAreaQuantity.Instance, ReciprocalAreaQuantity.Instance.GetUnitChoice(ReciprocalAreaQuantity.UnitChoicesEnum.ReciprocalSquareMetre));
            SetChoice(ReciprocalElectricTensionQuantity.Instance, ReciprocalElectricTensionQuantity.Instance.GetUnitChoice(ReciprocalElectricTensionQuantity.UnitChoicesEnum.ReciprocalMillivolt));
            SetChoice(ReciprocalForceQuantity.Instance, ReciprocalForceQuantity.Instance.GetUnitChoice(ReciprocalForceQuantity.UnitChoicesEnum.ReciprocalKilonewton));
            SetChoice(ReciprocalMassQuantity.Instance, ReciprocalMassQuantity.Instance.GetUnitChoice(ReciprocalMassQuantity.UnitChoicesEnum.ReciprocalGram));
            SetChoice(ReciprocalTimeQuantity.Instance, ReciprocalTimeQuantity.Instance.GetUnitChoice(ReciprocalTimeQuantity.UnitChoicesEnum.ReciprocalMinute));
            SetChoice(ReciprocalVolumeQuantity.Instance, ReciprocalVolumeQuantity.Instance.GetUnitChoice(ReciprocalVolumeQuantity.UnitChoicesEnum.ReciprocalLitre));
            SetChoice(ReluctanceQuantity.Instance, ReluctanceQuantity.Instance.GetUnitChoice(ReluctanceQuantity.UnitChoicesEnum.ReciprocalMillihenry));
            SetChoice(SectionModulusQuantity.Instance, SectionModulusQuantity.Instance.GetUnitChoice(SectionModulusQuantity.UnitChoicesEnum.CubicCentimetre));
            SetChoice(SpecificActivityQuantity.Instance, SpecificActivityQuantity.Instance.GetUnitChoice(SpecificActivityQuantity.UnitChoicesEnum.BecquerelPerGram));
            SetChoice(SpecificEnergyQuantity.Instance, SpecificEnergyQuantity.Instance.GetUnitChoice(SpecificEnergyQuantity.UnitChoicesEnum.KilojoulePerKilogram));
            SetChoice(SpecificProductivityIndexQuantity.Instance, SpecificProductivityIndexQuantity.Instance.GetUnitChoice(SpecificProductivityIndexQuantity.UnitChoicesEnum.CubicMetrePerDayBarMetre));
            SetChoice(ThermalConductanceQuantity.Instance, ThermalConductanceQuantity.Instance.GetUnitChoice(ThermalConductanceQuantity.UnitChoicesEnum.KilowattPerKelvin));
            SetChoice(ThermalInsulanceQuantity.Instance, ThermalInsulanceQuantity.Instance.GetUnitChoice(ThermalInsulanceQuantity.UnitChoicesEnum.SquareMetreKelvinPerWatt));
            SetChoice(ThermalResistanceQuantity.Instance, ThermalResistanceQuantity.Instance.GetUnitChoice(ThermalResistanceQuantity.UnitChoicesEnum.KelvinPerKilowatt));
            SetChoice(TimePerLengthQuantity.Instance, TimePerLengthQuantity.Instance.GetUnitChoice(TimePerLengthQuantity.UnitChoicesEnum.MinutePerMetre));
            SetChoice(TimePerMassQuantity.Instance, TimePerMassQuantity.Instance.GetUnitChoice(TimePerMassQuantity.UnitChoicesEnum.MinutePerKilogram));
            SetChoice(TimePerVolumeQuantity.Instance, TimePerVolumeQuantity.Instance.GetUnitChoice(TimePerVolumeQuantity.UnitChoicesEnum.MinutePerLitre));
            SetChoice(VolumeGradientPerLengthQuantity.Instance, VolumeGradientPerLengthQuantity.Instance.GetUnitChoice(VolumeGradientPerLengthQuantity.UnitChoicesEnum.LitrePerMetre));
            SetChoice(VolumePerAngleQuantity.Instance, VolumePerAngleQuantity.Instance.GetUnitChoice(VolumePerAngleQuantity.UnitChoicesEnum.LitrePerRevolution));
            SetChoice(VolumePerAreaQuantity.Instance, VolumePerAreaQuantity.Instance.GetUnitChoice(VolumePerAreaQuantity.UnitChoicesEnum.LitrePerSquareMetre));
            SetChoice(VolumePerAreaRateOfChangeQuantity.Instance, VolumePerAreaRateOfChangeQuantity.Instance.GetUnitChoice(VolumePerAreaRateOfChangeQuantity.UnitChoicesEnum.LitrePerSquareMetreMinute));
            SetChoice(VolumePerLengthRateOfChangeQuantity.Instance, VolumePerLengthRateOfChangeQuantity.Instance.GetUnitChoice(VolumePerLengthRateOfChangeQuantity.UnitChoicesEnum.LitrePerMetreMinute));
            SetChoice(VolumePerPressureQuantity.Instance, VolumePerPressureQuantity.Instance.GetUnitChoice(VolumePerPressureQuantity.UnitChoicesEnum.LitrePerBar));
            SetChoice(VolumePerPressureRateOfChangeQuantity.Instance, VolumePerPressureRateOfChangeQuantity.Instance.GetUnitChoice(VolumePerPressureRateOfChangeQuantity.UnitChoicesEnum.LitrePerBarMinute));
            SetChoice(VolumePerVolumeRateOfChangeQuantity.Instance, VolumePerVolumeRateOfChangeQuantity.Instance.GetUnitChoice(VolumePerVolumeRateOfChangeQuantity.UnitChoicesEnum.PercentPerSecond));
            SetChoice(VolumetricFlowRateGradientPerLengthQuantity.Instance, VolumetricFlowRateGradientPerLengthQuantity.Instance.GetUnitChoice(VolumetricFlowRateGradientPerLengthQuantity.UnitChoicesEnum.LitrePerMinuteMetre));
            SetChoice(VolumetricFlowRatePerAreaQuantity.Instance, VolumetricFlowRatePerAreaQuantity.Instance.GetUnitChoice(VolumetricFlowRatePerAreaQuantity.UnitChoicesEnum.LitrePerSquareMetreSecond));
            SetChoice(VolumetricHeatTransferCoefficientQuantity.Instance, VolumetricHeatTransferCoefficientQuantity.Instance.GetUnitChoice(VolumetricHeatTransferCoefficientQuantity.UnitChoicesEnum.KilowattPerCubicMetreKelvin));
            SetChoice(VolumetricThermalExpansionCoefficientQuantity.Instance, VolumetricThermalExpansionCoefficientQuantity.Instance.GetUnitChoice(VolumetricThermalExpansionCoefficientQuantity.UnitChoicesEnum.PercentPerKelvin));
        }

        private void AddNewUSChoices()
        {
            SetChoice(BendingMomentGradientQuantity.Instance, BendingMomentGradientQuantity.Instance.GetUnitChoice(BendingMomentGradientQuantity.UnitChoicesEnum.FootPoundForcePerFoot));
            SetChoice(EnergyPerAreaQuantity.Instance, EnergyPerAreaQuantity.Instance.GetUnitChoice(EnergyPerAreaQuantity.UnitChoicesEnum.FootPoundForcePerSquareFoot));
            SetChoice(MassPerEnergyQuantity.Instance, MassPerEnergyQuantity.Instance.GetUnitChoice(MassPerEnergyQuantity.UnitChoicesEnum.PoundPerBritishThermalUnit));
            SetChoice(PressurePerVolumeQuantity.Instance, PressurePerVolumeQuantity.Instance.GetUnitChoice(PressurePerVolumeQuantity.UnitChoicesEnum.PsiPerUSGallon));
            SetChoice(PressureTimePerVolumeQuantity.Instance, PressureTimePerVolumeQuantity.Instance.GetUnitChoice(PressureTimePerVolumeQuantity.UnitChoicesEnum.PsiSecondPerUSGallon));
            SetChoice(RelativeTemperaturePerPressureQuantity.Instance, RelativeTemperaturePerPressureQuantity.Instance.GetUnitChoice(RelativeTemperaturePerPressureQuantity.UnitChoicesEnum.RankinePerPsi));
            SetChoice(TemperatureRateOfChangeQuantity.Instance, TemperatureRateOfChangeQuantity.Instance.GetUnitChoice(TemperatureRateOfChangeQuantity.UnitChoicesEnum.RankinePerMinute));
            SetChoice(VolumePerEnergyQuantity.Instance, VolumePerEnergyQuantity.Instance.GetUnitChoice(VolumePerEnergyQuantity.UnitChoicesEnum.USGallonPerBritishThermalUnit));
            SetChoice(WorkGradientQuantity.Instance, WorkGradientQuantity.Instance.GetUnitChoice(WorkGradientQuantity.UnitChoicesEnum.FootPoundForcePerFoot));
            SetChoice(AbsorbedDoseQuantity.Instance, AbsorbedDoseQuantity.Instance.GetUnitChoice(AbsorbedDoseQuantity.UnitChoicesEnum.Rad));
            SetChoice(AmountSubstancePerAreaQuantity.Instance, AmountSubstancePerAreaQuantity.Instance.GetUnitChoice(AmountSubstancePerAreaQuantity.UnitChoicesEnum.MillimolePerSquareMetre));
            SetChoice(AmountSubstancePerAreaRateOfChangeQuantity.Instance, AmountSubstancePerAreaRateOfChangeQuantity.Instance.GetUnitChoice(AmountSubstancePerAreaRateOfChangeQuantity.UnitChoicesEnum.MillimolePerSquareMetreSecond));
            SetChoice(AmountSubstancePerVolumeQuantity.Instance, AmountSubstancePerVolumeQuantity.Instance.GetUnitChoice(AmountSubstancePerVolumeQuantity.UnitChoicesEnum.MillimolePerLitre));
            SetChoice(AmountSubstanceRateQuantity.Instance, AmountSubstanceRateQuantity.Instance.GetUnitChoice(AmountSubstanceRateQuantity.UnitChoicesEnum.MolePerMinute));
            SetChoice(AngularVelocityPerVolumetricFlowRateQuantity.Instance, AngularVelocityPerVolumetricFlowRateQuantity.Instance.GetUnitChoice(AngularVelocityPerVolumetricFlowRateQuantity.UnitChoicesEnum.RevolutionPerMinutePerGallonUSPerMinute));
            SetChoice(AreaPerAmountSubstanceQuantity.Instance, AreaPerAmountSubstanceQuantity.Instance.GetUnitChoice(AreaPerAmountSubstanceQuantity.UnitChoicesEnum.SquareCentimetrePerMole));
            SetChoice(AreaPerMassQuantity.Instance, AreaPerMassQuantity.Instance.GetUnitChoice(AreaPerMassQuantity.UnitChoicesEnum.SquareFootPerPound));
            SetChoice(AreaPerVolumeQuantity.Instance, AreaPerVolumeQuantity.Instance.GetUnitChoice(AreaPerVolumeQuantity.UnitChoicesEnum.SquareFootPerCubicFoot));
            SetChoice(AreaRateOfChangeQuantity.Instance, AreaRateOfChangeQuantity.Instance.GetUnitChoice(AreaRateOfChangeQuantity.UnitChoicesEnum.SquareFootPerSecond));
            SetChoice(DataStorageQuantity.Instance, DataStorageQuantity.Instance.GetUnitChoice(DataStorageQuantity.UnitChoicesEnum.Byte));
            SetChoice(DataTransferRateQuantity.Instance, DataTransferRateQuantity.Instance.GetUnitChoice(DataTransferRateQuantity.UnitChoicesEnum.MegabitPerSecond));
            SetChoice(DiffusivityQuantity.Instance, DiffusivityQuantity.Instance.GetUnitChoice(DiffusivityQuantity.UnitChoicesEnum.SquareFootPerHour));
            SetChoice(DigitalSymbolRateQuantity.Instance, DigitalSymbolRateQuantity.Instance.GetUnitChoice(DigitalSymbolRateQuantity.UnitChoicesEnum.Kilobaud));
            SetChoice(DoseEquivalentQuantity.Instance, DoseEquivalentQuantity.Instance.GetUnitChoice(DoseEquivalentQuantity.UnitChoicesEnum.Millisievert));
            SetChoice(DoseEquivalentRateQuantity.Instance, DoseEquivalentRateQuantity.Instance.GetUnitChoice(DoseEquivalentRateQuantity.UnitChoicesEnum.MicrosievertPerHour));
            SetChoice(ElectricChargePerAreaQuantity.Instance, ElectricChargePerAreaQuantity.Instance.GetUnitChoice(ElectricChargePerAreaQuantity.UnitChoicesEnum.CoulombPerSquareMetre));
            SetChoice(ElectricChargePerMassQuantity.Instance, ElectricChargePerMassQuantity.Instance.GetUnitChoice(ElectricChargePerMassQuantity.UnitChoicesEnum.CoulombPerKilogram));
            SetChoice(ElectricChargePerVolumeQuantity.Instance, ElectricChargePerVolumeQuantity.Instance.GetUnitChoice(ElectricChargePerVolumeQuantity.UnitChoicesEnum.CoulombPerCubicMetre));
            SetChoice(ElectricChargeQuantity.Instance, ElectricChargeQuantity.Instance.GetUnitChoice(ElectricChargeQuantity.UnitChoicesEnum.AmpereHour));
            SetChoice(ElectricConductanceQuantity.Instance, ElectricConductanceQuantity.Instance.GetUnitChoice(ElectricConductanceQuantity.UnitChoicesEnum.Siemens));
            SetChoice(ElectricConductivityQuantity.Instance, ElectricConductivityQuantity.Instance.GetUnitChoice(ElectricConductivityQuantity.UnitChoicesEnum.SiemensPerMetre));
            SetChoice(ElectricCurrentDensityQuantity.Instance, ElectricCurrentDensityQuantity.Instance.GetUnitChoice(ElectricCurrentDensityQuantity.UnitChoicesEnum.AmperePerSquareMetre));
            SetChoice(ElectricDipoleMomentQuantity.Instance, ElectricDipoleMomentQuantity.Instance.GetUnitChoice(ElectricDipoleMomentQuantity.UnitChoicesEnum.ElementaryChargeNanometre));
            SetChoice(ElectricFieldStrengthQuantity.Instance, ElectricFieldStrengthQuantity.Instance.GetUnitChoice(ElectricFieldStrengthQuantity.UnitChoicesEnum.VoltPerMetre));
            SetChoice(ElectricResistanceGradientPerLengthQuantity.Instance, ElectricResistanceGradientPerLengthQuantity.Instance.GetUnitChoice(ElectricResistanceGradientPerLengthQuantity.UnitChoicesEnum.OhmPerFoot));
            SetChoice(ElectricalMobilityQuantity.Instance, ElectricalMobilityQuantity.Instance.GetUnitChoice(ElectricalMobilityQuantity.UnitChoicesEnum.SquareCentimetrePerVoltSecond));
            SetChoice(ElectromagneticMomentQuantity.Instance, ElectromagneticMomentQuantity.Instance.GetUnitChoice(ElectromagneticMomentQuantity.UnitChoicesEnum.AmpereSquareMetre));
            SetChoice(ForceAreaQuantity.Instance, ForceAreaQuantity.Instance.GetUnitChoice(ForceAreaQuantity.UnitChoicesEnum.PoundForceSquareFoot));
            SetChoice(ForcePerVolumeQuantity.Instance, ForcePerVolumeQuantity.Instance.GetUnitChoice(ForcePerVolumeQuantity.UnitChoicesEnum.PoundForcePerCubicFoot));
            SetChoice(HeatCapacityQuantity.Instance, HeatCapacityQuantity.Instance.GetUnitChoice(HeatCapacityQuantity.UnitChoicesEnum.BritishThermalUnitPerDegreeFahrenheit));
            SetChoice(IlluminanceQuantity.Instance, IlluminanceQuantity.Instance.GetUnitChoice(IlluminanceQuantity.UnitChoicesEnum.FootCandle));
            SetChoice(InductanceQuantity.Instance, InductanceQuantity.Instance.GetUnitChoice(InductanceQuantity.UnitChoicesEnum.Henry));
            SetChoice(KinematicViscosityQuantity.Instance, KinematicViscosityQuantity.Instance.GetUnitChoice(KinematicViscosityQuantity.UnitChoicesEnum.Centistokes));
            SetChoice(LengthPerAngleQuantity.Instance, LengthPerAngleQuantity.Instance.GetUnitChoice(LengthPerAngleQuantity.UnitChoicesEnum.FootPerDegree));
            SetChoice(LengthPerMassQuantity.Instance, LengthPerMassQuantity.Instance.GetUnitChoice(LengthPerMassQuantity.UnitChoicesEnum.FootPerPound));
            SetChoice(LengthPerPressureQuantity.Instance, LengthPerPressureQuantity.Instance.GetUnitChoice(LengthPerPressureQuantity.UnitChoicesEnum.InchPerPsi));
            SetChoice(LengthPerTemperatureQuantity.Instance, LengthPerTemperatureQuantity.Instance.GetUnitChoice(LengthPerTemperatureQuantity.UnitChoicesEnum.FootPerDegreeFahrenheit));
            SetChoice(LengthPerVolumeQuantity.Instance, LengthPerVolumeQuantity.Instance.GetUnitChoice(LengthPerVolumeQuantity.UnitChoicesEnum.FootPerCubicFoot));
            SetChoice(LightExposureQuantity.Instance, LightExposureQuantity.Instance.GetUnitChoice(LightExposureQuantity.UnitChoicesEnum.FootCandleHour));
            SetChoice(LinearThermalExpansionCoefficientQuantity.Instance, LinearThermalExpansionCoefficientQuantity.Instance.GetUnitChoice(LinearThermalExpansionCoefficientQuantity.UnitChoicesEnum.ReciprocalDegreeFahrenheit));
            SetChoice(LuminanceQuantity.Instance, LuminanceQuantity.Instance.GetUnitChoice(LuminanceQuantity.UnitChoicesEnum.CandelaPerSquareFoot));
            SetChoice(LuminousEfficacyQuantity.Instance, LuminousEfficacyQuantity.Instance.GetUnitChoice(LuminousEfficacyQuantity.UnitChoicesEnum.LumenPerWatt));
            SetChoice(LuminousEnergyQuantity.Instance, LuminousEnergyQuantity.Instance.GetUnitChoice(LuminousEnergyQuantity.UnitChoicesEnum.LumenSecond));
            SetChoice(LuminousExitanceQuantity.Instance, LuminousExitanceQuantity.Instance.GetUnitChoice(LuminousExitanceQuantity.UnitChoicesEnum.LumenPerSquareFoot));
            SetChoice(LuminousFluxQuantity.Instance, LuminousFluxQuantity.Instance.GetUnitChoice(LuminousFluxQuantity.UnitChoicesEnum.Lumen));
            SetChoice(MagneticFieldStrengthQuantity.Instance, MagneticFieldStrengthQuantity.Instance.GetUnitChoice(MagneticFieldStrengthQuantity.UnitChoicesEnum.AmperePerMetre));
            SetChoice(MagneticFluxDensityGradientPerLengthQuantity.Instance, MagneticFluxDensityGradientPerLengthQuantity.Instance.GetUnitChoice(MagneticFluxDensityGradientPerLengthQuantity.UnitChoicesEnum.GaussPerCentimetre));
            SetChoice(MagneticPermeabilityQuantity.Instance, MagneticPermeabilityQuantity.Instance.GetUnitChoice(MagneticPermeabilityQuantity.UnitChoicesEnum.HenryPerMetre));
            SetChoice(MagneticVectorPotentialQuantity.Instance, MagneticVectorPotentialQuantity.Instance.GetUnitChoice(MagneticVectorPotentialQuantity.UnitChoicesEnum.GaussCentimetre));
            SetChoice(MassLengthQuantity.Instance, MassLengthQuantity.Instance.GetUnitChoice(MassLengthQuantity.UnitChoicesEnum.PoundFoot));
            SetChoice(MassPerAreaQuantity.Instance, MassPerAreaQuantity.Instance.GetUnitChoice(MassPerAreaQuantity.UnitChoicesEnum.PoundPerSquareFoot));
            SetChoice(MassRateGradientPerLengthQuantity.Instance, MassRateGradientPerLengthQuantity.Instance.GetUnitChoice(MassRateGradientPerLengthQuantity.UnitChoicesEnum.PoundPerMinuteFoot));
            SetChoice(MassRatePerAreaQuantity.Instance, MassRatePerAreaQuantity.Instance.GetUnitChoice(MassRatePerAreaQuantity.UnitChoicesEnum.PoundPerSquareFootSecond));
            SetChoice(MobilityQuantity.Instance, MobilityQuantity.Instance.GetUnitChoice(MobilityQuantity.UnitChoicesEnum.MillidarcyPerCentipoise));
            SetChoice(MolarEnergyQuantity.Instance, MolarEnergyQuantity.Instance.GetUnitChoice(MolarEnergyQuantity.UnitChoicesEnum.JoulePerMole));
            SetChoice(MolarHeatCapacityQuantity.Instance, MolarHeatCapacityQuantity.Instance.GetUnitChoice(MolarHeatCapacityQuantity.UnitChoicesEnum.JoulePerMoleKelvin));
            SetChoice(MolarMassQuantity.Instance, MolarMassQuantity.Instance.GetUnitChoice(MolarMassQuantity.UnitChoicesEnum.GramPerMole));
            SetChoice(MolarVolumeQuantity.Instance, MolarVolumeQuantity.Instance.GetUnitChoice(MolarVolumeQuantity.UnitChoicesEnum.LitrePerMole));
            SetChoice(MomentumQuantity.Instance, MomentumQuantity.Instance.GetUnitChoice(MomentumQuantity.UnitChoicesEnum.PoundFootPerSecond));
            SetChoice(PermeabilityLengthQuantity.Instance, PermeabilityLengthQuantity.Instance.GetUnitChoice(PermeabilityLengthQuantity.UnitChoicesEnum.MillidarcyFoot));
            SetChoice(PermittivityQuantity.Instance, PermittivityQuantity.Instance.GetUnitChoice(PermittivityQuantity.UnitChoicesEnum.PicofaradPerMetre));
            SetChoice(PlaneAnglePerVolumeQuantity.Instance, PlaneAnglePerVolumeQuantity.Instance.GetUnitChoice(PlaneAnglePerVolumeQuantity.UnitChoicesEnum.DegreePerLitre));
            SetChoice(PowerPerAreaQuantity.Instance, PowerPerAreaQuantity.Instance.GetUnitChoice(PowerPerAreaQuantity.UnitChoicesEnum.BritishThermalUnitPerHourSquareFoot));
            SetChoice(PowerPerVolumeQuantity.Instance, PowerPerVolumeQuantity.Instance.GetUnitChoice(PowerPerVolumeQuantity.UnitChoicesEnum.WattPerCubicMetre));
            SetChoice(PowerRateOfChangeQuantity.Instance, PowerRateOfChangeQuantity.Instance.GetUnitChoice(PowerRateOfChangeQuantity.UnitChoicesEnum.WattPerSecond));
            SetChoice(ProductivityIndexQuantity.Instance, ProductivityIndexQuantity.Instance.GetUnitChoice(ProductivityIndexQuantity.UnitChoicesEnum.BarrelPerDayPsi));
            SetChoice(ProportionRateOfChangeQuantity.Instance, ProportionRateOfChangeQuantity.Instance.GetUnitChoice(ProportionRateOfChangeQuantity.UnitChoicesEnum.PercentPerSecond));
            SetChoice(RadianceQuantity.Instance, RadianceQuantity.Instance.GetUnitChoice(RadianceQuantity.UnitChoicesEnum.WattPerSteradianSquareMetre));
            SetChoice(RadiantIntensityQuantity.Instance, RadiantIntensityQuantity.Instance.GetUnitChoice(RadiantIntensityQuantity.UnitChoicesEnum.WattPerSteradian));
            SetChoice(RadioactivityQuantity.Instance, RadioactivityQuantity.Instance.GetUnitChoice(RadioactivityQuantity.UnitChoicesEnum.Kilobecquerel));
            SetChoice(ReciprocalAreaQuantity.Instance, ReciprocalAreaQuantity.Instance.GetUnitChoice(ReciprocalAreaQuantity.UnitChoicesEnum.ReciprocalSquareInch));
            SetChoice(ReciprocalElectricTensionQuantity.Instance, ReciprocalElectricTensionQuantity.Instance.GetUnitChoice(ReciprocalElectricTensionQuantity.UnitChoicesEnum.ReciprocalVolt));
            SetChoice(ReciprocalForceQuantity.Instance, ReciprocalForceQuantity.Instance.GetUnitChoice(ReciprocalForceQuantity.UnitChoicesEnum.ReciprocalPoundForce));
            SetChoice(ReciprocalMassQuantity.Instance, ReciprocalMassQuantity.Instance.GetUnitChoice(ReciprocalMassQuantity.UnitChoicesEnum.ReciprocalPound));
            SetChoice(ReciprocalTimeQuantity.Instance, ReciprocalTimeQuantity.Instance.GetUnitChoice(ReciprocalTimeQuantity.UnitChoicesEnum.ReciprocalMinute));
            SetChoice(ReciprocalVolumeQuantity.Instance, ReciprocalVolumeQuantity.Instance.GetUnitChoice(ReciprocalVolumeQuantity.UnitChoicesEnum.ReciprocalCubicFoot));
            SetChoice(ReluctanceQuantity.Instance, ReluctanceQuantity.Instance.GetUnitChoice(ReluctanceQuantity.UnitChoicesEnum.ReciprocalHenry));
            SetChoice(SectionModulusQuantity.Instance, SectionModulusQuantity.Instance.GetUnitChoice(SectionModulusQuantity.UnitChoicesEnum.CubicInch));
            SetChoice(SpecificActivityQuantity.Instance, SpecificActivityQuantity.Instance.GetUnitChoice(SpecificActivityQuantity.UnitChoicesEnum.BecquerelPerGram));
            SetChoice(SpecificEnergyQuantity.Instance, SpecificEnergyQuantity.Instance.GetUnitChoice(SpecificEnergyQuantity.UnitChoicesEnum.BritishThermalUnitPerPound));
            SetChoice(SpecificProductivityIndexQuantity.Instance, SpecificProductivityIndexQuantity.Instance.GetUnitChoice(SpecificProductivityIndexQuantity.UnitChoicesEnum.BarrelPerDayPsiFoot));
            SetChoice(ThermalConductanceQuantity.Instance, ThermalConductanceQuantity.Instance.GetUnitChoice(ThermalConductanceQuantity.UnitChoicesEnum.BritishThermalUnitPerHourDegreeFahrenheit));
            SetChoice(ThermalInsulanceQuantity.Instance, ThermalInsulanceQuantity.Instance.GetUnitChoice(ThermalInsulanceQuantity.UnitChoicesEnum.SquareFootHourDegreeFahrenheitPerBritishThermalUnit));
            SetChoice(ThermalResistanceQuantity.Instance, ThermalResistanceQuantity.Instance.GetUnitChoice(ThermalResistanceQuantity.UnitChoicesEnum.HourDegreeFahrenheitPerBritishThermalUnit));
            SetChoice(TimePerLengthQuantity.Instance, TimePerLengthQuantity.Instance.GetUnitChoice(TimePerLengthQuantity.UnitChoicesEnum.SecondPerFoot));
            SetChoice(TimePerMassQuantity.Instance, TimePerMassQuantity.Instance.GetUnitChoice(TimePerMassQuantity.UnitChoicesEnum.SecondPerPound));
            SetChoice(TimePerVolumeQuantity.Instance, TimePerVolumeQuantity.Instance.GetUnitChoice(TimePerVolumeQuantity.UnitChoicesEnum.SecondPerCubicFoot));
            SetChoice(VolumeGradientPerLengthQuantity.Instance, VolumeGradientPerLengthQuantity.Instance.GetUnitChoice(VolumeGradientPerLengthQuantity.UnitChoicesEnum.CubicFootPerFoot));
            SetChoice(VolumePerAngleQuantity.Instance, VolumePerAngleQuantity.Instance.GetUnitChoice(VolumePerAngleQuantity.UnitChoicesEnum.CubicInchPerRevolution));
            SetChoice(VolumePerAreaQuantity.Instance, VolumePerAreaQuantity.Instance.GetUnitChoice(VolumePerAreaQuantity.UnitChoicesEnum.GallonUSPerSquareFoot));
            SetChoice(VolumePerAreaRateOfChangeQuantity.Instance, VolumePerAreaRateOfChangeQuantity.Instance.GetUnitChoice(VolumePerAreaRateOfChangeQuantity.UnitChoicesEnum.CubicMetrePerSquareMetreSecond));
            SetChoice(VolumePerLengthRateOfChangeQuantity.Instance, VolumePerLengthRateOfChangeQuantity.Instance.GetUnitChoice(VolumePerLengthRateOfChangeQuantity.UnitChoicesEnum.CubicMetrePerMetreSecond));
            SetChoice(VolumePerPressureQuantity.Instance, VolumePerPressureQuantity.Instance.GetUnitChoice(VolumePerPressureQuantity.UnitChoicesEnum.GallonUSPerPsi));
            SetChoice(VolumePerPressureRateOfChangeQuantity.Instance, VolumePerPressureRateOfChangeQuantity.Instance.GetUnitChoice(VolumePerPressureRateOfChangeQuantity.UnitChoicesEnum.CubicMetrePerPascalSecond));
            SetChoice(VolumePerVolumeRateOfChangeQuantity.Instance, VolumePerVolumeRateOfChangeQuantity.Instance.GetUnitChoice(VolumePerVolumeRateOfChangeQuantity.UnitChoicesEnum.PercentPerMinute));
            SetChoice(VolumetricFlowRateGradientPerLengthQuantity.Instance, VolumetricFlowRateGradientPerLengthQuantity.Instance.GetUnitChoice(VolumetricFlowRateGradientPerLengthQuantity.UnitChoicesEnum.GallonUSPerMinuteFoot));
            SetChoice(VolumetricFlowRatePerAreaQuantity.Instance, VolumetricFlowRatePerAreaQuantity.Instance.GetUnitChoice(VolumetricFlowRatePerAreaQuantity.UnitChoicesEnum.GallonUSPerMinuteSquareFoot));
            SetChoice(VolumetricHeatTransferCoefficientQuantity.Instance, VolumetricHeatTransferCoefficientQuantity.Instance.GetUnitChoice(VolumetricHeatTransferCoefficientQuantity.UnitChoicesEnum.WattPerCubicMetreKelvin));
            SetChoice(VolumetricThermalExpansionCoefficientQuantity.Instance, VolumetricThermalExpansionCoefficientQuantity.Instance.GetUnitChoice(VolumetricThermalExpansionCoefficientQuantity.UnitChoicesEnum.ReciprocalDegreeFahrenheit));
        }

        private void AddNewImperialChoices()
        {
            SetChoice(BendingMomentGradientQuantity.Instance, BendingMomentGradientQuantity.Instance.GetUnitChoice(BendingMomentGradientQuantity.UnitChoicesEnum.FootPoundForcePerFoot));
            SetChoice(EnergyPerAreaQuantity.Instance, EnergyPerAreaQuantity.Instance.GetUnitChoice(EnergyPerAreaQuantity.UnitChoicesEnum.FootPoundForcePerSquareFoot));
            SetChoice(MassPerEnergyQuantity.Instance, MassPerEnergyQuantity.Instance.GetUnitChoice(MassPerEnergyQuantity.UnitChoicesEnum.PoundPerBritishThermalUnit));
            SetChoice(PressurePerVolumeQuantity.Instance, PressurePerVolumeQuantity.Instance.GetUnitChoice(PressurePerVolumeQuantity.UnitChoicesEnum.PsiPerUKGallon));
            SetChoice(PressureTimePerVolumeQuantity.Instance, PressureTimePerVolumeQuantity.Instance.GetUnitChoice(PressureTimePerVolumeQuantity.UnitChoicesEnum.PsiSecondPerUKGallon));
            SetChoice(RelativeTemperaturePerPressureQuantity.Instance, RelativeTemperaturePerPressureQuantity.Instance.GetUnitChoice(RelativeTemperaturePerPressureQuantity.UnitChoicesEnum.RankinePerPsi));
            SetChoice(TemperatureRateOfChangeQuantity.Instance, TemperatureRateOfChangeQuantity.Instance.GetUnitChoice(TemperatureRateOfChangeQuantity.UnitChoicesEnum.RankinePerMinute));
            SetChoice(VolumePerEnergyQuantity.Instance, VolumePerEnergyQuantity.Instance.GetUnitChoice(VolumePerEnergyQuantity.UnitChoicesEnum.UKGallonPerBritishThermalUnit));
            SetChoice(WorkGradientQuantity.Instance, WorkGradientQuantity.Instance.GetUnitChoice(WorkGradientQuantity.UnitChoicesEnum.FootPoundForcePerFoot));
            SetChoice(AbsorbedDoseQuantity.Instance, AbsorbedDoseQuantity.Instance.GetUnitChoice(AbsorbedDoseQuantity.UnitChoicesEnum.Rad));
            SetChoice(AmountSubstancePerAreaQuantity.Instance, AmountSubstancePerAreaQuantity.Instance.GetUnitChoice(AmountSubstancePerAreaQuantity.UnitChoicesEnum.MillimolePerSquareMetre));
            SetChoice(AmountSubstancePerAreaRateOfChangeQuantity.Instance, AmountSubstancePerAreaRateOfChangeQuantity.Instance.GetUnitChoice(AmountSubstancePerAreaRateOfChangeQuantity.UnitChoicesEnum.MillimolePerSquareMetreSecond));
            SetChoice(AmountSubstancePerVolumeQuantity.Instance, AmountSubstancePerVolumeQuantity.Instance.GetUnitChoice(AmountSubstancePerVolumeQuantity.UnitChoicesEnum.MillimolePerLitre));
            SetChoice(AmountSubstanceRateQuantity.Instance, AmountSubstanceRateQuantity.Instance.GetUnitChoice(AmountSubstanceRateQuantity.UnitChoicesEnum.MolePerMinute));
            SetChoice(AngularVelocityPerVolumetricFlowRateQuantity.Instance, AngularVelocityPerVolumetricFlowRateQuantity.Instance.GetUnitChoice(AngularVelocityPerVolumetricFlowRateQuantity.UnitChoicesEnum.RevolutionPerMinutePerGallonUKPerMinute));
            SetChoice(AreaPerAmountSubstanceQuantity.Instance, AreaPerAmountSubstanceQuantity.Instance.GetUnitChoice(AreaPerAmountSubstanceQuantity.UnitChoicesEnum.SquareCentimetrePerMole));
            SetChoice(AreaPerMassQuantity.Instance, AreaPerMassQuantity.Instance.GetUnitChoice(AreaPerMassQuantity.UnitChoicesEnum.SquareFootPerPound));
            SetChoice(AreaPerVolumeQuantity.Instance, AreaPerVolumeQuantity.Instance.GetUnitChoice(AreaPerVolumeQuantity.UnitChoicesEnum.SquareFootPerCubicFoot));
            SetChoice(AreaRateOfChangeQuantity.Instance, AreaRateOfChangeQuantity.Instance.GetUnitChoice(AreaRateOfChangeQuantity.UnitChoicesEnum.SquareFootPerSecond));
            SetChoice(DataStorageQuantity.Instance, DataStorageQuantity.Instance.GetUnitChoice(DataStorageQuantity.UnitChoicesEnum.Byte));
            SetChoice(DataTransferRateQuantity.Instance, DataTransferRateQuantity.Instance.GetUnitChoice(DataTransferRateQuantity.UnitChoicesEnum.MegabitPerSecond));
            SetChoice(DiffusivityQuantity.Instance, DiffusivityQuantity.Instance.GetUnitChoice(DiffusivityQuantity.UnitChoicesEnum.SquareFootPerHour));
            SetChoice(DigitalSymbolRateQuantity.Instance, DigitalSymbolRateQuantity.Instance.GetUnitChoice(DigitalSymbolRateQuantity.UnitChoicesEnum.Kilobaud));
            SetChoice(DoseEquivalentQuantity.Instance, DoseEquivalentQuantity.Instance.GetUnitChoice(DoseEquivalentQuantity.UnitChoicesEnum.Millisievert));
            SetChoice(DoseEquivalentRateQuantity.Instance, DoseEquivalentRateQuantity.Instance.GetUnitChoice(DoseEquivalentRateQuantity.UnitChoicesEnum.MicrosievertPerHour));
            SetChoice(ElectricChargePerAreaQuantity.Instance, ElectricChargePerAreaQuantity.Instance.GetUnitChoice(ElectricChargePerAreaQuantity.UnitChoicesEnum.CoulombPerSquareMetre));
            SetChoice(ElectricChargePerMassQuantity.Instance, ElectricChargePerMassQuantity.Instance.GetUnitChoice(ElectricChargePerMassQuantity.UnitChoicesEnum.CoulombPerKilogram));
            SetChoice(ElectricChargePerVolumeQuantity.Instance, ElectricChargePerVolumeQuantity.Instance.GetUnitChoice(ElectricChargePerVolumeQuantity.UnitChoicesEnum.CoulombPerCubicMetre));
            SetChoice(ElectricChargeQuantity.Instance, ElectricChargeQuantity.Instance.GetUnitChoice(ElectricChargeQuantity.UnitChoicesEnum.AmpereHour));
            SetChoice(ElectricConductanceQuantity.Instance, ElectricConductanceQuantity.Instance.GetUnitChoice(ElectricConductanceQuantity.UnitChoicesEnum.Siemens));
            SetChoice(ElectricConductivityQuantity.Instance, ElectricConductivityQuantity.Instance.GetUnitChoice(ElectricConductivityQuantity.UnitChoicesEnum.SiemensPerMetre));
            SetChoice(ElectricCurrentDensityQuantity.Instance, ElectricCurrentDensityQuantity.Instance.GetUnitChoice(ElectricCurrentDensityQuantity.UnitChoicesEnum.AmperePerSquareMetre));
            SetChoice(ElectricDipoleMomentQuantity.Instance, ElectricDipoleMomentQuantity.Instance.GetUnitChoice(ElectricDipoleMomentQuantity.UnitChoicesEnum.ElementaryChargeNanometre));
            SetChoice(ElectricFieldStrengthQuantity.Instance, ElectricFieldStrengthQuantity.Instance.GetUnitChoice(ElectricFieldStrengthQuantity.UnitChoicesEnum.VoltPerMetre));
            SetChoice(ElectricResistanceGradientPerLengthQuantity.Instance, ElectricResistanceGradientPerLengthQuantity.Instance.GetUnitChoice(ElectricResistanceGradientPerLengthQuantity.UnitChoicesEnum.OhmPerFoot));
            SetChoice(ElectricalMobilityQuantity.Instance, ElectricalMobilityQuantity.Instance.GetUnitChoice(ElectricalMobilityQuantity.UnitChoicesEnum.SquareCentimetrePerVoltSecond));
            SetChoice(ElectromagneticMomentQuantity.Instance, ElectromagneticMomentQuantity.Instance.GetUnitChoice(ElectromagneticMomentQuantity.UnitChoicesEnum.AmpereSquareMetre));
            SetChoice(ForceAreaQuantity.Instance, ForceAreaQuantity.Instance.GetUnitChoice(ForceAreaQuantity.UnitChoicesEnum.PoundForceSquareFoot));
            SetChoice(ForcePerVolumeQuantity.Instance, ForcePerVolumeQuantity.Instance.GetUnitChoice(ForcePerVolumeQuantity.UnitChoicesEnum.PoundForcePerCubicFoot));
            SetChoice(HeatCapacityQuantity.Instance, HeatCapacityQuantity.Instance.GetUnitChoice(HeatCapacityQuantity.UnitChoicesEnum.BritishThermalUnitPerDegreeFahrenheit));
            SetChoice(IlluminanceQuantity.Instance, IlluminanceQuantity.Instance.GetUnitChoice(IlluminanceQuantity.UnitChoicesEnum.FootCandle));
            SetChoice(InductanceQuantity.Instance, InductanceQuantity.Instance.GetUnitChoice(InductanceQuantity.UnitChoicesEnum.Henry));
            SetChoice(KinematicViscosityQuantity.Instance, KinematicViscosityQuantity.Instance.GetUnitChoice(KinematicViscosityQuantity.UnitChoicesEnum.Centistokes));
            SetChoice(LengthPerAngleQuantity.Instance, LengthPerAngleQuantity.Instance.GetUnitChoice(LengthPerAngleQuantity.UnitChoicesEnum.FootPerDegree));
            SetChoice(LengthPerMassQuantity.Instance, LengthPerMassQuantity.Instance.GetUnitChoice(LengthPerMassQuantity.UnitChoicesEnum.FootPerPound));
            SetChoice(LengthPerPressureQuantity.Instance, LengthPerPressureQuantity.Instance.GetUnitChoice(LengthPerPressureQuantity.UnitChoicesEnum.InchPerPsi));
            SetChoice(LengthPerTemperatureQuantity.Instance, LengthPerTemperatureQuantity.Instance.GetUnitChoice(LengthPerTemperatureQuantity.UnitChoicesEnum.FootPerDegreeFahrenheit));
            SetChoice(LengthPerVolumeQuantity.Instance, LengthPerVolumeQuantity.Instance.GetUnitChoice(LengthPerVolumeQuantity.UnitChoicesEnum.FootPerCubicFoot));
            SetChoice(LightExposureQuantity.Instance, LightExposureQuantity.Instance.GetUnitChoice(LightExposureQuantity.UnitChoicesEnum.FootCandleHour));
            SetChoice(LinearThermalExpansionCoefficientQuantity.Instance, LinearThermalExpansionCoefficientQuantity.Instance.GetUnitChoice(LinearThermalExpansionCoefficientQuantity.UnitChoicesEnum.ReciprocalDegreeFahrenheit));
            SetChoice(LuminanceQuantity.Instance, LuminanceQuantity.Instance.GetUnitChoice(LuminanceQuantity.UnitChoicesEnum.CandelaPerSquareFoot));
            SetChoice(LuminousEfficacyQuantity.Instance, LuminousEfficacyQuantity.Instance.GetUnitChoice(LuminousEfficacyQuantity.UnitChoicesEnum.LumenPerWatt));
            SetChoice(LuminousEnergyQuantity.Instance, LuminousEnergyQuantity.Instance.GetUnitChoice(LuminousEnergyQuantity.UnitChoicesEnum.LumenSecond));
            SetChoice(LuminousExitanceQuantity.Instance, LuminousExitanceQuantity.Instance.GetUnitChoice(LuminousExitanceQuantity.UnitChoicesEnum.LumenPerSquareFoot));
            SetChoice(LuminousFluxQuantity.Instance, LuminousFluxQuantity.Instance.GetUnitChoice(LuminousFluxQuantity.UnitChoicesEnum.Lumen));
            SetChoice(MagneticFieldStrengthQuantity.Instance, MagneticFieldStrengthQuantity.Instance.GetUnitChoice(MagneticFieldStrengthQuantity.UnitChoicesEnum.AmperePerMetre));
            SetChoice(MagneticFluxDensityGradientPerLengthQuantity.Instance, MagneticFluxDensityGradientPerLengthQuantity.Instance.GetUnitChoice(MagneticFluxDensityGradientPerLengthQuantity.UnitChoicesEnum.GaussPerCentimetre));
            SetChoice(MagneticPermeabilityQuantity.Instance, MagneticPermeabilityQuantity.Instance.GetUnitChoice(MagneticPermeabilityQuantity.UnitChoicesEnum.HenryPerMetre));
            SetChoice(MagneticVectorPotentialQuantity.Instance, MagneticVectorPotentialQuantity.Instance.GetUnitChoice(MagneticVectorPotentialQuantity.UnitChoicesEnum.GaussCentimetre));
            SetChoice(MassLengthQuantity.Instance, MassLengthQuantity.Instance.GetUnitChoice(MassLengthQuantity.UnitChoicesEnum.PoundFoot));
            SetChoice(MassPerAreaQuantity.Instance, MassPerAreaQuantity.Instance.GetUnitChoice(MassPerAreaQuantity.UnitChoicesEnum.PoundPerSquareFoot));
            SetChoice(MassRateGradientPerLengthQuantity.Instance, MassRateGradientPerLengthQuantity.Instance.GetUnitChoice(MassRateGradientPerLengthQuantity.UnitChoicesEnum.PoundPerMinuteFoot));
            SetChoice(MassRatePerAreaQuantity.Instance, MassRatePerAreaQuantity.Instance.GetUnitChoice(MassRatePerAreaQuantity.UnitChoicesEnum.PoundPerSquareFootSecond));
            SetChoice(MobilityQuantity.Instance, MobilityQuantity.Instance.GetUnitChoice(MobilityQuantity.UnitChoicesEnum.MillidarcyPerCentipoise));
            SetChoice(MolarEnergyQuantity.Instance, MolarEnergyQuantity.Instance.GetUnitChoice(MolarEnergyQuantity.UnitChoicesEnum.JoulePerMole));
            SetChoice(MolarHeatCapacityQuantity.Instance, MolarHeatCapacityQuantity.Instance.GetUnitChoice(MolarHeatCapacityQuantity.UnitChoicesEnum.JoulePerMoleKelvin));
            SetChoice(MolarMassQuantity.Instance, MolarMassQuantity.Instance.GetUnitChoice(MolarMassQuantity.UnitChoicesEnum.GramPerMole));
            SetChoice(MolarVolumeQuantity.Instance, MolarVolumeQuantity.Instance.GetUnitChoice(MolarVolumeQuantity.UnitChoicesEnum.LitrePerMole));
            SetChoice(MomentumQuantity.Instance, MomentumQuantity.Instance.GetUnitChoice(MomentumQuantity.UnitChoicesEnum.PoundFootPerSecond));
            SetChoice(PermeabilityLengthQuantity.Instance, PermeabilityLengthQuantity.Instance.GetUnitChoice(PermeabilityLengthQuantity.UnitChoicesEnum.MillidarcyFoot));
            SetChoice(PermittivityQuantity.Instance, PermittivityQuantity.Instance.GetUnitChoice(PermittivityQuantity.UnitChoicesEnum.PicofaradPerMetre));
            SetChoice(PlaneAnglePerVolumeQuantity.Instance, PlaneAnglePerVolumeQuantity.Instance.GetUnitChoice(PlaneAnglePerVolumeQuantity.UnitChoicesEnum.DegreePerLitre));
            SetChoice(PowerPerAreaQuantity.Instance, PowerPerAreaQuantity.Instance.GetUnitChoice(PowerPerAreaQuantity.UnitChoicesEnum.BritishThermalUnitPerHourSquareFoot));
            SetChoice(PowerPerVolumeQuantity.Instance, PowerPerVolumeQuantity.Instance.GetUnitChoice(PowerPerVolumeQuantity.UnitChoicesEnum.WattPerCubicMetre));
            SetChoice(PowerRateOfChangeQuantity.Instance, PowerRateOfChangeQuantity.Instance.GetUnitChoice(PowerRateOfChangeQuantity.UnitChoicesEnum.WattPerSecond));
            SetChoice(ProductivityIndexQuantity.Instance, ProductivityIndexQuantity.Instance.GetUnitChoice(ProductivityIndexQuantity.UnitChoicesEnum.BarrelPerDayPsi));
            SetChoice(ProportionRateOfChangeQuantity.Instance, ProportionRateOfChangeQuantity.Instance.GetUnitChoice(ProportionRateOfChangeQuantity.UnitChoicesEnum.PercentPerSecond));
            SetChoice(RadianceQuantity.Instance, RadianceQuantity.Instance.GetUnitChoice(RadianceQuantity.UnitChoicesEnum.WattPerSteradianSquareMetre));
            SetChoice(RadiantIntensityQuantity.Instance, RadiantIntensityQuantity.Instance.GetUnitChoice(RadiantIntensityQuantity.UnitChoicesEnum.WattPerSteradian));
            SetChoice(RadioactivityQuantity.Instance, RadioactivityQuantity.Instance.GetUnitChoice(RadioactivityQuantity.UnitChoicesEnum.Kilobecquerel));
            SetChoice(ReciprocalAreaQuantity.Instance, ReciprocalAreaQuantity.Instance.GetUnitChoice(ReciprocalAreaQuantity.UnitChoicesEnum.ReciprocalSquareInch));
            SetChoice(ReciprocalElectricTensionQuantity.Instance, ReciprocalElectricTensionQuantity.Instance.GetUnitChoice(ReciprocalElectricTensionQuantity.UnitChoicesEnum.ReciprocalVolt));
            SetChoice(ReciprocalForceQuantity.Instance, ReciprocalForceQuantity.Instance.GetUnitChoice(ReciprocalForceQuantity.UnitChoicesEnum.ReciprocalPoundForce));
            SetChoice(ReciprocalMassQuantity.Instance, ReciprocalMassQuantity.Instance.GetUnitChoice(ReciprocalMassQuantity.UnitChoicesEnum.ReciprocalPound));
            SetChoice(ReciprocalTimeQuantity.Instance, ReciprocalTimeQuantity.Instance.GetUnitChoice(ReciprocalTimeQuantity.UnitChoicesEnum.ReciprocalMinute));
            SetChoice(ReciprocalVolumeQuantity.Instance, ReciprocalVolumeQuantity.Instance.GetUnitChoice(ReciprocalVolumeQuantity.UnitChoicesEnum.ReciprocalCubicFoot));
            SetChoice(ReluctanceQuantity.Instance, ReluctanceQuantity.Instance.GetUnitChoice(ReluctanceQuantity.UnitChoicesEnum.ReciprocalHenry));
            SetChoice(SectionModulusQuantity.Instance, SectionModulusQuantity.Instance.GetUnitChoice(SectionModulusQuantity.UnitChoicesEnum.CubicInch));
            SetChoice(SpecificActivityQuantity.Instance, SpecificActivityQuantity.Instance.GetUnitChoice(SpecificActivityQuantity.UnitChoicesEnum.BecquerelPerGram));
            SetChoice(SpecificEnergyQuantity.Instance, SpecificEnergyQuantity.Instance.GetUnitChoice(SpecificEnergyQuantity.UnitChoicesEnum.BritishThermalUnitPerPound));
            SetChoice(SpecificProductivityIndexQuantity.Instance, SpecificProductivityIndexQuantity.Instance.GetUnitChoice(SpecificProductivityIndexQuantity.UnitChoicesEnum.BarrelPerDayPsiFoot));
            SetChoice(ThermalConductanceQuantity.Instance, ThermalConductanceQuantity.Instance.GetUnitChoice(ThermalConductanceQuantity.UnitChoicesEnum.BritishThermalUnitPerHourDegreeFahrenheit));
            SetChoice(ThermalInsulanceQuantity.Instance, ThermalInsulanceQuantity.Instance.GetUnitChoice(ThermalInsulanceQuantity.UnitChoicesEnum.SquareFootHourDegreeFahrenheitPerBritishThermalUnit));
            SetChoice(ThermalResistanceQuantity.Instance, ThermalResistanceQuantity.Instance.GetUnitChoice(ThermalResistanceQuantity.UnitChoicesEnum.HourDegreeFahrenheitPerBritishThermalUnit));
            SetChoice(TimePerLengthQuantity.Instance, TimePerLengthQuantity.Instance.GetUnitChoice(TimePerLengthQuantity.UnitChoicesEnum.SecondPerFoot));
            SetChoice(TimePerMassQuantity.Instance, TimePerMassQuantity.Instance.GetUnitChoice(TimePerMassQuantity.UnitChoicesEnum.SecondPerPound));
            SetChoice(TimePerVolumeQuantity.Instance, TimePerVolumeQuantity.Instance.GetUnitChoice(TimePerVolumeQuantity.UnitChoicesEnum.SecondPerCubicFoot));
            SetChoice(VolumeGradientPerLengthQuantity.Instance, VolumeGradientPerLengthQuantity.Instance.GetUnitChoice(VolumeGradientPerLengthQuantity.UnitChoicesEnum.CubicFootPerFoot));
            SetChoice(VolumePerAngleQuantity.Instance, VolumePerAngleQuantity.Instance.GetUnitChoice(VolumePerAngleQuantity.UnitChoicesEnum.CubicInchPerRevolution));
            SetChoice(VolumePerAreaQuantity.Instance, VolumePerAreaQuantity.Instance.GetUnitChoice(VolumePerAreaQuantity.UnitChoicesEnum.GallonUKPerSquareFoot));
            SetChoice(VolumePerAreaRateOfChangeQuantity.Instance, VolumePerAreaRateOfChangeQuantity.Instance.GetUnitChoice(VolumePerAreaRateOfChangeQuantity.UnitChoicesEnum.CubicMetrePerSquareMetreSecond));
            SetChoice(VolumePerLengthRateOfChangeQuantity.Instance, VolumePerLengthRateOfChangeQuantity.Instance.GetUnitChoice(VolumePerLengthRateOfChangeQuantity.UnitChoicesEnum.CubicMetrePerMetreSecond));
            SetChoice(VolumePerPressureQuantity.Instance, VolumePerPressureQuantity.Instance.GetUnitChoice(VolumePerPressureQuantity.UnitChoicesEnum.GallonUKPerPsi));
            SetChoice(VolumePerPressureRateOfChangeQuantity.Instance, VolumePerPressureRateOfChangeQuantity.Instance.GetUnitChoice(VolumePerPressureRateOfChangeQuantity.UnitChoicesEnum.CubicMetrePerPascalSecond));
            SetChoice(VolumePerVolumeRateOfChangeQuantity.Instance, VolumePerVolumeRateOfChangeQuantity.Instance.GetUnitChoice(VolumePerVolumeRateOfChangeQuantity.UnitChoicesEnum.PercentPerMinute));
            SetChoice(VolumetricFlowRateGradientPerLengthQuantity.Instance, VolumetricFlowRateGradientPerLengthQuantity.Instance.GetUnitChoice(VolumetricFlowRateGradientPerLengthQuantity.UnitChoicesEnum.GallonUKPerMinuteFoot));
            SetChoice(VolumetricFlowRatePerAreaQuantity.Instance, VolumetricFlowRatePerAreaQuantity.Instance.GetUnitChoice(VolumetricFlowRatePerAreaQuantity.UnitChoicesEnum.GallonUKPerMinuteSquareFoot));
            SetChoice(VolumetricHeatTransferCoefficientQuantity.Instance, VolumetricHeatTransferCoefficientQuantity.Instance.GetUnitChoice(VolumetricHeatTransferCoefficientQuantity.UnitChoicesEnum.WattPerCubicMetreKelvin));
            SetChoice(VolumetricThermalExpansionCoefficientQuantity.Instance, VolumetricThermalExpansionCoefficientQuantity.Instance.GetUnitChoice(VolumetricThermalExpansionCoefficientQuantity.UnitChoicesEnum.ReciprocalDegreeFahrenheit));
        }



        protected void CheckMissing(List<BasePhysicalQuantity> quantities)
        {
            foreach (BasePhysicalQuantity quantity in quantities)
            {
                if (!Choices.ContainsKey(quantity.ID.ToString()))
                {
                    throw new Exception("missing default unit choice for this physical quantity");
                }
            }
        }
    }
}

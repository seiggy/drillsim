using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.DotnetLibraries.General.Statistics;
using OSDC.DotnetLibraries.General.DataManagement;
using System;
using System.Collections.Generic;
using NORCE.Drilling.DrillingFluid.ParticleSwarmOptimization;

namespace NORCE.Drilling.DrillingFluid.Model
{
    public class DrillingFluidOrder
    {
        /// <summary>
        /// a MetaInfo for the DrillingFluid
        /// </summary>
        public MetaInfo? MetaInfo { get; set; }

        /// <summary>
        /// name of the data
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// a description of the data
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// the date when the data was created
        /// </summary>
        public DateTimeOffset? CreationDate { get; set; }

        /// <summary>
        /// the date when the data was last modified
        /// </summary>
        public DateTimeOffset? LastModificationDate { get; set; }
        /// <summary>
        /// the ID of the fluid description used
        /// </summary>
        public Guid DrillingFluidDescriptionID { get; set; }
        
        public DrillingFluidDescription? DrillingFluidDescription { get; set; }
        /// <summary>
        /// the Temperature of the completed fluid
        /// </summary>
        public double Temperature { get; set; } = 50;
        
        public PTModelExtrapolation PTModelExtrapolation { get; set; }
        public RheologicalModelsEnum RheologicalModelsEnum { get; set; }
        public ParticleSwarmSolverOptions? Options { get; set; } = new ParticleSwarmSolverOptions
        {
            NumberOfIterations = 100_000,
            NumberOfParticles = 100
        };
        
        public CompletedDrillingFluid? CompletedDrillingFluid { get; set; }
        public double? OutputParam { get; set; }

        /// <summary>
        /// default constructor required for JSON serialization
        /// </summary>
        public DrillingFluidOrder() : base()
        {
        }
        #region Private variables
        /// <summary>
        /// default constructor required for JSON serialization
        /// </summary>
        private double totalMass;
        private double totalVolume;
        private double? totalDensity;
        private FlowCurve postProcessedFlowCurve;
        
        private SpecificHeatCapacity specificHeatCapacity = new SpecificHeatCapacity
        {
            Slope = new GaussianDrillingProperty
            {
                GaussianValue = new GaussianDistribution
                {
                    Mean = null,
                }
            },
            HeatCapacityAtZeroKelvin = new GaussianDrillingProperty
            {
                GaussianValue = new GaussianDistribution
                {
                    Mean = null,
                }
            },
            TemperatureRange = new double?[] { null, null },
            CoefficientRange = new double?[] { null, null },            
        };
        private ThermalConductivity thermalConductivity = new ThermalConductivity
        {
            Slope = new GaussianDrillingProperty
            {
                GaussianValue = new GaussianDistribution
                {
                    Mean = null,
                }
            },
            ThermalConductivityAtZeroKelvin = new GaussianDrillingProperty
            {
                GaussianValue = new GaussianDistribution
                {
                    Mean = null,
                }
            },
            TemperatureRange = new double?[] { null, null },
            CoefficientRange = new double?[] { null, null },                    
        };

        #endregion
        private double GaussianToDouble(GaussianDrillingProperty val)
        {
            if (val.Mean == null)
                return 0.0;
            else
                return (double)val.Mean;
        }
        /// <summary>
        /// main calculation method of the DrillingFluid
        /// </summary>
        /// <returns></returns>
        #region Rheological model calibration
        private List<GenericRheologicalModel> CalibrateRheologicalModel()
        {
            List<GenericRheologicalModel> genericRheologicalModelList = new();
            // Lop 
            if (this.Options == null)
            {
                this.Options =  new ParticleSwarmSolverOptions
                {
                    NumberOfIterations = 100_000,
                    NumberOfParticles = 100
                };
            }//Close if Options is null
             // Create the conversions for the current rheometer
            postProcessedFlowCurve = DrillingFluidDescription!.FlowCurve;
            RheometerConversions rheometerConversions = new RheometerConversions(DrillingFluidDescription.FlowCurve.CouetteRheometer); 
            foreach (FlowCurveTable flowCurveTable in postProcessedFlowCurve.TableValues)
            {
                rheometerConversions.FillTableFromInputs(flowCurveTable);
                //Each table can be used for a model calibration
                double tempTemperature = 0.0;
                if (flowCurveTable.Temperature.GaussianValue.Mean == null)
                {
                    tempTemperature = 0.0;
                }
                else
                {
                    tempTemperature = (double)flowCurveTable.Temperature.GaussianValue.Mean;
                }//close: if temperature null
                double tempPressure = 0.0;
                if (flowCurveTable.Pressure.GaussianValue.Mean == null)
                {
                    tempPressure = 0.0;
                }
                else
                {
                    tempPressure = (double)flowCurveTable.Pressure.GaussianValue.Mean;
                }//close: if pressure null
                List<ModelShared.RheometerMeasurement> rheometerMeasurements = flowCurveTable.RheometerMeasurements;
                ModelOptimizationOrder optimizationOrder = new ModelOptimizationOrder(this.Options, rheometerMeasurements, this.RheologicalModelsEnum);
                switch (this.RheologicalModelsEnum)
                {
                    case RheologicalModelsEnum.PowerLaw:
                        {
                            genericRheologicalModelList.Add(
                            new GenericRheologicalModel
                            {
                                Temperature = tempTemperature,
                                Pressure = tempPressure,
                                RheologicalModelUsed = RheologicalModelsEnum,
                                PowerLawViscosityInfty = optimizationOrder.RheologicalModelToFit!.ViscosityInfty,
                                PowerLawConsistencyIndex = optimizationOrder.RheologicalModelToFit!.ConsistencyIndex,
                            }
                            );
                            break;
                        }//Close case PowerLaw
                    case RheologicalModelsEnum.Bingham:
                        {
                            genericRheologicalModelList.Add(
                            new GenericRheologicalModel
                            {
                                Temperature = tempTemperature,
                                Pressure = tempPressure,
                                RheologicalModelUsed = RheologicalModelsEnum,
                                BinghamViscosityInfty = optimizationOrder.RheologicalModelToFit!.ViscosityInfty,
                                BinghamYieldStress = optimizationOrder.RheologicalModelToFit!.YieldStress,
                            }
                            );
                            break;
                        }//Close case Bingham
                    case RheologicalModelsEnum.HerschelBulkley:
                        {
                            genericRheologicalModelList.Add(
                            new GenericRheologicalModel
                            {
                                Temperature = tempTemperature,
                                Pressure = tempPressure,
                                RheologicalModelUsed = RheologicalModelsEnum,
                                HerschelBulkleyViscosityInfty = optimizationOrder.RheologicalModelToFit!.ViscosityInfty,
                                HerschelBulkleyYieldStress = optimizationOrder.RheologicalModelToFit!.YieldStress,
                                HerschelBulkleyConsistencyIndex = optimizationOrder.RheologicalModelToFit!.ConsistencyIndex
                            }
                            );
                            break;
                        }//Close case HerschelBulkley
                    case RheologicalModelsEnum.Quemada:
                        {
                            genericRheologicalModelList.Add(
                            new GenericRheologicalModel
                            {
                                Temperature = tempTemperature,
                                Pressure = tempPressure,
                                RheologicalModelUsed = RheologicalModelsEnum,
                                QuemadaViscosityInfty = optimizationOrder.RheologicalModelToFit!.ViscosityInfty,
                                QuemadaConsistencyIndex = optimizationOrder.RheologicalModelToFit!.ConsistencyIndex,
                                QuemadaCharacteristicShearRate = optimizationOrder.RheologicalModelToFit!.CharacteristicShearRate,
                                QuemadaXi = optimizationOrder.RheologicalModelToFit!.Xi
                            }
                            );
                            break;
                        }//Close case Quemada
                    case RheologicalModelsEnum.RobertsonStiff:
                        {
                            genericRheologicalModelList.Add(
                            new GenericRheologicalModel
                            {
                                Temperature = tempTemperature,
                                Pressure = tempPressure,
                                RheologicalModelUsed = RheologicalModelsEnum,
                                RobertsonStiffYieldStress = optimizationOrder.RheologicalModelToFit!.YieldStress,
                                RobertsonStiffCharacteristicShearRate = optimizationOrder.RheologicalModelToFit!.CharacteristicShearRate,
                                RobertsonStiffConsistencyIndex = optimizationOrder.RheologicalModelToFit!.ConsistencyIndex
                            }
                            );
                            break;
                        }//Close case RobertsonStiff
                    case RheologicalModelsEnum.Unkown:
                        {
                            break;
                        }//Close case Unkown                
                }//Close switch rheological model
            }//Close foreach flowtable loop
            return genericRheologicalModelList;
        }
        #endregion
        #region Thermophysical & physical properties
        private double[] LinearThermalConductivity(double temp1, double temp2, ThermalConductivity thermalConductivity_)
        {
            if (thermalConductivity_.Slope.GaussianValue.Mean != null &&
                thermalConductivity_.ThermalConductivityAtZeroKelvin.GaussianValue.Mean != null
                )
            {
                double k1 = (double)(temp1 * thermalConductivity_.Slope.GaussianValue.Mean + thermalConductivity_.ThermalConductivityAtZeroKelvin.GaussianValue.Mean);
                double k2 = (double)(temp2 * thermalConductivity_.Slope.GaussianValue.Mean + thermalConductivity_.ThermalConductivityAtZeroKelvin.GaussianValue.Mean);
                return new double[2] { k1, k2 };
            }
            else
            { 
                return new double[2] { 0, 0 };
            }
        }
        private void CalculatePhysicalProperties()
        {

            if (DrillingFluidDescription == null)
            {
                return;
            }

            double numAv = 1.0;
            double baseOilDensity = 0.0;
            double baseOilMass = 0.0;
            double baseOilHeatCapacityAtZeroKelvin = 0.0;
            double baseOilHeatCapacitySlope = 0.0;
            double baseOilVolume = 0.0;

            double brineDensity = 0.0;
            double brineMass = 0.0;
            double brineHeatCapacityAtZeroKelvin = 0.0;
            double brineHeatCapacitySlope = 0.0;
            double brineVolume = 0.0;

            double lowGravitySolidDensity = 0.0;
            double lowGravitySolidMass = 0.0;
            double lowGravitySolidHeatCapacityAtZeroKelvin = 0.0;
            double lowGravitySolidHeatCapacitySlope = 0.0;
            double lowGravitySolidVolume = 0.0;

            double temperature1thermalConductivity = 0.0;
            double temperature2thermalConductivity = 0.0;
            double temperature1HeatCapacity = 0.0;
            double temperature2HeatCapacity = 0.0;

            double highGravitySolidDensity = 0.0;
            double highGravitySolidMass = 0.0;
            double highGravitySolidHeatCapacityAtZeroKelvin = 0.0;
            double highGravitySolidHeatCapacitySlope = 0.0;
            double highGravitySolidVolume = 0.0;


            //Convert variables from Gaussian to doubles by extracting mean value.
            if (DrillingFluidDescription.DrillingFluidComposition.BaseOilProperies.MassDensity.GaussianValue.Mean != null &&
                DrillingFluidDescription.DrillingFluidComposition.BaseOilProperies.MassFraction.GaussianValue.Mean != null)
            {
                baseOilDensity = GaussianToDouble(DrillingFluidDescription.DrillingFluidComposition.BaseOilProperies.MassDensity);
                baseOilMass = GaussianToDouble(DrillingFluidDescription.DrillingFluidComposition.BaseOilProperies.MassFraction);
                baseOilHeatCapacityAtZeroKelvin = GaussianToDouble(DrillingFluidDescription.DrillingFluidComposition.BaseOilProperies.SpecificHeatCapacity.HeatCapacityAtZeroKelvin);
                baseOilHeatCapacitySlope = GaussianToDouble(DrillingFluidDescription.DrillingFluidComposition.BaseOilProperies.SpecificHeatCapacity.Slope);
                baseOilVolume = baseOilMass / baseOilDensity;
                if (DrillingFluidDescription.DrillingFluidComposition.BaseOilProperies.SpecificHeatCapacity.TemperatureRange[0] != null &&
                    DrillingFluidDescription.DrillingFluidComposition.BaseOilProperies.SpecificHeatCapacity.TemperatureRange[1] != null)
                {
                    temperature1HeatCapacity += (double)DrillingFluidDescription.DrillingFluidComposition.BaseOilProperies.SpecificHeatCapacity.TemperatureRange[0]!;
                    temperature2HeatCapacity += (double)DrillingFluidDescription.DrillingFluidComposition.BaseOilProperies.SpecificHeatCapacity.TemperatureRange[1]!;
                }
                if (DrillingFluidDescription.DrillingFluidComposition.BaseOilProperies.ThermalConductivity.TemperatureRange[0] != null &&
                    DrillingFluidDescription.DrillingFluidComposition.BaseOilProperies.ThermalConductivity.TemperatureRange[1] != null)
                {
                    //Thermal conductity stored times volume
                    temperature1thermalConductivity += (double)DrillingFluidDescription.DrillingFluidComposition.BaseOilProperies.ThermalConductivity.TemperatureRange[0]!;
                    temperature2thermalConductivity += (double)DrillingFluidDescription.DrillingFluidComposition.BaseOilProperies.ThermalConductivity.TemperatureRange[1]!;
                }
            }
            //Brine properties
            if (DrillingFluidDescription.DrillingFluidComposition.BrineProperies.MassDensity.GaussianValue.Mean != null &&
                DrillingFluidDescription.DrillingFluidComposition.BrineProperies.MassFraction.GaussianValue.Mean != null)
            {
                numAv += 1.0;
                brineDensity = GaussianToDouble(DrillingFluidDescription.DrillingFluidComposition.BrineProperies.MassDensity);
                brineMass = GaussianToDouble(DrillingFluidDescription.DrillingFluidComposition.BrineProperies.MassFraction);
                brineHeatCapacityAtZeroKelvin = GaussianToDouble(DrillingFluidDescription.DrillingFluidComposition.BrineProperies.SpecificHeatCapacity.HeatCapacityAtZeroKelvin);
                brineHeatCapacitySlope = GaussianToDouble(DrillingFluidDescription.DrillingFluidComposition.BrineProperies.SpecificHeatCapacity.Slope);
                brineVolume = brineMass / brineDensity;
                if (
                     DrillingFluidDescription.DrillingFluidComposition.BrineProperies.SpecificHeatCapacity.TemperatureRange[0] != null &&
                     DrillingFluidDescription.DrillingFluidComposition.BrineProperies.SpecificHeatCapacity.TemperatureRange[1] != null
                    )
                {
                    temperature1HeatCapacity += (double)DrillingFluidDescription.DrillingFluidComposition.BrineProperies.SpecificHeatCapacity.TemperatureRange[0]!;
                    temperature2HeatCapacity += (double)DrillingFluidDescription.DrillingFluidComposition.BrineProperies.SpecificHeatCapacity.TemperatureRange[1]!;
                }
                if (
                    DrillingFluidDescription.DrillingFluidComposition.BrineProperies.ThermalConductivity.TemperatureRange[0] != null &&
                    DrillingFluidDescription.DrillingFluidComposition.BrineProperies.ThermalConductivity.TemperatureRange[1] != null
                    )
                {
                    //Thermal conductity stored times volume
                    temperature1thermalConductivity += (double)DrillingFluidDescription.DrillingFluidComposition.BrineProperies.ThermalConductivity.TemperatureRange[0]!;
                    temperature2thermalConductivity += (double)DrillingFluidDescription.DrillingFluidComposition.BrineProperies.ThermalConductivity.TemperatureRange[1]!;
                }
            }

            //Low gravity solid properties
            if (DrillingFluidDescription.DrillingFluidComposition.LowGravitySolid.MassDensity.GaussianValue.Mean != null &&
                DrillingFluidDescription.DrillingFluidComposition.LowGravitySolid.MassFraction.GaussianValue.Mean != null)
            {
                numAv += 1.0;
                lowGravitySolidDensity = GaussianToDouble(DrillingFluidDescription.DrillingFluidComposition.LowGravitySolid.MassDensity);
                lowGravitySolidMass = GaussianToDouble(DrillingFluidDescription.DrillingFluidComposition.LowGravitySolid.MassFraction);
                lowGravitySolidHeatCapacityAtZeroKelvin = GaussianToDouble(DrillingFluidDescription.DrillingFluidComposition.LowGravitySolid.SpecificHeatCapacity.HeatCapacityAtZeroKelvin);
                lowGravitySolidHeatCapacitySlope = GaussianToDouble(DrillingFluidDescription.DrillingFluidComposition.LowGravitySolid.SpecificHeatCapacity.Slope);
                lowGravitySolidVolume = lowGravitySolidMass / lowGravitySolidDensity;
                if (
                    DrillingFluidDescription.DrillingFluidComposition.LowGravitySolid.SpecificHeatCapacity.TemperatureRange[0] != null &&
                    DrillingFluidDescription.DrillingFluidComposition.LowGravitySolid.SpecificHeatCapacity.TemperatureRange[1] != null
                    )
                {
                    temperature1HeatCapacity += (double)DrillingFluidDescription.DrillingFluidComposition.LowGravitySolid.SpecificHeatCapacity.TemperatureRange[0]!;
                    temperature2HeatCapacity += (double)DrillingFluidDescription.DrillingFluidComposition.LowGravitySolid.SpecificHeatCapacity.TemperatureRange[1]!;
                }
                if (DrillingFluidDescription.DrillingFluidComposition.LowGravitySolid.ThermalConductivity.TemperatureRange[0] != null &&
                     DrillingFluidDescription.DrillingFluidComposition.LowGravitySolid.ThermalConductivity.TemperatureRange[1] != null)
                {
                    temperature1thermalConductivity += (double)DrillingFluidDescription.DrillingFluidComposition.LowGravitySolid.ThermalConductivity.TemperatureRange[0]!;
                    temperature2thermalConductivity += (double)DrillingFluidDescription.DrillingFluidComposition.LowGravitySolid.ThermalConductivity.TemperatureRange[1]!;
                }
            }
            //High gravity solid properties
            if (DrillingFluidDescription.DrillingFluidComposition.HighGravitySolid.MassDensity.GaussianValue.Mean != null &&
                DrillingFluidDescription.DrillingFluidComposition.HighGravitySolid.MassFraction.GaussianValue.Mean != null)
            {
                numAv += 1.0;
                highGravitySolidDensity = GaussianToDouble(DrillingFluidDescription.DrillingFluidComposition.HighGravitySolid.MassDensity);
                highGravitySolidMass = GaussianToDouble(DrillingFluidDescription.DrillingFluidComposition.HighGravitySolid.MassFraction);
                highGravitySolidHeatCapacityAtZeroKelvin = GaussianToDouble(DrillingFluidDescription.DrillingFluidComposition.HighGravitySolid.SpecificHeatCapacity.HeatCapacityAtZeroKelvin);
                highGravitySolidHeatCapacitySlope = GaussianToDouble(DrillingFluidDescription.DrillingFluidComposition.HighGravitySolid.SpecificHeatCapacity.Slope);
                highGravitySolidVolume = highGravitySolidMass / highGravitySolidDensity;
                if (
                    DrillingFluidDescription.DrillingFluidComposition.HighGravitySolid.SpecificHeatCapacity.TemperatureRange[0] != null &&
                    DrillingFluidDescription.DrillingFluidComposition.HighGravitySolid.SpecificHeatCapacity.TemperatureRange[1] != null
                )
                {
                    temperature1HeatCapacity += (double)DrillingFluidDescription.DrillingFluidComposition.HighGravitySolid.SpecificHeatCapacity.TemperatureRange[0]!;
                    temperature2HeatCapacity += (double)DrillingFluidDescription.DrillingFluidComposition.HighGravitySolid.SpecificHeatCapacity.TemperatureRange[1]!;
                }
                if (DrillingFluidDescription.DrillingFluidComposition.HighGravitySolid.ThermalConductivity.TemperatureRange[0] != null &&
                    DrillingFluidDescription.DrillingFluidComposition.HighGravitySolid.ThermalConductivity.TemperatureRange[1] != null)
                {
                    temperature1thermalConductivity += (double)DrillingFluidDescription.DrillingFluidComposition.HighGravitySolid.ThermalConductivity.TemperatureRange[0]!;
                    temperature2thermalConductivity += (double)DrillingFluidDescription.DrillingFluidComposition.HighGravitySolid.ThermalConductivity.TemperatureRange[1]!;
                }
            }
            totalVolume = baseOilVolume + brineVolume + lowGravitySolidVolume + highGravitySolidVolume;
            totalMass = baseOilMass + brineMass + lowGravitySolidMass + highGravitySolidMass;

            double heatCapacityAtZeroKelvin = baseOilHeatCapacityAtZeroKelvin * baseOilMass + brineHeatCapacityAtZeroKelvin * brineMass
                + lowGravitySolidHeatCapacityAtZeroKelvin * lowGravitySolidMass + highGravitySolidHeatCapacityAtZeroKelvin * highGravitySolidMass;
            double heatCapacityLinearSlope = baseOilHeatCapacitySlope * baseOilMass + brineHeatCapacitySlope * brineMass
                + lowGravitySolidHeatCapacitySlope * lowGravitySolidMass + highGravitySolidHeatCapacitySlope * highGravitySolidMass;
            //Sum the volumes to calculate the volume fraction
            double volfracSolid = lowGravitySolidVolume + highGravitySolidVolume;
            double volfracLiquid = brineVolume + baseOilVolume;
            //Lost circulation material properties
            foreach (LostCirculationMaterial lostCirculationMaterial in DrillingFluidDescription.DrillingFluidComposition.LostCirculationMaterials)
            {
                if (lostCirculationMaterial.MassDensity.GaussianValue.Mean != null && lostCirculationMaterial.MassFraction.GaussianValue.Mean != null)
                {
                    numAv += 1.0;
                    double lostCirculationMaterialDensity = GaussianToDouble(lostCirculationMaterial.MassDensity);
                    double lostCirculationMaterialMass = GaussianToDouble(lostCirculationMaterial.MassFraction);
                    double lostCirculationMaterialVolume = lostCirculationMaterialMass / lostCirculationMaterialDensity;
                    double lostCirculationMaterialHeatCapacityAtZeroKelvin = GaussianToDouble(lostCirculationMaterial.SpecificHeatCapacity.HeatCapacityAtZeroKelvin);
                    double lostCirculationMaterialHeatCapacitySlope = GaussianToDouble(lostCirculationMaterial.SpecificHeatCapacity.Slope);
                    //Update total liquid volume fraction                    
                    volfracLiquid += lostCirculationMaterialVolume;
                    if (
                        lostCirculationMaterial.ThermalConductivity.TemperatureRange[0] != null &&
                        lostCirculationMaterial.ThermalConductivity.TemperatureRange[1] != null
                    )
                    {
                        temperature1thermalConductivity += (double)lostCirculationMaterial.ThermalConductivity.TemperatureRange[0]!;
                        temperature2thermalConductivity += (double)lostCirculationMaterial.ThermalConductivity.TemperatureRange[1]!;
                    }
                    if (
                        lostCirculationMaterial.SpecificHeatCapacity.TemperatureRange[0] != null &&
                        lostCirculationMaterial.SpecificHeatCapacity.TemperatureRange[1] != null
                    )
                    {
                        temperature1HeatCapacity += (double)lostCirculationMaterial.SpecificHeatCapacity.TemperatureRange[0]!;
                        temperature2HeatCapacity += (double)lostCirculationMaterial.SpecificHeatCapacity.TemperatureRange[1]!;
                    }
                    heatCapacityAtZeroKelvin += lostCirculationMaterialHeatCapacityAtZeroKelvin * lostCirculationMaterialMass;
                    heatCapacityLinearSlope += lostCirculationMaterialHeatCapacitySlope * lostCirculationMaterialMass;
                    //Update total mass and total volume
                    totalMass += lostCirculationMaterialMass;
                    totalVolume += lostCirculationMaterialVolume;
                }
            }//End foreach loop  
             //Check if the total volume > 0
            if (totalVolume > 0.0)
            {
                totalDensity = totalMass / totalVolume;
                heatCapacityAtZeroKelvin = heatCapacityAtZeroKelvin / totalMass;
                heatCapacityLinearSlope = heatCapacityLinearSlope / totalMass;
                //Calculate average temperatures
                temperature1HeatCapacity = temperature1HeatCapacity / numAv;
                temperature2HeatCapacity = temperature2HeatCapacity / numAv;
                temperature1thermalConductivity = temperature1thermalConductivity / numAv;
                temperature2thermalConductivity = temperature2thermalConductivity / numAv;
                double coef1 = heatCapacityLinearSlope * temperature1HeatCapacity + heatCapacityAtZeroKelvin;
                double coef2 = heatCapacityLinearSlope * temperature2HeatCapacity + heatCapacityAtZeroKelvin;
                specificHeatCapacity.TemperatureRange = new double?[2] { temperature1HeatCapacity, temperature2HeatCapacity };
                specificHeatCapacity.CoefficientRange = new double?[2] { coef1, coef2 };
                specificHeatCapacity.HeatCapacityAtZeroKelvin.GaussianValue.Mean = heatCapacityAtZeroKelvin;
                specificHeatCapacity.Slope.GaussianValue.Mean = heatCapacityLinearSlope;
                //Calculate thermal conductivity in the average temperature & add the new conductivity times volume            
                double[] ktc = LinearThermalConductivity(temperature1thermalConductivity, temperature2thermalConductivity, DrillingFluidDescription.DrillingFluidComposition.BaseOilProperies.ThermalConductivity);
                double thermalConductivityLiquid1xVolume = baseOilVolume * ktc[0];
                double thermalConductivityLiquid2xVolume = baseOilVolume * ktc[1];
                ktc = LinearThermalConductivity(temperature1thermalConductivity, temperature2thermalConductivity, DrillingFluidDescription.DrillingFluidComposition.BrineProperies.ThermalConductivity);
                thermalConductivityLiquid1xVolume += brineVolume * ktc[0];
                thermalConductivityLiquid2xVolume += brineVolume * ktc[1];
                foreach (LostCirculationMaterial lostCirculationMaterial in DrillingFluidDescription.DrillingFluidComposition.LostCirculationMaterials)
                {
                    if (lostCirculationMaterial.MassDensity.GaussianValue.Mean != null && lostCirculationMaterial.MassFraction.GaussianValue.Mean != null)
                    {
                        double lostCirculationMaterialDensity = GaussianToDouble(lostCirculationMaterial.MassDensity);
                        double lostCirculationMaterialMass = GaussianToDouble(lostCirculationMaterial.MassFraction);
                        double lostCirculationMaterialVolume = lostCirculationMaterialMass / lostCirculationMaterialDensity;
                        ktc = LinearThermalConductivity(temperature1thermalConductivity, temperature2thermalConductivity, lostCirculationMaterial.ThermalConductivity);
                        thermalConductivityLiquid1xVolume += lostCirculationMaterialVolume * ktc[0];
                        thermalConductivityLiquid2xVolume += lostCirculationMaterialVolume * ktc[1];
                    }
                }
                ktc = LinearThermalConductivity(temperature1thermalConductivity, temperature2thermalConductivity, DrillingFluidDescription.DrillingFluidComposition.LowGravitySolid.ThermalConductivity);
                double thermalConductivitySolid1xVolume = lowGravitySolidVolume * ktc[0];
                double thermalConductivitySolid2xVolume = lowGravitySolidVolume * ktc[1];
                ktc = LinearThermalConductivity(temperature1thermalConductivity, temperature2thermalConductivity, DrillingFluidDescription.DrillingFluidComposition.HighGravitySolid.ThermalConductivity);
                thermalConductivitySolid1xVolume += highGravitySolidVolume * ktc[0];
                thermalConductivitySolid2xVolume += highGravitySolidVolume * ktc[1];
                //Calculate volume fraction
                volfracSolid = volfracSolid / totalVolume;
                //Average solid and liquid properties by volume
                double thermalConductivityLiquid1 = thermalConductivityLiquid1xVolume / totalVolume;
                double thermalConductivityLiquid2 = thermalConductivityLiquid2xVolume / totalVolume;
                double thermalConductivitySolid1 = thermalConductivitySolid1xVolume / totalVolume;
                double thermalConductivitySolid2 = thermalConductivitySolid2xVolume / totalVolume;
                //Calculate ratios for the thermal conductivity at both available temperatures in the provided interval
                double kdDivkc1 = thermalConductivitySolid1 / thermalConductivityLiquid1;
                double kdDivkc2 = thermalConductivitySolid2 / thermalConductivityLiquid2;
                //Linearize again the equation within the given interval         
                double k1 = (kdDivkc1 + 2.0 - 2 * volfracSolid * (1.0 - kdDivkc1)) / (kdDivkc1 + 2.0 + 2 * volfracSolid * (1.0 - kdDivkc1));
                double k2 = (kdDivkc2 + 2.0 - 2 * volfracSolid * (1.0 - kdDivkc2)) / (kdDivkc2 + 2.0 + 2 * volfracSolid * (1.0 - kdDivkc2));
                double slope = (k2 - k1) / (temperature2thermalConductivity - temperature1thermalConductivity);
                double kt0 = k1 - slope * temperature1thermalConductivity;
                thermalConductivity.CoefficientRange = new double?[2] { k1, k2 };
                thermalConductivity.TemperatureRange = new double?[2] { temperature1thermalConductivity, temperature2thermalConductivity };
                thermalConductivity.Slope.GaussianValue.Mean = slope;
                thermalConductivity.ThermalConductivityAtZeroKelvin.GaussianValue.Mean = kt0;
            }//End if volume > 0     
            else
            {
                totalDensity = null;
            }
        }
        #endregion 
        public bool Calculate()
        {

            bool success;
            // If there is no fluid description, the calculation will fail.
            if (DrillingFluidDescription == null) { return false; }
            //Populate mass & volume variables         
            CalculatePhysicalProperties();
            //Create a list of calibrated rheological parameters
            List<GenericRheologicalModel> genericRheologicalModelList = CalibrateRheologicalModel();
            #region Completed fluid assembly
            this.CompletedDrillingFluid = new CompletedDrillingFluid
            {
                MassDensity = new GaussianDrillingProperty
                {
                    GaussianValue = new GaussianDistribution { Mean = totalDensity }
                },
                GellingProperties = DrillingFluidDescription.GellingProperties,
                SpecificHeatCapacity = specificHeatCapacity,
                PostProcessedFlowCurve = postProcessedFlowCurve,
                RheologicalModelProperties = genericRheologicalModelList,
                ThermalConductivity = thermalConductivity
            };  
            #endregion
            DrillingFluidDescription = null;
            if (CompletedDrillingFluid == null) { success = false; }
            else { success = true; }
            return success;
        }//Close Calculate method
    }
}

using System;
using System.Collections.Generic;
using NORCE.Drilling.DrillingFluid.ModelShared;
namespace NORCE.Drilling.DrillingFluid.Model
{
    public class RheometerConversions
    {
        public double RheometerR1;
        public double RheometerR2;
        public double Ksi;
        public double VolumeTimesCorrection;
        public RheometerConversions() { }
        public RheometerConversions(CouetteRheometer rheometer)
        {
            this.RheometerR1 = rheometer.BobRadius;
            this.RheometerR2 = rheometer.Gap + rheometer.BobRadius;
            this.Ksi = this.RheometerR2 / rheometer.BobRadius;
            this.VolumeTimesCorrection = Math.PI * rheometer.BobRadius * rheometer.BobRadius * rheometer.BobLength * rheometer.NewtonianEndEffectCorrection;
        }
        #region Shear rate/rotation conversions
        public void FillTableFromInputs(FlowCurveTable flowCurveTable)
        {
            int tableSize = flowCurveTable.RheometerMeasurements.Count;
            double ksi2 = Ksi * Ksi;
            double ksi2p1 = 1.0 + ksi2;
            switch (flowCurveTable.ShearRateReference)
            {
                case MeasurementTypesShearRate.Rotation:
                    {
                        //Update table with remaining values
                        for (int i = 0; i < tableSize; i++)
                        {
                            double omega = flowCurveTable.RheometerMeasurements[i].RotationalSpeed * 2.0 * Math.PI;
                            flowCurveTable.RheometerMeasurements[i].IsoNewtonianShearRate = ksi2p1 * omega / (ksi2 - 1.0);
                            flowCurveTable.RheometerMeasurements[i].BobNewtonianShearRate = 2.0 * ksi2 * omega / (ksi2 - 1.0);
                        }
                        break;
                    }
                case MeasurementTypesShearRate.IsoNewtonianShearRate:
                    {
                        //Update table with remaining values
                        for (int i = 0; i < tableSize; i++)
                        {
                            double omega = flowCurveTable.RheometerMeasurements[i].IsoNewtonianShearRate * (ksi2 - 1.0) / (1 + ksi2); ;
                            flowCurveTable.RheometerMeasurements[i].RotationalSpeed = omega / (2.0 * Math.PI);
                            flowCurveTable.RheometerMeasurements[i].BobNewtonianShearRate = 2.0 * ksi2 * omega / (ksi2 - 1.0);
                        }
                        break;
                    }
                case MeasurementTypesShearRate.BobNewtonianShearRate:
                    {
                        //Update table with remaining values
                        for (int i = 0; i < tableSize; i++)
                        {
                            double omega = flowCurveTable.RheometerMeasurements[i].BobNewtonianShearRate * (ksi2 - 1) / (2.0 * ksi2);
                            flowCurveTable.RheometerMeasurements[i].RotationalSpeed = omega / (2.0 * Math.PI);
                            flowCurveTable.RheometerMeasurements[i].IsoNewtonianShearRate = 2.0 * ksi2 * omega / (ksi2 - 1.0);
                        }
                        break;
                    }
            }//Close switch for shear rate
            switch (flowCurveTable.ShearStressReference)
            {
                case MeasurementTypesShearStress.Torque:
                    {
                        //Update table with remaining values
                        for (int i = 0; i < tableSize; i++)
                        {
                            double isoNewtonianShearStress = ksi2p1 * flowCurveTable.RheometerMeasurements[i].Torque / (2.0 * ksi2 * 2.0 * VolumeTimesCorrection);
                            flowCurveTable.RheometerMeasurements[i].IsoNewtonianShearStress = isoNewtonianShearStress;
                            flowCurveTable.RheometerMeasurements[i].BobNewtonianShearStress = 2.0 * ksi2 * isoNewtonianShearStress / ksi2p1;
                        }
                        break;
                    }
                case MeasurementTypesShearStress.IsoNewtonianShearStress:
                    {
                        //Update table with remaining values
                        for (int i = 0; i < tableSize; i++)
                        {
                            double isoNewtonianShearStress = flowCurveTable.RheometerMeasurements[i].IsoNewtonianShearStress;    
                            flowCurveTable.RheometerMeasurements[i].Torque = isoNewtonianShearStress * 2.0 * ksi2 * 2.0 * VolumeTimesCorrection / ksi2p1;
                            flowCurveTable.RheometerMeasurements[i].BobNewtonianShearStress = 2.0 * ksi2 * isoNewtonianShearStress / (1.0 + ksi2 );
                        }
                        break;
                    }
                case MeasurementTypesShearStress.BobNewtonianShearStress:
                    {
                        //Update table with remaining values
                        for (int i = 0; i < tableSize; i++)
                        {
                            double isoNewtonianShearStress = flowCurveTable.RheometerMeasurements[i].BobNewtonianShearStress *
                                ksi2p1 / (2.0 * ksi2);
                            flowCurveTable.RheometerMeasurements[i].IsoNewtonianShearStress = isoNewtonianShearStress;
                            flowCurveTable.RheometerMeasurements[i].Torque = isoNewtonianShearStress * 2.0 * ksi2 *
                                2.0 * VolumeTimesCorrection / ksi2p1;                       
                        }
                        break;
                    }
            }//Close switch for shear rate
        }//End FillTableFromInputs method     
        
        #endregion
        #region Shear-Stress/torque conversions
        #endregion

    }//Close class
}
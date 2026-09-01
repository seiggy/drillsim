using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.DotnetLibraries.General.DataManagement;
using System;
using System.Collections.Generic;
using NORCE.Drilling.DrillingFluid.ModelShared;

namespace NORCE.Drilling.DrillingFluid.Model
{
    /// <summary>
    /// a base class other classes may derive from
    /// </summary>
    public class FlowCurveTable
    {
        /// <summary>
        /// a Temperature data
        /// </summary>
        public GaussianDrillingProperty Temperature  { get; set; }
        /// <summary>
        /// a Pressure data
        /// </summary>
        public GaussianDrillingProperty Pressure  { get; set; }
        /// <summary>
        /// a Actual measurements data
        /// </summary>
        public List<RheometerMeasurement> RheometerMeasurements  { get; set; }
        /// <summary>
        /// a reference to which type of input is used data
        /// </summary>
        public MeasurementTypesShearRate ShearRateReference { get; set; }
        /// <summary>
        /// a reference to which type of shear-stress input is used
        /// </summary>
        public MeasurementTypesShearStress ShearStressReference { get; set; }
        /// <summary>
        /// default constructor required for JSON serialization
        /// </summary>
        public FlowCurveTable() : base()
        {
        }
    }
}

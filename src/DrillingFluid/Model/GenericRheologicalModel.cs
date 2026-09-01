namespace NORCE.Drilling.DrillingFluid.Model
{
    public class GenericRheologicalModel
    {
        /// <summary>
        /// Enum with available models for calibration
        /// </summary>
        public RheologicalModelsEnum RheologicalModelUsed { get; set; }
        /// <summary>
        /// Temperature of the measurements
        /// </summary>
        public double Temperature { get; set; }
        /// <summary>
        /// Pressure of the measurements
        /// </summary>
        public double Pressure { get; set; }
        
        /// <summary>
        /// Power law model viscosity coefficient
        /// </summary>
        public double? PowerLawViscosityInfty { get; set; }
        /// <summary>
        /// Power law model expoent (consistency index)
        /// </summary>
        public double? PowerLawConsistencyIndex { get; set; }
        /// <summary>
        /// Bingham plastic viscosity
        /// </summary>
        public double? BinghamViscosityInfty { get; set; }
        /// <summary>
        /// Bingham plastic yield stress
        /// </summary>
        public double? BinghamYieldStress { get; set; }
        /// <summary>
        /// Herschel-Bulkley viscosity coefficient
        /// </summary>
        public double? HerschelBulkleyViscosityInfty { get; set; }
        /// <summary>
        /// Herschel-Bulkley yield stress
        /// </summary>
        public double? HerschelBulkleyYieldStress { get; set; }
        /// <summary>
        /// Herschel-Bulkley expoent (consistency index)
        /// </summary>
        public double? HerschelBulkleyConsistencyIndex { get; set; }
        /// <summary>
        /// Quemada viscosity coefficient
        /// </summary>
        public double? QuemadaViscosityInfty { get; set; }
        /// <summary>
        /// Quemada characteristic shear-rate
        /// </summary>
        public double? QuemadaCharacteristicShearRate { get; set; }
        /// <summary>
        /// Quemada expoent
        /// </summary>
        public double? QuemadaConsistencyIndex { get; set; }
        /// <summary>
        /// Quemada Xi value
        /// </summary>
        public double? QuemadaXi { get; set; }
        /// <summary>
        /// Robertson-Stiff yield stress
        /// </summary>
        public double? RobertsonStiffYieldStress { get; set; }
        /// <summary>
        /// Robertson-Stiff Characteristic Shear-Rate
        /// </summary>
        public double? RobertsonStiffCharacteristicShearRate { get; set; }
        /// <summary>
        /// Robertson-Stiff Expoent
        /// </summary>
        public double? RobertsonStiffConsistencyIndex { get; set; }


    }//class
}//namespace
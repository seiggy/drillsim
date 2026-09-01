using System;
using System.Collections.Generic;
using NORCE.Drilling.DrillingFluid.ModelShared;

namespace RheologicalModelLib
{
    public class BinghamPlasticModel 
    {
        public double YieldStress;
        public double ViscosityInfty;
        public BinghamPlasticModel(GenericRheologicalModel rheoModel)
        {
            this.YieldStress = (double)rheoModel.BinghamYieldStress!;
            this.ViscosityInfty = (double)rheoModel.BinghamViscosityInfty!;
        }
        public double ShearStress(double gamma)
        {
            return this.YieldStress + this.ViscosityInfty * gamma;
        }           
    }//class
}//namespace

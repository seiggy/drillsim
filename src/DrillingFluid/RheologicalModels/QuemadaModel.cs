using System;
using System.Collections.Generic;
using NORCE.Drilling.DrillingFluid.ModelShared;
using NORCE.Drilling.DrillingFluid.ParticleSwarmOptimization;


namespace NORCE.Drilling.DrillingFluid.RheologicalModel
{
    public class QuemadaModel : RheologicalModel
    {
        public ModelConstraints Constraints = new ModelConstraints()
        {
            Dim = 4,
            minValues = new double[4] { 0.0, 0.0, -1.0, 0.0}, // {min ViscosityInfty, min CharacteristicShearRate, min ConsistencyIndex, min Xi}
            maxValues = new double[4] {10.0, 200.0, 1.0, 100.0 } // {max ViscosityInfty, max CharacteristicShearRate, max ConsistencyIndex, max Xi}
        };
        private readonly int DIM = 4;


        public double ShearStress(double gamma)
        {
            double admGammaN = Math.Pow(gamma / this.CharacteristicShearRate, this.ConsistencyIndex);
            return this.ViscosityInfty * Math.Pow((1.0 + admGammaN) / (this.Xi + admGammaN), 2);
        }

        public override double ObjectiveFunction(double[] par, DataPair[] xy)
        {
            //par[0] = ViscosityInfty
            //par[1] = CharacteristicShearRate
            //par[2] = ConsistencyIndex
            //par[3] = Xi  
            double residue = 0.0;
            for (int i = 0; i < xy.Length; i++)
            {
                double admGammaN = Math.Pow(xy[i].X / par[1], par[2]);
                double calcTau = par[0] * Math.Pow((1.0 + admGammaN) / (par[3] + admGammaN), 2);
                residue += Math.Pow(calcTau - xy[i].Y, 2);
            }
            return residue;
        }
        public QuemadaModel() { }
        public QuemadaModel(DataPair[] xy, ParticleSwarmSolverOptions options)
        {            
            double[] var = ParticleSwarmMethod.Main(options, this.Constraints, xy, ObjectiveFunction);
            this.ViscosityInfty = var[0];
            this.CharacteristicShearRate = var[1];
            this.ConsistencyIndex = var[2];
            this.Xi = var[3];
        }
        public QuemadaModel(List<RheometerMeasurement> rheoMeasurements)
        {
            int Nmeasurements = rheoMeasurements.Count;
            DataPair[] xy = new DataPair[Nmeasurements];
            for (int i = 0; i < Nmeasurements; i++)
            {
                xy[i] = new DataPair
                {
                    X = rheoMeasurements[i].IsoNewtonianShearRate,
                    Y = rheoMeasurements[i].IsoNewtonianShearStress
                };
            }
            double[] var = ParticleSwarmMethod.Main(DIM, xy, ObjectiveFunction);
            this.ViscosityInfty = var[0];
            this.CharacteristicShearRate = var[1];
            this.ConsistencyIndex = var[2];
            this.Xi = var[3];
        }

        //constructor
    }//class
}//namespace

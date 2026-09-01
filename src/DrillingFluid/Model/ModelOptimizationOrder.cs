using System;
using System.Collections.Generic;
using NORCE.Drilling.DrillingFluid.Model;
using NORCE.Drilling.DrillingFluid.RheologicalModel;
using NORCE.Drilling.DrillingFluid.ModelShared;
using NORCE.Drilling.DrillingFluid.ParticleSwarmOptimization;


namespace NORCE.Drilling.DrillingFluid.Model
{
    public class ModelOptimizationOrder
    {
        //Solver options
        public ParticleSwarmSolverOptions Options { get; set; } = new ParticleSwarmSolverOptions
        {
            NumberOfIterations = 100_000,
            NumberOfParticles = 100
        };
        public RheologicalModel.RheologicalModel? RheologicalModelToFit;
        private DataPair[] TargetDataSet;

        public ModelOptimizationOrder(
            ParticleSwarmSolverOptions options,
            List<RheometerMeasurement> rheometerMeasurements,
            RheologicalModelsEnum rheologicalModelsEnum
            )
        {
            this.Options = options;
            int Nmeasurements = rheometerMeasurements.Count;
            this.TargetDataSet = new DataPair[Nmeasurements];
            for (int i = 0; i < Nmeasurements; i++)
            {                
                TargetDataSet[i] = new DataPair
                {
                    X = rheometerMeasurements[i].IsoNewtonianShearRate,
                    Y = rheometerMeasurements[i].IsoNewtonianShearStress
                };
            }
            switch (rheologicalModelsEnum)
            {
                case RheologicalModelsEnum.PowerLaw:
                    {
                        this.RheologicalModelToFit = new PowerLawModel(this.TargetDataSet, this.Options);
                        Console.WriteLine("\n======= Power Law =======\n");
                        Console.WriteLine($"tau = {RheologicalModelToFit.ViscosityInfty} gamma^{RheologicalModelToFit.ConsistencyIndex}");
                        Console.WriteLine("ViscosityInfty: " + RheologicalModelToFit.ViscosityInfty);
                        Console.WriteLine("Consistency: " + RheologicalModelToFit.ConsistencyIndex);
                        break;
                    }
                case RheologicalModelsEnum.Bingham:
                    {
                        this.RheologicalModelToFit = new BinghamPlasticModel(this.TargetDataSet, this.Options);
                        Console.WriteLine("\n======= Bingham Plastic =======\n");
                        Console.WriteLine($"tau = {RheologicalModelToFit.YieldStress} + {RheologicalModelToFit.ViscosityInfty} gamma ");
                        Console.WriteLine("Yeild Stress: " + RheologicalModelToFit.YieldStress);
                        Console.WriteLine("ViscosityInfty: " + RheologicalModelToFit.ViscosityInfty);
                        break;
                    }
                case RheologicalModelsEnum.HerschelBulkley:
                    {
                        this.RheologicalModelToFit = new HerschelBulkleyModel(this.TargetDataSet, this.Options);
                        Console.WriteLine("\n======= Herschel-Bulkley =======\n");
                        Console.WriteLine($"tau = {RheologicalModelToFit.YieldStress} + {RheologicalModelToFit.ViscosityInfty} gamma ^ {RheologicalModelToFit.ConsistencyIndex} ");
                        Console.WriteLine("Yeild Stress: " + RheologicalModelToFit.YieldStress);
                        Console.WriteLine("ViscosityInfty: " + RheologicalModelToFit.ViscosityInfty);
                        Console.WriteLine("Consistency: " + RheologicalModelToFit.ConsistencyIndex);
                        break;
                    }
                case RheologicalModelsEnum.Quemada:
                    {
                        this.RheologicalModelToFit = new QuemadaModel(this.TargetDataSet, this.Options);
                        Console.WriteLine("\n======= Quemada =======\n");
                        Console.WriteLine($"tau(gamma) = {RheologicalModelToFit.ViscosityInfty}((1 + (gamma/{RheologicalModelToFit.CharacteristicShearRate})^({RheologicalModelToFit.ConsistencyIndex}))/({RheologicalModelToFit.Xi} + (gamma/{RheologicalModelToFit.CharacteristicShearRate})^({RheologicalModelToFit.ConsistencyIndex}))^2");
                        Console.WriteLine("Characteristic shear rate: " + RheologicalModelToFit.CharacteristicShearRate);
                        Console.WriteLine("ViscosityInfty: " + RheologicalModelToFit.ViscosityInfty);
                        Console.WriteLine("Consistency: " + RheologicalModelToFit.ConsistencyIndex);
                        Console.WriteLine("Xi: " + RheologicalModelToFit.Xi);
                        Console.WriteLine("\n\n\n");

                        Console.WriteLine("gamma_c = " + RheologicalModelToFit.CharacteristicShearRate);
                        Console.WriteLine("vinfty = " + RheologicalModelToFit.ViscosityInfty);
                        Console.WriteLine("n = " + RheologicalModelToFit.ConsistencyIndex);
                        Console.WriteLine("xi =  " + RheologicalModelToFit.Xi);
                        Console.WriteLine("tau(gamma) = vinfty * ( (1 + (gamma/gamma_c)^n ) / (xi + (gamma/gamma_c)^n) )^2");
                        
                        break;
                    }
                case RheologicalModelsEnum.RobertsonStiff:
                    {
                        this.RheologicalModelToFit = new RobertsonStiffModel(this.TargetDataSet, this.Options);
                        Console.WriteLine("\n======= Robertson-Stiff =======\n");
                        Console.WriteLine($"tau = {RheologicalModelToFit.YieldStress}(1 + gamma/{RheologicalModelToFit.CharacteristicShearRate})^ {RheologicalModelToFit.ConsistencyIndex}");
                        Console.WriteLine("Characteristic shear rate: " + RheologicalModelToFit.CharacteristicShearRate);
                        Console.WriteLine("ViscosityInfty: " + RheologicalModelToFit.YieldStress);
                        Console.WriteLine("Consistency: " + RheologicalModelToFit.ConsistencyIndex);
                        break;
                    }
                case RheologicalModelsEnum.Unkown:
                    {
                        this.RheologicalModelToFit = null;
                        break;
                    }
            }//Switch
        }//Constructor
    }//Class
}//Namespace
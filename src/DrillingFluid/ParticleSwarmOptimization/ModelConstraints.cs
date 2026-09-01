namespace NORCE.Drilling.DrillingFluid.ParticleSwarmOptimization
{
    public class ModelConstraints
    {
        public int Dim;
        public double[] minValues;
        public double[] maxValues;
        public ModelConstraints()
        {
            this.minValues = new double[1];
            this.maxValues = new double[1];
        }
        public ModelConstraints(int dim)
        {
            this.Dim = dim;
            this.minValues = new double[dim];
            this.maxValues = new double[dim];
        }//Constructor
    }//Class
}//Namespace
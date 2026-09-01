namespace NORCE.Drilling.GeothermalProperties.Interpolation
{
    public class LinearCurve
    {
        public double dYdX {get; set;}
        public double B {get; set;}
        public double[] pt0 {get; set;}
        public double[] pt1 {get; set;}

        public bool[] IsIntersection {get; set;}        

        public double f(double x)
        {
            return dYdX * x + B;
        }

        public LinearCurve(double x0, double y0, double? x1, double? y1, double? dydx1)
        {
            IsIntersection = new bool[2] {false, false}; 
            pt0 = new double[2] {x0, y0};            
            if (x1 != null && dydx1 != null)
            {
                dYdX = (double) dydx1;
                B = y0 - x0 * dYdX;
                y1 = x1 * dYdX + B;
                pt1 = new double[2] {(double) x1, (double) y1};     
            }            
            else if (x1 != null && y1 != null)
            {
                dYdX = ((double) y1 - y0) / ((double) x1 - x0);
                B = y0 - x0 * dYdX;                
                pt1 = new double[2] {(double) x1, (double) y1};
            }
            else if (y1 != null && dydx1 != null)
            {
                x1 = (double) (y1 - y0)/dydx1 + x0;
                dYdX =  ((double) y1 - y0)/((double) x1 - x0);
                B = y0 - x0 * dYdX;            
                pt1 = new double[2] {(double) x1, (double) y1};
            }           
            else
            {
                //Return error
                pt1 = new double[2] {-1, -1};
            }
        }
        public LinearCurve(LinearCurve line, double? x1, double? y1, double? dydx1)
        {
            IsIntersection = new bool[2] {false, false};             
            //Initial point of this line is the last of the previous line
            pt0 = line.pt1;
            if (x1 != null && dydx1 != null && y1 == null)
            {
                dYdX = (double) dydx1;
                B = line.pt1[1] - line.pt1[0] * dYdX;
                y1 = x1 * dYdX + B;
                pt1 = new double[2] {(double) x1, (double) y1};                 
            }            
            else if (x1 != null && y1 != null && dydx1 == null)
            {
                dYdX = ((double) y1 - line.pt1[1]) / ((double) x1 - line.pt1[0]);
                B = line.pt1[1] - line.pt1[0] * dYdX;    
                pt1 = new double[2] {(double) x1, (double) y1};                 
            }
            else if (y1 != null && dydx1 != null && x1 == null)
            {
                x1 = (double) (y1 - line.pt1[1])/dydx1 + line.pt1[0];
                dYdX =  ((double) y1 - line.pt1[1])/((double) x1 - line.pt1[0]);
                B = line.pt1[1] - line.pt1[0] * dYdX;
                pt1 = new double[2] {(double) x1, (double) y1};            
            }
            else if (y1 != null && dydx1 != null && x1 != null)
            {
                line.IsIntersection[1] = true;
                this.IsIntersection[0] = true;
                //If all points are known, then modifies to the intersection
                dYdX = (double) dydx1;
                B = (double) y1 - (double) x1 * (double) dydx1;
                double xi = (line.B - B)/(dYdX - line.dYdX);
                double yi = dYdX * xi + B;
                //Change initial point to the intersection
                pt0 = new double[2] {xi, yi};
                // Change the previous line 
                line.pt1 = new double[2] {xi, yi};
                pt1 = new double[2] {(double) x1, (double) y1};            
            }
            
            else
            {
                //Return error
                pt1 = new double[2] {-1, -1};
            }
        }

    }


}
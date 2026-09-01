using NORCE.Drilling.DrillingFluid.ModelShared;

namespace NORCE.Drilling.DrillingFluid.WebPages.Shared;

public class ConversionsFromOSDC
{
    public static double? GaussianToDouble(GaussianDrillingProperty? val)
    {
        if (val?.GaussianValue?.Mean is double value)
        {
            return value;
        }

        return null;
    }

    public static GaussianDrillingProperty DoubleToGaussian(double? val)
    {
        return new GaussianDrillingProperty()
        {
            GaussianValue = new GaussianDistribution() { Mean = val }
        };
    }
}

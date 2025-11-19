namespace Celer.Utilities
{
    public class ValueHelpers
    {
        public static double scaleToGraph(double value, double max)
        {
            return (2 * (value / max) - 1) * 100;
        }

    }
}

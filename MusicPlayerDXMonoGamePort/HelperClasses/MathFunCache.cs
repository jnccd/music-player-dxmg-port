using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayerDXMonoGamePort
{
    public class MathFunCache
    {
        double[] values;
        double resolution, lowerBound, upperBound;
        Func<double, double> function;

        public MathFunCache(Func<double, double> function, double resolution = 0.01, double lowerBound = 0, double upperBound = 1)
        {
            this.function = function;
            this.resolution = resolution;
            this.lowerBound = lowerBound;
            this.upperBound = upperBound;

            var boundSize = upperBound - lowerBound;
            var arrLength = boundSize / resolution;

            values = new double[(int)arrLength];
            for (int i = 0; i < values.Length; i++)
                values[i] = double.NaN;
        }

        int MapToArraySpace(double x)
        {
            if (x < lowerBound || x > upperBound)
                throw new ArgumentOutOfRangeException(x.ToString());
            else
                return (int)((x - lowerBound) / resolution);
        }

        public double Get(double x)
        {
            int index = MapToArraySpace(x);

            if (values[index] == double.NaN)
            {
                values[index] = function(x);
                return values[index];
            }
            else
                return values[index];
        }
    }
}

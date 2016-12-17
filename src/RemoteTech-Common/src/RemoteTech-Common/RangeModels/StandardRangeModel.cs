using System;
using CommNet;

namespace RemoteTech.Common.RangeModels
{
    public class StandardRangeModel : IRangeModel
    {
        /// <summary>
        ///     Returns the maximum range between the two antenna powers.
        /// </summary>
        /// <param name="aPower">Antenna a power.</param>
        /// <param name="bPower">Antenna b power.</param>
        /// <returns></returns>
        public double GetMaximumRange(double aPower, double bPower)
        {
            return Math.Min(aPower, bPower);
        }

        /// <summary>
        ///     Given two antenna powers, return the 1 - distance / (their range).
        /// </summary>
        /// <param name="aPower">Antenna a power.</param>
        /// <param name="bPower">Antenna b power.</param>
        /// <param name="distance">Distance between the two antennas.</param>
        /// <returns></returns>
        public double GetNormalizedRange(double aPower, double bPower, double distance)
        {
            return 1.0 - distance / GetMaximumRange(aPower, bPower);
        }

        /// <summary>
        ///     Return true if (and only if) the connection a&lt;-&gt;b is in range, given the square of the distance between them
        ///     (<paramref name="sqrDistance" />).
        /// </summary>
        /// <param name="aPower">Antenna a power.</param>
        /// <param name="bPower">Antenna b power.</param>
        /// <param name="sqrDistance">The squared distance between the two antennas.</param>
        /// <returns></returns>
        public bool InRange(double aPower, double bPower, double sqrDistance)
        {
            var distance = Math.Sqrt(sqrDistance);
            var maxRange = GetMaximumRange(aPower, bPower);
            return (distance <= maxRange) && (distance >= 0.0d) && (maxRange > 0.0d);
        }
    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codealytics
{
    public class RuntimePerformanceInfromation : IRuntimePerformanceInfromation
    {
        /// <summary>
        /// Returns the avg ellepsed milliseconds after n runs
        /// </summary>
        public double AvgEllepsedMilliseconds
        {
            get
            {
                return EllepsedMilliseconsList.Sum() / (double)EllepsedMilliseconsList.Count;
            }
        }

        /// <summary>
        /// Returns the total amount of ellepsed milliseconds ellepsed.
        /// </summary>
        public int TotalEllepsedMilliseconds { get { return EllepsedMilliseconsList.Sum(); } }


        /// <summary>
        /// The variance of the list
        /// </summary>
        public double Variance
        {
            get
            {
                double avg = AvgEllepsedMilliseconds;
                double variance = 0;
                foreach (var e in EllepsedMilliseconsList)
                {
                    variance += Math.Pow(e - avg, 2);
                }
                variance /= EllepsedMilliseconsList.Count();
                return Variance;
            }
        }

        /// <summary>
        /// The standard deviation of the list
        /// </summary>
        public double StandardDeviation
        {
            get
            {
                return Math.Sqrt(Variance);
            }
        }

        /// <summary>
        /// The list contianing individual results.
        /// </summary>
        public ConcurrentBag<int> EllepsedMilliseconsList { get; private set; } = new ConcurrentBag<int>();

        /// <summary>
        /// Adds a result to the list of results
        /// </summary>
        /// <param name="ellepsedMilliseconds">The ellepsed milliseconds for the execution</param>
        public void AddResult(int ellepsedMilliseconds)
        {
            EllepsedMilliseconsList.Add(ellepsedMilliseconds);
        }

        /// <summary>
        /// Converts the information to a string.
        /// </summary>
        /// <returns>Returns a string containing the informations of the object</returns>
        public override string ToString()
        {
            string output = "";

            output += $"AvgRuntime: {AvgEllepsedMilliseconds}";
            output += $"; TotalRuntime: {TotalEllepsedMilliseconds}";

            return output;
        }
    }
}

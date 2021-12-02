using System;
using System.Collections.Generic;
using Xunit;

namespace Codealytics.Test
{
    public class AnalyticsPerformanceTest
    {
        Analytics analytics;

        [Fact]
        void TestRuntimePerformanceInformationExecution()
        {
            bool executed = false;
            analytics = new Analytics();
            analytics.CodeRuntimePerformance(() => { executed = true; });
            Assert.True(executed);
        }

        [Fact]
        void TestRuntimePerformanceInformationAccuracy()
        {
            analytics = new Analytics();
            string id = analytics.CodeRuntimePerformance(() => { System.Threading.Thread.Sleep(200); });
            Assert.InRange(analytics.GetMetric<RuntimePerformanceInfromation>(id).AvgEllepsedMilliseconds, 200, 220);

            for (int i = 0; i < 10; i++)
            {
                analytics.CodeRuntimePerformance(id, () => { System.Threading.Thread.Sleep(100); });
            }

            Assert.InRange(analytics.GetMetric<RuntimePerformanceInfromation>(id).AvgEllepsedMilliseconds, 101, 121);
        }

        [Fact]
        void TestRuntimePerformanceInformationVarianceAndStandardDeviation()
        {
            analytics = new Analytics();
            analytics.AddMetric<IRuntimePerformanceInfromation>("rpi", new RuntimePerformanceInfromation());
            var liTimes = new List<int>() { 100, 200, 300, 150, 250 };
            foreach (var mst in liTimes)
            {
                analytics.CodeRuntimePerformance("rpi", () =>
                {
                    System.Threading.Thread.Sleep(mst-15);
                });
            }
            RuntimePerformanceInfromation rpi =  (RuntimePerformanceInfromation)analytics.GetMetric<IRuntimePerformanceInfromation>("rpi");
            Assert.InRange(rpi.Variance, 4500, 5500);
            Assert.InRange(rpi.StandardDeviation, 65, 75);
        }
    }
}
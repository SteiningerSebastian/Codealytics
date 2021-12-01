using System;
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
            Assert.InRange(analytics.GetMetric<RuntimePerformanceInfromation>(id).AvgEllepsedMilliseconds, 200,220);

            for(int i = 0; i < 10; i++)
            {
                analytics.CodeRuntimePerformance(id, () => { System.Threading.Thread.Sleep(100); });
            }

            Assert.InRange(analytics.GetMetric<RuntimePerformanceInfromation>(id).AvgEllepsedMilliseconds, 101, 121);
        }
    }
}
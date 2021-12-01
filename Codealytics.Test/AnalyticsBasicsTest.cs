using System;
using Xunit;

namespace Codealytics.Test
{
    public class AnalyticsBasicsTest
    {
        Analytics analytics;
        [Fact]
        public void TestConstructor()
        {
            Analytics analytics_tmp = new Analytics();
        }

        [Fact]
        public void TestAddMetricVariable()
        {
            analytics = new Analytics();
            analytics.AddMetric<string>("varString", "Hello World!");
            analytics.AddMetric<int>("varInt", 10);
            analytics.AddMetric<double>("varDouble", 10.94);
        }

        [Fact]
        public void TestGetMetricVariable()
        {
            analytics = new Analytics();
            analytics.AddMetric<string>("varString", "Hello World!");
            analytics.AddMetric<int>("varInt", 10);
            analytics.AddMetric<double>("varDouble", 10.94);

            Assert.Equal("Hello World!", analytics.GetMetric<string>("varString"));
            Assert.Equal(10, analytics.GetMetric<int>("varInt"));
            Assert.Equal(10.94, analytics.GetMetric<double>("varDouble"));
        }

        [Fact]
        public void TestAddMetricFunction()
        {
            analytics = new Analytics();
            analytics.AddMetric<string>("varString", () => { return "Hello World!"; });
            analytics.AddMetric<int>("varInt", () => { return 10; });
            analytics.AddMetric<double>("varDouble", () => { return 10.94; });
        }

        [Fact]
        public void TestGetMetricFunction()
        {
            analytics = new Analytics();
            analytics.AddMetric<string>("varString", "Hello World!");
            analytics.AddMetric<int>("varInt", 10);
            analytics.AddMetric<double>("varDouble", 10.94);

            Assert.Equal("Hello World!", analytics.GetMetric<string>("varString"));
            Assert.Equal(10, analytics.GetMetric<int>("varInt"));
            Assert.Equal(10.94, analytics.GetMetric<double>("varDouble"));
        }

        [Fact]
        public void TestUpdateMetricVariableUsingVariable()
        {
            analytics = new Analytics();
            analytics.AddMetric<string>("varString", "World!");
            analytics.AddMetric<int>("varInt", 3);
            analytics.AddMetric<double>("varDouble", 14);

            analytics.UpdateMetric<string>("varString", "Hello World!");
            analytics.UpdateMetric<int>("varInt", 10);
            analytics.UpdateMetric<double>("varDouble", 10.94);

            Assert.Equal("Hello World!", analytics.GetMetric<string>("varString"));
            Assert.Equal(10, analytics.GetMetric<int>("varInt"));
            Assert.Equal(10.94, analytics.GetMetric<double>("varDouble"));
        }

        [Fact]
        public void TestUpdateMetricVariableUsingFunction()
        {
            analytics = new Analytics();
            analytics.AddMetric<string>("varString", "Hello World!");
            analytics.AddMetric<int>("varInt", 3);
            analytics.AddMetric<double>("varDouble", 60);

            analytics.UpdateMetric<string>("varString", (s) => { return s.Split(" ")[0]; });
            analytics.UpdateMetric<int>("varInt", (i) => { return i + 7; });
            analytics.UpdateMetric<double>("varDouble", (i) => { return i / 10; });

            Assert.Equal("Hello", analytics.GetMetric<string>("varString"));
            Assert.Equal(10, analytics.GetMetric<int>("varInt"));
            Assert.Equal(6, analytics.GetMetric<double>("varDouble"));
        }

        [Fact]
        public void TestUpdateMetricFunctionUsingFunction()
        {
            analytics = new Analytics();

            analytics.AddMetric<bool>("varTest", () => { return false; });
            analytics.UpdateMetric<bool>("varTest", () => { return true; });

            Assert.True(analytics.GetMetric<bool>("varTest"));
        }

        [Theory]
        [InlineData("1Test", true)]
        [InlineData("_1Test", false)]
        [InlineData("-Alph", true)]
        [InlineData("'Äglk", true)]
        [InlineData("sdfs sfdsd", true)]
        [InlineData("(test)", true)]
        [InlineData(":test", true)]
        public void TestInvalidNames(string name, bool invalid)
        {
            analytics = new Analytics();
            if (invalid)
            {
                Assert.Throws<ArgumentException>(()=> { analytics.AddMetric<bool>(name, true); });
            }
            else
            {
                analytics.AddMetric<bool>(name, true);
            }
        }

        [Fact]
        public void TestToString()
        {
            analytics = new Analytics();
            analytics.AddMetric<string>("varString", "World!");
            analytics.AddMetric<int>("varInt", 3);
            analytics.AddMetric<double>("varDouble", 14, true);
            analytics.AddMetric<string>("varStringFunc", () => { return "Hello!"; });
            analytics.AddMetric<int>("varIntFunc", () => { return 1 + 3; });
            Assert.Equal("--------------- Analytics ---------------\nvarInt: 3 \nvarIntFunc: 4 \nvarString: World! \nvarStringFunc: Hello! \n------- Dev. Sebastian Steininger -------\n", analytics.ToString());
        }

        [Fact]
        public void CheckMetric()
        {
            analytics = new Analytics();
            analytics.AddMetric<string>("varString", "Hello World!");

            Assert.True(analytics.MetricExists("varString"));
            Assert.False(analytics.MetricExists("null"));
        }
    }
}
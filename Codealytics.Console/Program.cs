using Codealytics;
using Codealytics.HardwareMonitor;

Analytics analytics = new Analytics();
analytics.HandleUi = true;
////Console.WriteLine(analytics.HandleUi);
//analytics.AddMetric<string>("myString", "hello", true);
//Thread.Sleep(1000);
//analytics.AddMetric<bool>("IsWorking", () => { return true; });
//Thread.Sleep(500);
//analytics.AddMetric<int>("num", 0);
//Thread.Sleep(1000);

////Console.WriteLine(analytics);

//analytics.UpdateMetric<int>("num", 10);
//Thread.Sleep(1000);
//analytics.UpdateMetric<int>("num", (i) => { return i + 90; });

////analytics.Prefix = "Test\n";

//Thread.Sleep(1000);

//analytics.AddMetric<string>("varString", "Hello World!");

//Thread.Sleep(1000);

//analytics.HandleUi = false;
//Thread.Sleep(1000);
//analytics.Prefix = "Test\n";

//analytics.HandleUi = true;
//Random random = new Random();  

//analytics.AddMetric<IRuntimePerformanceInfromation>("rpi", new RuntimePerformanceInfromation());
//Parallel.For(0, 100, (i) =>
//{
//    analytics.CodeRuntimePerformance("rpi", () =>
//    {

//        System.Threading.Thread.Sleep(random.Next(0, 1000));

//    });
//});

//var rpi = analytics.GetMetric<IRuntimePerformanceInfromation>("rpi");

//analytics.AddMetric<string>("varString", "World!");
//Thread.Sleep(200);
//analytics.AddMetric<int>("varInt", 3);
//Thread.Sleep(200);
//analytics.AddMetric<double>("varDouble", 2);
//Thread.Sleep(200);
//analytics.AddMetric<string>("varStringFunc", () => { return "Hello!"; });
//Thread.Sleep(200);
//analytics.AddMetric<int>("varIntFunc", () => { return 1 + 3; });

//while (true)
//{
//    analytics.UpdateMetric<double>("varDouble", (d) =>
//    {
//        if (d > 1000)
//        {
//            return (int)d / 3;
//        }
//        return d * 2 -1;
//    });
//}

//string a = analytics.ToString();
//Console.WriteLine(analytics);


//analytics.AddMetric<int>("varInt", 10);
//analytics.AddMetric<double>("varDouble", 10.94);

//Console.WriteLine(analytics.GetMetric<string>("varString"));

//Console.WriteLine(analytics);

//string id = analytics.CodeRuntimePerformance(() => { System.Threading.Thread.Sleep(1000); });
//System.Threading.Thread.Sleep(1000);

//while (true)
//{
//    System.Threading.Thread.Sleep(100);
//    analytics.CodeRuntimePerformance(id, () => { System.Threading.Thread.Sleep(900); });
//}

Console.WriteLine(Math.Round(HardwareMonitor.Instance.CPU).ToString() + "%");
analytics.AddMetric<string>("CPU", () => { return Math.Round(HardwareMonitor.Instance.CPU).ToString() + "%"; });
analytics.AddMetric<string>("RAM", () => { return Math.Round(HardwareMonitor.Instance.RAM).ToString() + "%"; });
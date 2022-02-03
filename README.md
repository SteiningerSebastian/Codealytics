# Codealytics
Codelytics is a nuget package for performance analytics, hardware-monitoring and statistics.
It also provides a function to handle the console, and display all metrics.

## Examples
### Add Metric

    analytics.AddMetric<string>("myString", "hello World!");
    analytics.AddMetric<bool>("IsWorking", () => { return true; }, true) //Not displayed or shown using toString, hidden.
    
### Update Metric

    analytics.UpdateMetric<string>("myString", "Hi");
### HandleUi
Set the bool HandleUi to let the library take control over the console.

    analytics.HandleUi = false;

### Check if a Metric exists

    analytics.MetricExists("myString"); //true or false
### ToString
Returns a string build with Prefix and Suffix that contians all values that should be displayed.

    analytics.toString();

### Hardware-Monitoring
    analytics.AddMetric<string>("CPU", () => { return Math.Round(HardwareMonitor.Instance.CPU).ToString() + "%"; });
    analytics.AddMetric<string>("RAM", () => { return Math.Round(HardwareMonitor.Instance.RAM).ToString() + "%"; });

### Code Runtime Performance
    analytics.AddMetric<IRuntimePerformanceInfromation>("G_Train_Performance", new RuntimePerformanceInfromation());
    analytics.CodeRuntimePerformance("G_Train_Performance", () =>
    {
        (costEval, accEval) = model.TrainOnBatch(inputs.ToArray(), outputs.ToArray());
    });
    string id = analytics.CodeRuntimePerformance(() =>
    {
        (costEval, accEval) = model.TrainOnBatch(inputs.ToArray(), outputs.ToArray());
    });
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Codealytics.HardwareMonitor
{
    public class HardwareMonitor
    {
        //Singelton
        private static HardwareMonitor _instance = new HardwareMonitor();
        private HardwareMonitor() {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException();
            }

            //Start new thread that handels the PerformanceCounters
            Thread thPerformanceCounter = new Thread(HandlePerformanceCounter);
            thPerformanceCounter.Start();

            Thread.Sleep(100);
        }
        public static HardwareMonitor Instance { get { return _instance; } }

        /// <summary>
        /// The delay between updates
        /// </summary>
        public int Delay { get; set; } = 300;

        //All performanceCounters which handle cpu performance
        private List<PerformanceCounter>? cpuPCounters = null;

        private List<float> cpuAllCors_ = new List<float>();

        /// <summary>
        /// Returns a list containing the CPU-Usage of all cores, only supported on Windows.
        /// </summary>
        public List<float> CPUAllCores
        {
            get
            {
                lock (cpuAllCors_)
                {
                    return cpuAllCors_;
                }
            }
            private set
            {
                lock (cpuAllCors_)
                {
                    cpuAllCors_ = value;
                }
            }
        }

        /// <summary>
        /// Returns a the CPU-Usage as avg of all cores, only supported on Windows.
        /// </summary>
        public float CPU
        {
            get
            {
                lock (CPUAllCores)
                {
                    return CPUAllCores.Sum() / CPUAllCores.Count;
                }
            }
        }


        /// <summary>
        /// Handels all PerformanceCounter, shoul not be invoked by the main thread
        /// </summary>
        private void HandlePerformanceCounter()
        {
            while (true)
            {
                Thread.Sleep(Delay);
                UpdateCPUUsage();
            }
        }

        /// <summary>
        /// Updates the CPUUsage
        /// </summary>
        /// <exception cref="PlatformNotSupportedException">Is thrown if the platform is not supported.</exception>
        private void UpdateCPUUsage()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException();
            }

            if (cpuPCounters == null || cpuPCounters.Count == 0)
            {
                var cat = new PerformanceCounterCategory("Processor Information");
                char[] instances = cat.GetInstanceNames().Where(i => !i.Contains("Total")).Select(i => i[2]).ToArray();
                cpuPCounters = new List<PerformanceCounter>();
                for (int i = 0; i < Environment.ProcessorCount; i++)
                {
                    cpuPCounters.Add(new PerformanceCounter("Processor", "% Processor Time", $"{instances[i]}"));
                }
            }

            List<float> cpuCores = new List<float>();
            //Check if the platform is windows otherwise throw PlatformNotSupportedException

            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                if (cpuCores.Count <= i)
                {
                    cpuCores.Add(cpuPCounters[i].NextValue());
                }
            }

            lock (CPUAllCores)
            {
                CPUAllCores.Clear();
                CPUAllCores.AddRange(cpuCores);
            }
        }
    }
}
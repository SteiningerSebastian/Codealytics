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

        private float ramUsage =-1;

        /// <summary>
        /// Gets the percentage of used ram
        /// </summary>
        public float RAM
        {
            get
            {
                return ramUsage;
            }
            set{
                ramUsage = value;
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
                UpdateRamUsage();
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

        /// <summary>
        /// Update the ram Usage
        /// </summary>
        private void UpdateRamUsage()
        {
            MEMORY_INFO ramInfo = GetRAMStatus();
            RAM = ramInfo.dwMemoryLoad;
        }

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GlobalMemoryStatusEx(ref MEMORY_INFO mi);

        //Define the information structure of memory
        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_INFO
        {
            public uint dwLength; //Current structure size
            public uint dwMemoryLoad; //Current memory utilization
            public ulong ullTotalPhys; //Total physical memory size
            public ulong ullAvailPhys; //Available physical memory size
            public ulong ullTotalPageFile; //Total Exchange File Size
            public ulong ullAvailPageFile; //Total Exchange File Size
            public ulong ullTotalVirtual; //Total virtual memory size
            public ulong ullAvailVirtual; //Available virtual memory size
            public ulong ullAvailExtendedVirtual; //Keep this value always zero
        }

        /// <summary>
        /// Get the current ram usage
        /// </summary>
        /// <returns>Returns a structure containing information about the memory</returns>
        private MEMORY_INFO GetRAMStatus()
        {
            MEMORY_INFO mi = new MEMORY_INFO();
            mi.dwLength = (uint)System.Runtime.InteropServices.Marshal.SizeOf(mi);
            GlobalMemoryStatusEx(ref mi);
            return mi;
        }
    }
}
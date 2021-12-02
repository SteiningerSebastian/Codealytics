using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Codealytics
{
    public class Analytics
    {
        /// <summary>
        /// A threadsafe dictionary to save metrics the user registers
        /// </summary>
        ConcurrentDictionary<string, (Type type, dynamic value)> metrics = new ConcurrentDictionary<string, (Type type, dynamic value)>();

        /// <summary>
        /// A threadsafe dictionary to save information about visibility
        /// </summary>
        ConcurrentDictionary<string, bool> hiddenMetrics = new ConcurrentDictionary<string, bool>();

        /// <summary>
        /// The lockObj is used to manage redraws
        /// </summary>
        object lockObjRedraw = new object();

        /// <summary>
        /// The lockObj is used to manage termination of handle UI
        /// </summary>
        object lockObjHandleUI = new object();

        /// <summary>
        /// Bool for redarawing the Ui
        /// </summary>
        bool redraw = true;

        /// <summary>
        /// List of characters a id is not allowed to contain
        /// </summary>
        private List<string> invalidChars = new List<string>() {"\"", "!", "°", "^", "+", "-", "*", ":", ";", ",", "!",
        "?", "=", "(", ")", "{", "}", "[", "]", "$","%","&","/","@","<",">","|", "\'"};

        /// <summary>
        /// The prefix is displayed in the console when printed
        /// </summary>
        string prefix = "--------------- Analytics ---------------\n";
        public string Prefix
        {
            get { return prefix; }
            set
            {
                if (HandleUi)
                {
                    throw new InvalidOperationException("You can not set the Prefix while the class handles the UI please try to set it before.");
                }
                else
                {
                    prefix = value;
                }
            }
        }

        /// <summary>
        /// The suffix is displayed in the console when printed
        /// </summary>
        string suffix = "------- Dev. Sebastian Steininger -------\n";
        public string Suffix
        {
            get { return suffix; }
            set
            {
                if (HandleUi)
                {
                    throw new InvalidOperationException("You can not set the Prefix while the class handles the UI please try to set it before.");
                }
                else
                {
                    suffix = value;
                }
            }
        }

        /// <summary>
        /// Can be set to let the class handle the console output.
        /// </summary>
        private bool handleUi = false;
        public bool HandleUi
        {
            get { lock (lockObjHandleUI) { return handleUi; } }
            set
            {
                lock (lockObjHandleUI)
                {
                    handleUi = value;
                    if (handleUi)
                    {
                        this.StartHandleUI();
                    }
                }
            }
        }

        /// <summary>
        /// A private uuid for auto generated metrics.
        /// </summary>
        private int uuid = 0;

        /// <summary>
        /// Adds a metric (e.g. string, int, delegate, etc.)
        /// </summary>
        /// <typeparam name="T">The datatype of the metric.</typeparam>
        /// <param name="id">The identifier for the metric.</param>
        /// <param name="value">The value of the metric.</param>
        /// <exception cref="ArgumentException">Is thrown if a entry with the given id allready exists or the identifier did not match our guidlines (no spaces, etc)!</exception>
        /// <exception cref="Exception">Is thrown if the id does not exist but the value could not be added!</exception>
        /// <exception cref="ArgumentNullException">Is thrown if the value parameter is null!</exception>
        public void AddMetric<T>(string id, T value, bool hide = false)
        {
            if (value == null)
            {
                throw new ArgumentNullException("The value can not be null!");
            }

            CheckIdentifier(id);

            //Redraw is needed if new metrics are added that should be displayed
            if (!hide)
            {
                lock (lockObjRedraw)
                {
                    redraw = true;
                }
            }

            //try to add the metric to the dictionary
            if (!metrics.TryAdd(id, (value.GetType(), value)) || !hiddenMetrics.TryAdd(id, hide))
            {
                if (metrics.ContainsKey(id) && hiddenMetrics.ContainsKey(id))
                {
                    throw new ArgumentException("Identifier allready exists, use UpdateMetric if you wish to change the definition of the metric!");
                }

                //Remove definition from dictionaries
                metrics.TryRemove(id, out var _Ttmp);
                hiddenMetrics.TryRemove(id, out bool _btmp);

                throw new Exception("Something went wrong! (e.g. compromised thread safety)");
            }
        }

        /// <summary>
        /// Adds a metric (e.g. string, int, delegate, etc.)
        /// </summary>
        /// <typeparam name="T">The datatype of the metric.</typeparam>
        /// <param name="id">The identifier for the metric.</param>
        /// <param name="calculation">The delegate/Func defining the metric.</param>
        /// <exception cref="ArgumentException">Is thrown if a entry with the given id allready exists or the identifier did not match our guidlines (no spaces, etc)!</exception>
        /// <exception cref="Exception">Is thrown if the id does not exist but the value could not be added!</exception>
        /// <exception cref="ArgumentNullException">Is thrown if the value parameter is null!</exception>
        public void AddMetric<T>(string id, Func<T> calculation, bool hide = false)
        {
            if (calculation == null)
            {
                throw new ArgumentNullException("The calculation can not be null!");
            }

            CheckIdentifier(id);

            //Redraw is needed if new metrics are added that should be displayed
            if (!hide)
            {
                lock (lockObjRedraw)
                {
                    redraw = true;
                }
            }

            //try to add the metric to the dictionary
            if (!metrics.TryAdd(id, (calculation.GetType(), calculation)) || !hiddenMetrics.TryAdd(id, hide))
            {
                if (metrics.ContainsKey(id) && hiddenMetrics.ContainsKey(id))
                {
                    throw new ArgumentException("Identifier allready exists, use UpdateMetric if you wish to change the definition of the metric!");
                }

                //Remove definition from dictionaries
                metrics.TryRemove(id, out var _Ttmp);
                hiddenMetrics.TryRemove(id, out bool _btmp);

                throw new Exception("Something went wrong! (e.g. compromised thread safety)");
            }
        }

        /// <summary>
        /// Get the metric by given identifier
        /// </summary>
        /// <typeparam name="T">The data-type of the metric.</typeparam>
        /// <param name="id">The id of the metric.</param>
        /// <returns>Returns the value of the metric.</returns>
        /// <exception cref="NullReferenceException">Is thrown if no definition for the metric is known or does not exist!</exception>
        /// <exception cref="Exception">Is thrown if a callable type was expacted but gut a non callable type!</exception>
        public T GetMetric<T>(string id)
        {
            CheckIdentifier(id);

            //Try to get a value from the dictionary
            if (metrics.TryGetValue(id, out (Type type, dynamic value) value))
            {
                //check if the value equals the type
                if (value.type.GetProperty("Method") == null)
                {
                    return (T)value.value;
                }
                else
                {
                    try
                    {
                        return (T)value.value();
                    }
                    catch
                    {
                        throw new Exception("Unable to call object (Func, delegate, Event, etc.), expacted a callable type!");
                    }
                }
            }

            throw new NullReferenceException("No definition for the given metric could be found or and other error occurred (eg. compromised thread safety).");
        }

        /// <summary>
        /// Updates a definition of a metric
        /// </summary>
        /// <typeparam name="T">The data-type of the metric.</typeparam>
        /// <param name="id">The identifier for the metric.</param>
        /// <param name="value">The new value for the metric.</param>
        /// <exception cref="NullReferenceException">Is thrown if no definition for the metric is known or does not exist!</exception>
        /// <exception cref="ArgumentNullException">Is thrown if the value parameter is null!</exception>
        public void UpdateMetric<T>(string id, T value, bool hide = false)
        {
            if (value == null)
            {
                throw new ArgumentNullException("The value can not be null!");
            }

            CheckIdentifier(id);

            //lock metrics and hiddenmetrics to ensure thread-safe updates
            lock (metrics)
            {
                lock (hiddenMetrics)
                {
                    if (metrics.TryGetValue(id, out (Type type, dynamic value) compVal) && hiddenMetrics.TryGetValue(id, out bool isHidden))
                    {
                        if (isHidden != hide)
                        {
                            hiddenMetrics.TryUpdate(id, isHidden, hide);
                        }
                        if (metrics.TryUpdate(id, (value.GetType(), value), compVal))
                        {
                            return;
                        }
                    }
                }
            }

            throw new NullReferenceException("No definition for the given metric could be found or and other error occurred (eg. compromised thread safety).");
        }

        /// <summary>
        /// Updates a definition/value of an matric using a function
        /// </summary>
        /// <typeparam name="T">The data-type of the metric.</typeparam>
        /// <param name="id">The identifier for the metric.</param>
        /// <param name="func">The function that updates the value.</param>
        /// <exception cref="NullReferenceException">Is thrown if no definition for the metric is known or does not exist!</exception>
        /// <exception cref="ArgumentNullException">Is thrown if the func parameter is null!</exception>
        public void UpdateMetric<T>(string id, Func<T, T> func, bool hide = false)
        {
            if (func == null)
            {
                throw new ArgumentNullException("Func can not be null!");
            }

            CheckIdentifier(id);

            //lock metrics and hiddenmetrics to ensure thread-safe updates
            lock (metrics)
            {
                lock (hiddenMetrics)
                {
                    if (metrics.TryGetValue(id, out (Type type, dynamic value) compVal) && hiddenMetrics.TryGetValue(id, out bool isHidden))
                    {
                        if (isHidden != hide)
                        {
                            hiddenMetrics.TryUpdate(id, isHidden, hide);
                        }
                        T value = func(compVal.value);
                        if (metrics.TryUpdate(id, (value.GetType(), value), compVal))
                        {
                            return;
                        }
                    }
                }
            }

            throw new NullReferenceException("No definition for the given metric could be found or and other error occurred (eg. compromised thread safety).");
        }

        /// <summary>
        /// Updates a definition of a metric to a delegate
        /// </summary>
        /// <typeparam name="T">The return data-type of the function.</typeparam>
        /// <param name="id">The identifier for the metric.</param>
        /// <param name="calculation">The calculation of the metric.</param>
        /// <exception cref="NullReferenceException">Is thrown if no definition for the metric is known or does not exist!</exception>
        public void UpdateMetric<T>(string id, Func<T> calculation)
        {
            if (calculation == null)
            {
                throw new ArgumentNullException("The calculation can not be null");
            }

            CheckIdentifier(id);

            //lock metrics and hiddenmetrics to ensure thread-safe updates
            lock (metrics)
            {
                lock (hiddenMetrics)
                {
                    if (metrics.TryGetValue(id, out (Type type, dynamic value) compCalc))
                    {
                        if (metrics.TryUpdate(id, (calculation.GetType(), calculation), compCalc))
                        {
                            return;
                        }
                    }
                }
            }
            throw new NullReferenceException("No definition for the given metric could be found or and other error occurred (eg. compromised thread safety).");
        }

        /// <summary>
        /// Checks if a metric with the identifier exists.
        /// </summary>
        /// <param name="id">The identifier to check for a metric.</param>
        /// <returns>Returns a true if the metric exists</returns>
        public bool MetricExists(string id)
        {
            CheckIdentifier(id);
            return metrics.ContainsKey(id);
        }


        /// <summary>
        /// Analyzes the given code.
        /// </summary>
        /// <param name="id">The id of the metric of type RuntimePerformanceInformation in which result should be saved in.</param>
        /// <param name="action">The code that should be analyced during runtime.</param>
        public void CodeRuntimePerformance(string id, Action action)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            action();
            stopwatch.Stop();
            int elepsedMSec = (int)stopwatch.ElapsedMilliseconds;
            UpdateMetric<IRuntimePerformanceInfromation>(id, (rpi) =>
            {
                rpi.AddResult(elepsedMSec);
                return rpi;
            });
        }

        /// <summary>
        /// Analyzes the given code.
        /// </summary>
        /// <param name="id">The id of the metric of type RuntimePerformanceInformation in which result should be saved in.</param>
        /// <param name="action">The code that should be analyced during runtime.</param>
        public string CodeRuntimePerformance(Action action)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            action();
            stopwatch.Stop();
            int ellepsedMSec = (int)stopwatch.ElapsedMilliseconds;

            string id = $"RuntimePerformanceInformation_{uuid}";
            uuid++;
            IRuntimePerformanceInfromation rpi = new RuntimePerformanceInfromation();
            rpi.AddResult(ellepsedMSec);

            AddMetric<IRuntimePerformanceInfromation>(id, rpi);
            return id;
        }

        /// <summary>
        /// Starts a new Thread that handels the presentation of the analytics. (Pleas do not use the console)
        /// </summary>
        /// <param name="ups">Updates per second.</param>
        private void StartHandleUI(int ups = 32)
        {
            //Create a new thread handling the Ui and return after start
            Thread th = new Thread(() =>
            {
                Console.CursorVisible = false;
                Console.Clear();
                Console.CursorLeft = 0;
                Console.CursorTop = 0;

                //Update loop
                while (true)
                {
                    string output = "";
                    //check if a redraw must be done (new metrics, etc.)
                    bool redraw_;
                    lock (lockObjRedraw)
                    {
                        redraw_ = redraw;
                    }
                    List<KeyValuePair<string, (Type type, dynamic value)>> kvpList = metrics.ToList().OrderBy(metric => metric.Key).ToList();
                    if (redraw_)
                    {
                        Console.Clear();
                        //add the Prefix to the output
                        output += prefix;
                        foreach (KeyValuePair<string, (Type type, dynamic value)> kvp in kvpList)
                        {
                            string id = kvp.Key;
                            //do not display hidden metrics
                            if (!hiddenMetrics[id])
                            {
                                //Check if metric is callable
                                if (kvp.Value.type.GetProperty("Method") != null)
                                {
                                    output += $"{id}: {metrics[id].value()} \n";
                                }
                                else
                                {
                                    output += $"{id}: {metrics[id].value} \n";
                                }

                            }
                        }
                        output += suffix;
                        lock (lockObjRedraw)
                        {
                            redraw_ = false;
                            redraw = redraw_;
                        }
                        Console.WriteLine(output);
                    }
                    //update values
                    else
                    {
                        //set the cursor top position
                        Console.CursorTop = prefix.Split('\n').Length - 1;
                        foreach (KeyValuePair<string, (Type type, dynamic value)> kvp in kvpList)
                        {
                            string id = kvp.Key;
                            //Check if the metric is hidden
                            if (!hiddenMetrics[id])
                            {
                                //Check if metric is callable
                                if (kvp.Value.type.GetProperty("Method") != null)
                                {
                                    Console.CursorLeft = ($"{id}: ").Length;
                                    Console.Write(new string(' ', Console.WindowWidth - ($"{id}: ").Length));
                                    Console.CursorLeft = ($"{id}: ").Length;
                                    var v = metrics[id].value();
                                    Console.Write($"{v}\n");
                                }
                                else
                                {
                                    Console.CursorLeft = ($"{id}: ").Length;
                                    Console.Write(new string(' ', Console.WindowWidth- ($"{id}: ").Length));
                                    Console.CursorLeft = ($"{id}: ").Length;
                                    Console.Write($"{metrics[id].value}\n");
                                }
                            }
                        }
                    }

                    lock (lockObjHandleUI)
                    {
                        if (!HandleUi)
                        {
                            Console.Clear();
                            return;
                        }
                    }

                    //wait for x seconds to get updates per second
                    Thread.Sleep(1000 / ups);
                }
            });
            th.Start();
        }

        /// <summary>
        /// Creates a string containing all information of the class (all variables that are not hidden)
        /// </summary>
        /// <returns>Returns a string contining information of the class</returns>
        public override string ToString()
        {
            string output = "";
            output += prefix;
            List<KeyValuePair<string, (Type type, dynamic value)>> kvpList = metrics.ToList().OrderBy(metric => metric.Key).ToList();
            foreach (KeyValuePair<string, (Type type, dynamic value)> kvp in kvpList)
            {
                string id = kvp.Key;
                //do not display hidden metrics
                if (!hiddenMetrics[id])
                {
                    //Check if metric is callable
                    if (kvp.Value.type.GetProperty("Method") != null)
                    {
                        output += $"{id}: {metrics[id].value()} \n";
                    }
                    else
                    {
                        output += $"{id}: {metrics[id].value} \n";
                    }
                }
            }
            output += suffix;
            return output;
        }

        /// <summary>
        /// Checks if a identifier is valid or not
        /// </summary>
        /// <param name="id">The identifier of the metric.</param>
        /// <exception cref="ArgumentException">Thrown if the id is not valid!</exception>
        private void CheckIdentifier(string id)
        {
            if (id.Length == 0)
            {
                throw new ArgumentException("Identifier must not be \"\"");
            }
            else if (id.Contains(" "))
            {
                throw new ArgumentException("Identifier is not allowed to contain \" \" ");
            }
            else if (id[0].ToString().Any(Char.IsDigit))
            {
                throw new ArgumentException("Identifier is not allowed to contain digits at index zero!");
            }
            else if (invalidChars.Any(s => id.Contains(s)))
            {
                throw new ArgumentException("Identifier contains invalid characters!");
            }
        }
    }
}

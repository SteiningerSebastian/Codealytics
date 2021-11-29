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
        /// The following lockObj is used to manage redraws
        /// </summary>
        object lockObj = new object();
        bool redraw = true;

        /// <summary>
        /// List of characters a id is not allowed to contain
        /// </summary>
        private List<string> invalidChars = new List<string>() {"\"", "!", "°", "^", "+", "-", "*", ":", ";", ",", "!",
        "?", "=", "(", ")", "{", "}", "[", "]", "$","%","&","/","@","<",">","|"};

        /// <summary>
        /// The prefix is displayed in the console when printed
        /// </summary>
        public string Prefix { get; set; } = "--------------- Analytics ---------------\n";

        /// <summary>
        /// The suffix is displayed in the console when printed
        /// </summary>
        public string Suffix { get; set; } = "------- Dev. Sebastian Steininger -------\n";

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
                lock (lockObj)
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
                lock (lockObj)
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
                if (value.GetType() == typeof(T))
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
                    if (metrics.TryGetValue(id, out (Type type, dynamic value) compVal) && hiddenMetrics.TryUpdate(id, hide, !hide))
                    {
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
                    if (metrics.TryGetValue(id, out (Type type, dynamic value) compVal) && hiddenMetrics.TryUpdate(id, hide, !hide))
                    {
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
        /// Handels the presentation of the analytics.
        /// </summary>
        /// <param name="ups">Updates per second.</param>
        public void HandleUI(int ups = 32)
        {
            //Create a new thread handling the Ui and return after start
            Thread th = new Thread(() =>
            {
                Console.CursorVisible = false;

                //Update loop
                while (true)
                {
                    string output = "";
                    //check if a redraw must be done (new metrics, etc.)
                    if (redraw)
                    {
                        //add the Prefix to the output
                        output += Prefix;
                        foreach (KeyValuePair<string, (Type type, dynamic value)> kvp in metrics)
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
                        output += Suffix;
                    }
                    //update values
                    else
                    {
                        //set the cursor top position
                        Console.CursorTop = Prefix.Split('\n').Length;
                        foreach (KeyValuePair<string, (Type type, dynamic value)> kvp in metrics)
                        {
                            string id = kvp.Key;
                            //Check if the metric is hidden
                            if (!hiddenMetrics[id])
                            {
                                //Check if metric is callable
                                if (kvp.Value.type.GetProperty("Method") != null)
                                {
                                    Console.CursorLeft = ($"{id}: ").Length;
                                    var v = metrics[id].value();
                                    Console.Write($"{v}\n");
                                    //increase cursorTop position to get to the right lin
                                    Console.CursorTop += $"{v}\n".Split('\n').Length;
                                }
                                else
                                {
                                    Console.CursorLeft = ($"{id}: ").Length;
                                    Console.Write($"{metrics[id].value}\n");
                                    //increase cursorTop position to get to the right lin
                                    Console.CursorTop += $"{metrics[id].value}\n".Split('\n').Length;
                                }
                            }
                        }
                    }
                    Console.WriteLine(output);

                    //Reset the position of the cursor for the next update
                    Console.CursorLeft = 0;
                    Console.CursorTop = 0;

                    //wait for x seconds to get updates per second
                    Thread.Sleep(1000 / ups);
                }
            });
            th.Start();
        }

        /// <summary>
        /// Prints the analysed data
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string output = "";
            output += Prefix;
            foreach (KeyValuePair<string, (Type type, dynamic value)> kvp in metrics)
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
            output += Suffix;
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
            else if (id.All(char.IsDigit))
            {
                throw new ArgumentException("Identifier is not allowed to contain digits");
            }
            else if (invalidChars.Any(s => id.Contains(s)))
            {
                throw new ArgumentException("Identifier contains invalid characters!");
            }
        }
    }
}

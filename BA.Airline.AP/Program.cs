using BA.Airline.AP.Airline;
using BA.Airline.AP.Airline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BA.Airline.AP
{
    class Program
    {
        static void Main(string[] args)
        {
            InitializeConsole();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("You are welcome to use Airline manager 2.0");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("This is a manger which allows you to manage flights, which have a passanger you can manage too");
            Console.WriteLine("You will always have a menu with operation you can do on current object");
            Console.WriteLine("If you have any questions regarding current object, please look into 'Info' menu");
            Console.Write("While doing operations with editing, deleting or adding items please pay attention at");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(" BLUE ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("hint");

            Console.WriteLine("Please press 'Enter' button to proceed");
            Console.ReadLine();

            Start(AirlineFactory.Create(args));

        }

        /// <summary>
        /// Runs the Airline manager
        /// </summary>
        /// <param name="airlineManager">Airline manager need to run</param>
        static void Start(IAirlineManager airlineManager)
        {
            airlineManager.DisplayInfoChanged += DisplayInfoChanged;

        start:
            airlineManager.Reset();
            airlineManager.ShowOptions();
            airlineManager.SelectOptions(Console.ReadLine());

            string[] options = null;
            if (airlineManager.NeedOption)
            {
                if (!airlineManager.IsServiceOptionNow)
                    options = GetOptionsFromConsole(airlineManager.MultipleOption, airlineManager.Properties);
                else
                    options = new string[] { Console.ReadLine() };
            }
            airlineManager.ProcessOptions(options);
            airlineManager.Show();
            var currentAirlineManager = airlineManager.CurrentAirlineManager;


            if (currentAirlineManager == null)
            {
                airlineManager.DisplayInfoChanged -= DisplayInfoChanged;
                return;
            }
            else {
                if (currentAirlineManager == airlineManager)
                    goto start;
                else {
                    Start(currentAirlineManager);
                    goto start;
                }
            }
        }

        /// <summary>
        /// Get options from console, helps with autofilling properties
        /// </summary>
        /// <param name="multiple">If should be more than one property</param>
        /// <param name="properties">List of properties for autofilling</param>
        /// <returns>Array of entered values</returns>
        private static string[] GetOptionsFromConsole(bool multiple, string[] properties)
        {
            StringBuilder options = new StringBuilder();
            string line = string.Empty;

            do
            {
                line = string.Empty;
                int position = 0;
                do
                {
                    var key = Console.ReadKey();
                    if (key.Key != ConsoleKey.Spacebar)
                    {
                        if ((key.Key == ConsoleKey.Enter) && (string.IsNullOrWhiteSpace(line)))
                            break;
                        if (!string.IsNullOrWhiteSpace(line))
                            line = line.Substring(0, position);
                        line += key.KeyChar;
                        line = line.Trim();
                        position = line.Length;

                        ClearCurrentConsoleLine();

                        line = properties.FirstOrDefault(arg => arg.StartsWith(line, StringComparison.InvariantCultureIgnoreCase));
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            WriteOptionName(line, position, Console.CursorTop);

                        }


                        if (key.Key == ConsoleKey.Enter)
                        {
                            line += " ";
                            ClearCurrentConsoleLine();
                            Console.Write(line);
                            break;
                        }
                    }
                    else {
                        line += " ";
                        ClearCurrentConsoleLine();
                        Console.Write(line);
                        break;
                    }

                } while (true);

                if (!string.IsNullOrWhiteSpace(line))
                    line += Console.ReadLine();

                options.Append(line + ";");
                if (!multiple)
                    break;

            } while (!string.IsNullOrWhiteSpace(line));

            return options.ToString().Split(';').Where(arg => !string.IsNullOrWhiteSpace(arg)).ToArray();
        }

        /// <summary>
        /// Writes autofilled property instead of entered line
        /// </summary>
        /// <param name="line">Current line</param>
        /// <param name="position">Position of last enterd character</param>
        /// <param name="cursorTop">Top position of cursor</param>
        private static void WriteOptionName(string line, int position, int cursorTop)
        {
            Console.Write(line.Substring(0, position));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(line.Substring(position, line.Length - position));
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.SetCursorPosition(position, cursorTop);
        }

        /// <summary>
        /// Handler for displaying changes in console
        /// </summary>
        /// <param name="sender">Sender of an object</param>
        /// <param name="e">Parametres for displaying</param>
        private static void DisplayInfoChanged(object sender, AirlineObjectEventArgs e)
        {
            if (e.ClearConsoleLine)
                ClearCurrentConsoleLine(true);

            if (e.ClearConsole)
                Console.Clear();

            if (!string.IsNullOrWhiteSpace(e.DisplayInfo))
            {
                Console.ForegroundColor = e.ConsoleColor;
                Console.WriteLine(e.DisplayInfo);
                Console.ForegroundColor = ConsoleColor.Gray;
            }

        }

        /// <summary>
        /// Clears the console/line
        /// </summary>
        /// <param name="previousLine">Flag if need to clear previos line</param>
        private static void ClearCurrentConsoleLine(bool previousLine = false)
        {
            int currentLineCursor = Console.CursorTop - (previousLine ? 1 : 0);
            Console.SetCursorPosition(0, currentLineCursor);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }


        #region console view


        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        static public void InitializeConsole()
        {
            IntPtr hConsole = GetStdHandle(-11);   // get console handle
            COORD xy = new COORD(100, 100);
            SetConsoleDisplayMode(hConsole, 1, out xy); // set the console to fullscreen
            //SetConsoleDisplayMode(hConsole, 2);   // set the console to windowed


        }

        [StructLayout(LayoutKind.Sequential)]
        public struct COORD
        {

            public short X;
            public short Y;
            public COORD(short x, short y)
            {
                this.X = x;
                this.Y = y;
            }

        }
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetStdHandle(int handle);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleDisplayMode(
            IntPtr ConsoleOutput
            , uint Flags
            , out COORD NewScreenBufferDimensions
            );
        #endregion
    }
}

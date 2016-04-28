using BA.Airline.AP.Airline.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BA.Airline.AP.Airline.Models
{
    class CityAirport : AirlineManager
    {
        const int MaxFlights = 100;



        private static readonly AirlineOptions[] s_general = { AirlineOptions.ShowAllFlights, AirlineOptions.ShowArrivals, AirlineOptions.ShowDepartues, AirlineOptions.SearchFlights, AirlineOptions.AddAFlight, AirlineOptions.ClearTheConsole, AirlineOptions.Info, AirlineOptions.LoadFromFile, AirlineOptions.SaveToFile, AirlineOptions.ExitOrLevelUp };
        private static readonly AirlineOptions[] s_edit = { AirlineOptions.EditTheFlight, AirlineOptions.DeleteTheFlight, AirlineOptions.EditPassangersOfTheFlight, AirlineOptions.SearchFlights, AirlineOptions.AddAFlight };
        private static readonly AirlineOptions[] s_service = { AirlineOptions.LoadFromFile, AirlineOptions.SaveToFile };

        private static readonly AirlineOptions[] s_noNeedOptions = { AirlineOptions.ShowAllFlights, AirlineOptions.ShowArrivals, AirlineOptions.ShowDepartues, AirlineOptions.ClearTheConsole, AirlineOptions.Info, AirlineOptions.ExitOrLevelUp };
        private static readonly AirlineOptions[] s_multipleOptions = { AirlineOptions.EditTheFlight, AirlineOptions.AddAFlight };

        public CityAirport()
        {
            Options = s_general;

            AirlineObjects = new Flight[MaxFlights];

            CurrentAirlineManager = this;
            CurrentAirlineObjects = AirlineObjects;
            IndexOfCurrentAirlineManager = -1;
            Reset();
        }

        public CityAirport(int SizeOfFlights) : this()
        {
            if (SizeOfFlights > 0)
                InitializeFlightsByDefault(SizeOfFlights);
        }

        /// <summary>
        /// Initialize Default array of flights wit ha fake data
        /// </summary>
        /// <param name="sizeOfFlights">Amout of fake flights</param>
        private void InitializeFlightsByDefault(int sizeOfFlights)
        {
            if (sizeOfFlights > MaxFlights)
                sizeOfFlights = MaxFlights;

            for (int i = 0; i < sizeOfFlights; i++)
            {
                AirlineObjects[i] = new Flight(MaxFlights)
                {
                    Type = (i > 2) ? FlightType.Arrival : FlightType.Departure,
                    Airline = "test",
                    ArrivalCity = "Kharkiv",
                    DepartureCity = "Kiev",
                    Gate = "G" + 1 * i,
                    Arrival = DateTime.Now,
                    Departure = DateTime.Now,
                    Number = "72" + 2 * i,
                    Status = (i < 9) ? (FlightStatus)i : (FlightStatus)8,
                    Terminal = "F" + i
                };
            }
        }


        public override string[] TableHeader
        {
            get
            {
                return new string[]
                {
                    "ID",
                    (_selectedOption == AirlineOptions.ShowArrivals) ? nameof(Flight.Arrival) : nameof(Flight.Departure),
                    (_selectedOption == AirlineOptions.ShowArrivals) ? nameof(Flight.Departure) : nameof(Flight.Arrival),
                    nameof(Flight.Number) ,
                    (_selectedOption == AirlineOptions.ShowArrivals) ? nameof(Flight.ArrivalCity) : nameof(Flight.DepartureCity) ,
                    (_selectedOption == AirlineOptions.ShowArrivals) ? nameof(Flight.DepartureCity) : nameof(Flight.ArrivalCity) ,
                    nameof(Flight.Airline),
                    nameof(Flight.Terminal),
                    nameof(Flight.Gate),
                    nameof(Flight.Status),
                    nameof(Flight.Type),
                    "Count of passangers"
                };
            }
        }

        public override string[,] TableView
        {
            get
            {
                if (CurrentAirlineObjects != null)
                {
                    var count = CurrentAirlineObjects.Count(arg => arg != null);
                    var tableView = new string[count + 1, TableHeader.Length];


                    for (int i = 0; i < TableHeader.Length; i++)
                        tableView[0, i] = TableHeader[i];

                    for (int i = 0; i < tableView.GetLength(0) - 1; i++)
                    {
                        tableView[i + 1, 0] = (i + 1).ToString();
                        var flight = CurrentAirlineObjects[i] as Flight;
                        if (flight != null)
                        {
                            tableView[i + 1, 1] = (flight.Type == FlightType.Arrival) ? flight.Arrival.ToString() : flight.Departure.ToString();
                            tableView[i + 1, 2] = (flight.Type == FlightType.Arrival) ? flight.Departure.ToString() : flight.Arrival.ToString();
                            tableView[i + 1, 3] = flight.Number;
                            tableView[i + 1, 4] = (flight.Type == FlightType.Arrival) ? flight.ArrivalCity.ToString() : flight.DepartureCity.ToString();
                            tableView[i + 1, 5] = (flight.Type == FlightType.Arrival) ? flight.DepartureCity.ToString() : flight.ArrivalCity.ToString();
                            tableView[i + 1, 6] = flight.Airline;
                            tableView[i + 1, 7] = flight.Terminal;
                            tableView[i + 1, 8] = flight.Gate;
                            tableView[i + 1, 9] = flight.Status.ToString();
                            tableView[i + 1, 10] = flight.Type.ToString();
                            tableView[i + 1, 11] = flight.AirlineObjects.Count(arg => arg != null).ToString() + "/" + flight.AirlineObjects.Length.ToString();
                        }
                    }
                    return tableView;
                }
                return new string[0, 0];
            }
        }

        protected override AirlineOptions[] MultipleOptions { get { return s_multipleOptions; } }

        protected override AirlineOptions[] NoNeedOptions { get { return s_noNeedOptions; } }

        protected override AirlineOptions[] EditableOptions { get { return s_edit; } }

        public override string[] Properties
        {
            get
            {
                return new string[]
                {
                    "ID",
                    nameof(Flight.Airline),
                    nameof(Flight.Arrival),
                    nameof(Flight.ArrivalCity),
                    nameof(Flight.Departure),
                    nameof(Flight.DepartureCity),
                    nameof(Flight.Number),
                    nameof(Flight.Status),
                    nameof(Flight.Type),
                    nameof(Flight.Gate),
                    nameof(Flight.Terminal),
                    "Count"
                };
            }
        }

        public override bool IsServiceOptionNow
        {
            get
            {
                return s_service.Contains(_selectedOption);
            }
        }

        public override string Show()
        {
            var view = base.Show();
            if (!string.IsNullOrWhiteSpace(view))
                OnDisplayInfoChanged(new AirlineObjectEventArgs { DisplayInfo = view });
            return view;
        }        

        public override void ProcessOptions(string[] values)
        {
            var id = 0;
            switch (_selectedOption)
            {

                case AirlineOptions.ShowAllFlights:
                    CurrentAirlineObjects = AirlineObjects.Where(arg => (arg as Flight) != null).ToArray();
                    if (CurrentAirlineObjects.Length > 0)
                    {
                        Options = new AirlineOptions[s_general.Length + s_edit.Length];
                        s_edit.CopyTo(Options, 0);
                        s_general.CopyTo(Options, s_edit.Length);
                    }
                    break;
                case AirlineOptions.ShowArrivals:
                    CurrentAirlineObjects = AirlineObjects.Where(arg => (arg as Flight) != null && ((Flight)arg).Type == FlightType.Arrival).ToArray();
                    if (CurrentAirlineObjects.Length > 0)
                    {
                        Options = new AirlineOptions[s_general.Length + s_edit.Length];
                        s_edit.CopyTo(Options, 0);
                        s_general.CopyTo(Options, s_edit.Length);
                    }
                    break;
                case AirlineOptions.ShowDepartues:
                    CurrentAirlineObjects = AirlineObjects.Where(arg => (arg as Flight) != null && ((Flight)arg).Type == FlightType.Departure).ToArray();
                    if (CurrentAirlineObjects.Length > 0)
                    {
                        Options = new AirlineOptions[s_general.Length + s_edit.Length];
                        s_edit.CopyTo(Options, 0);
                        s_general.CopyTo(Options, s_edit.Length);
                    }
                    break;
                case AirlineOptions.SearchFlights:
                    if (values.Length > 0)
                    {
                        Find(values);
                    }
                    else
                        OnDisplayInfoChanged(new AirlineObjectEventArgs { DisplayInfo = "Empty values provided", ConsoleColor = ConsoleColor.Red, HasError = true });
                    Options = new AirlineOptions[s_general.Length + s_edit.Length];
                    s_edit.CopyTo(Options, 0);
                    s_general.CopyTo(Options, s_edit.Length);
                    break;
                case AirlineOptions.AddAFlight:
                    if (values.Length > 0)
                    {
                        if (Add(values, new Flight(), new Passenger[0]))
                            OnDisplayInfoChanged(new AirlineObjectEventArgs { DisplayInfo = "The Flight has been added successfully", ConsoleColor = ConsoleColor.Green });
                        else
                            OnDisplayInfoChanged(new AirlineObjectEventArgs { DisplayInfo = "The Flight wasn't added", ConsoleColor = ConsoleColor.Red, HasError = true });
                    }
                    else
                        OnDisplayInfoChanged(new AirlineObjectEventArgs { DisplayInfo = "Empty values provided", ConsoleColor = ConsoleColor.Red, HasError = true });
                    CurrentAirlineObjects = AirlineObjects.ToArray();
                    Options = new AirlineOptions[s_general.Length + s_edit.Length];
                    s_edit.CopyTo(Options, 0);
                    s_general.CopyTo(Options, s_edit.Length);
                    break;
                case AirlineOptions.EditTheFlight:
                    if (values.Length > 0)
                    {
                        if (Edit(values))
                            OnDisplayInfoChanged(new AirlineObjectEventArgs { DisplayInfo = "The Flight has been edited successfully", ConsoleColor = ConsoleColor.Green });
                    }
                    else
                        OnDisplayInfoChanged(new AirlineObjectEventArgs { DisplayInfo = "Empty values provided", ConsoleColor = ConsoleColor.Red, HasError = true });
                    CurrentAirlineObjects = AirlineObjects.ToArray();
                    Options = new AirlineOptions[s_general.Length + s_edit.Length];
                    s_edit.CopyTo(Options, 0);
                    s_general.CopyTo(Options, s_edit.Length);
                    break;
                case AirlineOptions.EditPassangersOfTheFlight:
                    if (values.Length > 0)
                    {
                        var optionsArray = values[0].Split(' ');
                        if ((optionsArray.Length == 3) && (int.TryParse(optionsArray[2], out id)) && (id > 0) && (id < CurrentAirlineObjects.Length + 1))
                        {
                            var flight = CurrentAirlineObjects[id - 1] as Flight;
                            if (flight != null)
                            {
                                IndexOfCurrentAirlineManager = id - 1;
                                CurrentAirlineManager = flight;
                            }
                        }
                    }
                    CurrentAirlineObjects = null;
                    Options = s_general;
                    break;
                case AirlineOptions.DeleteTheFlight:
                    if (values.Length > 0)
                    {
                        var optionsArray = values[0].Split(' ');
                        if ((optionsArray.Length == 3) && (int.TryParse(optionsArray[2], out id)) && (id > 0) && (id < CurrentAirlineObjects.Length + 1))
                        {
                            if (Delete(CurrentAirlineObjects[id - 1]))
                            {
                                CurrentAirlineObjects = AirlineObjects.ToArray();
                                OnDisplayInfoChanged(new AirlineObjectEventArgs { DisplayInfo = "Removed Successfully" });
                            }
                        }
                    }
                    else
                        OnDisplayInfoChanged(new AirlineObjectEventArgs { DisplayInfo = "Empty values provided", ConsoleColor = ConsoleColor.Red, HasError = true });
                    Options = new AirlineOptions[s_general.Length + s_edit.Length];
                    s_edit.CopyTo(Options, 0);
                    s_general.CopyTo(Options, s_edit.Length);
                    break;
                case AirlineOptions.ClearTheConsole:
                    OnDisplayInfoChanged(new AirlineObjectEventArgs { ClearConsole = true });
                    CurrentAirlineObjects = null;
                    Options = s_general;
                    break;
                case AirlineOptions.Info:
                    OnDisplayInfoChanged(new AirlineObjectEventArgs
                    {
                        ConsoleColor = ConsoleColor.Yellow,
                        DisplayInfo = "You are inside Airport manager where you can work with flights and it's info, "+
                                      "if you would like to receive an information about passanger and edit it, please go to 'Edit passangers of the flight'.\n"+
                                      "In case if you need to exit from Application, just chose 'Exit or level up' menu item"
                    });
                    CurrentAirlineObjects = null;
                    Options = s_general;
                    break;
                case AirlineOptions.LoadFromFile:
                    if (values.Length > 0)
                    {
                        try
                        {
                            if (OpenFromFile(values[0]))
                            {
                                OnDisplayInfoChanged(new AirlineObjectEventArgs { DisplayInfo = "Loaded successfully", ConsoleColor = ConsoleColor.Green });
                                CurrentAirlineObjects = AirlineObjects.ToArray();
                            }
                            else
                                OnDisplayInfoChanged(new AirlineObjectEventArgs { DisplayInfo = "Can't load from file", ConsoleColor = ConsoleColor.Red, HasError = true });
                        }
                        catch (Exception ex)
                        {
                            OnDisplayInfoChanged(new AirlineObjectEventArgs { DisplayInfo = $"Couldn't load data from file because of: {ex.Message}", ConsoleColor = ConsoleColor.Red, HasError = true });
                        }
                    }
                    else
                        OnDisplayInfoChanged(new AirlineObjectEventArgs { DisplayInfo = "Empty values provided", ConsoleColor = ConsoleColor.Red, HasError = true });
                    break;
                case AirlineOptions.SaveToFile:
                    if (values.Length > 0)
                    {
                        try
                        {
                            if (SaveToFile(values[0]))
                                OnDisplayInfoChanged(new AirlineObjectEventArgs { DisplayInfo = "Saved successfully", ConsoleColor = ConsoleColor.Green });
                            else
                                OnDisplayInfoChanged(new AirlineObjectEventArgs { DisplayInfo = "Can't save to file", ConsoleColor = ConsoleColor.Red, HasError = true });
                        }
                        catch (Exception ex)
                        {
                            OnDisplayInfoChanged(new AirlineObjectEventArgs { DisplayInfo = $"Couldn't save data to file because of: {ex.Message}", ConsoleColor = ConsoleColor.Red, HasError = true });
                        }
                    }
                    else
                        OnDisplayInfoChanged(new AirlineObjectEventArgs { DisplayInfo = "Empty values provided", ConsoleColor = ConsoleColor.Red, HasError = true });
                    break;
                case AirlineOptions.ExitOrLevelUp:
                    CurrentAirlineManager = null;
                    CurrentAirlineObjects = null;
                    Options = s_general;
                    Reset();
                    break;
            }
        }


        /// <summary>
        /// Opens file with flight's data
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns>Positive if flights are loaded</returns>
        private bool OpenFromFile(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    string text = File.ReadAllText(path);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        AirlineObjects = JsonConvert.DeserializeObject<Flight[]>(text, new JsonSerializerSettings { ContractResolver = new AirlineManagerContractResolver() });
                        return true;
                    }
                }
                catch { throw; }
            }
            return false;
        }

        /// <summary>
        /// Saves flights to the file
        /// </summary>
        /// <param name="path">Path to file where flight's data should be saved</param>
        /// <returns>Positive if flights are saved</returns>
        private bool SaveToFile(string path)
        {
            try
            {
                var written = false;
                var flightsJson = JsonConvert.SerializeObject(AirlineObjects, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                using (StreamWriter file = new StreamWriter(path, true))
                {
                    file.WriteLine(flightsJson);
                    written = true;
                }
                if (written)
                    return true;
            }
            catch { throw; }
            return false;
        }

        public override void Reset()
        {
            _selectedOption = AirlineOptions.NULL;
            base.Reset();
        }

        public override bool IsValid()
        {
            return false;
        }

        protected override bool CanBeResized()
        {
            return true;
        }
    }
    
}

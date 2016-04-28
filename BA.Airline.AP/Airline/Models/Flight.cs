using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BA.Airline.AP.Airline.Models
{
    class Flight : AirlineManager
    {

        #region properties
        public string Number { get; set; }
        public DateTime Arrival { get; set; }
        public DateTime Departure { get; set; }
        public string ArrivalCity { get; set; }
        public string DepartureCity { get; set; }
        public string Airline { get; set; }
        public string Terminal { get; set; }
        public string Gate { get; set; }
        public FlightStatus Status { get; set; }
        public FlightType Type { get; set; }
        #endregion


        private static readonly AirlineOptions[] s_general = { AirlineOptions.ShowPassangers, AirlineOptions.SearchPassangers, AirlineOptions.AddAPassanger, AirlineOptions.ClearTheConsole, AirlineOptions.Info, AirlineOptions.ExitOrLevelUp };
        private static readonly AirlineOptions[] s_edit = { AirlineOptions.EditThePassanger, AirlineOptions.DeleteThePassanger, AirlineOptions.SearchPassangers, AirlineOptions.AddAPassanger, };
        //private static readonly string[] s_all = { "Edit the passanger", "Delete the passanger", "Edit passangers of the flight", "Show Arrivals", "Show Departues", "Search flights", "Add a flight", "Clear console", "Info", "Exit/Level Up" };

        private static readonly AirlineOptions[] s_noNeedOptions = { AirlineOptions.ShowPassangers, AirlineOptions.ClearTheConsole, AirlineOptions.Info, AirlineOptions.ExitOrLevelUp };
        private static readonly AirlineOptions[] s_multipleOptions = { AirlineOptions.EditThePassanger, AirlineOptions.AddAPassanger };

        public Flight()
        {
            ID = Guid.NewGuid();
            Airline = string.Empty;
            ArrivalCity = string.Empty;
            DepartureCity = string.Empty;
            Number = string.Empty;
            Gate = string.Empty;
            Terminal = string.Empty;
            Options = s_general;
            CurrentAirlineManager = this;
            IndexOfCurrentAirlineManager = -1;

            if (AirlineObjects == null)
                AirlineObjects = new Passenger[_CountOfPassangers];
        }

        protected override AirlineOptions[] MultipleOptions { get { return s_multipleOptions; } }

        protected override AirlineOptions[] NoNeedOptions { get { return s_noNeedOptions; } }

        protected override AirlineOptions[] EditableOptions { get { return s_edit; } }

        private int _CountOfPassangers = 100;

        public Flight(int passangersCount) : this()
        {
            if (passangersCount > 0)
                _CountOfPassangers = passangersCount;

            AirlineObjects = new Passenger[_CountOfPassangers];
        }

        public override string[] TableHeader
        {
            get
            {
                return new string[]
                {
                    "ID",
                    nameof(Passenger.FirstName),
                    nameof(Passenger.LastName),
                    nameof(Passenger.Passport),
                    nameof(Passenger.Sex),
                    nameof(Passenger.Birthday),
                    nameof(Ticket.Cost),
                    nameof(Ticket.Type)
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
                        var passanger = CurrentAirlineObjects[i] as Passenger;
                        if (passanger != null)
                        {
                            tableView[i + 1, 1] = passanger.FirstName;
                            tableView[i + 1, 2] = passanger.LastName;
                            tableView[i + 1, 3] = passanger.Passport;
                            tableView[i + 1, 4] = passanger.Sex.ToString();
                            tableView[i + 1, 5] = passanger.Birthday.ToString("dd-MM-yyyy");
                            tableView[i + 1, 6] = passanger.Ticket.Cost.ToString();
                            tableView[i + 1, 7] = passanger.Ticket.Type.ToString();
                        }
                    }
                    return tableView;
                }
                return new string[0, 0];
            }
        }

        public override string[] Properties
        {
            get
            {
                return new string[]
                {
                    "ID",
                    nameof(Passenger.Birthday),
                    nameof(Passenger.FirstName),
                    nameof(Passenger.LastName),
                    nameof(Passenger.Passport),
                    nameof(Passenger.Sex),
                    nameof(Ticket.Cost),
                    nameof(Ticket.Type),
                };
            }
        }

        public override bool IsServiceOptionNow
        {
            get
            {
                return false;
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
                case AirlineOptions.ShowPassangers:
                    CurrentAirlineObjects = AirlineObjects.Where(arg => (arg as Passenger) != null).ToArray();
                    Options = new AirlineOptions[s_general.Length + s_edit.Length];
                    s_edit.CopyTo(Options, 0);
                    s_general.CopyTo(Options, s_edit.Length);
                    break;
                case AirlineOptions.SearchPassangers:
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
                case AirlineOptions.AddAPassanger:
                    if (values.Length > 0)
                    {
                        if (Add(values, new Passenger(), null))
                            OnDisplayInfoChanged(new AirlineObjectEventArgs { DisplayInfo = "The Passanger has been added successfully", ConsoleColor = ConsoleColor.Green });
                        else
                            OnDisplayInfoChanged(new AirlineObjectEventArgs { DisplayInfo = "The Passanger wasn't added", ConsoleColor = ConsoleColor.Red, HasError = true });
                    }
                    else
                        OnDisplayInfoChanged(new AirlineObjectEventArgs { DisplayInfo = "Empty values provided", ConsoleColor = ConsoleColor.Red, HasError = true });
                    CurrentAirlineObjects = AirlineObjects.ToArray();
                    Options = new AirlineOptions[s_general.Length + s_edit.Length];
                    s_edit.CopyTo(Options, 0);
                    s_general.CopyTo(Options, s_edit.Length);
                    break;
                case AirlineOptions.EditThePassanger:
                    if (values.Length > 0)
                    {
                        if (Edit(values))
                            OnDisplayInfoChanged(new AirlineObjectEventArgs { DisplayInfo = "The Passanger has been edited successfully", ConsoleColor = ConsoleColor.Green });
                    }
                    else
                        OnDisplayInfoChanged(new AirlineObjectEventArgs { DisplayInfo = "Empty values provided", ConsoleColor = ConsoleColor.Red, HasError = true });
                    CurrentAirlineObjects = AirlineObjects.ToArray();
                    Options = new AirlineOptions[s_general.Length + s_edit.Length];
                    s_edit.CopyTo(Options, 0);
                    s_general.CopyTo(Options, s_edit.Length);
                    break;
                case AirlineOptions.DeleteThePassanger:
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
                        DisplayInfo = "You are inside flight where you can work with passangers and it's info, \n" +                                      
                                      "In case if you need to go back to Airline manager, just chose 'Exit or level up' menu item"
                    });
                    CurrentAirlineObjects = null;
                    Options = s_general;
                    break;
                case AirlineOptions.ExitOrLevelUp:
                    CurrentAirlineManager = null;
                    CurrentAirlineObjects = null;
                    Options = s_general;
                    Reset();
                    break;
            }
        }

        protected override bool CanBeResized()
        {
            return (AirlineObjects.Count(arg => arg != null) - 1) < _CountOfPassangers;
        }

        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Airline) &&
                Arrival != null &&
                !string.IsNullOrWhiteSpace(ArrivalCity) &&
                Departure != null &&
                !string.IsNullOrWhiteSpace(DepartureCity) &&
                !string.IsNullOrWhiteSpace(Number) &&
                !string.IsNullOrWhiteSpace(Gate) &&
                ID != null &&
                !string.IsNullOrWhiteSpace(Terminal);
        }

        public static bool operator true(Flight flight)
        {
            return !string.IsNullOrWhiteSpace(flight.Airline) &&
                flight.Arrival != null &&
                !string.IsNullOrWhiteSpace(flight.ArrivalCity) &&
                flight.Departure != null &&
                !string.IsNullOrWhiteSpace(flight.DepartureCity) &&
                !string.IsNullOrWhiteSpace(flight.Number) &&
                !string.IsNullOrWhiteSpace(flight.Gate) &&
                flight.ID != null &&
                !string.IsNullOrWhiteSpace(flight.Terminal);
        }

        public static bool operator false(Flight flight)
        {
            return string.IsNullOrWhiteSpace(flight.Airline) ||
                flight.Arrival == null ||
                string.IsNullOrWhiteSpace(flight.ArrivalCity) ||
                flight.Departure == null ||
                string.IsNullOrWhiteSpace(flight.DepartureCity) ||
                string.IsNullOrWhiteSpace(flight.Number) ||
                string.IsNullOrWhiteSpace(flight.Gate) ||
                flight.ID == null ||
                string.IsNullOrWhiteSpace(flight.Terminal);
        }

    }
    
}

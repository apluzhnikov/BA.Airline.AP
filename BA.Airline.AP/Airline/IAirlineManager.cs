using BA.Airline.AP.Airline.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.Airline.AP.Airline
{
    public class AirlineObjectEventArgs : EventArgs
    {
        public string DisplayInfo { get; set; }
        public ConsoleColor ConsoleColor { get; set; }
        public bool HasError { get; set; }
        public bool ClearConsole { get; set; }
        public bool ClearConsoleLine { get; set; }

        public AirlineObjectEventArgs()
        {
            DisplayInfo = string.Empty;
            ConsoleColor = ConsoleColor.Gray;
            HasError = false;
            ClearConsole = false;
            ClearConsoleLine = false;
        }
    }

    public interface IAirlineManager: IAirlineObjectView
    {

        event EventHandler<AirlineObjectEventArgs> DisplayInfoChanged;

        void ShowOptions();
        void SelectOptions(string option);
        void ProcessOptions(string[] values);

        object this[string propertyName] { get; set; }
        string[] Properties { get; }

        bool Find(string[] FieldsValues);        
        bool Add(string[] FieldsValues, AirlineObject airlineObject, AirlineObject[] airlineObjects);
        bool Edit(string[] FieldsValues);
        bool Delete(AirlineObject airlineObject);

        void Reset();
        bool NeedOption { get; }
        bool IsServiceOptionNow { get; }
        


        bool MultipleOption { get; }
        IAirlineManager CurrentAirlineManager { get; }
    }
}

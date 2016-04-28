using BA.Airline.AP.Airline.Model;
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
    abstract class AirlineManager : AirlineObject, IAirlineManager
    {
        public AirlineObject[] AirlineObjects { get; protected set; }
        //public AirlineObject[] AirlineObjects { get; set; }

        protected AirlineObject[] CurrentAirlineObjects;

        public abstract string[] Properties { get; }

        public AirlineOptions[] Options { get; protected set; }
        protected AirlineOptions _selectedOption = AirlineOptions.NULL;
        protected abstract AirlineOptions[] MultipleOptions { get; }
        protected abstract AirlineOptions[] NoNeedOptions { get; }
        protected abstract AirlineOptions[] EditableOptions { get; }
        public abstract bool IsServiceOptionNow { get; }

        /// <summary>
        /// The event for displaing the info about airline object
        /// </summary>
        public event EventHandler<AirlineObjectEventArgs> DisplayInfoChanged;

        private readonly object eventLock = new object();

        /// <summary>
        /// Handler for displaing the info about airline object
        /// </summary>
        /// <param name="args">Arguments for displaying</param>
        protected virtual void OnDisplayInfoChanged(AirlineObjectEventArgs args)
        {
            EventHandler<AirlineObjectEventArgs> handler;
            lock (eventLock)
            {
                handler = DisplayInfoChanged;
            }
            if (handler != null)
            {
                handler(this, args);
            }
        }

        /// <summary>
        /// Shows options for current airline manager
        /// </summary>
        public void ShowOptions()
        {
            var options = new StringBuilder();
            options.AppendLine("Please select an option");
            for (int i = 0; i < Options.Length; i++)
            {


                options.AppendFormat("{0} - {1}", i + 1, ParseOption(Options[i]));
                options.AppendLine();
            }

            OnDisplayInfoChanged(new AirlineObjectEventArgs { DisplayInfo = options.ToString() });
        }

        private string ParseOption(AirlineOptions flightOption)
        {
            var option = flightOption.ToString();
            option = string.Concat(option.Select(c => char.IsUpper(c) ? " " + c.ToString().ToLower() : c.ToString())).TrimStart();
            option = char.ToUpper(option[0]) + option.Substring(1);
            return option;
        }

        /// <summary>
        /// Processes and prepare functioanlity depending on the options received
        /// </summary>
        /// <param name="option">Received option</param>
        public void SelectOptions(string option)
        {
            int optionId = 0;
            CurrentAirlineManager = this;
            if ((int.TryParse(option, out optionId)) && (optionId > 0) && (optionId < Options.Length + 1))
            {
                //var AirlineOption = Options[optionId - 1];
                _selectedOption = Options[optionId - 1];
                MultipleOption = MultipleOptions.Contains(_selectedOption);
                NeedOption = !NoNeedOptions.Contains(_selectedOption);
                OnDisplayInfoChanged(new AirlineObjectEventArgs { DisplayInfo = ParseOption(_selectedOption), ClearConsoleLine = true, ConsoleColor = ConsoleColor.DarkYellow });
                if (EditableOptions.Contains(_selectedOption))
                    OnDisplayInfoChanged(new AirlineObjectEventArgs
                    {
                        DisplayInfo = "Use the column names, " +
                        "then please use eq - equals/gt - greater than/lt - lower than for applying the values like 'FlightNumber eq RG24'\n" +
                        "For editing the airlineObject you should put ID filed on first place like 'ID eq 1'\n" +
                        "For deleting the airlineObject it's enough to provide just id like '1'\n",
                        ConsoleColor = ConsoleColor.Blue
                    });
                if (IsServiceOptionNow)
                    OnDisplayInfoChanged(new AirlineObjectEventArgs { DisplayInfo = "Please type path to file below :" });
            }
            else
            {
                OnDisplayInfoChanged(new AirlineObjectEventArgs { HasError = true, ConsoleColor = ConsoleColor.Red, DisplayInfo = "WRONG OPTION!!!!" });
                CurrentAirlineObjects = null;
                _selectedOption = AirlineOptions.NULL;
            }
        }

        /// <summary>
        /// Processes the option
        /// </summary>
        /// <param name="values">Values to process</param>
        public abstract void ProcessOptions(string[] values);

        /// <summary>
        /// Resets the default values
        /// </summary>
        public virtual void Reset()
        {
            NeedOption = false;
            MultipleOption = false;
            _selectedOption = AirlineOptions.NULL;
        }

        /// <summary>
        /// Removes an airline object from list
        /// </summary>
        /// <param name="airlineObject">Airline object you need to remove</param>
        /// <returns>Positive if removed</returns>
        public virtual bool Delete(AirlineObject airlineObject)
        {
            try
            {
                var prevCount = AirlineObjects.Count();
                var tmpAirlineObjects = AirlineObjects.Where(arg => arg != null && arg.ID != airlineObject.ID).ToArray();
                Array.Resize(ref tmpAirlineObjects, prevCount);
                AirlineObjects = tmpAirlineObjects;
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        const int fieldNameIndex = 0;
        const int fieldConditionIndex = 1;
        const int fieldValueIndex = 2;

        /// <summary>
        /// Does search in array of airline object based on search values
        /// </summary>
        /// <param name="FieldsValues">Array of fields and their values</param>
        /// <returns>Array of airline objects found</returns>
        public virtual bool Find(string[] FieldsValues)
        {
            //var objects = new IAirlineManager[CurrentAirlineObjects.Length];
            Array.Clear(CurrentAirlineObjects, 0, CurrentAirlineObjects.Length);
            var FieldsValuesSearch = GetAirlineObjectInfo(FieldsValues);
            var indexOfAirlineObjects = -1;
            try
            {
                if (FieldsValuesSearch.Length > 0 && !string.IsNullOrWhiteSpace(FieldsValuesSearch[0][fieldNameIndex]) && Properties.Contains(FieldsValuesSearch[0][fieldNameIndex]))
                {
                    foreach (AirlineObject airlineObject in AirlineObjects)
                    {
                        if (airlineObject != null)
                        {
                            var airlineObjectPropertyVal = airlineObject[FieldsValuesSearch[0][fieldNameIndex]];
                            ConditionalTypes conditionalType = (ConditionalTypes)Enum.Parse(typeof(ConditionalTypes), FieldsValuesSearch[0][fieldConditionIndex]);

                            if (Search(conditionalType, FieldsValuesSearch[0][fieldValueIndex], airlineObjectPropertyVal))
                            {
                                if (++indexOfAirlineObjects > CurrentAirlineObjects.Length - 1)
                                    Array.Resize(ref CurrentAirlineObjects, CurrentAirlineObjects.Length * 2);
                                CurrentAirlineObjects[indexOfAirlineObjects] = airlineObject;

                            }
                        }
                    }
                }
            }
            catch { }

            return CurrentAirlineObjects.Count(arg => arg != null) > 0;
        }

        /// <summary>
        /// Does search in airline object itself based on field name and it's value 
        /// </summary>
        /// <param name="conditionalType">type of comparsion</param>
        /// <param name="searchValue">value for search</param>
        /// <param name="compareValue">original value</param>
        /// <returns>Positive if found</returns>
        private bool Search(ConditionalTypes conditionalType, object searchValue, object compareValue)
        {
            if (compareValue.GetType().IsEnum)
            {
                return compareValue.ToString() == searchValue.ToString();
            }
            else {

                switch (Type.GetTypeCode(compareValue.GetType()))
                {
                    case TypeCode.Int32:
                        {
                            switch (conditionalType)
                            {
                                case ConditionalTypes.eq:
                                    return (int)searchValue == (int)compareValue;
                                case ConditionalTypes.gt:
                                    return (int)searchValue > (int)compareValue;
                                case ConditionalTypes.lt:
                                    return (int)searchValue < (int)compareValue;
                            }
                        }
                        break;
                    case TypeCode.String:
                        {
                            switch (conditionalType)
                            {
                                case ConditionalTypes.eq:
                                    return (string.Compare((string)searchValue, (string)compareValue, false) == 0) || (((string)compareValue).Contains((string)searchValue));
                                case ConditionalTypes.gt:
                                    return string.Compare((string)searchValue, (string)compareValue, false) < 0;
                                case ConditionalTypes.lt:
                                    return string.Compare((string)searchValue, (string)compareValue, false) > 0;
                            }
                        }
                        break;
                    case TypeCode.DateTime:
                        {
                            switch (conditionalType)
                            {
                                case ConditionalTypes.eq:
                                    return DateTime.Compare(Convert.ToDateTime(searchValue), (DateTime)compareValue) == 0;
                                case ConditionalTypes.gt:
                                    return DateTime.Compare(Convert.ToDateTime(searchValue), (DateTime)compareValue) > 0;
                                case ConditionalTypes.lt:
                                    return DateTime.Compare(Convert.ToDateTime(searchValue), (DateTime)compareValue) < 0;
                            }
                        }
                        break;
                }
            }
            return false;
        }

        protected abstract bool CanBeResized();

        /// <summary>
        /// Adds the airline object to array
        /// </summary>
        /// <param name="FieldsValues">Airline object's fields and their values</param>
        /// <param name="airlineObject">New Airline object for a collection</param>
        /// <param name="airlineObjects">New Airline object's list for a main object if it's a manager</param>
        /// <returns>Positive if added</returns>
        public virtual bool Add(string[] FieldsValues, AirlineObject airlineObject, AirlineObject[] airlineObjects)
        {
            var FieldsValuesUpdate = GetAirlineObjectInfo(FieldsValues);
            var index = AirlineObjects.Count(arg => arg != null);
            if (index > -1)
            {

                if (index >= AirlineObjects.Length)
                {
                    if (CanBeResized())
                    {
                        var tmpAirlineObjects = AirlineObjects;
                        Array.Resize(ref tmpAirlineObjects, AirlineObjects.Length * 2);
                        AirlineObjects = tmpAirlineObjects;
                    }
                    else {
                        OnDisplayInfoChanged(new AirlineObjectEventArgs { HasError = true, ConsoleColor = ConsoleColor.Red, DisplayInfo = "Can't add an object, max size reached" });
                        return false;
                    }
                }

                try
                {
                    foreach (string[] fieldsValues in FieldsValuesUpdate)
                    {
                        if (fieldsValues[fieldNameIndex] == "Count")
                        {
                            var airlineManager = airlineObject as AirlineManager;
                            if (airlineManager != null)
                            {
                                int airlineObjectsCount = 0;
                                if (int.TryParse(fieldsValues[fieldValueIndex], out airlineObjectsCount))
                                    Array.Resize(ref airlineObjects, airlineObjectsCount);
                                airlineManager.AirlineObjects = airlineObjects;
                            }
                            continue;
                        }
                        if (Properties.Contains(fieldsValues[fieldNameIndex]))
                        {
                            var originalAirlineObject = airlineObject;
                            var isAlreadyProcessed = false;
                        secondChance:
                            try
                            {
                                if (airlineObject[fieldsValues[fieldNameIndex]].GetType().IsEnum)
                                {
                                    try
                                    {
                                        var enumType = airlineObject[fieldsValues[fieldNameIndex]].GetType();
                                        var enumString = char.ToUpper(fieldsValues[fieldValueIndex][0]) + fieldsValues[fieldValueIndex].Substring(1).ToLower();
                                        airlineObject[fieldsValues[fieldNameIndex]] = Enum.Parse(enumType, enumString);
                                    }
                                    catch { }
                                }
                                else {

                                    switch (Type.GetTypeCode(airlineObject[fieldsValues[fieldNameIndex]].GetType()))
                                    {
                                        case TypeCode.Int32:
                                            try
                                            {
                                                airlineObject[fieldsValues[fieldNameIndex]] = Convert.ToInt32(fieldsValues[fieldValueIndex]);
                                            }
                                            catch { }
                                            break;
                                        case TypeCode.Decimal:
                                            try
                                            {
                                                airlineObject[fieldsValues[fieldNameIndex]] = Convert.ToDecimal(fieldsValues[fieldValueIndex]);
                                            }
                                            catch { }
                                            break;
                                        case TypeCode.String:
                                            try
                                            {
                                                airlineObject[fieldsValues[fieldNameIndex]] = fieldsValues[fieldValueIndex];
                                            }
                                            catch { }
                                            break;
                                        case TypeCode.DateTime:
                                            try
                                            {
                                                airlineObject[fieldsValues[fieldNameIndex]] = Convert.ToDateTime(fieldsValues[fieldValueIndex]);
                                            }
                                            catch { }
                                            break;
                                    }
                                }
                            }
                            catch
                            {
                                if (!isAlreadyProcessed)
                                {
                                    isAlreadyProcessed = true;
                                    var passanger = airlineObject as Passenger;
                                    if (passanger != null)
                                    {
                                        airlineObject = passanger.Ticket;
                                        goto secondChance;
                                    }
                                }
                            }
                            airlineObject = originalAirlineObject;
                        }
                    }

                    if (airlineObject.IsValid())
                    {
                        AirlineObjects[index] = airlineObject;
                        return true;
                    }
                }
                catch (Exception ex) { }
            }
            return false;
        }

        private static string[][] GetAirlineObjectInfo(string[] FieldsValues)
        {
            string[][] updateInfo = new string[FieldsValues.Length][];
            string[] updateLine = null;
            int index = 0;
            do
            {
                updateLine = GetFieldAndValues(FieldsValues[index]);
                if (updateLine.Length == 3)
                    updateInfo[index] = updateLine;
                else
                    break;
                index++;

            } while (index < FieldsValues.Length);

            return updateInfo.Where(arg => arg != null).ToArray();
        }

        private static string[] GetFieldAndValues(string searchString)
        {
            const int searchArraySize = 3;
            string[] searchCriteria = new string[searchArraySize];

            var searchArray = searchString.Trim().Split(' ');

            if (searchArray.Length > 3)
            {
                searchArray[2] = string.Join(" ", searchArray.Where((key, arg) => Convert.ToInt32(arg) > 1).ToArray());
            }

            if (searchArray.Length > 2)
            {
                for (int i = 0; i < searchArraySize; i++)
                {
                    searchCriteria[i] = searchArray[i].Trim();
                }
            }

            return searchCriteria.Where(arg => arg != null).ToArray();
        }

        /// <summary>
        /// Parse the option values and transfers them to update
        /// </summary>
        /// <param name="FieldsValues">Fields and their values to update</param>
        /// <returns>Positive if updated</returns>
        public virtual bool Edit(string[] FieldsValues)
        {
            var FieldsValuesUpdate = GetAirlineObjectInfo(FieldsValues);

            if (FieldsValuesUpdate.Length > 0 && FieldsValuesUpdate[0][fieldNameIndex] == "ID")
            {
                var id = 0;
                if (int.TryParse(FieldsValuesUpdate[0][fieldValueIndex], out id))
                {
                    if (id > 0 && id < AirlineObjects.Length + 1)
                    {
                        id -= 1;
                        return Update(id, FieldsValuesUpdate.Where(arg => arg[fieldNameIndex] != "ID").ToArray());
                    }
                    else
                        OnDisplayInfoChanged(new AirlineObjectEventArgs { HasError = true, ConsoleColor = ConsoleColor.Red, DisplayInfo = "This Id doesn't exist, can't select the object to edit" });
                }
                else
                    OnDisplayInfoChanged(new AirlineObjectEventArgs { HasError = true, ConsoleColor = ConsoleColor.Red, DisplayInfo = "Wrong id value provided" });
            }
            else
                OnDisplayInfoChanged(new AirlineObjectEventArgs { HasError = true, ConsoleColor = ConsoleColor.Red, DisplayInfo = "The id doesn't exist, can't select the object to edit" });

            return false;
        }

        /// <summary>
        /// Updates the airline object
        /// </summary>
        /// <param name="objectId">Airline object for update</param>
        /// <param name="updateValues">Fields and their values to update</param>
        /// <returns>Positive if updated</returns>
        private bool Update(int objectId, string[][] updateValues)
        {
            var airlineObject = CurrentAirlineObjects[objectId];
            if (airlineObject == null)
                return false;
            try
            {
                foreach (string[] updateValue in updateValues)
                {
                    if (Properties.Contains(updateValue[fieldNameIndex]))
                    {
                        var originalAirlineObject = airlineObject;
                        var isAlreadyProcessed = false;
                    secondChance:
                        try
                        {

                            if (airlineObject[updateValue[fieldNameIndex]].GetType().IsEnum)
                            {
                                try
                                {
                                    var enumType = airlineObject[updateValue[fieldNameIndex]].GetType();
                                    var enumString = char.ToUpper(updateValue[fieldValueIndex][0]) + updateValue[fieldValueIndex].Substring(1).ToLower();
                                    airlineObject[updateValue[fieldNameIndex]] = Enum.Parse(enumType, enumString);
                                }
                                catch { }
                            }
                            else {
                                switch (Type.GetTypeCode(airlineObject[updateValue[fieldNameIndex]].GetType()))
                                {
                                    case TypeCode.Int32:
                                        try
                                        {
                                            airlineObject[updateValue[fieldNameIndex]] = Convert.ToInt32(updateValue[fieldValueIndex]);
                                        }
                                        catch { }
                                        break;
                                    case TypeCode.String:
                                        try
                                        {
                                            airlineObject[updateValue[fieldNameIndex]] = updateValue[fieldValueIndex];
                                        }
                                        catch { }
                                        break;
                                    case TypeCode.DateTime:
                                        try
                                        {
                                            airlineObject[updateValue[fieldNameIndex]] = Convert.ToDateTime(updateValue[fieldValueIndex]);
                                        }
                                        catch { }
                                        break;
                                }
                            }
                        }
                        catch
                        {
                            if (!isAlreadyProcessed)
                            {
                                isAlreadyProcessed = true;
                                var passanger = airlineObject as Passenger;
                                if (passanger != null)
                                {
                                    airlineObject = passanger.Ticket;
                                    goto secondChance;
                                }
                            }
                        }
                        airlineObject = originalAirlineObject;
                    }
                }

                if (airlineObject.IsValid())
                {
                    objectId = Array.FindIndex(AirlineObjects, (arg) => arg.ID == airlineObject.ID);
                    if (objectId > -1)
                    {
                        AirlineObjects[objectId] = airlineObject;
                        return true;
                    }
                    else
                        throw new Exception("Can't find the airline object using the indeex provided");
                }
            }
            catch (Exception ex)
            {
                OnDisplayInfoChanged(new AirlineObjectEventArgs { HasError = true, ConsoleColor = ConsoleColor.Red, DisplayInfo = $"Error during update, message: {ex.Message}" });
            }

            return false;
        }

        public bool NeedOption { get; protected set; }
        public bool MultipleOption { get; protected set; }
        public int IndexOfCurrentAirlineManager { get; protected set; }

        public IAirlineManager CurrentAirlineManager { get; protected set; }
    }

    class AirlineManagerContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty prop = base.CreateProperty(member, memberSerialization);

            if (member.DeclaringType == typeof(AirlineManager) && prop.PropertyName == "AirlineObjects")
            {
                prop.Writable = true;
            }

            return prop;
        }
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BA.Airline.AP.Airline;
using BA.Airline.AP.Airline.Models;

namespace BA.Airline.AP.UnitTest
{
    [TestClass]
    public class ViewUnitTest : AirlineTestInitializor
    {
        IAirlineManager _AirlineManager;

        [TestInitialize]
        public void TestInitialize() {
            _AirlineManager = GetManager();
            _AirlineManager.DisplayInfoChanged += DisplayInfoChanged;
        }

        [TestMethod]
        public void RightOptionSelectionTest() {
            var option = "10";
            _AirlineManager.SelectOptions(option);
            Assert.IsTrue(!HasError);
        }

        [TestMethod]
        public void AddNewFlight() {

            var fieldsValues = new string[]
            {
                "Arrival eq 13.12.2004 04:10:06",
                "Departure eq 13.10.2004 04:10:06",
                "Airline eq test airline",
                "ArrivalCity eq Kharkiv",
                "DepartureCity eq NY",
                "Number eq 777",
                "Gate eq test T3",
                "Terminal eq E2"
            };

            Assert.IsTrue(_AirlineManager.Add(fieldsValues, new Flight(), new Passenger[0]));
        }

        [TestMethod]
        public void AddWrongFlight() {

            //var fieldsValues = new string[] { "" };

            var fieldsValues = new string[]
            {
                "Number eq 777",
                "Gate eq test T3",
                "Terminal eq E2"
             };

            Assert.IsFalse(_AirlineManager.Add(fieldsValues, new Flight(), new Passenger[0]));
        }

        private void DisplayInfoChanged(object sender, AirlineObjectEventArgs e) {
            HasError = e.HasError;
        }
    }
}

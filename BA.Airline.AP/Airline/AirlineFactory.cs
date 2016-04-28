using BA.Airline.AP.Airline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.Airline.AP.Airline
{
    static class AirlineFactory
    {
        public static IAirlineManager Create(string[] args) {
            var countAirlineObjects = 0;
            if (args != null && args.Length > 0)
                int.TryParse(args[0], out countAirlineObjects);
            return new CityAirport(countAirlineObjects);
        }
    }
}

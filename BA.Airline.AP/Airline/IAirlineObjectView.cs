using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.Airline.AP.Airline
{
    public interface IAirlineObjectView
    {

        string[] TableHeader { get; }

        string [,] TableView { get; }

        string Show();
    }
}

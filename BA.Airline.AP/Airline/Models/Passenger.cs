using BA.Airline.AP.Airline.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.Airline.AP.Airline.Models
{
    class Passenger : AirlineObject
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Passport { get; set; }
        public DateTime Birthday { get; set; }
        public Sex Sex { get; set; }
        public Ticket Ticket { get; set; }

        public Passenger()
        {
            ID = Guid.NewGuid();
            FirstName = string.Empty;
            LastName = string.Empty;
            Passport = string.Empty;
            Birthday = DateTime.Now;
            Sex = Sex.Female;
            Ticket = new Ticket { Cost = 100m, Type = TicketType.Economy };            
        }


        public override string[] TableHeader
        {
            get
            {
                return new string[0];
            }
        }

        public override string[,] TableView
        {
            get
            {
                return new string[0, 0];
            }
        }

        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(FirstName) &&
                   !string.IsNullOrWhiteSpace(LastName) &&
                   !string.IsNullOrWhiteSpace(Passport);
        }
    }
}

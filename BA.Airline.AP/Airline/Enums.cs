namespace BA.Airline.AP.Airline
{
    enum TicketType
    {
        Business,
        Economy
    }

    enum FlightStatus
    {
        CheckIn,
        GateClosed,
        Arrived,
        DepartedAt,
        Unknown,
        Canceled,
        ExpectedAt,
        Delayed,
        InFlight
    }

    enum FlightType
    {
        Arrival,
        Departure
    }

    enum AirlineOptions
    {
        NULL,
        ShowAllFlights,
        ShowArrivals,
        ShowDepartues,
        SearchFlights,
        AddAFlight,
        ClearTheConsole,
        Info,
        ExitOrLevelUp,
        EditTheFlight,
        DeleteTheFlight,
        EditPassangersOfTheFlight,

        ShowPassangers,
        SearchPassangers,
        AddAPassanger,
        EditThePassanger,
        DeleteThePassanger,

        LoadFromFile,
        SaveToFile
    }

    enum ConditionalTypes
    {
        eq,
        lt,
        gt
    }
    enum Sex
    {
        Male,
        Female
    }
}

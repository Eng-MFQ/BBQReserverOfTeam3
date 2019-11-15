using System.Collections.Generic;
using BBQReserverBot.Model.Entities;

namespace BBQReserverBot.Model
{
    public class Schedule
    {
        static Schedule()
        {
            Records = new List<Record>();
        }

        public static bool CreateRecord(Record record)
        {
            Records.Add(record);
            return true;
        }
        public static List<Record> Records { get; set; }
    }
}

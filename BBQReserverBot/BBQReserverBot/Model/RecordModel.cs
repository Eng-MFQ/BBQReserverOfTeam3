using System;
using System.Collections.Generic;
using System.Linq;
using BBQReserverBot.Controllers;
using BBQReserverBot.Model.Entities;
using Telegram.Bot.Types;

namespace BBQReserverBot.Model
{
    public class RecordModel
    {
        public static Record CreateRecord(User user, int selectedDay, int selectedMonth, int selectedStart,
            int selectedEnd)
        {
            var record = new Record(user, selectedDay, selectedMonth, selectedStart, selectedEnd);
            return record;
        }

        public static bool CreateRecord(User user, int selectedDay, int selectedMonth, int selectedStart,
            int selectedEnd, bool checkAndWrite)
        {
            if (!checkAndWrite) return false;
            try
            {
                var record = CreateRecord(user, selectedDay, selectedMonth, selectedStart, selectedEnd);
                if (CheckForTimeIntersections(record)) throw new ArgumentException("Date already taken");
                if (RecordIsInPast(record)) throw new ArgumentException("Date lies in past");

                return WriteRecord(record);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        private static bool WriteRecord(Record record)
        {
            var fromTime = record.FromTime.ToString("yyyy-MM-dd HH:mm:ss.sss");
            var toTime = record.ToTime.ToString("yyyy-MM-dd HH:mm:ss.sss");
            var command = "insert into records (fromTime, toTime, userID) values ('"+fromTime+"','"+toTime+"', '"+record.User.Id+"')";
            DatabaseController.ExecuteCommand(command);
            return true;
        }

        public static bool DeleteRecord(Record record, int requestingUserId)
        {
            if (!DatabaseController.RecordExists(record)) return false;
            if (record.User.Id != requestingUserId) return false;
            var command = "delete from records where id = " + record.Id;
            DatabaseController.ExecuteCommand(command);
            return true;
        }

        public static bool UpdateRecord(Record oldRecord, User user, int newStart, int newEnd)
        {
            if (user.Id != oldRecord.User.Id) return false;
            var day = oldRecord.FromTime.Day;
            var month = oldRecord.FromTime.Month;
            try
            {
                var record = CreateRecord(user, day, month, newStart, newEnd);
                if (CheckForTimeIntersectionsExcept(record, oldRecord))
                    throw new ArgumentException("Change not possible, overlapping");
                DeleteRecord(oldRecord, user.Id);
                WriteRecord(record);
                return true;
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public static Record FindRecordByUserInputString(string text)
        {
            foreach (var record in RecordModel.GetAllRecords())
            {
                if (text.Equals(record.FromTime.ToString("dd MMMM, HH:mm") + "—" + record.ToTime.Hour + ":00"))
                    return record;
            }

            return null;
        }

        public static List<Record> GetAllRecords()
        {
            return DatabaseController.GetRecords();
        }

        private static bool CheckForTimeIntersections(Record newRecord)
        {
            var intersections = from records in GetAllRecords()
                where records.FromTime < newRecord.ToTime && records.ToTime > newRecord.FromTime
                select records;
            return intersections.Any();
        }

        private static bool CheckForTimeIntersectionsExcept(Record newRecord, Record exceptRecord)
        {
            var intersections = from records in GetAllRecords()
                where records.FromTime < newRecord.ToTime && records.ToTime > newRecord.FromTime
                select records;
            intersections = from records in intersections
                where records.Id != exceptRecord.Id
                select records;
            return intersections.Any();
        }

        private static bool RecordIsInPast(Record record)
        {
            var now = DateTime.Now;
            //record is before now, return true
            return DateTime.Compare(now, record.FromTime) >= 0;
        }
    }
}
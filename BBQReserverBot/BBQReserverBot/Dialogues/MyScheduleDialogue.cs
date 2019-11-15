using BBQReserverBot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace BBQReserverBot.Dialogues
{
    public class MyScheduleDialogue:AbstractDialogue
    {
        public MyScheduleDialogue(Func<string, IReplyMarkup, Task<bool>> onMessage) : base(onMessage) { }
        public override Task<AbstractDialogue> OnMessage(MessageEventArgs args)
        {
            switch(args.Message.Text)
            {
                case "My schedule":
                    break;
                case "Daily":
                    break;
                case "Weekly":
                    break;
                case "Monthly":
                    break;
            }
            throw new NotImplementedException();
        }

        private string createSchedule(DateTime start, DateTime end)
        {
            var result = new StringBuilder();
            var recordsByDay = (from record in RecordModel.GetAllRecords() where record.FromTime < end && record.ToTime > start select record).GroupBy((record)=>record.FromTime);
            for(DateTime i = start; i != end; i.AddDays(1))
            {
                string DaySchedule = i.ToString("DDDD dd: ");
                var records =( from day in recordsByDay where day.Key == i select day).FirstOrDefault().OrderBy(t => t.FromTime).ToList();
                if(records.Any())
                {
                    if ((records.First()).FromTime.Hour == 8)
                    {
                        DaySchedule =  records.First().FromTime.ToString("hh:mm") ;
                    }
                    else
                    {
                        DaySchedule += $" 08:00 - { records.First().FromTime.ToString("hh:mm")}\n";
                        DaySchedule += $"{ records.First().ToTime.ToString("hh:mm")}";
                    }

                    if (records.Count > 1)
                    {
                        foreach (var record in records.Skip(1).Take(records.Count - 2))
                        {
                            DaySchedule += $" - { record.FromTime.ToString("hh:mm")}\n";
                            DaySchedule += $"{ record.ToTime.ToString("hh:mm")}";
                        }
                        DaySchedule += $" - { records.Last().FromTime.ToString("hh:mm")}\n";
                        if(records.Last().ToTime.Hour < 21)
                        {
                            DaySchedule += $"{ records.Last().ToTime.ToString("hh:mm")} - 21:00";
                        }
                    }
                    else
                    {
                        DaySchedule += $"21:00";
                    }
                }

                result.Append(DaySchedule);
                result.Append("\n-----------\n");
            }
            return result.ToString();
        }


    }
}

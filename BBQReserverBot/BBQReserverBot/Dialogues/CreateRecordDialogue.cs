﻿using BBQReserverBot.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace BBQReserverBot.Dialogues
{
    /// <summary>
    ///   US002 continues here after the Main Menu
    /// </summary>
    public class CreateRecordDialogue : AbstractDialogue
    {


        private Dictionary<string, int> Months = new Dictionary<string, int>()
        {
            {"January", 1},
            {"February", 2},
            {"March", 3},
            {"April", 4},
            {"May", 5},
            {"Jun", 6},
            {"July", 7},
            {"August", 8},
            {"September", 9},
            {"October", 10},
            {"November", 11},
            {"December", 12}
        };

        private Dictionary<string, bool> Approves = new Dictionary<string, bool>()
        {
            {"Approve", true},
            {"I've changed my mind", false}
        };

        enum State
        {
            Month,
            Day,
            Start,
            End,
            Approve,
            Success,
            Fail
        }

        private State CurrentState;

        public CreateRecordDialogue(Func<string, IReplyMarkup, Task<bool>> onMessage) : base(onMessage)
        {
            CurrentState = State.Month;
        }

        private Dictionary<string, int> Times = Enumerable
            .Range(8, 15)
            .ToDictionary(x => String.Concat(x, ":00"),
                          x => x);

        private ReplyKeyboardMarkup MakeTimeKeyboard()
        {
            return (ReplyKeyboardMarkup)
                Times
                .Keys
                .Select(x => new []{x})
                .ToArray();
        }

        private (string, IReplyMarkup) Questions(State state)
        {
            switch(state)
            {
                case State.Month:
                    return ("Select the month",
                            (IReplyMarkup)
                            (ReplyKeyboardMarkup)
                            Months.Keys.Select(x => new []{x}).ToArray());
                case State.Day:
                    return ("Select the day. Enter the number.",
                            (IReplyMarkup)
                            new ReplyKeyboardRemove());
                case State.Start:
                    return ("Select the time when you want to start",
                            (IReplyMarkup)
                            MakeTimeKeyboard());
                case State.End:
                    return ("Select the time when you want to stop",
                            (IReplyMarkup)
                            MakeTimeKeyboard());
                case State.Approve:
                    return ("Great! Just approve your reservation and that's it!",
                            (IReplyMarkup)
                            (ReplyKeyboardMarkup)
                            Approves.Keys.Select(x => new[]{x}).ToArray());
                default: throw new NotImplementedException();
            }
        }


        public void ProcessMonth(string text)
        {
            var x = Months.ContainsKey(text) ? (int?) Months[text] : null;
            if (x != null)
            {
                SelectedMonth = (int) x;
                CurrentState = State.Day;
            }
        }

        public void ProcessDay(string text)
        {
            var last = DateTime.DaysInMonth(DateTime.Now.Year, SelectedMonth);
            var x = 0;
            Int32.TryParse(text, out x);
            if (x >= 1 && x <= last)
            {
                SelectedDay = x;
                CurrentState = State.Start;
            }
        }

        public void ProcessTime(string text, bool isStart)
        {
            var x = Times.ContainsKey(text) ? (int?) Times[text] : null;
            if (x != null)
            {
                if (isStart)
                {
                    SelectedStart = (int) x;
                    CurrentState = State.End;
                }
                else
                {
                    SelectedEnd = (int) x;
                    CurrentState = State.Approve;
                }
            }
        }

        public void ProcessApprove(string text)
        {
            var x = Approves.ContainsKey(text) ? (bool?) Approves[text] : null;
            if (x != null)
            {
                SelectedApproved = (bool) x;
                CurrentState = SelectedApproved ? State.Success : State.Fail;
            }
        }

        private int SelectedMonth;
        private int SelectedDay;
        private int SelectedStart;
        private int SelectedEnd;
        private bool SelectedApproved;

        public void Process(MessageEventArgs args)
        {
            string text = args.Message.Text;
            switch(CurrentState)
            {
                case(State.Month): ProcessMonth(text); break;
                case(State.Day): ProcessDay(text); break;
                case(State.Start): ProcessTime(text, true); break;
                case(State.End): ProcessTime(text, false); break;
                case(State.Approve): ProcessApprove(text); break;
                default: throw new NotImplementedException();
            }
        }

        public async void SendQuestion()
        {
            var msg = Questions(CurrentState);
            await _sendMessege(msg.Item1, msg.Item2);
        }

        private bool CheckTimeInterval(BBQReserverBot.Model.Record record)
        {
            var interseptions = from r in Schedule.Records
                where r.FromTime < record.ToTime && r.ToTime > record.FromTime
                select r;
            return interseptions.Count() > 0;
        }

        public bool Create(Telegram.Bot.Types.User user)
        {
            BBQReserverBot.Model.Record r = new BBQReserverBot.Model.Record();
            r.Id = Guid.NewGuid();
            r.User = user;
            r.FromTime = new DateTime(DateTime.Now.Year, SelectedMonth, SelectedDay, SelectedStart, 0, 0);
            r.ToTime = new DateTime(DateTime.Now.Year, SelectedMonth, SelectedDay, SelectedEnd, 0, 0);
            if (r.FromTime < r.ToTime)
            {
                r.ToTime.AddDays(1);
            }
            if (r.FromTime < DateTime.Now)
            {
                r.FromTime.AddYears(1);
                r.ToTime.AddYears(1);
            }
            if (CheckTimeInterval(r))
            {
                return false;
            }
            Schedule.Records.Add(r);
            return true;
        }

        public async override Task<AbstractDialogue> OnMessage(MessageEventArgs args)
        {

            Process(args);

            if (CurrentState == State.Success)
            {
                bool success = Create(args.Message.From);
                if (success)
                {
                    await _sendMessege("Yay! Your reservation is approved. Have a great BBQing!",
                                 new ReplyKeyboardRemove());
                }
                else
                {
                    await _sendMessege("There is already a reservation at that time. Maybe you can join them))",
                                 new ReplyKeyboardRemove());
                }
                var md = new MainMenuDialogue(_sendMessege);
                await md.OnMessage(args);
                return md;
            }

            if (CurrentState == State.Fail)
            {
                await _sendMessege("Ok!)", new ReplyKeyboardRemove());
                var md = new MainMenuDialogue(_sendMessege);
                await md.OnMessage(args);
                return md;
            }

            SendQuestion();
            return this;
        }
    }
}

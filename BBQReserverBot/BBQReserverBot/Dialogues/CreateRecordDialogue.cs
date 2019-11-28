using BBQReserverBot.Model;
using System;
using System.Collections.Generic;
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
        private int SelectedMonth;
        private int SelectedDay;
        private int SelectedStart;
        private int SelectedEnd;
        private bool SelectedApproved;
        private State CurrentState;


        private readonly Dictionary<string, int> Months = new Dictionary<string, int>()
        {
            {"January", 1},
            {"February", 2},
            {"March", 3},
            {"April", 4},
            {"May", 5},
            {"June", 6},
            {"July", 7},
            {"August", 8},
            {"September", 9},
            {"October", 10},
            {"November", 11},
            {"December", 12}
        };

        private readonly Dictionary<string, bool> Approves = new Dictionary<string, bool>()
        {
            {"Approve", true},
            {"I've changed my mind", false}
        };

        private enum State
        {
            Month,
            Day,
            Start,
            End,
            Approve,
            Success,
            Fail
        }


        public CreateRecordDialogue(Func<string, IReplyMarkup, Task<bool>> onMessage) : base(onMessage)
        {
            CurrentState = State.Month;
        }

        public static readonly Dictionary<string, int> Times = Enumerable
            .Range(8, 15)
            .ToDictionary(x => String.Concat(x, ":00"),
                x => x);

        public static ReplyKeyboardMarkup MakeTimeKeyboard()
        {
            return (ReplyKeyboardMarkup)
                Times
                    .Keys
                    .Select(x => new[] {x})
                    .ToArray();
        }

        private (string, IReplyMarkup) Questions(State state)
        {
            switch (state)
            {
                case State.Month:
                    return ("Select the month",
                        (IReplyMarkup)
                        (ReplyKeyboardMarkup)
                        Months.Keys.Select(x => new[] {x}).ToArray());
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
                        Approves.Keys.Select(x => new[] {x}).ToArray());
                default: throw new NotImplementedException();
            }
        }


        public void ProcessMonth(string text)
        {
            var x = Months.ContainsKey(text) ? (int?) Months[text] : null;
            if (x == null) return;
            SelectedMonth = (int) x;
            CurrentState = State.Day;
        }

        public void ProcessDay(string text)
        {
            var last = DateTime.DaysInMonth(DateTime.Now.Year, SelectedMonth);
            var x = 0;
            Int32.TryParse(text, out x);
            if (x < 1 || x > last) return;
            SelectedDay = x;
            CurrentState = State.Start;
        }

        public void ProcessTime(string text, bool isStart)
        {
            var x = Times.ContainsKey(text) ? (int?) Times[text] : throw new ArgumentOutOfRangeException("Can't start before 8:00 or end after 22:00");
            if (x == null) return;
            if (isStart)
            {
                SelectedStart = (int) x;
                CurrentState = State.End;
            }
            else
            {
                SelectedEnd = (int) x;
                if (SelectedStart != SelectedEnd)
                    CurrentState = State.Approve;
            }
        }

        public void ProcessApprove(string text)
        {
            var x = Approves.ContainsKey(text) ? (bool?) Approves[text] : null;
            if (x == null) return;
            SelectedApproved = (bool) x;
            CurrentState = SelectedApproved ? State.Success : State.Fail;
        }

        private void Process(MessageEventArgs args)
        {
            var text = args.Message.Text;
            switch (CurrentState)
            {
                case (State.Month):
                    ProcessMonth(text);
                    break;
                case (State.Day):
                    ProcessDay(text);
                    break;
                case (State.Start):
                    ProcessTime(text, true);
                    break;
                case (State.End):
                    ProcessTime(text, false);
                    break;
                case (State.Approve):
                    ProcessApprove(text);
                    break;
                default: throw new NotImplementedException();
            }
        }

        public async void SendQuestion()
        {
            var msg = Questions(CurrentState);
            await _sendMessege(msg.Item1, msg.Item2);
        }

        public bool Create(Telegram.Bot.Types.User user)
        { 
            return RecordModel.CreateRecord(user, SelectedDay, SelectedMonth, SelectedStart, SelectedEnd, true);
        }

        public override async Task<AbstractDialogue> OnMessage(MessageEventArgs args)
        {
            Process(args);

            if (CurrentState == State.Success)
            {
                bool success = Create(args.Message.From);
                if (success)
                {
                    await _sendMessege("Yay! Your reservation is approved. Have a great BBQing!",
                        MainMenuDialogue.getMainMenuKeyboard());
                }
                else
                {
                    await _sendMessege("There is already a reservation at that time. Maybe you can join them))",
                        MainMenuDialogue.getMainMenuKeyboard());
                }

                var md = new MainMenuDialogue(_sendMessege);
                await md.OnMessage(args);
                return md;
            }

            if (CurrentState == State.Fail)
            {
                await _sendMessege("Something went wrong!)", MainMenuDialogue.getMainMenuKeyboard());
                var md = new MainMenuDialogue(_sendMessege);
                await md.OnMessage(args);
                return md;
            }

            SendQuestion();
            return this;
        }
    }
}
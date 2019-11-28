using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;
using BBQReserverBot.Model;

namespace BBQReserverBot.Dialogues
{
    /// <summary>
    ///   Use Cases US001, US002, US003, US004, US005 start from the Main Menu.
    /// </summary>
    public class MainMenuDialogue : AbstractDialogue
    {

        public static ReplyKeyboardMarkup getMainMenuKeyboard()
        {
            return new[]
            {
                "Create a new reservation",
                "Update or Remove an existing reservation",
                "My schedule"
            };
        }
        public MainMenuDialogue(Func<string, IReplyMarkup, Task<bool>> sendMessege) : base(sendMessege)
        {
        }

        private static string _menueMessage = "What do you like to do?";

        /// <summary>
        ///   US005 continues here after the Main Menu.
        /// </summary>
        class MyScheduleDialogue2 : AbstractDialogue
        {
            public MyScheduleDialogue2(Func<string, IReplyMarkup, Task<bool>> onMessage) : base(onMessage)
            {
            }

            public override async Task<AbstractDialogue> OnMessage(MessageEventArgs args)
            {
                var records = from record in RecordModel.GetAllRecords()
                    where record.User.Id == args.Message.From.Id
                    select record;
                await _sendMessege("You have the following reservations:",
                    (ReplyKeyboardMarkup)
                    records
                        .Select(x => new[] {x.FromTime.ToString("dd MMMM, HH:mm") + "—" + x.ToTime.Hour + ":00"})
                        .ToArray());

                var md = new MainMenuDialogue(_sendMessege);
                return md;
            }
        }

        public override async Task<AbstractDialogue> OnMessage(MessageEventArgs args)
        {
            var msgText = args.Message.Text;
            switch (msgText)
            {
                case "My schedule":
                {
                    var dialogue = new MyScheduleDialogue2(_sendMessege);
                    return await dialogue.OnMessage(args);
                }
                case "Create a new reservation":
                {
                    var dialogue = new CreateRecordDialogue(_sendMessege);
                    return await dialogue.OnMessage(args);
                }
                case "Update or Remove an existing reservation":
                {
                    var dialogue = new DeleteUpdateRecordDialogue(_sendMessege);
                    return await dialogue.OnMessage(args);
                }
                default:
                {
                    await _sendMessege(_menueMessage, getMainMenuKeyboard());
                    return this;
                }
            }
        }
    }
}
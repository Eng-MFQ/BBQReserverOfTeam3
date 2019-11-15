using System;
using System.Linq;
using System.Threading.Tasks;
using BBQReserverBot.Model;
using BBQReserverBot.Model.Entities;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;


namespace BBQReserverBot.Dialogues
{
    /// <summary>
    ///   US001, US003
    /// </summary>
    public class DeleteUpdateRecordDialogue : AbstractDialogue
    {
        private int _state = 1;
        private Record _record;
        private int _updateStart = 0;
        private int _updateStop = 0;
        public DeleteUpdateRecordDialogue(Func<string, IReplyMarkup, Task<bool>> onMessage) : base(onMessage) { }

        public override async Task<AbstractDialogue> OnMessage(MessageEventArgs args)
        {
            return await messageHandler(args);
        }

        private async Task<AbstractDialogue> messageHandler(MessageEventArgs args)
        {
            var text = args.Message.Text;
            var userId = args.Message.From.Id;
            switch (_state)
            {
                case 1:
                    AskForRecord(userId);
                    _state++;
                    break;
                case 2:
                    _record = RecordModel.FindRecordByUserInputString(text);
                    if (_record == null)
                    {
                        AskForRecord(userId);
                        break;
                    }
                    AskForOption();
                    _state++;
                    break;
                case 3:
                    _state++;
                    if (Operate(text))
                        await _sendMessege("Deletion successful", new ReplyKeyboardRemove());
                    break;
                case 4:
                    ProcessTime(args.Message.Text, true);
                    GetUpdatedTimes("Select the time when you want to finish");
                    _state++;
                    break;
                case 5:
                    ProcessTime(args.Message.Text, false);
                    if (RecordModel.UpdateRecord(_record, _record.User, _updateStart, _updateStop))
                        await _sendMessege("Update successful", new ReplyKeyboardRemove());
                    _state = 99;
                    break;
                case 99:
                    _state = 1; //for some reason, there is no new object created; at least the counter stays; so reset
                    var md = new MainMenuDialogue(_sendMessege);
                    await md.OnMessage(args);
                    return md;
                default:
                    return this;
            }
            return this;
        }

        internal async void AskForRecord(int userId)
        {
            var records = from record in RecordModel.GetAllRecords() where record.User.Id == userId select record;
            await _sendMessege("Select one of your reservations to update or delete:",
                (ReplyKeyboardMarkup)
                records
                    .Select(x => new []{x.FromTime.ToString("dd MMMM, HH:mm") + "—" + x.ToTime.Hour + ":00"})
                    .ToArray());
        }

        private async void AskForOption()
        {
            ReplyKeyboardMarkup markup = new[]
            {
                "Update",
                "Delete"
            };
            await _sendMessege("Do you want to update or to delete your entry?", markup);
        }

        private bool Operate(string text)
        {
            if (text.Equals("Delete"))
            {
                RecordModel.DeleteRecord(_record);
                _state = 99;
                return true;
            }
            if (text.Equals("Update"))
            {
                GetUpdatedTimes("Select the time when you want to start");
                return false;
            }
            return false;
        }

        private async void GetUpdatedTimes(string text)
        {
            await _sendMessege(text, (IReplyMarkup)
               CreateRecordDialogue.MakeTimeKeyboard());
        }
        
        private void ProcessTime(string text, bool isStart)
        {
            var x = CreateRecordDialogue.Times.ContainsKey(text) ? (int?) CreateRecordDialogue.Times[text] : null;
            if (x == null) return;
            if (isStart)
                _updateStart = (int) x;
            else
                _updateStop = (int) x;
        }
    }
}
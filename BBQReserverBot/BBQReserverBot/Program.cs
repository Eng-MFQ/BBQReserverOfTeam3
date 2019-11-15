using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using BBQReserverBot.Controllers;
using BBQReserverBot.Dialogues;
using BBQReserverBot.Model;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace BBQReserverBot
{
    public class Program
    {
        private static ConcurrentDictionary<int, AbstractDialogue> users = new ConcurrentDictionary<int, AbstractDialogue>();
        private static TelegramBotClient Bot;
        public static void Main(string[] args)
        {
            DatabaseController.CreateDatabase();
            Bot = new TelegramBotClient("1041560156:AAHa75a3ywVBanzZhnhkTVH3n475aGKX6mM");
            var me = Bot.GetMeAsync().Result;
            Console.Title = me.Username;

            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            Bot.OnInlineQuery += BotOnInlineQueryReceived;
            Bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            Bot.OnReceiveError += BotOnReceiveError;
            DatabaseController.CreateDatabase();

            Bot.StartReceiving(Array.Empty<UpdateType>());
            Console.WriteLine($"Start listening for @{me.Username}");
            while (true) { Thread.Sleep(100); }
            //Bot.StopReceiving();
            // CreateWebHostBuilder(args).Build().Run();
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            if (!TryFindUser(messageEventArgs, out var user))
            {
                var startDialog = new StartDialogue(async (string msg, IReplyMarkup markup) =>
                {
                    await Bot.SendTextMessageAsync(
                    messageEventArgs.Message.Chat.Id,
                    msg,
                    replyMarkup: markup);
                    return true;
                });
                users.TryAdd(messageEventArgs.Message.From.Id, startDialog);
                startDialog.PrintInitialMessage();
            }
            var dialog = await users[messageEventArgs.Message.From.Id].OnMessage(messageEventArgs);
            users[user.GetValueOrDefault()] = dialog;
        }
        private static bool TryFindUser(MessageEventArgs messageEventArgs, out int? user )
        {
            var us = from u in users.Keys where u == messageEventArgs.Message.From.Id select u;
            if(us.Count() > 0)
            {
                user = us.FirstOrDefault();
                return true;
            }
            else
            {
                user = null;
                return false;
            }
        }
        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var callbackQuery = callbackQueryEventArgs.CallbackQuery;

            await Bot.AnswerCallbackQueryAsync(
                callbackQuery.Id,
                $"Received {callbackQuery.Data}");

            await Bot.SendTextMessageAsync(
                callbackQuery.Message.Chat.Id,
                $"Received {callbackQuery.Data}");
        }

        private static async void BotOnInlineQueryReceived(object sender, InlineQueryEventArgs inlineQueryEventArgs)
        {
            Console.WriteLine($"Received inline query from: {inlineQueryEventArgs.InlineQuery.From.Id}");

            InlineQueryResultBase[] results = {
                new InlineQueryResultLocation(
                    id: "1",
                    latitude: 40.7058316f,
                    longitude: -74.2581888f,
                    title: "New York")   // displayed result
                    {
                        InputMessageContent = new InputLocationMessageContent(
                            latitude: 40.7058316f,
                            longitude: -74.2581888f)    // message if result is selected
                    },

                new InlineQueryResultLocation(
                    id: "2",
                    latitude: 13.1449577f,
                    longitude: 52.507629f,
                    title: "Berlin") // displayed result
                    {
                        InputMessageContent = new InputLocationMessageContent(
                            latitude: 13.1449577f,
                            longitude: 52.507629f)   // message if result is selected
                    }
            };

            await Bot.AnswerInlineQueryAsync(
                inlineQueryEventArgs.InlineQuery.Id,
                results,
                isPersonal: true,
                cacheTime: 0);
        }

        private static void BotOnChosenInlineResultReceived(object sender, ChosenInlineResultEventArgs chosenInlineResultEventArgs)
        {
            Console.WriteLine($"Received inline result: {chosenInlineResultEventArgs.ChosenInlineResult.ResultId}");
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Console.WriteLine("Received error: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message);
        }
    }
}

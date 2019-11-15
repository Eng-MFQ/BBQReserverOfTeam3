using System;
using System.Collections.Generic;
using System.Data.SQLite;
using BBQReserverBot.Model;
using BBQReserverBot.Model.Entities;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http2.HPack;
using Telegram.Bot.Types;

namespace BBQReserverBot.Controllers
{
     public class DatabaseController
     {
         private static string _database = @"URI=file:bbq.db";
         private static SQLiteConnection _connection;
         private static SQLiteCommand _command;
         
         public static void CreateDatabase()
         {
             _connection = new SQLiteConnection(_database);
             _connection.Open();

             _command = new SQLiteCommand(_connection);

             _command.CommandText = @"CREATE TABLE IF NOT EXISTS users(id INTEGER PRIMARY KEY,
                     name TEXT)";
             _command.ExecuteNonQuery();
             
             _command.CommandText = @"CREATE TABLE IF NOT EXISTS records(id INTEGER PRIMARY KEY AUTOINCREMENT,
                    fromTime TIMESTAMP, toTime TIMESTAMP, userID int, FOREIGN KEY (userID) REFERENCES users(id))";
             _command.ExecuteNonQuery();
         }

         public static void DestroyDatabase()
         {
             _command.CommandText = @"delete from records;";
             _command.ExecuteNonQuery();
         }

         public static void ExecuteCommand(String command)
         {
             if (_connection == null)
             {
                 CreateDatabase();
             }
             
             _command.CommandText = command;
             _command.ExecuteNonQuery();
         }

         public static List<Record> GetRecords()
         {
             var connection = new SQLiteConnection(_database);
             connection.Open();
             var command = new SQLiteCommand(connection);
             
            command.CommandText = "select * from records";
             SQLiteDataReader reader = command.ExecuteReader();

             List<Record> recordList = new List<Record>();
             while (reader.Read())
             {
                 var user = new User();
                 user.Id = (int) reader["userID"];
                 DateTime fromTime = (DateTime) reader["fromTime"];
                 DateTime toTime = (DateTime) reader["toTime"];
                 long id = (long) reader["id"];
                 var record = new Record(id, user, fromTime, toTime);
                 recordList.Add(record);
             }
             return recordList;
         }

         public static bool RecordExists(Record record)
         {
             if (record == null) return false;
             if (_connection == null)
             {
                 CreateDatabase();
             }
             var sql = "select * from records where id = " + record.Id;
             _command.CommandText = sql;
             return Convert.ToInt32(_command.ExecuteScalar()) > 1;
         }
     }
 }
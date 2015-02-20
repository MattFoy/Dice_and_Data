using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Diagnostics;

namespace Dice_and_Data
{
    class SQLiteDBWrapper
    {
        private SQLiteConnection connection;
        private string sql;
        private SQLiteCommand command;
        private int SessionID;
        private SQLiteDataReader reader;

        private static SQLiteDBWrapper dbWrapRef = new SQLiteDBWrapper();

        public static SQLiteDBWrapper getReference()
        {
            return dbWrapRef;
        }

        public Boolean RecordRoll(String rollKey, int result)
        {
            sql = "INSERT INTO RollHistory (diceCode, value, session_id) VALUES ('" + rollKey + "', " + result + ", " + SessionID + ")";
            return ExecuteNonQuery() == 1;
        }

        private int ExecuteNonQuery()
        {
            command = new SQLiteCommand(sql, connection);
            return command.ExecuteNonQuery();
        }

        private void ExecuteReader()
        {
            command = new SQLiteCommand(sql, connection);
            reader = command.ExecuteReader();
        }

        private SQLiteDBWrapper()
        {
            if (!System.IO.File.Exists("RollTracker.sqlite"))
            {
                SQLiteConnection.CreateFile("RollTracker.sqlite");
                Trace.WriteLine("SQLite File created.");
            }
            connection = new SQLiteConnection("Data Source=RollTracker.sqlite;Version=3;");
            connection.Open();
            System.Diagnostics.Trace.WriteLine("Connection opened");

            //sql = "DROP TABLE IF EXISTS RollHistory; DROP TABLE IF EXISTS Sessions;";
            //ExecuteNonQuery();

            sql = "CREATE TABLE IF NOT EXISTS RollHistory (id INTEGER PRIMARY KEY, diceCode VARCHAR(20), value INT, session_id INT)";
            ExecuteNonQuery();

            sql = "CREATE TABLE IF NOT EXISTS Sessions (id INTEGER PRIMARY KEY, name VARCHAR(20))";
            ExecuteNonQuery();

            sql = "SELECT * FROM Sessions;";
            ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    Trace.WriteLine("Id: " + reader["id"] + "\tName: " + reader["name"]);
                }
            }

            String sessionName = System.DateTime.Now.ToString("yyyy/M/d HH:mm") + " Dice Session";
            sql = "INSERT INTO Sessions (name) VALUES ('" + sessionName + "');";
            ExecuteNonQuery();

            sql = "SELECT id FROM Sessions WHERE name = '" + sessionName + "';";
            ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    SessionID = Int32.Parse(reader["id"].ToString());
                }
            }
            Trace.WriteLine("SessionID: " + SessionID);
        }
    }
}

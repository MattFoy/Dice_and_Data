using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Diagnostics;

namespace Dice_and_Data.Data
{
    class SQLiteDBWrapper
    {
        private string sql;
        private int SessionID;        
        private SQLiteConnection connection;
        private SQLiteCommand command;
        private SQLiteDataReader reader;
        private static SQLiteDBWrapper dbWrapRef = new SQLiteDBWrapper();

        public static SQLiteDBWrapper getReference()
        {
            return dbWrapRef;
        }

        public Boolean RecordRoll(String rollKey, int result)
        {
            if (rollKey.Length == 0)
            {
                throw new Exception("Cannot record the roll. DicePattern invalid.");
            }
            sql = "INSERT INTO RollHistory (diceCode, value, session_id) VALUES ('" + rollKey + "', " + result + ", " + SessionID + ")";
            return ExecuteNonQuery() == 1;
        }

        public int[] GetRollHistory(String pattern)
        {
            List<int> results = new List<int>();

            sql = "SELECT value FROM RollHistory WHERE diceCode = '" + pattern + "' AND session_id = " + SessionID + ";";
            ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    results.Add(Int32.Parse(reader["value"].ToString()));
                }
            }
            return results.ToArray();
        }

        public List<KeyValuePair<int, string>> GetSessionList()
        {
            List<KeyValuePair<int, string>> result = new List<KeyValuePair<int, string>>();

            sql = "SELECT * FROM Sessions;";
            ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    result.Add(new KeyValuePair<int, string>(Int32.Parse(reader["id"].ToString()), reader["name"].ToString()));
                    //Trace.WriteLine("Id: " + reader["id"] + "\tName: " + reader["name"]);
                }
            }

            return result;
        }

        public void SetSession(KeyValuePair<int, string> sessionRow) {
            SessionID = sessionRow.Key;
        }

        public void CacheRollPattern(String pattern, int min, int max, double mean, double stdDev, String pTableJSON, long calcTime)
        {
            sql = "SELECT 1 FROM CompositeProbabilityDistributions WHERE pattern = '" + pattern + "';";
            ExecuteReader();
            if (reader.HasRows)
            {
                sql = "UPDATE CompositeProbabilityDistributions SET "
                    + "min = " + min + ", max = " + max + ", mean = " 
                    + mean + ", stdDev = " + stdDev + ", pTable = '" + pTableJSON + "', calcTime = " + calcTime
                    + " WHERE pattern = '" + pattern + "';";
                ExecuteNonQuery();
            }
            else
            {
                sql = "INSERT INTO CompositeProbabilityDistributions (pattern, min, max, mean, stdDev, pTable, calcTime) VALUES ('"
                    + pattern + "', " + min + ", " + max + ", " + mean + ", " + stdDev + ", '" + pTableJSON + "', " + calcTime + ");";
                ExecuteNonQuery();
            }
        }

        public RollPartial CheckCache(String pattern)
        {
            sql = "SELECT * FROM CompositeProbabilityDistributions WHERE pattern = '" + pattern + "';";
            ExecuteReader();
            if (reader.HasRows && reader.Read())
            {
                return new RollPartial(Int32.Parse(reader["min"].ToString()), Int32.Parse(reader["max"].ToString()), Double.Parse(reader["mean"].ToString()), 
                    Double.Parse(reader["stdDev"].ToString()), reader["pTable"].ToString(), Int64.Parse(reader["calcTime"].ToString()));
            }
            else
            {
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("^[0-9]+d[0-9]+$");
                if (regex.IsMatch(pattern))
                {
                    String[] split = pattern.Split(new char[] { 'd' }, StringSplitOptions.RemoveEmptyEntries);
                    return CheckCache(Int32.Parse(split[1]), Int32.Parse(split[0]));
                }
                
                return new RollPartial(0, 0, 0, 0, "", 0);
            }
        }

        public RollPartial CheckCache(int diceSides, int diceCount)
        {
            sql = "SELECT * FROM SingleProbabilityDistributions WHERE diceSides = " + diceSides + " AND diceCount = " + diceCount + ";";
            ExecuteReader();
            if (reader.HasRows && reader.Read())
            {
                return new RollPartial(Int32.Parse(reader["min"].ToString()), Int32.Parse(reader["max"].ToString()), Double.Parse(reader["mean"].ToString()), 
                    Double.Parse(reader["stdDev"].ToString()), reader["pTable"].ToString(), Int64.Parse(reader["calcTime"].ToString()));
            }
            else
            {
                return new RollPartial(0, 0, 0, 0, "", 0);
            }
        }

        public void CacheRollPlan(int diceCount, int diceSides, int min, int max, double mean, double stdDev, String pTableJSON, long calcTime)
        {
            sql = "SELECT 1 FROM SingleProbabilityDistributions WHERE diceSides = " + diceSides + " AND diceCount = " + diceCount + ";";
            ExecuteReader();
            if (reader.HasRows && reader.Read())
            {
                sql = "UPDATE SingleProbabilityDistributions SET "
                    + "min = " + min + ", max = " + max + ", mean = "
                    + mean + ", stdDev = " + stdDev + ", pTable = '" + pTableJSON + "', calcTime = " + calcTime
                    + " WHERE diceSides = " + diceSides + " AND diceCount = " + diceCount + ";";
                ExecuteNonQuery();
            }
            else
            {
                sql = "INSERT INTO SingleProbabilityDistributions (diceSides, diceCount, min, max, mean, stdDev, pTable, calcTime) VALUES ('"
                    + diceSides + "', " + diceCount + ", " + min + ", " + max + ", " + mean + ", " + stdDev + ", '" + pTableJSON + "', " + calcTime + ");";
                ExecuteNonQuery();
            }
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

        //Singleton insures this runs once and only once.
        private SQLiteDBWrapper()
        {
            if (System.IO.File.Exists("db.sqlite"))
            {
                //System.IO.File.Delete("db.sqlite"); // for testing
            }
            
            if (!System.IO.File.Exists("db.sqlite"))
            {// CREATE IF NOT EXISTS the file!                
                SQLiteConnection.CreateFile("db.sqlite");
                Trace.WriteLine("SQLite File created.");
            }
            connection = new SQLiteConnection("Data Source=db.sqlite;Version=3;");
            connection.Open();
            System.Diagnostics.Trace.WriteLine("Connection opened");

            sql = "CREATE TABLE IF NOT EXISTS RollHistory (id INTEGER PRIMARY KEY, diceCode VARCHAR(20), value INT, session_id INT)";
            ExecuteNonQuery();

            sql = "CREATE TABLE IF NOT EXISTS Sessions (id INTEGER PRIMARY KEY, name VARCHAR(20))";
            ExecuteNonQuery();

            sql = "CREATE TABLE IF NOT EXISTS CompositeProbabilityDistributions (id INTEGER PRIMARY KEY, pattern VARCHAR(30), min INT, max INT, mean REAL, stdDev REAL, pTable TEXT, calcTime INT)";
            ExecuteNonQuery();

            sql = "CREATE TABLE IF NOT EXISTS SingleProbabilityDistributions (id INTEGER PRIMARY KEY, diceCount INT, diceSides INT, min INT, max INT, mean REAL, stdDev REAL, pTable TEXT, calcTime INT)";
            ExecuteNonQuery();

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

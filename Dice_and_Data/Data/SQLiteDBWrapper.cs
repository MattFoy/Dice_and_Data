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

        public bool RecordRoll(String pattern, int value)
        {
            sql = "INSERT INTO RollHistory (pattern, value, session_id) VALUES ('" + pattern + "', " + value + ", " + SessionID + ")";
            return ExecuteNonQuery() == 1;
        }

        public Dictionary<string,RollHistory> GetRollHistory()
        {
            Dictionary<string, RollHistory> results = new Dictionary<string, RollHistory>();

            sql = "SELECT pattern, value FROM RollHistory WHERE session_id = " + SessionID + ";";
            ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    string pattern = reader["pattern"].ToString();
                    int value = Int32.Parse(reader["value"].ToString());
                    if (!results.ContainsKey(pattern))
                    {
                        results.Add(pattern, new RollHistory());
                    }
                    if (!results[pattern].ContainsKey(value))
                    {
                        results[pattern].Add(value, 1);
                    }
                    else
                    {
                        results[pattern][value]++;
                    }

                }
            }
            return results;
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
                return new RollPartial(reader["pattern"].ToString(), Int32.Parse(reader["min"].ToString()), Int32.Parse(reader["max"].ToString()), Double.Parse(reader["mean"].ToString()), 
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
                
                return new RollPartial("", 0, 0, 0, 0, "", 0);
            }
        }

        public RollPartial CheckCache(int diceSides, int diceCount)
        {
            sql = "SELECT * FROM SingleProbabilityDistributions WHERE diceSides = " + diceSides + " AND diceCount = " + diceCount + ";";
            ExecuteReader();
            if (reader.HasRows && reader.Read())
            {
                return new RollPartial(reader["diceCount"].ToString() + reader["diceSides"].ToString(), Int32.Parse(reader["min"].ToString()), Int32.Parse(reader["max"].ToString()), Double.Parse(reader["mean"].ToString()), 
                    Double.Parse(reader["stdDev"].ToString()), reader["pTable"].ToString(), Int64.Parse(reader["calcTime"].ToString()));
            }
            else
            {
                return new RollPartial("", 0, 0, 0, 0, "", 0);
            }
        }

        public bool VerifyCacheIntegrity()
        {
            int singlesChecked = 0;
            sql = "SELECT * FROM SingleProbabilityDistributions;";
            ExecuteReader();
            while (reader.HasRows && reader.Read())
            {
                singlesChecked++;
                RollPartial rp = new RollPartial(reader["diceCount"].ToString() + reader["diceSides"].ToString(), Int32.Parse(reader["min"].ToString()), Int32.Parse(reader["max"].ToString()), Double.Parse(reader["mean"].ToString()),
                    Double.Parse(reader["stdDev"].ToString()), reader["pTable"].ToString(), Int64.Parse(reader["calcTime"].ToString()));
                if (!rp.IsValid())
                {
                    System.Diagnostics.Trace.WriteLine("Invalid RollPartial for " + rp.pattern);
                    return false;
                }
                else
                {
                    System.Diagnostics.Trace.WriteLine("Valid RollPartial for " + rp.pattern);
                }
            }

            int compositesChecked = 0;
            sql = "SELECT * FROM CompositeProbabilityDistributions;";
            ExecuteReader();
            while (reader.HasRows && reader.Read())
            {
                compositesChecked++;
                RollPartial rp = new RollPartial(reader["pattern"].ToString(), Int32.Parse(reader["min"].ToString()), Int32.Parse(reader["max"].ToString()), Double.Parse(reader["mean"].ToString()),
                    Double.Parse(reader["stdDev"].ToString()), reader["pTable"].ToString(), Int64.Parse(reader["calcTime"].ToString()));
                if (!rp.IsValid())
                {
                    System.Diagnostics.Trace.WriteLine("Invalid RollPartial for " + rp.pattern);
                    return false;
                }
                else
                {
                    System.Diagnostics.Trace.WriteLine("Valid RollPartial for " + rp.pattern);
                }
            }
            System.Diagnostics.Trace.WriteLine("[CACHE VALID!] Checked " + singlesChecked + " single dice patterns and " + compositesChecked + " composite patterns.");
            return true;
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

        private int GetLatestSession()
        {
            sql = "SELECT id FROM Sessions ORDER BY id DESC;";
            ExecuteReader();
            if (reader.HasRows && reader.Read())
            {
                return Int16.Parse(reader["id"].ToString());
            }
            else
            {
                return NewSession();
            }
        }

        public int NewSession()
        {
            String sessionName = System.DateTime.Now.ToString("yyyy/M/d HH:mm") + " Dice Session";
            sql = "INSERT INTO Sessions (name) VALUES ('" + sessionName + "');";
            ExecuteNonQuery();

            sql = "SELECT id FROM Sessions WHERE name = '" + sessionName + "' ORDER BY id DESC;";
            ExecuteReader();
            if (reader.HasRows && reader.Read())
            {
                SessionID = Int32.Parse(reader["id"].ToString());
                return SessionID;
            }
            else
            {
                return 0;
            }
        }

        public bool SetLastPattern(String pattern)
        {
            sql = "UPDATE Sessions SET lastPattern = '" + pattern + "' WHERE id = " + SessionID + ";";
            return ExecuteNonQuery() == 1;
        }

        public string GetLastPattern()
        {
            sql = "SELECT lastPattern FROM Sessions WHERE id = " + SessionID + ";";
            ExecuteReader();
            if (reader.HasRows && reader.Read())
            {
                return reader["lastPattern"].ToString();
            }
            else
            {
                return "";
            }
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

            sql = "CREATE TABLE IF NOT EXISTS RollHistory (id INTEGER PRIMARY KEY, pattern TEXT, value INT, session_id INT)";
            ExecuteNonQuery();

            sql = "CREATE TABLE IF NOT EXISTS Sessions (id INTEGER PRIMARY KEY, name TEXT, lastPattern TEXT)";
            ExecuteNonQuery();

            sql = "CREATE TABLE IF NOT EXISTS CompositeProbabilityDistributions (id INTEGER PRIMARY KEY, pattern TEXT, min INT, max INT, mean REAL, stdDev REAL, pTable TEXT, calcTime INT)";
            ExecuteNonQuery();

            sql = "CREATE TABLE IF NOT EXISTS SingleProbabilityDistributions (id INTEGER PRIMARY KEY, diceCount INT, diceSides INT, min INT, max INT, mean REAL, stdDev REAL, pTable TEXT, calcTime INT)";
            ExecuteNonQuery();

            SessionID = GetLatestSession();
  
            Trace.WriteLine("SessionID: " + SessionID);
        }
    }
}

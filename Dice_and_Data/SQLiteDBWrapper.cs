using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace Dice_and_Data
{
    class SQLiteDBWrapper
    {
        private static SQLiteConnection conn;

        private static SQLiteDBWrapper dbWrapRef = new SQLiteDBWrapper();

        public static SQLiteDBWrapper getReference()
        {
            return dbWrapRef;
        }

        private SQLiteDBWrapper()
        {
            if (!System.IO.File.Exists("RollTracker.sqlite"))
            {
                SQLiteConnection.CreateFile("RollTracker.sqlite");
            }
            conn = new SQLiteConnection("Data Source=RollTracker.sqlite;Version=4;");
            System.Diagnostics.Trace.WriteLine("connection opened");
        }        

    }
}

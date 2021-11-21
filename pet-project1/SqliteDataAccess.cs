using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Configuration;
using System.Net;
using System.IO;
using System.Xml.Linq;

namespace pet_project1
{
    public class SqliteDataAccess
    {
        // Create table for Rates 
        public static void CreateTable()
        {
            using (SQLiteConnection cnn = new SQLiteConnection(ConnConfig()))
            {
                cnn.Open();
                var command = cnn.CreateCommand();
                command.CommandText = @"CREATE TABLE IF NOT EXISTS Rates (
                                       id INTEGER PRIMARY KEY AUTOINCREMENT,
                                       curr text NOT NULL,
                                       rate DECIMAL NOT NULL,
                                       date_update DATE NOT NULL
                                       ); ";
                 command.ExecuteNonQuery();

            }
        }
        
        
        // Get rate from table rates
        public static double GetRate(string code)
        {
            using (SQLiteConnection cnn = new SQLiteConnection(ConnConfig()))
            {
                cnn.Open();
                var command = cnn.CreateCommand();
                command.CommandText = @"SELECT rate FROM Rates WHERE curr = $code";
                command.Parameters.AddWithValue("code", code);

                double rate = double.Parse(command.ExecuteScalar().ToString());
                return rate;
                
            }

        }
        
        // Load all Currencies from table 
        public static List<String> LoadCurr()
        {
            List<String> curr = new List<String>();
            using (var cnn = new SQLiteConnection(ConnConfig()))
            {
                cnn.Open();
                
                var command = cnn.CreateCommand();
                command.CommandText = @"SELECT curr FROM Rates";
                
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        curr.Add(reader.GetString(0));
                    }
                }
            }
            return curr;
        }

        // Populate table with curr and rate. Get xml file from cbar.az then parse it and insert into table
        public static void InsertTable()
        {
            using (SQLiteConnection cnn = new SQLiteConnection(ConnConfig()))
            {
                cnn.Open();

                string respDate = DateTime.Now.ToString("dd.MM.yyyy");
                if (DateTime.Now.DayOfWeek.ToString() == "Sunday" || DateTime.Now.DayOfWeek.ToString() == "Saturday")
                {
                    int day = ((int)DateTime.Now.DayOfWeek == 0) ? 7 : (int)DateTime.Now.DayOfWeek;
                    respDate = DateTime.Now.AddDays(5 - day).ToString("dd.MM.yyyy");
                }

                WebRequest request = WebRequest.Create("https://www.cbar.az/currencies/" + respDate + ".xml");
                string response;
                // Get the response.
                try
                {
                    using (StreamReader streamIn = new StreamReader((request.GetResponse()).GetResponseStream(), Encoding.UTF8))
                    {
                        response = streamIn.ReadToEnd();
                        streamIn.Close();
                    }
                }
                finally
                {
                    request.Abort();

                }
                
  
                // parse response
                
                XDocument xDoc = XDocument.Parse(response); 
                
                var items = from el in xDoc.Descendants("Valute") select el; // get all 'Valute'
                
                var command = cnn.CreateCommand();
                
                // iterate each 'Valute' and insert into table Code and Value
                foreach (XElement el in items)
                {
                    // update if date_update doesnt equal DateTime.Now, otherwise insert. It means I update rates when you run program 
                    command.CommandText = @"UPDATE Rates
                                            SET  curr=$curr,rate=$rate,date_update = $date_update
                                            WHERE date_update != $date_update AND curr = $curr;

                                            INSERT INTO Rates (curr, rate, date_update) 
                                            SELECT $curr, $rate, $date_update
                                            WHERE (SELECT COUNT(*) FROM Rates WHERE  date_update = $date_update AND curr = $curr) = 0;";
                                               
                    command.Parameters.AddWithValue("$curr", el.Attribute("Code").Value);
                    command.Parameters.AddWithValue("$rate", el.Element("Value").Value);
                    command.Parameters.AddWithValue("date_update", DateTime.Parse(respDate));
                    command.ExecuteNonQuery();
                
                }
                
            }
        } 
        
        // config for connecting to database
        private static string ConnConfig(string id = "Test")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }
    }
}

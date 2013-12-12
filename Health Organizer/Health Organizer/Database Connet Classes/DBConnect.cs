using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using Health_Organizer.Data_Model_Classes;

namespace Health_Organizer.Database_Connet_Classes
{
    class DBConnect : InterfaceDBConnect
    {
        string dbPath;
        SQLiteAsyncConnection conn;     

        //This is the constructor wherein we get path of App and append HealthOrganizerDB to it. So dbPath becomes ../HealthOrganizerDb/<Tables>
        public DBConnect() {
            dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "HealthOrganizerDB");
            conn = new SQLiteAsyncConnection(dbPath);    
        }

        //This used to connect to the Tables or Create Tables.All the tables we need to add in future would be mentioned here.
        public async Task InitializeDatabase() {
            await conn.CreateTableAsync<BasicDiseases>();
            await conn.CreateTableAsync<BasicFirstAid>();

        }

        public SQLiteAsyncConnection GetAsyncConnection()
        {
            return conn;
        }
    }
}

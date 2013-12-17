using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using SQLiteWinRT;
using Health_Organizer.Data_Model_Classes;

namespace Health_Organizer.Database_Connet_Classes
{
    class DBConnect
    {
        public const int ORG_HOME_DB = 0;
        public const int DOC_KIT_DB = 1;
        
        SQLiteAsyncConnection conn;
        Database database;

        //This is the constructor wherein we get path of App and append HealthOrganizerDB to it. So dbPath becomes ../HealthOrganizerDb/<Tables>
        public DBConnect() {
            conn = new SQLiteAsyncConnection(Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "HealthOrganizerDB"));
            database = new Database(Windows.Storage.ApplicationData.Current.LocalFolder, "HealthOrganizerDB");
        }

        //This used to connect to the Tables or Create Tables.All the tables we need to add in future would be mentioned here.
        public async Task InitializeDatabase(int database_name) {
            if (database_name == DOC_KIT_DB)
            {
                await conn.CreateTableAsync<BasicDiseases>();
                await conn.CreateTableAsync<BasicFirstAid>();
            }
            else if(database_name == ORG_HOME_DB)
            {
                await database.OpenAsync();
                await CreateTableAsync(); 
            }
        }

        public SQLiteAsyncConnection GetAsyncConnection()
        {
            return conn;
        }

        public Database GetConnection()
        {
            return database;
        }

        public void CloseConnection(int databaseName) 
        { 
            if(databaseName == DOC_KIT_DB)
            {
                conn = null;
            }else if(databaseName == ORG_HOME_DB)
            {
                database.Dispose();
                database = null;
            }
            GC.Collect();
        }

        private async Task CreateTableAsync()
        {
            string query1 = "CREATE TABLE IF NOT EXISTS Patient (" +
                            "PID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL," +
                            "FirstName TEXT NOT NULL, " +
                            "LastName TEXT NOT NULL, " +
                            "BloodGroup TEXT NOT NULL, " +
                            "Sex TEXT NOT NULL, " +
                            "Birthday TEXT, " +
                            "Image TEXT);";

            string query2 = "CREATE TABLE IF NOT EXISTS MutableDetails (" +
                            "Married VARCHAR(2) NOT NULL, " +
                            "Occupation TEXT NOT NULL, " +
                            "FamilyBackground TEXT, " +
                            "Email TEXT NOT NULL, " +
                            "Mobile UNSIGNED BIG INT NOT NULL, " +
                            "EmMobile INTEGER, " +
                            "PID INTEGER PRIMARY KEY NOT NULL, " +
                            "FOREIGN KEY(PID) REFERENCES Patient(PID) ON DELETE CASCADE);";

            string query3 = "CREATE TABLE IF NOT EXISTS MutableDetailsAllergy (" +
                            "PID INTEGER NOT NULL, " +
                            "Allergy TEXT NOT NULL, " +
                            "PRIMARY KEY(PID, Allergy), " +
                            "UNIQUE(PID, Allergy)" +
                            "FOREIGN KEY(PID) REFERENCES MutableDetails(PID) ON DELETE CASCADE);";

           string query4 = "CREATE TABLE IF NOT EXISTS MutableDetailsAddiction (" +
                            "PID INTEGER NOT NULL, " +
                            "Addiction TEXT NOT NULL, " +
                            "PRIMARY KEY(PID, Addiction), " +
                            "UNIQUE(PID, Addiction)" +
                            "FOREIGN KEY(PID) REFERENCES MutableDetails(PID) ON DELETE CASCADE);";

            string query5 = "CREATE TABLE IF NOT EXISTS MutableDetailsOperation (" +
                            "PID INTEGER NOT NULL," +
                            "Operation TEXT NOT NULL, " +
                            "PRIMARY KEY(PID, Operation), " +
                            "UNIQUE(PID, Operation), " +
                            "FOREIGN KEY(PID) REFERENCES MutableDetails(PID) ON DELETE CASCADE);";

            string query6 = "CREATE TABLE IF NOT EXISTS Address (" +
                            "PID INTEGER PRIMARY KEY NOT NULL," +
                            "ZIP INTEGER NOT NULL, " +
                            "Street TEXT NOT NULL, " +
                            "FOREIGN KEY(PID) REFERENCES Patient(PID) ON DELETE CASCADE);";

            string query7 = "CREATE TABLE IF NOT EXISTS AddressZIP (" +
                            "ZIP INTEGER PRIMARY KEY NOT NULL, " +
                            "City TEXT NOT NULL, " +
                            "FOREIGN KEY(ZIP) REFERENCES Address(ZIP) ON DELETE CASCADE);";

            string query8 = "CREATE TABLE IF NOT EXISTS AddressCity (" +
                            "City TEXT PRIMARY KEY NOT NULL, " +
                            "State TEXT NOT NULL, " +
                            "FOREIGN KEY(City) REFERENCES AddressZIP(City) ON DELETE CASCADE);";

            string query9 = "CREATE TABLE IF NOT EXISTS AddressState (" +
                            "State TEXT PRIMARY KEY NOT NULL, " +
                            "Country TEXT NOT NULL, " +
                            "FOREIGN KEY(State) REFERENCES AddressCity(State) ON DELETE CASCADE);";

            string query10 = "CREATE TABLE IF NOT EXISTS MedicalDetails (" +
                             "PID INTEGER NOT NULL, " +
                             "DateVisited TEXT NOT NULL, " +
                             "Age INTEGER NOT NULL, " +
                             "BloodGlucose INTEGER, " +
                             "SystolicBP INTEGER, " +
                             "DiastolicBP INTEGER, " +
                             "DiseaseFound TEXT, " +
                             "Height REAL NOT NULL, " +
                             "Weight INTEGER NOT NULL, " +
                             "Symptoms TEXT NOT NULL, " + 
                             "BMI REAL NOT NULL, " +
                             "PRIMARY KEY(PID, DateVisited)" +
                             "FOREIGN KEY(PID) REFERENCES Patient(PID) ON DELETE CASCADE);";

            string query11 = "CREATE TABLE IF NOT EXISTS MedicalDetailsBMI (" +
                             "Height REAL NOT NULL, " +
                             "Weight INTEGER NOT NULL, " +
                             "BMI REAL NOT NULL, " +
                             "UNIQUE(Height, Weight), " +
                             "PRIMARY KEY(Height, Weight), " +
                             "FOREIGN KEY(Height, Weight) REFERENCES MedicalDetails(Height, Weight) ON DELETE CASCADE);";

            string query12 = "CREATE TABLE IF NOT EXISTS MedicalDetailsMedicine (" +
                             "PID INTEGER NOT NULL, " +
                             "DateVisited TEXT NOT NULL, " +
                             "Medicine TEXT NOT NULL, " +
                             "UNIQUE(PID, DateVisited, Medicine), " +
                             "PRIMARY KEY(PID, DateVisited, Medicine), " +
                             "FOREIGN KEY(PID, DateVisited) REFERENCES MedicalDetails(PID, DateVisited) ON DELETE CASCADE);";

            string query13 = "CREATE TABLE IF NOT EXISTS MedicalDetailsVaccine (" +
                             "PID INTEGER NOT NULL, " +
                             "DateVisited DATE NOT NULL, " +
                             "Vaccine TEXT NOT NULL, " +
                             "UNIQUE(PID, DateVisited, Vaccine), " +
                             "PRIMARY KEY(PID, DateVisited, Vaccine), " +
                             "FOREIGN KEY(PID, DateVisited) REFERENCES MedicalDetails(PID, DateVisited) ON DELETE CASCADE);";

            await database.ExecuteStatementAsync(query1);
            await database.ExecuteStatementAsync(query2);
            await database.ExecuteStatementAsync(query3);
            await database.ExecuteStatementAsync(query4);
            await database.ExecuteStatementAsync(query5);
            await database.ExecuteStatementAsync(query6);
            await database.ExecuteStatementAsync(query7);
            await database.ExecuteStatementAsync(query8);
            await database.ExecuteStatementAsync(query9);
            await database.ExecuteStatementAsync(query10);
            //await database.ExecuteStatementAsync(query11);
            await database.ExecuteStatementAsync(query12);
            await database.ExecuteStatementAsync(query13);
        }
    }
}

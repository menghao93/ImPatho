using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace Health_Organizer.Database_Connet_Classes
{
    //This is just an Interface provded for the class DBConnect.cs
    public interface InterfaceDBConnect
    {
        Task InitializeDatabase();
        SQLiteAsyncConnection GetAsyncConnection();
    }
}

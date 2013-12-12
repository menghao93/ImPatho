using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using Health_Organizer.Data_Model_Classes;
using Health_Organizer.Database_Connet_Classes;

namespace Health_Organizer.DML_Method_Classes
{
    class DiseasesTable : InterfaceDiseasesTable
    {
        SQLiteAsyncConnection conn;

        public DiseasesTable(DBConnect connection) {
            conn = connection.GetAsyncConnection();
        }

        public async Task InsertDisease(BasicDiseases disease) 
        {
            await conn.InsertAsync(disease);
        }

        public async Task UpdateDisease(BasicDiseases disease)
        {
            await conn.UpdateAsync(disease);
        }

        public async Task DeleteDisease(BasicDiseases disease)
        {
            await conn.DeleteAsync(disease);
        }

        public async Task<List<BasicDiseases>> SelectAllDisease()
        {
            return await conn.Table<BasicDiseases>().ToListAsync();
        }

        public async Task<List<BasicDiseases>> SelectDisease(string query)
        {
            return await conn.QueryAsync<BasicDiseases>(query);
        }

        public async Task<BasicDiseases> FindSingleDisease(string name)
        {
            return await conn.FindAsync<BasicDiseases>(name);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using Health_Organizer.Database_Connet_Classes;
using Health_Organizer.Data_Model_Classes;

namespace Health_Organizer.DML_Method_Classes
{
    class FirstAidTable : InterfaceFirstAidTable
    {
         SQLiteAsyncConnection conn;

        public FirstAidTable(DBConnect connection) {
            conn = connection.GetAsyncConnection();
        }

        public async Task InsertFirstAid(BasicFirstAid firstAid) 
        {
            await conn.InsertAsync(firstAid);
        }

        public async Task UpdateFirstAid(BasicFirstAid firstAid)
        {
            await conn.UpdateAsync(firstAid);
        }

        public async Task DeleteFirstAid(BasicFirstAid firstAid)
        {
            await conn.DeleteAsync(firstAid);
        }

        public async Task<List<BasicFirstAid>> SelectAllFirstAids()
        {
            return await conn.Table<BasicFirstAid>().ToListAsync();
        }

        public async Task<List<BasicFirstAid>> SelectFirstAid(string query)
        {
            return await conn.QueryAsync<BasicFirstAid>(query);
        }

        public async Task<BasicFirstAid> FindSingleFirstAid(string name)
        {
            return await conn.FindAsync<BasicFirstAid>(name);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Health_Organizer.Data_Model_Classes;

namespace Health_Organizer.DML_Method_Classes
{
    interface InterfaceFirstAidTable
    {
        Task InsertFirstAid(BasicFirstAid firstAid);
        Task UpdateFirstAid(BasicFirstAid firstAid);
        Task DeleteFirstAid(BasicFirstAid firstAid);
        Task<List<BasicFirstAid>> SelectAllFirstAids();
        Task<List<BasicFirstAid>> SelectFirstAid(string query);
        Task<BasicFirstAid> FindSingleFirstAid(string name);
    }
}

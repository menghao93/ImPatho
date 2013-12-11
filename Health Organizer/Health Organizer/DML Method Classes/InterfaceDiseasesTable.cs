using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Health_Organizer.Data_Model_Classes;

namespace Health_Organizer.DML_Method_Classes
{
    interface InterfaceDiseasesTable
    {
        Task InsertDisease(BasicDiseases disease);
        Task UpdateDisease(BasicDiseases disease);
        Task DeleteDisease(BasicDiseases employee);
        Task<List<BasicDiseases>> SelectAllDisease();
        Task<List<BasicDiseases>> SelectDisease(string query);
    }
}

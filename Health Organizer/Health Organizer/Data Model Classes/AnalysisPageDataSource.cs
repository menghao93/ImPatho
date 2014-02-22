using Health_Organizer.Database_Connet_Classes;
using SQLiteWinRT;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace Health_Organizer.Data_Model_Classes
{
    public class AnalysisSampleDataItem
    {
        public AnalysisSampleDataItem()
        {
            this.Allergy = new List<string>();
            this.Addiction = new List<string>();
            this.Operation = new List<string>();
            this.Diseases = new Dictionary<string, string>();
            this.Vaccines = new Dictionary<string, string>();
            this.DatesVisited = new List<string>();
        }

        public AnalysisSampleDataItem(string uniqueID, string name, string bloodGroup, char sex, bool married, BitmapImage bmp, string occupation, string familyBG, string[] allergy, string[] addiction, string[] operation, string city, string state, string country, string[] dateVisited)
        {
            this.UniqueId = uniqueID;
            this.Name = name;
            this.BloodGroup = bloodGroup;
            this.Sex = sex;
            this.Married = married;
            this.Image = bmp;
            this.Occupation = occupation;
            this.FamilyBG = familyBG;

            if (allergy != null)
            {
                this.Allergy = new List<string>(allergy);
            }
            else
            {
                this.Allergy = new List<string>();
            }

            if (addiction != null)
            {
                this.Addiction = new List<string>(addiction);
            }
            else
            {
                this.Addiction = new List<string>();
            }

            if (operation != null)
            {
                this.Operation = new List<string>(operation);
            }
            else
            {
                this.Operation = new List<string>();
            }

            this.City = city;
            this.State = state;
            this.Country = country;

            this.Diseases = new Dictionary<string, string>();
            this.Vaccines = new Dictionary<string, string>();

            if (dateVisited != null)
            {
                this.DatesVisited = new List<string>(dateVisited);
            }
            else
            {
                this.DatesVisited = new List<string>();
            }
        }

        public string UniqueId { get; set; }
        public string Name { get; set; }
        public string BloodGroup { get; set; }
        public BitmapImage Image { get; set; }
        public char Sex { get; set; }
        public bool Married { get; set; }
        public string Occupation { get; set; }
        public string FamilyBG { get; set; }
        public List<string> Allergy { get; set; }
        public List<string> Addiction { get; set; }
        public List<string> Operation { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public Dictionary<string, string> Diseases { get; set; }
        public Dictionary<string, string> Vaccines { get; set; }
        public List<string> DatesVisited { get; set; }

        public override string ToString()
        {
            return this.Name;
        }

        public string Description
        {
            get
            {
                return Sex.ToString() + ", " + Occupation;
            }
        }
    }

    public sealed class AnalysisPageDataSoure
    {
        public static AnalysisPageDataSoure _sampleDataSource = new AnalysisPageDataSoure();
        private Database db = null;
        private ObservableCollection<AnalysisSampleDataItem> _groups = new ObservableCollection<AnalysisSampleDataItem>();
        public ObservableCollection<AnalysisSampleDataItem> Groups
        {
            get { return this._groups; }
        }

        public static async Task<IEnumerable<AnalysisSampleDataItem>> GetItemsAsync()
        {
            await _sampleDataSource.GetSampleDataAsync();

            return _sampleDataSource.Groups;
        }

        public static async Task<AnalysisSampleDataItem> GetItemAsync(string uniqueId)
        {
            await _sampleDataSource.GetSampleDataAsync();
            var matches = _sampleDataSource.Groups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static async Task<int> DelItemAsync(string uniqueId)
        {
            try
            {
                AnalysisSampleDataItem sampleItem = await GetItemAsync(uniqueId);
                _sampleDataSource.Groups.Remove(sampleItem);
                return 1;
            }
            catch
            {
                return -1;
            }
        }

        private async Task GetSampleDataAsync()
        {
            if (this._groups.Count != 0)
                return;


            this.db = App.database;

            if (this.db != null)
            {
                //Create a hashmap of (pid, item) and one by one query DB and add all the details to it.
                try
                {
                    //string query = "SELECT * FROM (Patient NATURAL JOIN MutableDetails) NATURAL JOIN (Address NATURAL JOIN (AddressZIP NATURAL JOIN (AddressCity NATURAL JOIN AddressState)))";
                    string query = "SELECT Patient.PID, Patient.FirstName, Patient.LastName, Patient.BloodGroup, Patient.Sex, Patient.Birthday, Patient.Image, MutableDetails.Married, MutableDetails.Occupation, MutableDetails.FamilyBackground, MutableDetails.Email, MutableDetails.Mobile, MutableDetails.EmMobile, Address.ZIP, Address.Street, AddressCity.City, AddressCity.State, AddressState.Country FROM (((((Patient INNER JOIN MutableDetails ON Patient.PID = MutableDetails.PID) INNER JOIN Address ON MutableDetails.PID = Address.PID) INNER JOIN AddressZIP ON Address.ZIP = AddressZIP.ZIP) INNER JOIN AddressCity ON AddressZIP.CITY = AddressCity.CITY)  INNER JOIN AddressState ON AddressCity.STATE = AddressState.STATE)";

                    Statement statement = await db.PrepareStatementAsync(query);
                    statement.EnableColumnsProperty();
                    Dictionary<string, AnalysisSampleDataItem> group = new Dictionary<string, AnalysisSampleDataItem>();

                    while (await statement.StepAsync())
                    {
                        //Debug.WriteLine(statement.Columns["PID"] + " " + statement.Columns["FirstName"] + " " + statement.Columns["LastName"] + " " + statement.Columns["ZIP"] + " " + statement.Columns["City"] + " " + statement.Columns["State"] + " " + statement.Columns["Country"] + " " + statement.Columns["Occupation"] + " " + statement.Columns["Married"]);
                        AnalysisSampleDataItem newItem = new AnalysisSampleDataItem();
                        newItem.UniqueId = statement.Columns["PID"];
                        BitmapImage bmp = await ImageMethods.Base64StringToBitmap(statement.Columns["Image"]);
                        newItem.Name = statement.Columns["FirstName"] + " " + statement.Columns["LastName"];
                        newItem.BloodGroup = statement.Columns["BloodGroup"];
                        newItem.Sex = statement.Columns["Sex"][0];
                        newItem.Image = bmp;

                        if (statement.Columns["Married"].Equals("T"))
                        {
                            newItem.Married = true;
                        }
                        else
                        {
                            newItem.Married = false;
                        }

                        newItem.Occupation = statement.Columns["Occupation"];
                        newItem.FamilyBG = statement.Columns["FamilyBackground"];
                        newItem.City = statement.Columns["City"];
                        newItem.State = statement.Columns["State"];
                        newItem.Country = statement.Columns["Country"];
                        group.Add((statement.Columns["PID"]), newItem);
                    }

                    //Now after adding the basic details and creating their HASHMAP get other details from DB & add it to the HASHMAP which later 
                    //would be added to Groups of the GRID VIEW.
                    statement.Reset();
                    string queryAllergy = "SELECT * FROM MutableDetailsAllergy";
                    statement = await this.db.PrepareStatementAsync(queryAllergy);
                    statement.EnableColumnsProperty();

                    while (await statement.StepAsync())
                    {
                        List<string> sample = group[(statement.Columns["PID"])].Allergy;
                        if (sample != null)
                        {
                            sample.Add(statement.Columns["Allergy"]);
                        }
                    }

                    statement.Reset();
                    string queryAddiction = "SELECT * FROM MutableDetailsAddiction";
                    statement = await this.db.PrepareStatementAsync(queryAddiction);
                    statement.EnableColumnsProperty();

                    while (await statement.StepAsync())
                    {
                        List<string> sample = group[(statement.Columns["PID"])].Addiction;
                        if (sample != null)
                        {
                            sample.Add(statement.Columns["Addiction"]);
                        }
                    }

                    statement.Reset();
                    string queryOperation = "SELECT * FROM MutableDetailsOperation";
                    statement = await this.db.PrepareStatementAsync(queryOperation);
                    statement.EnableColumnsProperty();

                    while (await statement.StepAsync())
                    {
                        List<string> sample = group[(statement.Columns["PID"])].Operation;
                        if (sample != null)
                        {
                            sample.Add(statement.Columns["Operation"]);
                        }
                    }

                    statement.Reset();
                    string queryDateVisited = "SELECT * FROM MedicalDetails";
                    statement = await this.db.PrepareStatementAsync(queryDateVisited);
                    statement.EnableColumnsProperty();

                    while (await statement.StepAsync())
                    {
                        List<string> sampleDatesVisited = group[(statement.Columns["PID"])].DatesVisited;
                        Dictionary<string, string> sampleVaccines = group[(statement.Columns["PID"])].Vaccines;
                        Dictionary<string, string> sampleDiseases = group[(statement.Columns["PID"])].Diseases;
                        if (sampleDatesVisited != null && sampleDiseases != null && sampleVaccines != null)
                        {
                            sampleDatesVisited.Add(statement.Columns["DateVisited"]);
                            sampleVaccines.Add(statement.Columns["DateVisited"], "0");
                            sampleDiseases.Add(statement.Columns["DateVisited"], statement.Columns["DiseaseFound"]);
                        }
                    }

                    statement.Reset();
                    string queryVaccines = "SELECT * FROM MedicalDetailsVaccine";
                    statement = await this.db.PrepareStatementAsync(queryVaccines);
                    statement.EnableColumnsProperty();

                    while (await statement.StepAsync())
                    {
                        Dictionary<string, string> sampleVaccines = group[(statement.Columns["PID"])].Vaccines;
                        if (sampleVaccines != null)
                        {
                            sampleVaccines[statement.Columns["DateVisited"]] = statement.Columns["Vaccine"];
                        }
                    }

                    //After adding all the details add the items to GRID VIEW
                    foreach (KeyValuePair<string, AnalysisSampleDataItem> sample in group)
                    {
                        Groups.Add(sample.Value);
                    }

                    //Collect garbage after all this loading happens
                    GC.Collect();
                }
                catch (Exception ex)
                {
                    var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                    Debug.WriteLine("ANALYSIS_PAGE_DATA_SOURCE---LOAD_ITEMS_ASYNC" + "\n" + ex.Message + "\n" + result.ToString());
                }
            }

        }
    }
}

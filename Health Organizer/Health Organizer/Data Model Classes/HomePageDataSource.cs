using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Health_Organizer.Database_Connet_Classes;
using SQLiteWinRT;
using System.Diagnostics;

namespace Health_Organizer.Data
{
    public class SampleDataItem
    {
        public SampleDataItem(String uniqueId, String title, String description, BitmapImage image)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Description = description;
            this.Image = image;
        }

        public string UniqueId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public BitmapImage Image { get; set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    public class SampleDataGroup
    {
        public SampleDataGroup(String uniqueId, String title)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Items = new ObservableCollection<SampleDataItem>();
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public ObservableCollection<SampleDataItem> Items { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    public sealed class HomePageDataSoure
    {
        public static HomePageDataSoure _sampleDataSource = new HomePageDataSoure();
        private Database db = null;
        public ObservableCollection<SampleDataGroup> _groups = new ObservableCollection<SampleDataGroup>();
        public ObservableCollection<SampleDataGroup> Groups
        {
            get { return this._groups; }
        }

        public static async Task<IEnumerable<SampleDataGroup>> GetGroupsAsync()
        {
            await _sampleDataSource.GetSampleDataAsync();

            return _sampleDataSource.Groups;
        }

        public static async Task<SampleDataGroup> GetGroupAsync(string uniqueId)
        {
            await _sampleDataSource.GetSampleDataAsync();
            var matches = _sampleDataSource.Groups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static async Task<SampleDataItem> GetItemAsync(string uniqueId)
        {
            await _sampleDataSource.GetSampleDataAsync();
            var matches = _sampleDataSource.Groups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        private async Task GetSampleDataAsync()
        {
            if (this._groups.Count != 0)
                return;
            
            try
            {
                DBConnect connection = new DBConnect();
                await connection.InitializeDatabase(DBConnect.ORG_HOME_DB);
                db = connection.GetConnection();

                if (this.db != null)
                {
                    string query = "SELECT * FROM (Patient NATURAL JOIN Address) NATURAL JOIN AddressZIP ORDER BY City";
                    Statement statement = await db.PrepareStatementAsync(query);
                    statement.EnableColumnsProperty();
                    string prevGroup = "xxx";
                    SampleDataGroup groups = null;

                    //Check previous entered city and current city are same. if same -> add new item to same grp; if not -> create new grp and
                    //add the new item to this new grp.
                    while (await statement.StepAsync())
                    {
                        //Debug.WriteLine(statement.Columns["PID"] + " " + statement.Columns["FirstName"] + " " + statement.Columns["LastName"] + " " + statement.Columns["ZIP"] + " " + statement.Columns["City"]);
                        string currentGroup = statement.Columns["City"];
                        if (currentGroup.Equals(prevGroup))
                        {
                            BitmapImage bmp = await ImageMethods.Base64StringToBitmap(statement.Columns["Image"]);
                            groups.Items.Add(new SampleDataItem(statement.Columns["PID"], statement.Columns["FirstName"] + " " + statement.Columns["LastName"], statement.Columns["Street"], bmp));
                        }
                        else
                        {
                            if (groups != null)
                            {
                                this.Groups.Add(groups);
                            }

                            BitmapImage bmp = await ImageMethods.Base64StringToBitmap(statement.Columns["Image"]);
                            groups = new SampleDataGroup(statement.Columns["ZIP"], statement.Columns["City"]);
                            groups.Items.Add(new SampleDataItem(statement.Columns["City"], statement.Columns["FirstName"] + " " + statement.Columns["LastName"], statement.Columns["Street"], bmp));
                        }
                        prevGroup = currentGroup;
                    }
                    this.Groups.Add(groups);
                }
                this.db.Dispose();
                connection.CloseConnection(DBConnect.ORG_HOME_DB);
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("HOME_PAGE_DATA_SOURCE---LOAD_GROUP_ASYNC" + "\n" + ex.Message + "\n" + result.ToString());
            }
        }
    }
}
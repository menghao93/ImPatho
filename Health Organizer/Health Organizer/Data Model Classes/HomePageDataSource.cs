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
    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class SampleDataItem
    {
        public SampleDataItem(String uniqueId, String title, String description, BitmapImage image)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Description = description;
            this.Image = image;
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public BitmapImage Image { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
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

    /// <summary>
    /// Creates a collection of groups and items with content read from a static json file.
    /// 
    /// SampleDataSource initializes with data read from a static json file included in the 
    /// project.  This provides sample data at both design-time and run-time.
    /// </summary>
    public sealed class HomePageDataSoure
    {
        private static HomePageDataSoure _sampleDataSource = new HomePageDataSoure();
        private Database db = null;
        private ObservableCollection<SampleDataGroup> _groups = new ObservableCollection<SampleDataGroup>();
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
                    string query = "SELECT * FROM (Patient NATURAL JOIN Address NATURAL JOIN AddressZIP) GROUP BY City";
                    Statement statement = await db.PrepareStatementAsync(query);
                    statement.EnableColumnsProperty();
                    string prevGroup = "xxx";
                    SampleDataGroup groups = null;

                    while (await statement.StepAsync())
                    {
                        Debug.WriteLine(statement.Columns["FirstName"] + " " + statement.Columns["LastName"] + " " + statement.Columns["ZIP"] + " " + statement.Columns["City"]);
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
                            groups.Items.Add(new SampleDataItem(statement.Columns["PID"], statement.Columns["FirstName"] + " " + statement.Columns["LastName"], statement.Columns["Street"], bmp));
                        }
                        this.Groups.Add(groups);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.Source);
                Debug.WriteLine(ex.InnerException);
                Debug.WriteLine(ex.Data);
                Debug.WriteLine(ex.StackTrace);
            }
        }
    }
}
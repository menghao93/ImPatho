﻿using System;
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

        public SampleDataGroup(string uniqueId, string title, List<SampleDataItem> items)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Items = new ObservableCollection<SampleDataItem>(items);
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
        private const int GROUP_ITEMS_LIMIT = 10;
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

        public static async Task<IEnumerable<SampleDataGroup>> GetLimitedGroupsAsync()
        {
            await _sampleDataSource.GetSampleDataAsync();

            var sampleLimited = _sampleDataSource.Groups;

            ObservableCollection<SampleDataGroup> limitedItemsGroup = new ObservableCollection<SampleDataGroup>();

            SampleDataGroup temp;

            if (sampleLimited.Count() > 0)
            {
                foreach (SampleDataGroup xgroup in sampleLimited)
                {
                    temp = new SampleDataGroup(xgroup.UniqueId, xgroup.Title, xgroup.Items.Take(GROUP_ITEMS_LIMIT).ToList());
                    limitedItemsGroup.Add(temp);
                }
            }

            return limitedItemsGroup.AsEnumerable();
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

        public static async Task<int> DelItemAsync(string uniqueId)
        {
            await _sampleDataSource.GetSampleDataAsync();

            SampleDataItem xItem = await GetItemAsync(uniqueId);
            foreach (SampleDataGroup sample in _sampleDataSource.Groups)
            {
                if (sample.Items.Contains(xItem))
                {
                    sample.Items.Remove(xItem);

                    if (sample.Items.Count() < 1)
                    {
                        _sampleDataSource.Groups.Remove(sample);
                    }
                    return 1;
                }

            }
            return -1;
        }

        private async Task GetSampleDataAsync()
        {
            if (this.Groups.Count() == 0)
            {
                try
                {
                    this.db = App.database;

                    if (this.db != null)
                    {
                        //string query = "SELECT * FROM (Patient NATURAL JOIN (Address NATURAL JOIN AddressZIP));";
                        string query = "SELECT Patient.PID, Patient.FirstName, Patient.LastName, Patient.BloodGroup, Patient.Sex, Patient.Birthday, Patient.Image, Address.ZIP, Address.Street, AddressZIP.City FROM ((Patient INNER JOIN Address ON Patient.PID = Address.PID) INNER JOIN AddressZIP ON AddressZIP.ZIP = Address.ZIP)";
                        Statement statement = await db.PrepareStatementAsync(query);
                        statement.EnableColumnsProperty();
                        List<SampleDataGroup> sampleList = new List<SampleDataGroup>();
                        int edgeCaseCount = 0;
                        //string prevGroup = "xxx";
                        //SampleDataGroup groups = null;

                        //Check previous entered city and current city are same. if same -> add new item to same grp; if not -> create new grp and
                        //add the new item to this new grp.
                        while (await statement.StepAsync())
                        {
                            //Debug.WriteLine(statement.Columns["PID"] + " " + statement.Columns["FirstName"] + " " + statement.Columns["LastName"] + " " + statement.Columns["ZIP"] + " " + statement.Columns["City"]);
                            edgeCaseCount++;
                            BitmapImage bmp = await ImageMethods.Base64StringToBitmap(statement.Columns["Image"]);

                            SampleDataGroup sampleGroup = Groups.ToList().Find(item => item.Title.Equals(statement.Columns["City"]));
                            if (sampleGroup == null)
                            {
                                sampleGroup = new SampleDataGroup(statement.Columns["City"], statement.Columns["City"]);
                                sampleGroup.Items.Add(new SampleDataItem(statement.Columns["PID"], statement.Columns["FirstName"] + " " + statement.Columns["LastName"], statement.Columns["Street"], bmp));
                                Groups.Add(sampleGroup);
                            }
                            else
                            {
                                sampleGroup.Items.Add(new SampleDataItem(statement.Columns["PID"], statement.Columns["FirstName"] + " " + statement.Columns["LastName"], statement.Columns["Street"], bmp));
                            }
                            //string currentGroup = statement.Columns["City"];
                            //if (currentGroup.Equals(prevGroup))
                            //{
                            //    BitmapImage bmp = await ImageMethods.Base64StringToBitmap(statement.Columns["Image"]);
                            //    groups.Items.Add(new SampleDataItem(statement.Columns["PID"], statement.Columns["FirstName"] + " " + statement.Columns["LastName"], statement.Columns["Street"], bmp));
                            //}
                            //else
                            //{
                            //    if (groups != null)
                            //    {
                            //        this.Groups.Add(groups);
                            //    }

                            //    BitmapImage bmp = await ImageMethods.Base64StringToBitmap(statement.Columns["Image"]);
                            //    groups = new SampleDataGroup(statement.Columns["City"], statement.Columns["City"]);
                            //    groups.Items.Add(new SampleDataItem(statement.Columns["PID"], statement.Columns["FirstName"] + " " + statement.Columns["LastName"], statement.Columns["Street"], bmp));
                            //}
                            //prevGroup = currentGroup;
                        }
                        //foreach (SampleDataGroup group in sampleList)
                        //{
                        //    this.Groups.Add(group);
                        //}
                        //if (groups != null)
                        //{
                        //    this.Groups.Add(groups);
                        //}
                    }
                }
                catch (Exception ex)
                {
                    var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                    Debug.WriteLine("HOME_PAGE_DATA_SOURCE---LOAD_GROUP_ASYNC" + "\n" + ex.Message + "\n" + result.ToString());
                }
            }
            return;
        }
    }
}
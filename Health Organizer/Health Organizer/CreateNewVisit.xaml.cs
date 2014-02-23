using Health_Organizer.Data;
using SQLiteWinRT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Health_Organizer.Database_Connet_Classes;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Windows.UI;


namespace Health_Organizer
{
    public sealed partial class CreateNewVisit : Page
    {

        private NavigationHelper navigationHelper;
        private Database database;
        private string PID = "0";
        ObservableCollection<string> ocString;
        private bool isUpdating = false;
        Boolean check = true;
        private SettingsFlyout1 settings;

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public CreateNewVisit()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
            this.ocString = new ObservableCollection<string>();
            settings = new SettingsFlyout1();
            VisitListBox.ItemsSource = this.ocString;
            this.InitializeVisitDetialsComboBox();
        }

        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
            //this.PID = Int32.Parse(e.Parameter as string);
            this.PID = (e.Parameter as string);
            this.InitializeDB(this.PID);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        private async void InitializeDB(string pid)
        {
            this.database = App.database;

            string query = "SELECT * FROM MedicalDetails WHERE PID = @pid";
            Statement statement = await this.database.PrepareStatementAsync(query);
            statement.BindTextParameterWithName("@pid", pid);
            statement.EnableColumnsProperty();
            while (await statement.StepAsync())
            {
                this.ocString.Add(statement.Columns["DateVisited"]);
            }

            await this.loadPatientDetails(pid);
            if (this.ocString.Count() > 0)
            {
                VisitListBox.SelectedIndex = 0;
            }
            else
            {
                this.collapseStackPanels();
            }
            //this.queryDB();
        }


        private void InitializeVisitDetialsComboBox()
        {
            this.ClearAllFields();
            //Adding days and years to combobox in form
            for (int i = 0; i < 31; i++)
            {
                VisitDayComboBox.Items.Add(i + 1);
            }

            for (int i = 2000; i <= DateTime.Now.Year; i++)
            {
                VisitYearComboBox.Items.Add(i);
            }

            //Set current date in form
            VisitDayComboBox.SelectedItem = DateTime.Now.Day;
            VisitMonthComboBox.SelectedIndex = DateTime.Now.Month - 1;
            VisitYearComboBox.SelectedItem = DateTime.Now.Year;

        }

        private async void queryDB()
        {
            string query = "SELECT * FROM Patient";
            Statement statement = await database.PrepareStatementAsync(query);
            statement.EnableColumnsProperty();
            while (await statement.StepAsync())
            {
                Debug.WriteLine(statement.Columns["PID"] + " " + statement.Columns["FirstName"] + " " + statement.Columns["LastName"]);
            }

            query = "SELECT * FROM Address";
            statement = await database.PrepareStatementAsync(query);
            statement.EnableColumnsProperty();
            while (await statement.StepAsync())
            {
                Debug.WriteLine(statement.Columns["PID"] + " " + statement.Columns["ZIP"] + " " + statement.Columns["Street"]);
            }

            query = "SELECT * FROM (Address NATURAL JOIN AddressZIP) ORDER BY City";
            statement = await database.PrepareStatementAsync(query);
            statement.EnableColumnsProperty();
            while (await statement.StepAsync())
            {
                Debug.WriteLine(statement.Columns["PID"] + " " + statement.Columns["ZIP"] + " " + statement.Columns["City"]);
            }
        }

        private async Task<int> loadPatientDetails(string pid)
        {
            try
            {
                string q = "SELECT * FROM Patient WHERE PID = @pid";
                Statement s = await this.database.PrepareStatementAsync(q);
                s.BindTextParameterWithName("@pid", pid);
                s.EnableColumnsProperty();
                if (await s.StepAsync())
                {
                    VisitPatientName.Text = s.Columns["FirstName"] + " " + s.Columns["LastName"];
                    VisitPatientPhoto.Source = await ImageMethods.Base64StringToBitmap(s.Columns["Image"]);
                }

                return DBConnect.RESULT_OK;
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_NEW_VISIT---LOAD_PATIENT_DETAILS" + "\n" + ex.Message + "\n" + result.ToString());

                return DBConnect.RESULT_ERROR;
            }
        }

        private async void AddVisitClicked(object sender, RoutedEventArgs e)
        {
            VisitFormCmdbar.IsOpen = false;
            VisitFormBar.IsOpen = true;
            VisitCustomDialogAnimation.Begin();

            try
            {
                string query = "SELECT Height, Weight FROM MedicalDetails WHERE PID = @pid";
                Statement statement = await database.PrepareStatementAsync(query);
                statement.BindTextParameterWithName("@pid", this.PID);
                statement.EnableColumnsProperty();
                double height = -1;
                Int32 weight = -1;
                while (await statement.StepAsync())
                {
                    height = Double.Parse(statement.Columns["Height"]);
                    weight = Int32.Parse(statement.Columns["Weight"]);
                }

                if (height != -1)
                {
                    this.setHeightBoxFromMeterHeight(height);
                }
                if (weight != -1)
                {
                    VisitWeight.Text = weight.ToString(); 
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_NEW_VISIT---Add_Visit_Clicked" + "\n" + ex.Message + "\n" + result.ToString());
            }
        }

        private async void EditVisitClicked(object sender, RoutedEventArgs e)
        {
            VisitFormCmdbar.IsOpen = false;
            VisitCustomDialogAnimation.Begin();
            if (VisitListBox.SelectedItem != null)
            {
                VisitFormBar.IsOpen = true;

                try
                {
                    string query = "SELECT * FROM MedicalDetails WHERE PID = @pid AND DateVisited = @dv";
                    Statement statement = await this.database.PrepareStatementAsync(query);
                    statement.BindTextParameterWithName("@pid", this.PID);
                    statement.BindTextParameterWithName("@dv", VisitListBox.Items[VisitListBox.SelectedIndex].ToString());
                    statement.EnableColumnsProperty();
                    if (await statement.StepAsync())
                    {
                        string[] dv = statement.Columns["DateVisited"].Split('-');

                        this.setHeightBoxFromMeterHeight(Convert.ToDouble(statement.Columns["Height"]));

                        VisitDayComboBox.SelectedIndex = VisitDayComboBox.Items.IndexOf(Int32.Parse(dv[2]));
                        VisitMonthComboBox.SelectedIndex = VisitMonthComboBox.Items.IndexOf(dv[1]);
                        VisitYearComboBox.SelectedIndex = VisitYearComboBox.Items.IndexOf(Int32.Parse(dv[0]));
                        VisitSymptoms.Text = statement.Columns["Symptoms"];
                        VisitDiseasesDiagnosed.Text = statement.Columns["DiseaseFound"];

                        VisitWeight.Text = statement.Columns["Weight"];
                        VisitSystolicBP.Text = statement.Columns["SystolicBP"];
                        VisitDiastolicBP.Text = statement.Columns["DiastolicBP"];
                        VisitBloodGlucose.Text = statement.Columns["BloodGlucose"];
                    }
                }
                catch (Exception ex)
                {
                    var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                    Debug.WriteLine("CREATE_NEW_VISIT---EDIT_VISIT_CLICKED---MEDIC_DETAILS" + "\n" + ex.Message + "\n" + result.ToString());
                }

                try
                {
                    string query = "SELECT Medicine FROM MedicalDetailsMedicine WHERE PID = @pid AND DateVisited = @dv";
                    Statement statement = await this.database.PrepareStatementAsync(query);
                    statement.BindTextParameterWithName("@pid", this.PID);
                    statement.BindTextParameterWithName("@dv", VisitListBox.Items[VisitListBox.SelectedIndex].ToString());
                    statement.EnableColumnsProperty();
                    VisitMedicineGiven.Text = "";
                    while (await statement.StepAsync())
                    {
                        VisitMedicineGiven.Text += statement.Columns["Medicine"] + ",";
                    }

                    if (!VisitMedicineGiven.Text.Equals(""))
                    {
                        VisitMedicineGiven.Text = VisitMedicineGiven.Text.Substring(0, VisitMedicineGiven.Text.Length - 1);
                    }
                }
                catch (Exception ex)
                {
                    var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                    Debug.WriteLine("CREATE_NEW_VISIT---EDIT_VISIT_CLICKED---MEDIC_DETAILS_MEDICINE" + "\n" + ex.Message + "\n" + result.ToString());
                }

                try
                {
                    string query = "SELECT Vaccine FROM MedicalDetailsVaccine WHERE PID = @pid AND DateVisited = @dv";
                    Statement statement = await this.database.PrepareStatementAsync(query);
                    statement.BindTextParameterWithName("@pid", this.PID);
                    statement.BindTextParameterWithName("@dv", VisitListBox.Items[VisitListBox.SelectedIndex].ToString());
                    statement.EnableColumnsProperty();
                    VisitVaccine.Text = "";
                    while (await statement.StepAsync())
                    {
                        //Debug.WriteLine(statement.Columns["Vaccine"]);
                        VisitVaccine.Text += statement.Columns["Vaccine"] + ",";
                    }

                    if (!VisitVaccine.Text.Equals(""))
                    {
                        VisitVaccine.Text = VisitVaccine.Text.Substring(0, VisitVaccine.Text.Length - 1);
                    }
                }
                catch (Exception ex)
                {
                    var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                    Debug.WriteLine("CREATE_NEW_VISIT---EDIT_VISIT_CLICKED---MEDIC_DETAILS_VACCINE" + "\n" + ex.Message + "\n" + result.ToString());
                }

                isUpdating = true;
            }
        }

        private async void DeleteVisitClicked(object sender, RoutedEventArgs e)
        {
            VisitFormCmdbar.IsOpen = false;
            if (VisitListBox.SelectedItem != null)
            {
                string DateVisited = VisitListBox.Items[VisitListBox.SelectedIndex].ToString();

                try
                {
                    string deleteQuery = "DELETE FROM MedicalDetails WHERE PID = @pid AND DateVisited = @dv";
                    Statement statement = await this.database.PrepareStatementAsync(deleteQuery);
                    statement.BindTextParameterWithName("@pid", this.PID);
                    statement.BindTextParameterWithName("@dv", DateVisited);
                    await statement.StepAsync();
                    this.ocString.Remove(DateVisited);
                }
                catch (Exception ex)
                {
                    var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                    Debug.WriteLine("CREATE_NEW_VISIT---DEL_VISIT_CLICKED---MEDIC_DETAILS" + "\n" + ex.Message + "\n" + result.ToString());
                }

                try
                {
                    string deleteQuery = "DELETE FROM MedicalDetailsVaccine WHERE PID = @pid AND DateVisited = @dv";
                    Statement statement = await this.database.PrepareStatementAsync(deleteQuery);
                    statement.BindTextParameterWithName("@pid", this.PID);
                    statement.BindTextParameterWithName("@dv", DateVisited);
                    while (await statement.StepAsync())
                    {
                        Debug.WriteLine(statement.Columns["Vaccine"]);
                    }
                }
                catch (Exception ex)
                {
                    var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                    Debug.WriteLine("CREATE_NEW_VISIT---EDIT_VISIT_CLICKED---MEDIC_DETAILS_VACCINE" + "\n" + ex.Message + "\n" + result.ToString());
                }

                try
                {
                    string deleteQuery = "DELETE FROM MedicalDetailsMedicine WHERE PID = @pid AND DateVisited = @dv";
                    Statement statement = await this.database.PrepareStatementAsync(deleteQuery);
                    statement.BindTextParameterWithName("@pid", this.PID);
                    statement.BindTextParameterWithName("@dv", DateVisited);
                    while (await statement.StepAsync())
                    {
                        Debug.WriteLine(statement.Columns["Medicine"]);
                    }
                }
                catch (Exception ex)
                {
                    var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                    Debug.WriteLine("CREATE_NEW_VISIT---EDIT_VISIT_CLICKED---MEDIC_DETAILS_MEDICINE" + "\n" + ex.Message + "\n" + result.ToString());
                }
            }

            if (this.ocString.Count() <= 0)
            {
                this.collapseStackPanels();
            }
            else
            {
                VisitListBox.SelectedIndex = 0;
            }
        }

        private async void VisitSaveClicked(object sender, RoutedEventArgs e)
        {
            check = false;
            if (await this.CheckIfFilled())
            {
                if (isUpdating)
                {
                    await this.UpdateDetails();
                }
                else
                {
                    await this.InsertDetails();
                }

                VisitFormBar.IsOpen = false;
            }
            else
            {
                if (check)
                {
                    var messageDialog = new Windows.UI.Popups.MessageDialog("Please complete the form correctly before saving it.", "Error!");
                    messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("Okay", null));
                    var dialogResult = await messageDialog.ShowAsync();
                }
            }

            this.resetAllBorders();
        }

        private void ViewProfileClicked(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(ProfileDetailsPage), this.PID.ToString());
            }
        }

        private void VisitCancelClicked(object sender, RoutedEventArgs e)
        {
            this.ClearAllFields();
            this.resetAllBorders();
            VisitMedicineGiven.IsEnabled = true;
            VisitVaccine.IsEnabled = true;
            VisitFormBar.IsOpen = false;
        }

        private async Task<int> InsertDetails()
        {
            double height = ((VisitHeightFeet.SelectedIndex + 1) * 12 + VisitHeightInch.SelectedIndex) * 0.0254;
            int weight = Int32.Parse(VisitWeight.Text.ToString());
            string DateVisited = VisitYearComboBox.Items[VisitYearComboBox.SelectedIndex].ToString() + "-" + VisitMonthComboBox.Items[VisitMonthComboBox.SelectedIndex].ToString() + "-" + VisitDayComboBox.Items[VisitDayComboBox.SelectedIndex].ToString();
            double bmi = ExtraModules.CalculateBMI(VisitHeightFeet.SelectedIndex + 1, VisitHeightInch.SelectedIndex, weight);

            try
            {
                string insertQuery = "INSERT INTO MedicalDetails (TimeStamp ,PID, DateVisited, Age, BloodGlucose, SystolicBP, DiastolicBP, DiseaseFound, Height, Weight, Symptoms, BMI) " +
                                     "VALUES (@ts, @pid, @dv, @age, @bg, @sbp, @dbp, @disease, @height, @weight, @symptoms, @bmi)";
                Statement statement = await this.database.PrepareStatementAsync(insertQuery);
                statement.BindTextParameterWithName("@pid", this.PID);
                statement.BindTextParameterWithName("@ts", DateTime.Now.ToString(ExtraModules.datePatt));
                statement.BindTextParameterWithName("@dv", DateVisited);
                statement.BindIntParameterWithName("@age", await this.GetPatientAge(this.PID));
                if (!VisitBloodGlucose.Text.ToString().Equals(""))
                {
                    statement.BindIntParameterWithName("@bg", Int32.Parse(VisitBloodGlucose.Text.ToString()));
                }
                else
                {
                    statement.BindIntParameterWithName("@bg", 0);
                }
                if (!VisitDiastolicBP.Text.ToString().Equals(""))
                {
                    statement.BindIntParameterWithName("@dbp", Int32.Parse(VisitDiastolicBP.Text.ToString()));
                }
                else
                {
                    statement.BindIntParameterWithName("@dbp", 0);
                }
                if (!VisitSystolicBP.Text.ToString().Equals(""))
                {
                    statement.BindIntParameterWithName("@sbp", Int32.Parse(VisitSystolicBP.Text.ToString()));
                }
                else
                {
                    statement.BindIntParameterWithName("@sbp", 0);
                }
                statement.BindTextParameterWithName("@disease", VisitDiseasesDiagnosed.Text.ToString());
                statement.BindDoubleParameterWithName("@height", height);
                statement.BindIntParameterWithName("@weight", weight);
                statement.BindTextParameterWithName("@symptoms", ExtraModules.RemoveExtraCommas(ExtraModules.RemoveStringNewLine(VisitSymptoms.Text.ToString())));
                statement.BindDoubleParameterWithName("@bmi", bmi);

                await statement.StepAsync();
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_NEW_VISIT---INSERT_DETAILS---MEDIC_DETAILS" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                string insertMedicine = "INSERT INTO MedicalDetailsMedicine (TimeStamp, PID, DateVisited, Medicine) VALUES (@ts, @pid, @dv, @medicine)";
                foreach (string str in ExtraModules.RemoveExtraCommas(ExtraModules.RemoveStringNewLine(VisitMedicineGiven.Text.ToString())).Split(','))
                {
                    Statement statement = await this.database.PrepareStatementAsync(insertMedicine);
                    statement.BindTextParameterWithName("@pid", this.PID);
                    statement.BindTextParameterWithName("@ts", DateTime.Now.ToString(ExtraModules.datePatt));
                    statement.BindTextParameterWithName("@dv", DateVisited);
                    statement.BindTextParameterWithName("@medicine", ExtraModules.removesemicolon(str));

                    await statement.StepAsync();
                    statement.Reset();
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_NEW_VISIT---INSERT_DETAILS---MEDIC_DETAILS_MEDICINE" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                string insertVaccine = "INSERT INTO MedicalDetailsVaccine (TimeStamp, PID, DateVisited, Vaccine) VALUES (@ts, @pid, @dv, @vaccine)";

                foreach (string str in ExtraModules.RemoveExtraCommas(ExtraModules.RemoveStringNewLine(VisitVaccine.Text.ToString())).Split(','))
                {
                    Statement statement = await this.database.PrepareStatementAsync(insertVaccine);
                    statement.BindTextParameterWithName("@pid", this.PID);
                    statement.BindTextParameterWithName("@ts", DateTime.Now.ToString(ExtraModules.datePatt));
                    statement.BindTextParameterWithName("@dv", DateVisited);
                    statement.BindTextParameterWithName("@vaccine", ExtraModules.removesemicolon(str));

                    await statement.StepAsync();
                    statement.Reset();
                }
                this.ocString.Add(DateVisited);
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_NEW_VISIT---UPDATE_DETAILS---MEDIC_DETAILS_VACCINE" + "\n" + ex.Message + "\n" + result.ToString());
            }

            this.ClearAllFields();
            VisitListBox.SelectedIndex = this.ocString.IndexOf(DateVisited);

            if (this.ocString.Count() == 1)
            {
                this.visibleStackPanels();
            }

            return DBConnect.RESULT_OK;
        }

        private async Task<int> UpdateDetails()
        {
            double height = ((VisitHeightFeet.SelectedIndex + 1) * 12 + VisitHeightInch.SelectedIndex) * 0.0254;
            int weight = Int32.Parse(VisitWeight.Text.ToString());
            string DateVisited = VisitYearComboBox.Items[VisitYearComboBox.SelectedIndex].ToString() + "-" + VisitMonthComboBox.Items[VisitMonthComboBox.SelectedIndex].ToString() + "-" + VisitDayComboBox.Items[VisitDayComboBox.SelectedIndex].ToString();
            double bmi = ExtraModules.CalculateBMI(VisitHeightFeet.SelectedIndex + 1, VisitHeightInch.SelectedIndex, weight);

            try
            {
                string updateQuery = "UPDATE MedicalDetails SET TimeStamp = @ts, BloodGlucose = @bg , SystolicBP = @sbp , DiastolicBP = @dbp , DiseaseFound = @disease , Height = @height , Weight = @weight , Symptoms = @symptoms , BMI = @bmi  WHERE PID = @pid AND DateVisited = @dv";
                Statement statement = await this.database.PrepareStatementAsync(updateQuery);
                statement.BindTextParameterWithName("@pid", this.PID);
                statement.BindTextParameterWithName("@ts", DateTime.Now.ToString(ExtraModules.datePatt));
                statement.BindTextParameterWithName("@dv", DateVisited);
                if (!VisitBloodGlucose.Text.ToString().Equals(""))
                {
                    statement.BindIntParameterWithName("@bg", Int32.Parse(VisitBloodGlucose.Text.ToString()));
                }
                else
                {
                    statement.BindIntParameterWithName("@bg", 0);
                }
                if (!VisitDiastolicBP.Text.ToString().Equals(""))
                {
                    statement.BindIntParameterWithName("@dbp", Int32.Parse(VisitDiastolicBP.Text.ToString()));
                }
                else
                {
                    statement.BindIntParameterWithName("@dbp", 0);
                }
                if (!VisitSystolicBP.Text.ToString().Equals(""))
                {
                    statement.BindIntParameterWithName("@sbp", Int32.Parse(VisitSystolicBP.Text.ToString()));
                }
                else
                {
                    statement.BindIntParameterWithName("@sbp", 0);
                }
                statement.BindTextParameterWithName("@disease",ExtraModules.removesemicolon(VisitDiseasesDiagnosed.Text.ToString()));
                statement.BindDoubleParameterWithName("@height", height);
                statement.BindIntParameterWithName("@weight", weight);
                statement.BindTextParameterWithName("@symptoms", ExtraModules.removesemicolon(ExtraModules.RemoveExtraCommas(ExtraModules.RemoveStringNewLine(VisitSymptoms.Text.ToString()))));
                statement.BindDoubleParameterWithName("@bmi", bmi);

                await statement.StepAsync();
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_NEW_VISIT---UPDATE_DETAILS---MEDIC_DETAILS" + "\n" + ex.Message + "\n" + result.ToString());
            }

            //statement.Reset();
            //string insertBMI = "UPDATE MedicalDetailsBMI SET BMI = @bmi WHERE Height = @height AND Weight = @weight";
            //statement = await this.database.PrepareStatementAsync(insertBMI);
            //statement.BindDoubleParameterWithName("@height", height);
            //statement.BindIntParameterWithName("@weight", weight);
            //statement.BindDoubleParameterWithName("@bmi", bmi);
            //await statement.StepAsync();

            try
            {
                string deleteMedicine = "DELETE FROM MedicalDetailsMedicine WHERE PID = @pid AND DateVisited = @dv";
                Statement statement = await this.database.PrepareStatementAsync(deleteMedicine);
                statement.BindTextParameterWithName("@pid", this.PID);
                statement.BindTextParameterWithName("@dv", DateVisited);
                await statement.StepAsync();
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_NEW_VISIT---UPDATE_DETAILS---MEDIC_DETAILS_DEL" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                string deleteVaccine = "DELETE FROM MedicalDetailsVaccine WHERE PID = @pid AND DateVisited = @dv";
                Statement statement = await this.database.PrepareStatementAsync(deleteVaccine);
                statement.BindTextParameterWithName("@pid", this.PID);
                statement.BindTextParameterWithName("@dv", DateVisited);
                await statement.StepAsync();
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_NEW_VISIT---UPDATE_DETAILS---MEDIC_DETAILS_VACCINE_DEL" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                string insertMedicine = "INSERT INTO MedicalDetailsMedicine (TimeStamp, PID, DateVisited, Medicine) VALUES (@ts, @pid, @dv, @medicine)";

                foreach (string str in ExtraModules.RemoveExtraCommas(ExtraModules.RemoveStringNewLine(VisitMedicineGiven.Text.ToString())).Split(','))
                {
                    if (str != "")
                    {
                        Statement statement = await this.database.PrepareStatementAsync(insertMedicine);
                        statement.BindTextParameterWithName("@pid", this.PID);
                        statement.BindTextParameterWithName("@ts", DateTime.Now.ToString(ExtraModules.datePatt));
                        statement.BindTextParameterWithName("@dv", DateVisited);
                        statement.BindTextParameterWithName("@medicine", ExtraModules.removesemicolon(str));

                        await statement.StepAsync();
                        statement.Reset();
                    }
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_NEW_VISIT---UPDATE_DETAILS---MEDIC_DETAILS_MEDICINE_INSERT" + "\n" + ex.Message + "\n" + result.ToString());
            }

            try
            {
                string insertVaccine = "INSERT INTO MedicalDetailsVaccine (TimeStamp, PID, DateVisited, Vaccine) VALUES (@ts, @pid, @dv, @vaccine)";

                foreach (string str in ExtraModules.RemoveExtraCommas(ExtraModules.RemoveStringNewLine(VisitVaccine.Text.ToString())).Split(','))
                {
                    if (str != "")
                    {
                        Statement statement = await this.database.PrepareStatementAsync(insertVaccine);
                        statement.BindTextParameterWithName("@pid", this.PID);
                        statement.BindTextParameterWithName("@ts", DateTime.Now.ToString(ExtraModules.datePatt));
                        statement.BindTextParameterWithName("@dv", DateVisited);
                        statement.BindTextParameterWithName("@vaccine", ExtraModules.removesemicolon(str));

                        await statement.StepAsync();
                        statement.Reset();
                    }
                }
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_NEW_VISIT---UPDATE_DETAILS---MEDIC_DETAILS_VACCINE_INSERT" + "\n" + ex.Message + "\n" + result.ToString());
            }

            this.ClearAllFields();
            isUpdating = false;
            this.UpdateEditedDetails();

            return DBConnect.RESULT_OK;
        }

        private async void UpdateEditedDetails()
        {
            if (VisitListBox.SelectedItem != null)
            {
                try
                {
                    string query = "SELECT * FROM MedicalDetails WHERE PID = @pid AND DateVisited = @dv";
                    Statement statement = await this.database.PrepareStatementAsync(query);
                    statement.BindTextParameterWithName("@pid", this.PID);
                    statement.BindTextParameterWithName("@dv", VisitListBox.Items[VisitListBox.SelectedIndex].ToString());
                    statement.EnableColumnsProperty();
                    if (await statement.StepAsync())
                    {

                        VisitSymptomsPanel.Children.Clear();

                        foreach (string str in statement.Columns["Symptoms"].Split(','))
                        {
                            StackPanel VisitSymptomsStackPanels = new StackPanel();
                            VisitSymptomsStackPanels.Margin = new Thickness(0, 15, 0, 0);
                            VisitSymptomsStackPanels.Orientation = Orientation.Horizontal;

                            TextBlock dot = new TextBlock();
                            dot.Width = 10;
                            dot.FontSize = 15;
                            dot.Text = "•";
                            VisitSymptomsStackPanels.Children.Add(dot);

                            TextBlock vaccineName = new TextBlock();
                            vaccineName.Width = 280;
                            vaccineName.Text = ExtraModules.RemoveExtraCommas(ExtraModules.RemoveStringSpace(str));
                            vaccineName.TextWrapping = TextWrapping.Wrap;
                            vaccineName.FontSize = 15;
                            VisitSymptomsStackPanels.Children.Add(vaccineName);

                            VisitSymptomsPanel.Children.Add(VisitSymptomsStackPanels);
                        }

                        VisitTextDisease.Text = "\n" + statement.Columns["DiseaseFound"];
                        VisitTextBG.Text = statement.Columns["BloodGlucose"];
                        VisitTextBP.Text = statement.Columns["SystolicBP"] + "/" + statement.Columns["DiastolicBP"];

                        double BMIDouble = Convert.ToDouble(statement.Columns["BMI"]);
                        Double BMIRounded3 = Math.Round(BMIDouble, 3);
                        VisitTextBMI.Text = BMIRounded3.ToString();

                        VisitTextWeight.Text = statement.Columns["Weight"];
                        VisitTextHeight.Text = statement.Columns["Height"];
                    }
                }
                catch (Exception ex)
                {
                    var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                    Debug.WriteLine("CREATE_NEW_VISIT---UPDATE_EDITED_DETAILS" + "\n" + ex.Message + "\n" + result.ToString());
                }

                try
                {
                    string query = "SELECT * FROM MedicalDetailsMedicine WHERE PID = @pid AND DateVisited = @dv";
                    Statement statement = await this.database.PrepareStatementAsync(query);
                    statement.BindTextParameterWithName("@pid", this.PID);
                    statement.BindTextParameterWithName("@dv", VisitListBox.Items[VisitListBox.SelectedIndex].ToString());
                    statement.EnableColumnsProperty();

                    VisitTextMedicines.Children.Clear();
                    while (await statement.StepAsync())
                    {
                        StackPanel VisitMedicineStackPanels = new StackPanel();
                        VisitMedicineStackPanels.Margin = new Thickness(0, 15, 0, 0);
                        VisitMedicineStackPanels.Orientation = Orientation.Horizontal;

                        TextBlock dot = new TextBlock();
                        dot.Width = 10;
                        dot.FontSize = 15;
                        dot.Text = "•";
                        VisitMedicineStackPanels.Children.Add(dot);

                        TextBlock medicineName = new TextBlock();
                        medicineName.Width = 280;
                        medicineName.Text = ExtraModules.RemoveExtraCommas(ExtraModules.RemoveStringSpace(statement.Columns["Medicine"]));
                        medicineName.TextWrapping = TextWrapping.Wrap;
                        medicineName.FontSize = 15;
                        VisitMedicineStackPanels.Children.Add(medicineName);

                        VisitTextMedicines.Children.Add(VisitMedicineStackPanels);
                    }
                }
                catch (Exception ex)
                {
                    var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                    Debug.WriteLine("CREATE_NEW_VISIT---UPDATE_EDITED_DETAILS---MEDICINE" + "\n" + ex.Message + "\n" + result.ToString());
                }

                try
                {
                    string query = "SELECT * FROM MedicalDetailsVaccine WHERE PID = @pid AND DateVisited = @dv";
                    Statement statement = await this.database.PrepareStatementAsync(query);
                    statement.BindTextParameterWithName("@pid", this.PID);
                    statement.BindTextParameterWithName("@dv", VisitListBox.Items[VisitListBox.SelectedIndex].ToString());
                    statement.EnableColumnsProperty();

                    VisitTextVaccine.Children.Clear();
                    while (await statement.StepAsync())
                    {
                        if (statement.Columns["Vaccine"].Equals(""))
                        {
                            break;
                        }

                        StackPanel VisitVaccineStackPanels = new StackPanel();
                        VisitVaccineStackPanels.Margin = new Thickness(0, 15, 0, 0);
                        VisitVaccineStackPanels.Orientation = Orientation.Horizontal;

                        TextBlock dot = new TextBlock();
                        dot.Width = 10;
                        dot.FontSize = 15;
                        dot.Text = "•";
                        VisitVaccineStackPanels.Children.Add(dot);

                        TextBlock VaccineName = new TextBlock();
                        VaccineName.Width = 280;
                        VaccineName.Text = ExtraModules.RemoveExtraCommas(ExtraModules.RemoveStringSpace(statement.Columns["Vaccine"]));
                        VaccineName.TextWrapping = TextWrapping.Wrap;
                        VaccineName.FontSize = 15;
                        VisitVaccineStackPanels.Children.Add(VaccineName);

                        VisitTextVaccine.Children.Add(VisitVaccineStackPanels);
                    }
                }
                catch (Exception ex)
                {
                    var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                    Debug.WriteLine("CREATE_NEW_VISIT---UPDATE_EDITED_DETAILS---VACCINE" + "\n" + ex.Message + "\n" + result.ToString());
                }

                this.visibleStackPanels();
            }
        }

        private async Task<int> GetPatientAge(string p)
        {
            try
            {
                string query = "SELECT Birthday FROM Patient WHERE PID = @pid";
                Statement statement = await this.database.PrepareStatementAsync(query);
                statement.BindTextParameterWithName("@pid", p);
                statement.EnableColumnsProperty();
                if (await statement.StepAsync())
                {
                    //Debug.WriteLine(Int32.Parse(statement.Columns["Birthday"].Split('-')[0]));
                    int age = DateTime.Now.Year - Int32.Parse(statement.GetTextAt(0).Split('-')[0]);
                    return age;
                }

                return DBConnect.RESULT_ERROR;
            }
            catch (Exception ex)
            {
                var result = SQLiteWinRT.Database.GetSqliteErrorCode(ex.HResult);
                Debug.WriteLine("CREATE_NEW_VISIT---GET_PATIENT_AGE" + "\n" + ex.Message + "\n" + result.ToString());

                return DBConnect.RESULT_ERROR;
            }
        }

        private void visitSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count() < 0)
                return;

            this.UpdateEditedDetails();
        }

        //For sorting the date
        private void SortAscending(object sender, RoutedEventArgs e)
        {
            List<string> searchList = ocString.ToList();
            List<DateTime> dateList = new List<DateTime>();
            for (int i = 0; i < searchList.Count(); i++)
            {
                dateList.Add(Convert.ToDateTime(searchList.ElementAt(i)));
            }

            ocString.Clear();

            dateList.Sort(delegate(DateTime c1, DateTime c2)
            {
                return c1.CompareTo(c2);
            });

            searchList.Clear();

            for (int i = 0; i < dateList.Count(); i++)
            {
                searchList.Add(Convert.ToString(dateList.ElementAt(i).ToString("yyyy-MMMM-dd")));
            }

            this.ocString = new ObservableCollection<string>(searchList);
            searchList.Clear();
            this.VisitListBox.ItemsSource = this.ocString;
        }

        private void SortDescending(object sender, RoutedEventArgs e)
        {
            List<string> searchList = ocString.ToList();
            List<DateTime> dateList = new List<DateTime>();

            for (int i = 0; i < searchList.Count(); i++)
            {
                dateList.Add(Convert.ToDateTime(searchList.ElementAt(i)));
            }

            ocString.Clear();

            dateList.Sort(delegate(DateTime c1, DateTime c2)
            {
                return c2.CompareTo(c1);
            });

            searchList.Clear();

            for (int i = 0; i < dateList.Count(); i++)
            {
                searchList.Add(Convert.ToString(dateList.ElementAt(i).ToString("yyyy-MMMM-dd")));
            }

            this.ocString = new ObservableCollection<string>(searchList);
            searchList.Clear();
            this.VisitListBox.ItemsSource = this.ocString;
        }

        //Validation for numeric entries in weight, bp and glucose
        private void numberValidation_decimal(object sender, KeyRoutedEventArgs e)
        {
            if (((uint)e.Key >= (uint)Windows.System.VirtualKey.Number0
          && (uint)e.Key <= (uint)Windows.System.VirtualKey.Number9) || ((uint)e.Key >= (uint)Windows.System.VirtualKey.NumberPad0 && (uint)e.Key <= (uint)Windows.System.VirtualKey.NumberPad9) || (uint)e.Key == (uint)Windows.System.VirtualKey.Tab || (uint)e.Key == (uint)Windows.System.VirtualKey.Decimal)
            {
                e.Handled = false;
            }
            else e.Handled = true;
        }

        private void numberValidation_integer(object sender, KeyRoutedEventArgs e)
        {
            if (((uint)e.Key >= (uint)Windows.System.VirtualKey.Number0
          && (uint)e.Key <= (uint)Windows.System.VirtualKey.Number9) || ((uint)e.Key >= (uint)Windows.System.VirtualKey.NumberPad0 && (uint)e.Key <= (uint)Windows.System.VirtualKey.NumberPad9) || (uint)e.Key == (uint)Windows.System.VirtualKey.Tab)
            {
                e.Handled = false;
            }
            else e.Handled = true;
        }

        private async Task<bool> CheckIfFilled()
        {
            int temp;
            bool ret = true;
            VisitDiseasesDiagnosed.ClearValue(BorderBrushProperty);
            VisitSymptoms.ClearValue(BorderBrushProperty);
            VisitMedicineGiven.ClearValue(BorderBrushProperty);
            VisitWeight.ClearValue(BorderBrushProperty);
            VisitHeightFeet.ClearValue(BorderBrushProperty);
            VisitHeightInch.ClearValue(BorderBrushProperty);
            VisitMonthComboBox.ClearValue(BorderBrushProperty);
            VisitDayComboBox.ClearValue(BorderBrushProperty);
            VisitYearComboBox.ClearValue(BorderBrushProperty);

            if (!isUpdating && (ocString.Contains(VisitYearComboBox.Items[VisitYearComboBox.SelectedIndex] + "-" + VisitMonthComboBox.Items[VisitMonthComboBox.SelectedIndex] + "-" + VisitDayComboBox.Items[VisitDayComboBox.SelectedIndex])))
            {
                check = false;
                var messageDialog = new Windows.UI.Popups.MessageDialog("You cannot select the same date again.", "Error!");
                messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("Okay", null));
                var dialogResult = await messageDialog.ShowAsync();
                VisitDayComboBox.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                VisitYearComboBox.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                VisitMonthComboBox.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);

                return false;
            }
            else
            {
                check = true;
            }

            if (VisitDiseasesDiagnosed.Text.Equals(""))
            {
                VisitDiseasesDiagnosed.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                ret = false;
            }
            if (VisitSymptoms.Text.Equals(""))
            {
                VisitSymptoms.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                ret = false;
            }
            if (VisitMedicineGiven.Text.Equals(""))
            {
                VisitMedicineGiven.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                ret = false;
            }
            if (VisitWeight.Text.Equals("") || !int.TryParse(VisitWeight.Text.ToString(), out temp))
            {
                VisitWeight.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                ret = false;
            }
            //if (VisitSystolicBP.Text.Equals("") || !int.TryParse(VisitSystolicBP.Text.ToString(), out temp))
            //{
            //    VisitSystolicBP.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
            //    ret = false;
            //}
            //if (VisitDiastolicBP.Text.Equals("") || !int.TryParse(VisitDiastolicBP.Text.ToString(), out temp))
            //{
            //    VisitDiastolicBP.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
            //    ret = false;
            //}
            //if (VisitBloodGlucose.Text.Equals("") || !int.TryParse(VisitBloodGlucose.Text.ToString(), out temp))
            //{
            //    VisitBloodGlucose.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
            //    ret = false;
            //}
            if (VisitHeightFeet.SelectedItem == null)
            {
                VisitHeightFeet.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                ret = false;
            }
            if (VisitHeightInch.SelectedItem == null)
            {
                VisitHeightInch.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                ret = false;
            }
            return ret;
        }

        public void collapseStackPanels()
        {
            VisitStackPanel1.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            VisitStackPanel2.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            VisitStackPanel3.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            EditVisit.IsEnabled = false;
            DeleteVisit.IsEnabled = false;

        }

        public void visibleStackPanels()
        {
            VisitStackPanel1.Visibility = Windows.UI.Xaml.Visibility.Visible;
            VisitStackPanel2.Visibility = Windows.UI.Xaml.Visibility.Visible;
            VisitStackPanel3.Visibility = Windows.UI.Xaml.Visibility.Visible;

            EditVisit.IsEnabled = true;
            DeleteVisit.IsEnabled = true;
        }

        private void resetAllBorders()
        {
            VisitDiseasesDiagnosed.BorderBrush = new SolidColorBrush(Windows.UI.Colors.White);

            VisitSymptoms.BorderBrush = new SolidColorBrush(Windows.UI.Colors.White);

            VisitMedicineGiven.BorderBrush = new SolidColorBrush(Windows.UI.Colors.White);

            VisitWeight.BorderBrush = new SolidColorBrush(Windows.UI.Colors.White);

            VisitSystolicBP.BorderBrush = new SolidColorBrush(Windows.UI.Colors.White);

            VisitDiastolicBP.BorderBrush = new SolidColorBrush(Windows.UI.Colors.White);

            VisitBloodGlucose.BorderBrush = new SolidColorBrush(Windows.UI.Colors.White);

            VisitHeightFeet.BorderBrush = new SolidColorBrush(Windows.UI.Colors.White);

            VisitHeightInch.BorderBrush = new SolidColorBrush(Windows.UI.Colors.White);
        }

        private void ClearAllFields()
        {
            VisitSymptoms.Text = "";
            VisitDiseasesDiagnosed.Text = "";
            VisitMedicineGiven.Text = "";
            VisitBloodGlucose.Text = "";
            VisitSystolicBP.Text = "";
            VisitDiastolicBP.Text = "";
            VisitVaccine.Text = "";
            VisitHeightFeet.SelectedItem = null;
            VisitHeightInch.SelectedItem = null;
            VisitWeight.Text = "";
            isUpdating = false;
        }

        private void setHeightBoxFromMeterHeight(double meter)
        {
            double totalInchHeight = meter * 39.3701;
            double inchHeight = totalInchHeight % 12;
            double feetHeight = (totalInchHeight - inchHeight) / 12;

            int itemFeetHeight = Convert.ToInt32(Math.Round(feetHeight));
            int itemInchHeight = Convert.ToInt32(Math.Round(inchHeight));

            if (itemInchHeight == 12)
            {
                itemFeetHeight += 1;
                itemInchHeight = 0;
            }

            VisitHeightFeet.SelectedIndex = VisitHeightFeet.Items.IndexOf(itemFeetHeight.ToString());
            VisitHeightInch.SelectedIndex = VisitHeightInch.Items.IndexOf(itemInchHeight.ToString());
        }
        private void navigateBack(object sender, KeyRoutedEventArgs e)
        {
            if ((uint)e.Key == (uint)Windows.System.VirtualKey.Back)
            {
                NavigationHelper.GoBack();
            }
        }

        private void CreateNewVisitSettingsClicked(object sender, RoutedEventArgs e)
        {
            String hexaColor = "#00A2E8";
            Color color = Color.FromArgb(255, Convert.ToByte(hexaColor.Substring(1, 2), 16), Convert.ToByte(hexaColor.Substring(3, 2), 16), Convert.ToByte(hexaColor.Substring(5, 2), 16));
            settings.HeaderBackground = new SolidColorBrush(color);
            settings.Background = new SolidColorBrush(color);
            settings.ShowCustom();
        }
    }
}

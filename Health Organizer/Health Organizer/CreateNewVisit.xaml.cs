using Health_Organizer.Common;
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


namespace Health_Organizer
{
    public sealed partial class CreateNewVisit : Page
    {

        private NavigationHelper navigationHelper;
        private DBConnect connection;
        private string decodedImage = null;
        private Database database;
        private int PID = 0;
        ObservableCollection<string> ocString;
        private bool isUpdating = false;
        Boolean check = true;
        int counterComma = 0;
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
            VisitListBox.ItemsSource = this.ocString;
            this.InitializeVisitDetialsComboBox();
            this.InitializeDB(1);
            this.PID = 1;
        }

        private async void InitializeDB(int PID)
        {
            this.connection = new DBConnect();
            await this.connection.InitializeDatabase(DBConnect.ORG_HOME_DB);
            database = this.connection.GetConnection();

            string query = "SELECT * FROM MedicalDetails";
            Statement statement = await this.database.PrepareStatementAsync(query);
            //statement.BindIntParameterWithName("@pid", PID);
            statement.EnableColumnsProperty();
            while (await statement.StepAsync())
            {
                Debug.WriteLine(statement.Columns["DateVisited"]);
                this.ocString.Add(statement.Columns["DateVisited"]);
            }
            this.loadPatientDetails();
        }

        private async void loadPatientDetails()
        {
            string q = "SELECT * FROM Patient WHERE PID = @pid";
            Statement s = await this.database.PrepareStatementAsync(q);
            s.BindIntParameterWithName("@pid", this.PID);
            s.EnableColumnsProperty();
            if (await s.StepAsync())
            {
                Debug.WriteLine(s.Columns["PID"]);
                visitPatientName.Text = s.Columns["FirstName"] + s.Columns["LastName"];
                visitPatientPhoto.Source = await ImageMethods.Base64StringToBitmap(s.Columns["Image"]);
            }
        }

        private void InitializeVisitDetialsComboBox()
        {
            this.ClearAllFields();
            //Adding days and years to combobox in form
            for (int i = 0; i < 31; i++)
            {
                visitDayComboBox.Items.Add(i + 1);
            }

            for (int i = 2000; i <= DateTime.Now.Year; i++)
            {
                visitYearComboBox.Items.Add(i);
            }

            //Set current date in form
            visitDayComboBox.SelectedItem = DateTime.Now.Day;
            visitMonthComboBox.SelectedIndex = DateTime.Now.Month - 1;
            visitYearComboBox.SelectedItem = DateTime.Now.Year;
        }


        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

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

        private void AddVisitClicked(object sender, RoutedEventArgs e)
        {
            VisitFormCmdbar.IsOpen = false;
            VisitFormBar.IsOpen = true;
            counterComma = 0;
        }

        private async void EditVisitClicked(object sender, RoutedEventArgs e)
        {
            counterComma = 0;
            VisitFormCmdbar.IsOpen = false;
            if (VisitListBox.SelectedItem != null)
            {
                VisitFormBar.IsOpen = true;
                string query = "SELECT * FROM MedicalDetails WHERE PID = @pid AND DateVisited = @dv";
                Statement statement = await this.database.PrepareStatementAsync(query);
                statement.BindIntParameterWithName("@pid", this.PID);
                statement.BindTextParameterWithName("@dv", VisitListBox.Items[VisitListBox.SelectedIndex].ToString());
                statement.EnableColumnsProperty();
                if (await statement.StepAsync())
                {
                    Debug.WriteLine(statement.Columns["DiastolicBP"] + statement.Columns["PID"]);
                    string[] dv = statement.Columns["DateVisited"].Split('-');
                    string[] height = statement.Columns["Height"].ToString().Split('.');

                    visitDayComboBox.SelectedIndex = visitDayComboBox.Items.IndexOf(Int32.Parse(dv[2]));
                    visitMonthComboBox.SelectedIndex = visitMonthComboBox.Items.IndexOf(dv[1]);
                    visitYearComboBox.SelectedIndex = visitYearComboBox.Items.IndexOf(Int32.Parse(dv[0]));
                    VisitSymptoms.Text = statement.Columns["Symptoms"];
                    VisitDiseasesDiagnosed.Text = statement.Columns["DiseaseFound"];

                    VisitHeightFeet.SelectedIndex = VisitHeightFeet.Items.IndexOf(height[0]);
                    VisitHeightInch.SelectedIndex = VisitHeightInch.Items.IndexOf(height[1]);
                    VisitWeight.Text = statement.Columns["Weight"];
                    VisitSystolicBP.Text = statement.Columns["SystolicBP"];
                    VisitDiastolicBP.Text = statement.Columns["DiastolicBP"];
                    VisitBloodGlucose.Text = statement.Columns["BloodGlucose"];
                }

                statement.Reset();
                query = "SELECT Medicine FROM MedicalDetailsMedicine WHERE PID = @pid AND DateVisited = @dv";
                statement = await this.database.PrepareStatementAsync(query);
                statement.BindIntParameterWithName("@pid", this.PID);
                statement.BindTextParameterWithName("@dv", VisitListBox.Items[VisitListBox.SelectedIndex].ToString());
                statement.EnableColumnsProperty();
                VisitMedicineGiven.Text = "";
                while (await statement.StepAsync())
                {
                    //Debug.WriteLine(statement.Columns["Medicine"]);
                    VisitMedicineGiven.Text += statement.Columns["Medicine"] + ",";
                }

                statement.Reset();
                query = "SELECT Vaccine FROM MedicalDetailsVaccine WHERE PID = @pid AND DateVisited = @dv";
                statement = await this.database.PrepareStatementAsync(query);
                statement.BindIntParameterWithName("@pid", this.PID);
                statement.BindTextParameterWithName("@dv", VisitListBox.Items[VisitListBox.SelectedIndex].ToString());
                statement.EnableColumnsProperty();
                VisitVaccine.Text = "";
                while (await statement.StepAsync())
                {
                    //Debug.WriteLine(statement.Columns["Vaccine"]);
                    VisitVaccine.Text += statement.Columns["Vaccine"] + ",";
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
                string deleteQuery = "DELETE FROM MedicalDetails WHERE PID = @pid AND DateVisited = @dv";
                Statement statement = await this.database.PrepareStatementAsync(deleteQuery);
                statement.BindIntParameterWithName("@pid", this.PID);
                statement.BindTextParameterWithName("@dv", DateVisited);
                await statement.StepAsync();
                this.ocString.Remove(DateVisited);

                statement.Reset();
                deleteQuery = "DELETE FROM MedicalDetailsVaccine WHERE PID = @pid AND DateVisited = @dv";
                statement = await this.database.PrepareStatementAsync(deleteQuery);
                statement.BindIntParameterWithName("@pid", this.PID);
                statement.BindTextParameterWithName("@dv", DateVisited);
                while (await statement.StepAsync())
                {
                    Debug.WriteLine(statement.Columns["Vaccine"]);
                }

                statement.Reset();
                deleteQuery = "DELETE FROM MedicalDetailsMedicine WHERE PID = @pid AND DateVisited = @dv";
                statement = await this.database.PrepareStatementAsync(deleteQuery);
                statement.BindIntParameterWithName("@pid", this.PID);
                statement.BindTextParameterWithName("@dv", DateVisited);
                while (await statement.StepAsync())
                {
                    Debug.WriteLine(statement.Columns["Medicine"]);
                }
            }
        }

        private async void VisitSaveClicked(object sender, RoutedEventArgs e)
        {
            bool x = await this.CheckIfFilled();

            if (x)
            {
                try
                {
                    if (isUpdating)
                    {
                        await this.UpdateDetails();
                    }
                    else
                    {
                        await this.InsertDetails();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                VisitFormBar.IsOpen = false;
            }
            else
            {
                if (check)
                {
                    var messageDialog = new Windows.UI.Popups.MessageDialog("Please complete the form before saving it.", "Error!");
                    messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("Okay", null));
                    var dialogResult = await messageDialog.ShowAsync();
                }
            }
        }

        private async Task<int> UpdateDetails()
        {
            double height = Double.Parse(VisitHeightFeet.Items[VisitHeightFeet.SelectedIndex].ToString() + "." + VisitHeightInch.Items[VisitHeightInch.SelectedIndex].ToString());
            int weight = Int32.Parse(VisitWeight.Text.ToString());
            string DateVisited = visitYearComboBox.Items[visitYearComboBox.SelectedIndex].ToString() + "-" + visitMonthComboBox.Items[visitMonthComboBox.SelectedIndex].ToString() + "-" + visitDayComboBox.Items[visitDayComboBox.SelectedIndex].ToString();
            double bmi = (1.0 * height) / weight;

            string updateQuery = "UPDATE MedicalDetails SET BloodGlucose = @bg , SystolicBP = @sbp , DiastolicBP = @dbp , DiseaseFound = @disease , Height = @height , Weight = @weight , Symptoms = @symptoms , BMI = @bmi  WHERE PID = @pid AND DateVisited = @dv";
            Statement statement = await this.database.PrepareStatementAsync(updateQuery);
            statement.BindIntParameterWithName("@pid", this.PID);
            statement.BindTextParameterWithName("@dv", DateVisited);
            statement.BindIntParameterWithName("@bg", Int32.Parse(VisitBloodGlucose.Text.ToString()));
            statement.BindIntParameterWithName("@dbp", Int32.Parse(VisitDiastolicBP.Text.ToString()));
            statement.BindIntParameterWithName("@sbp", Int32.Parse(VisitSystolicBP.Text.ToString()));
            statement.BindTextParameterWithName("@disease", VisitDiseasesDiagnosed.Text.ToString());
            statement.BindDoubleParameterWithName("@height", height);
            statement.BindIntParameterWithName("@weight", weight);
            statement.BindTextParameterWithName("@symptoms", VisitSymptoms.Text.ToString());
            statement.BindDoubleParameterWithName("@bmi", bmi);

            await statement.StepAsync();

            //statement.Reset();
            //string insertBMI = "UPDATE MedicalDetailsBMI SET BMI = @bmi WHERE Height = @height AND Weight = @weight";
            //statement = await this.database.PrepareStatementAsync(insertBMI);
            //statement.BindDoubleParameterWithName("@height", height);
            //statement.BindIntParameterWithName("@weight", weight);
            //statement.BindDoubleParameterWithName("@bmi", bmi);

            //await statement.StepAsync();

            //statement.Reset();
            //string updateMedicine = "UPDATE MedicalDetailsMedicine SET Medicine = @medicine WHERE PID = @pid AND DateVisited = @dv";

            //foreach (string str in VisitMedicineGiven.Text.Split(','))
            //{
            //    statement = await this.database.PrepareStatementAsync(updateMedicine);
            //    statement.BindIntParameterWithName("@pid", this.PID);
            //    statement.BindTextParameterWithName("@dv", DateVisited);
            //    statement.BindTextParameterWithName("@medicine", str);

            //    await statement.StepAsync();
            //    statement.Reset();
            //}

            //string updateVaccine = "UPDATE MedicalDetailsVaccine SET Vaccine = @vaccine WHERE PID = @pid AND DateVisited = @dv";
            //foreach (string str in VisitVaccine.Text.Split(','))
            //{
            //    statement = await this.database.PrepareStatementAsync(updateVaccine);
            //    statement.BindIntParameterWithName("@pid", this.PID);
            //    statement.BindTextParameterWithName("@dv", DateVisited);
            //    statement.BindTextParameterWithName("@vaccine", str);

            //    await statement.StepAsync();
            //    statement.Reset();
            //}
            this.ClearAllFields();
            isUpdating = false;

            return 1;
        }

        private async Task<int> InsertDetails()
        {
            double height = Double.Parse(VisitHeightFeet.Items[VisitHeightFeet.SelectedIndex].ToString() + "." + VisitHeightInch.Items[VisitHeightInch.SelectedIndex].ToString());
            int weight = Int32.Parse(VisitWeight.Text.ToString());
            string DateVisited = visitYearComboBox.Items[visitYearComboBox.SelectedIndex].ToString() + "-" + visitMonthComboBox.Items[visitMonthComboBox.SelectedIndex].ToString() + "-" + visitDayComboBox.Items[visitDayComboBox.SelectedIndex].ToString();
            double bmi = 1.0 * height / weight;

            string insertQuery = "INSERT INTO MedicalDetails (PID, DateVisited, Age, BloodGlucose, SystolicBP, DiastolicBP, DiseaseFound, Height, Weight, Symptoms, BMI) " +
                                 "VALUES (@pid, @dv, @age, @bg, @sbp, @dbp, @disease, @height, @weight, @symptoms, @bmi)";
            Statement statement = await this.database.PrepareStatementAsync(insertQuery);
            statement.BindIntParameterWithName("@pid", this.PID);
            statement.BindTextParameterWithName("@dv", DateVisited);
            statement.BindIntParameterWithName("@age", await this.GetPatientAge(this.PID));
            statement.BindIntParameterWithName("@bg", Int32.Parse(VisitBloodGlucose.Text.ToString()));
            statement.BindIntParameterWithName("@dbp", Int32.Parse(VisitDiastolicBP.Text.ToString()));
            statement.BindIntParameterWithName("@sbp", Int32.Parse(VisitSystolicBP.Text.ToString()));
            statement.BindTextParameterWithName("@disease", VisitDiseasesDiagnosed.Text.ToString());
            statement.BindDoubleParameterWithName("@height", height);
            statement.BindIntParameterWithName("@weight", weight);
            statement.BindTextParameterWithName("@symptoms", VisitSymptoms.Text.ToString());
            statement.BindDoubleParameterWithName("@bmi", bmi);

            await statement.StepAsync();

            //statement.Reset();
            //double bmi = 1.0 * height / weight;
            //string insertBMI = "INSERT INTO MedicalDetailsBMI (Height, Weight, BMI) VALUES (@height, @weight, @bmi)";
            //statement = await this.database.PrepareStatementAsync(insertBMI);
            //statement.BindDoubleParameterWithName("@height", height);
            //statement.BindIntParameterWithName("@weight", weight);
            //statement.BindDoubleParameterWithName("@bmi", bmi);
            //await statement.StepAsync();

            statement.Reset();
            string insertMedicine = "INSERT INTO MedicalDetailsMedicine (PID, DateVisited, Medicine) VALUES (@pid, @dv, @medicine)";

            foreach (string str in VisitMedicineGiven.Text.Split(','))
            {
                statement = await this.database.PrepareStatementAsync(insertMedicine);
                statement.BindIntParameterWithName("@pid", this.PID);
                statement.BindTextParameterWithName("@dv", DateVisited);
                statement.BindTextParameterWithName("@medicine", str);

                await statement.StepAsync();
                statement.Reset();
            }

            string insertVaccine = "INSERT INTO MedicalDetailsVaccine (PID, DateVisited, Vaccine) VALUES (@pid, @dv, @vaccine)";

            foreach (string str in VisitVaccine.Text.Split(','))
            {
                statement = await this.database.PrepareStatementAsync(insertVaccine);
                statement.BindIntParameterWithName("@pid", this.PID);
                statement.BindTextParameterWithName("@dv", DateVisited);
                statement.BindTextParameterWithName("@vaccine", str);

                await statement.StepAsync();
                statement.Reset();
            }
            this.ocString.Add(DateVisited);
            this.ClearAllFields();

            return 1;
        }

        private async Task<int> GetPatientAge(int p)
        {
            string query = "SELECT Birthday FROM Patient WHERE PID = @pid";
            Statement statement = await this.database.PrepareStatementAsync(query);
            statement.BindIntParameterWithName("@pid", p);
            statement.EnableColumnsProperty();
            if (await statement.StepAsync())
            {
                Debug.WriteLine(Int32.Parse(statement.Columns["Birthday"].Split('-')[0]));
                int age = DateTime.Now.Year - Int32.Parse(statement.GetTextAt(0).Split('-')[0]);
                return age;
            }
            return -1;
        }

        private async Task<bool> CheckIfFilled()
        {
            VisitDiseasesDiagnosed.ClearValue(BorderBrushProperty);
            VisitSymptoms.ClearValue(BorderBrushProperty);
            VisitMedicineGiven.ClearValue(BorderBrushProperty);
            VisitWeight.ClearValue(BorderBrushProperty);
            VisitHeightFeet.ClearValue(BorderBrushProperty);
            VisitHeightInch.ClearValue(BorderBrushProperty);
            visitMonthComboBox.ClearValue(BorderBrushProperty);
            visitDayComboBox.ClearValue(BorderBrushProperty);
            visitYearComboBox.ClearValue(BorderBrushProperty);

           
            if (VisitDiseasesDiagnosed.Text.Equals("") || VisitSymptoms.Text.Equals("") ||
                VisitMedicineGiven.Text.Equals("") || VisitWeight.Text.Equals("") || VisitHeightFeet.SelectedItem == null || VisitHeightInch.SelectedItem == null ||
                (ocString.Contains(visitYearComboBox.SelectedItem + "-" + visitMonthComboBox.SelectedItem + "-" + visitDayComboBox.SelectedItem)))
            {
               
                if ((ocString.Contains(visitYearComboBox.SelectedItem + "-" + visitMonthComboBox.SelectedItem + "-" + visitDayComboBox.SelectedItem)))
                {
                    check = false;
                    var messageDialog = new Windows.UI.Popups.MessageDialog("You cannot select the same date again.", "Error!");
                    messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("Okay", null));
                    var dialogResult = await messageDialog.ShowAsync();
                    visitDayComboBox.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                    visitYearComboBox.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                    visitMonthComboBox.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                }
                else
                {
                    check = true;
                }
                if (VisitDiseasesDiagnosed.Text.Equals(""))
                {
                    VisitDiseasesDiagnosed.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                }
                if (VisitSymptoms.Text.Equals(""))
                {
                    VisitSymptoms.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                }
                if (VisitMedicineGiven.Text.Equals(""))
                {
                    VisitMedicineGiven.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                }
                if (VisitWeight.Text.Equals(""))
                {
                    VisitWeight.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                }
                if (VisitHeightFeet.SelectedItem == null)
                {
                    VisitHeightFeet.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);

                }
                if (VisitHeightInch.SelectedItem == null)
                {
                    VisitHeightInch.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        private void VisitCancelClicked(object sender, RoutedEventArgs e)
        {
            this.ClearAllFields();
            VisitFormBar.IsOpen = false;
        }

        private async void visitSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count() < 0)
                return;

            string query = "SELECT * FROM MedicalDetails WHERE PID = @pid AND DateVisited = @dv";
            Statement statement = await this.database.PrepareStatementAsync(query);
            statement.BindIntParameterWithName("@pid", this.PID);
            statement.BindTextParameterWithName("@dv", VisitListBox.Items[VisitListBox.SelectedIndex].ToString());
            statement.EnableColumnsProperty();
            if (await statement.StepAsync())
            {
                Debug.WriteLine("okay selected");
                VisitTextSymptoms.Text = "";
                foreach (string str in statement.Columns["Symptoms"].Split(','))
                {
                    VisitTextSymptoms.Text += "\n• " + str;
                }

                VisitTextDisease.Text = "\n" + statement.Columns["DiseaseFound"];
                VisitTextBG.Text = statement.Columns["BloodGlucose"];
                VisitTextBP.Text = statement.Columns["SystolicBP"] + "/" + statement.Columns["DiastolicBP"];
                VisitTextBMI.Text = statement.Columns["BMI"];
                VisitTextWeight.Text = statement.Columns["Weight"];
                VisitTextHeight.Text = statement.Columns["Height"];
            }

            statement.Reset();
            query = "SELECT * FROM MedicalDetailsMedicine WHERE PID = @pid AND DateVisited = @dv";
            statement = await this.database.PrepareStatementAsync(query);
            statement.BindIntParameterWithName("@pid", this.PID);
            statement.BindTextParameterWithName("@dv", VisitListBox.Items[VisitListBox.SelectedIndex].ToString());
            statement.EnableColumnsProperty();
            VisitTextMedicines.Text = "";
            while (await statement.StepAsync())
            {
                VisitTextMedicines.Text += "\n• " + statement.Columns["Medicine"];
            }
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
        private void commaKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (counterComma >= 1 && ((uint)e.Key == 188))
            {
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }

            if (((uint)e.Key == 188))
            {
                counterComma++;
            }
            else
            {
                counterComma = 0;
            }

        }

        private void ViewProfileClicked(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(ProfileDetailsPage));
            }
        }

    }
}

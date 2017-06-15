using Microsoft.Win32;
using MonthlyShifts.Properties;
using MonthlyShifts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MonthlyShifts
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<Participant> _participants;
        private ExcelWorksheet _worksheet;
        private Dictionary<int, string> _months;
        private int _cols;

        #region ctor
        public MainWindow()
        {
            InitializeComponent();
            InitializeMonths();
            _participants = new ObservableCollection<Participant>();
            //listViewPeopleList.ItemsSource = _participants;
        }

        private void InitializeMonths()
        {
            _months = new Dictionary<int, string>();
            for (int i = 1; i <= 12; i++)
            {
                _months.Add(i, GenerateMonthInHebrew(i));
            }
        }
        #endregion

        #region Ships
        private void MakeShips()
        {
            CreateShipsFromDictionary(ReadShipsFromConfigFile());
        }

        private Dictionary<string, string[]> ReadShipsFromConfigFile()
        {
            try
            {
                return File.ReadAllLines(Settings.Default.SHIPS_FILE).Select(l => l.Split(',')).ToDictionary(l => l[0], l => l.Skip(1).ToArray());
            }
            catch (Exception e)
            {
                MessageBox.Show("בעיה התרחשה בנסיון לקרוא את כמות המשמרות השבועיות. נא וודא כי קובץ המשמרות השבועיות קיים.");
                throw e;
            }
        }

        private void CreateShipsFromDictionary(Dictionary<string, string[]> dictionary)
        {
            DateTime currentDate = SetFirstDate();
            int relevantMonth = currentDate.Month;
            int row = 2;
            while (currentDate.Month == relevantMonth)
            {
                _worksheet.Cells[row, 1].Value = string.Format("{0} {1}/{2}", TranslateDayToHebrew(currentDate.DayOfWeek.ToString()), currentDate.Day, currentDate.Month);
                System.Drawing.Color color = System.Drawing.Color.FromKnownColor((KnownColor)((int)currentDate.DayOfWeek % 3 + 40));
                foreach (var ship in dictionary[currentDate.DayOfWeek.ToString()])
                {
                    SetRowStyle(row, color);
                    _worksheet.Cells[row++, 2].Value = ship;
                }
                _worksheet.Cells[2, 1, row, 2].AutoFitColumns();
                currentDate = currentDate.AddDays(1).DayOfWeek == DayOfWeek.Saturday ? currentDate.AddDays(2) : currentDate.AddDays(1);
            }
        }
        #endregion

        #region etc
        private DateTime SetFirstDate()
        {
            DateTime displayDate = monthPicker.DisplayDate;
            return new DateTime(displayDate.Year, displayDate.Month, 1).DayOfWeek == DayOfWeek.Saturday ? new DateTime(displayDate.Year, displayDate.Month, 2) : new DateTime(displayDate.Year, displayDate.Month, 1);
        }

        private void SetRowStyle(int row, System.Drawing.Color color)
        {
            _worksheet.Cells[row, 1, row, 2].Style.Font.Bold = true;
            _worksheet.Cells[row, 1, row, _cols].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            _worksheet.Cells[row, 1, row, _cols].Style.Fill.BackgroundColor.SetColor(color);
            _worksheet.Cells[row, 1, row, _cols].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            _worksheet.Cells[row, 1, row, _cols].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        }
        #endregion

        #region File
        private void CreateWorkbookWorksheet(ExcelPackage p)
        {
            _worksheet = p.Workbook.Worksheets.Add("משמרות");
            _worksheet.View.RightToLeft = true;
        }

        private void OpenSaveFileDialog(ExcelPackage package)
        {
            string month;
            if (!_months.TryGetValue(monthPicker.DisplayDate.Month, out month))
            {
                month = monthPicker.DisplayDate.Month.ToString();
            }
            SaveFileDialog a = new SaveFileDialog()
            {
                FileName = string.Format("{0} {1} {2}", "תבנית משמרות", month, monthPicker.DisplayDate.Year),
                Filter = "Excel Documents |*.xlsx"
            };
            if (a.ShowDialog() == true)
            {
                package.SaveAs(new System.IO.FileInfo(a.FileName));
            }
        }
        #endregion

        #region Headers
        private void MakeHeaders()
        {
            string[] headers;
            headers = ReadHeadersConfigFile();
            CreateHeadersFromArray(headers);
            SetHeadersStyling(_worksheet.Cells[1, 1, 1, headers.Length]);
            _cols = headers.Length;
        }

        private void CreateHeadersFromArray(string[] headers)
        {
            for (int i = 1; i <= headers.Length; i++)
            {
                var cell = _worksheet.Cells[1, i];
                cell.Value = headers[i - 1];
                cell.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin, System.Drawing.Color.Black);
            }
        }

        private void SetHeadersStyling(ExcelRange excelRange)
        {
            excelRange.AutoFitColumns();
            excelRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            excelRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Brown);
            excelRange.Style.Font.Color.SetColor(System.Drawing.Color.White);
            excelRange.Style.Font.Bold = true;
        }

        private string[] ReadHeadersConfigFile()
        {
            try { return System.IO.File.ReadAllLines(Settings.Default.HEADERS_FILE); }
            catch (Exception e)
            {
                MessageBox.Show("בעיה התרחשה בנסיון לקרוא את הכותרות. נא וודא כי קובץ הכותרות קיים.");
                throw e;
            }
        }
        #endregion

        #region Hebrew translations
        public string TranslateDayToHebrew(string english)
        {
            switch (english)
            {
                case "Sunday":
                    return "ראשון";
                case "Monday":
                    return "שני";
                case "Tuesday":
                    return "שלישי";
                case "Wednesday":
                    return "רביעי";
                case "Thursday":
                    return "חמישי";
                case "Friday":
                    return "שישי";
                case "Saturday":
                    return "שבת";
                default:
                    return english;
            }
        }

        private string GenerateMonthInHebrew(int i)
        {
            switch (i)
            {
                case 1:
                    return "ינואר";
                case 2:
                    return "פבואר";
                case 3:
                    return "מרץ";
                case 4:
                    return "אפריל";
                case 5:
                    return "מאי";
                case 6:
                    return "יוני";
                case 7:
                    return "יולי";
                case 8:
                    return "אוגוסט";
                case 9:
                    return "ספטמבר";
                case 10:
                    return "אוקטובר";
                case 11:
                    return "נובמבר";
                case 12:
                    return "דצמבר";
            }
            return "";
        }
        #endregion

        #region View logic
        private void Calendar_DisplayModeChanged(object sender, CalendarModeChangedEventArgs e)
        {
            monthPicker.DisplayMode = CalendarMode.Year;
            Mouse.Capture(null);
        }

        private void CreateTemplate_Click(object sender, RoutedEventArgs e)
        {
            ExcelPackage package = new ExcelPackage();
            CreateWorkbookWorksheet(package);
            try { MakeHeaders(); }
            catch { return; }
            try { MakeShips(); }
            catch { return; }
            OpenSaveFileDialog(package);
        }
        #endregion

        #region Process poll results

        private void textBoxDoodleUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && readDoodleButton.IsEnabled)
            {
                GetDoodleResults_Click(sender, e);
            }
        }

        private async void GetDoodleResults_Click(object sender, RoutedEventArgs e)
        {
            string json;
            _participants.Clear();
            try
            {
                readDoodleButton.IsEnabled = false;
                textBoxDoodleUrl.IsEnabled = false;
                await Task.Run(async () =>
                {
                    using (var doodleRequest = new WebClient())
                    {
                        doodleRequest.Encoding = Encoding.UTF8;
                        json = await Dispatcher.InvokeAsync(() => doodleRequest.DownloadString("http://doodle.com/np/data?timeZoneClient=Asia/Beirut&token=&locale=en_IL&pollId=" + textBoxDoodleUrl.Text.Split('/').Last()));
                    }
                    var pollResults = JsonConvert.DeserializeObject<DoodleResponse>(json).Poll;
                    foreach (var participant in pollResults.Participants.Where(p => !p.Preferences.All(c => c == 'n'))
                    .OrderBy(n => n.Preferences.Count(c => c == 'y'))
                    .ThenBy(n => n.Preferences.Count(c => c == 'i')))
                    {
                        var optionsY = GenerateParsedPreferencesForParticipant(participant, pollResults, 'y');
                        var optionsM = GenerateParsedPreferencesForParticipant(participant, pollResults, 'i');
                        Dispatcher.Invoke(() =>
                        {
                            _participants.Add(new Participant()
                            {
                                Name = participant.Name,
                                Preferences = participant.Preferences,
                                OptionsY = new ObservableCollection<ObjectHolder>(optionsY),
                                OptionsM = new ObservableCollection<ObjectHolder>(optionsM)
                            });
                        });
                    }
                });
                listViewPeopleList.ItemsSource = _participants;
            }
            catch
            {
                MessageBox.Show(string.Format("{0}\n{1}", "בעיה התרחשה בקריאת הסקר. נא ודא שהוכנסה כתובת נכונה. דוגמא:", "http://doodle.com/poll/632tfd3cge2tpctr"));
                return;
            }
            finally
            {
                readDoodleButton.IsEnabled = true;
                textBoxDoodleUrl.IsEnabled = true;
            }
        }

        private ObservableCollection<ObjectHolder> GenerateParsedPreferencesForParticipant(Participant participant, DoodlePoll pollResults, char charToParse)
        {
            ObservableCollection<ObjectHolder> result = new ObservableCollection<ObjectHolder>();
            for (int i = 0; i < participant.Preferences.Length; i++)
            {
                if (participant.Preferences[i] == charToParse)
                {
                    result.Add(new ObjectHolder() { Text = pollResults.OptionsText[i] });
                }
            }
            return result;
        }
        #endregion

        #region Handle option check
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox && ((CheckBox)sender).DataContext is ObjectHolder)
            {
                var option = (ObjectHolder)((CheckBox)sender).DataContext;
                string optionText = option.Text;
                int previousTimesOptionSelected = _participants.Count(p => p.OptionsY.Union(p.OptionsM).Any(o => o.Text == optionText && o.IsChecked));
                option.IsChecked = ((CheckBox)sender).IsChecked ?? false;
                int currentTimesOptionSelected = _participants.Count(p => p.OptionsY.Union(p.OptionsM).Any(o => o.Text == optionText && o.IsChecked));
                SetTimesSelectedForOption(optionText, currentTimesOptionSelected);
                ReOrderParticipants();
            }
        }

        private void ReOrderParticipants()
        {
            int timesSelectedToOrderBy = _participants.Min(p =>
            {
                try
                {
                    return p.OptionsY.Min(o => o.TimesSelected);
                }
                catch
                {
                    return 0;
                }
            });
            _participants = new ObservableCollection<Participant>(_participants.OrderBy(p => p.OptionsY.Any(o => o.IsChecked))
                                .ThenByDescending(p => p.OptionsY.Any(o => o.TimesSelected == timesSelectedToOrderBy))
                                .ThenBy(p => p.OptionsY.Count(o => o.TimesSelected == timesSelectedToOrderBy)));
            listViewPeopleList.ItemsSource = _participants;
        }

        /// <summary>
        /// Iterates over all participants, checks which ones have marked an option equivalent to <paramref name="optionText"/>, and updates their TimesSelected property.
        /// </summary>
        /// <param name="optionText">Relevant option to check</param>
        /// <param name="value">The amount of times that <paramref="optionText"/> is selected</param>
        private void SetTimesSelectedForOption(string optionText, int value)
        {
            foreach (var participant in _participants)
            {
                foreach (var opt in participant.OptionsY.Union(participant.OptionsM))
                {
                    if (opt.Text == optionText)
                    {
                        opt.TimesSelected = value;
                    }
                }
            }
        }
        #endregion
    }

    class DoodleResponse
    {
        public DoodlePoll Poll;
    }
}

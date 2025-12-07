using DigitalWellbeing.Core;
using DigitalWellbeingWPF.Helpers;
using DigitalWellbeing.Core.Models;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DigitalWellbeingWPF.ViewModels
{
    public class DateRangeAppUsageViewModel : INotifyPropertyChanged
    {
        // --- ADDED: Cache for generated app colors ---
        private Dictionary<string, Brush> _appColorCache = new Dictionary<string, Brush>();
        private const double GoldenAngle = 137.50776405; // Golden angle in degrees

        // --- Properties for Data Binding ---

        private DateTime _startDate = DateTime.Now.AddDays(-7); // Default start date
        public DateTime StartDate
        {
            get { return _startDate; }
            set
            {
                if (_startDate != value)
                {
                    _startDate = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime _endDate = DateTime.Now; // Default end date
        public DateTime EndDate
        {
            get { return _endDate; }
            set
            {
                if (_endDate != value)
                {
                    _endDate = value;
                    OnPropertyChanged();
                }
            }
        }

        private SeriesCollection _chartSeries;
        public SeriesCollection ChartSeries
        {
            get { return _chartSeries; }
            set
            {
                if (_chartSeries != value)
                {
                    _chartSeries = value;
                    OnPropertyChanged();
                }
            }
        }

        // --- ADDED: Property for chart view mode (Tag or App) ---
        private bool _isTagView = true; // Default to showing tags
        public bool IsTagView
        {
            get { return _isTagView; }
            set
            {
                if (_isTagView != value)
                {
                    _isTagView = value;
                    OnPropertyChanged();
                    GenerateChart(); // Regenerate chart when view changes
                }
            }
        }


        // --- Commands (for button clicks) ---

        // We'll use a simple DelegateCommand for the "Generate Chart" button.
        // You could use a more sophisticated command implementation (like RelayCommand)
        // if you're using a framework like MVVM Light or Prism.
        public DelegateCommand GenerateChartCommand { get; private set; }


        // --- Constructor ---

        public DateRangeAppUsageViewModel()
        {
            // Initialize the chart series collection
            ChartSeries = new SeriesCollection();

            // Initialize the command
            GenerateChartCommand = new DelegateCommand(GenerateChart);
        }


        // --- Methods ---

        private async void GenerateChart()
        {
            // 1. Validate the date range (optional, but good practice)
            if (EndDate < StartDate)
            {
                // Show an error message (you might want to use a more sophisticated
                // approach than MessageBox in a real application).
                System.Windows.MessageBox.Show("End date must be after start date.", "Invalid Date Range");
                return;
            }

            // 2. Load the app usage data for the selected date range
            List<AppUsage> appUsageData = await LoadDataForDateRange(StartDate, EndDate);

            // 3. Aggregate the data (by tag or by app, depending on IsTagView)
            if (IsTagView)
            {
                // Aggregate by tag
                Dictionary<AppTag, double> tagDurations = AggregateDataByTag(appUsageData);
                ChartSeries = CreatePieSeriesCollection(tagDurations);
            }
            else
            {
                // Aggregate by app
                Dictionary<string, double> appDurations = AggregateDataByApp(appUsageData);
                ChartSeries = CreateAppPieSeriesCollection(appDurations);
            }

            // 4. Create the chart series (we'll have separate methods for tag and app views)

        }

        //Added method to filter by app
        private Dictionary<string, double> AggregateDataByApp(List<AppUsage> appUsageData)
        {
            Dictionary<string, double> appDurations = new Dictionary<string, double>();
            foreach (AppUsage appUsage in appUsageData)
            {
                if (AppUsageViewModel.IsProcessExcluded(appUsage.ProcessName)) continue;
                if (appUsage.Duration < Properties.Settings.Default.MinumumDuration) continue;

                // Use ProcessName as the key for individual app aggregation
                string appKey = appUsage.ProcessName;
                if (appDurations.ContainsKey(appKey))
                {
                    appDurations[appKey] += appUsage.Duration.TotalMinutes;
                }
                else
                {
                    appDurations[appKey] = appUsage.Duration.TotalMinutes;
                }
            }
            return appDurations;

        }

        private async Task<List<AppUsage>> LoadDataForDateRange(DateTime startDate, DateTime endDate)
        {
            List<AppUsage> allUsageData = new List<AppUsage>();

            // Iterate through each date in the range
            for (DateTime date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                // Load the data for the current date (using the existing GetData method)
                List<AppUsage> dailyData = await AppUsageViewModel.GetData(date);
                allUsageData.AddRange(dailyData); // Add the daily data to the overall list
            }

            return allUsageData;
        }
        private Dictionary<AppTag, double> AggregateDataByTag(List<AppUsage> appUsageList)
        {
            Dictionary<AppTag, double> tagDurations = new Dictionary<AppTag, double>();

            // Initialize the dictionary with all tags (and 0 duration)
            foreach (AppTag tag in Enum.GetValues(typeof(AppTag)))
            {
                tagDurations[tag] = 0;
            }

            // Group by tag and sum durations
            foreach (AppUsage appUsage in appUsageList)
            {
                // Exclude filtered processes
                if (AppUsageViewModel.IsProcessExcluded(appUsage.ProcessName)) continue;

                AppTag tag = AppTagHelper.GetAppTag(appUsage.ProcessName);

                // Consider minimum duration
                if (appUsage.Duration > Properties.Settings.Default.MinumumDuration)
                {
                    tagDurations[tag] += appUsage.Duration.TotalMinutes; // Accumulate minutes per tag
                }

            }

            return tagDurations;
        }
        private SeriesCollection CreatePieSeriesCollection(Dictionary<AppTag, double> tagDurations)
        {
            SeriesCollection seriesCollection = new SeriesCollection();
            // --- ADDED: Sort by duration (descending) ---
            var sortedTagDurations = tagDurations.OrderByDescending(kvp => kvp.Value);
            foreach (var tagDuration in sortedTagDurations)
            {
                if (tagDuration.Value > 0)  // Only add tags with non-zero durations
                {
                    seriesCollection.Add(new PieSeries
                    {
                        Title = AppTagHelper.GetTagDisplayName(tagDuration.Key), // Use tag name
                        Values = new ChartValues<double> { tagDuration.Value }, // Total minutes for the tag
                        Fill = AppTagHelper.GetTagColor(tagDuration.Key), // Use tag color
                        DataLabels = true, // You might want to customize this
                                           // --- MODIFIED: Show hours and minutes, handle null SeriesView ---
                        LabelPoint = chartPoint =>
                        {
                            TimeSpan duration = TimeSpan.FromMinutes(chartPoint.Y);
                            int totalHours = (int)duration.TotalHours; // Use TotalHours, cast to int
                                                                       // Use null-conditional operator and null-coalescing operator
                            string title = chartPoint.SeriesView?.Title ?? "N/A";
                            return chartPoint.Participation < 0.005 ? null : string.Format("{0} ({1}:{2:mm})", title, totalHours, duration); //Correct display
                        }

                    });
                }
            }
            //Added no data series
            if (seriesCollection.Count == 0)
            {
                seriesCollection.Add(new PieSeries
                {
                    Title = "No Data",
                    Values = new ChartValues<double> { 1 },
                    Fill = Brushes.LightGray,
                    DataLabels = true,
                    LabelPoint = chartPoint => "No Data"
                });
            }
            return seriesCollection;
        }

        // --- MODIFIED: Method to create series collection for app view using generated colors ---
        private SeriesCollection CreateAppPieSeriesCollection(Dictionary<string, double> appDurations)
        {
            SeriesCollection seriesCollection = new SeriesCollection();

            // --- ADDED: Sort by duration (descending) ---
            var sortedAppDurations = appDurations.OrderByDescending(kvp => kvp.Value);

            foreach (var appDuration in sortedAppDurations) // Use the sorted data
            {
                if (appDuration.Value > 0)
                {
                    // Get the color for the app (either from the cache or generate a new one)
                    Brush appBrush = GetAppColor(appDuration.Key);

                    seriesCollection.Add(new PieSeries
                    {
                        Title = appDuration.Key,
                        Values = new ChartValues<double> { appDuration.Value },
                        Fill = appBrush, // Use the dynamically generated color
                        DataLabels = true,
                        // --- MODIFIED: Show hours and minutes, handle null SeriesView ---
                        LabelPoint = chartPoint =>
                        {
                            TimeSpan duration = TimeSpan.FromMinutes(chartPoint.Y);
                            int totalHours = (int)duration.TotalHours; // Use TotalHours, cast to int
                                                                       // Use null-conditional operator and null-coalescing operator
                            string title = chartPoint.SeriesView?.Title ?? "N/A";
                            //MODIFIED: return null if participation is less than 0.005 (0.5%)
                            return chartPoint.Participation < 0.005 ? null : string.Format("{0} ({1}:{2:mm})", title, totalHours, duration); //Correct display
                        }
                    });
                }
            }

            if (seriesCollection.Count == 0)
            {
                seriesCollection.Add(new PieSeries
                {
                    Title = "No Data",
                    Values = new ChartValues<double> { 1 },
                    Fill = Brushes.LightGray,
                    DataLabels = true,
                    LabelPoint = chartPoint => "No Data"
                });
            }

            return seriesCollection;
        }

        // --- ADDED: Method to get or generate a color for an app ---
        private Brush GetAppColor(string appName)
        {
            if (_appColorCache.ContainsKey(appName))
            {
                return _appColorCache[appName]; // Return cached color
            }
            else
            {
                // Generate a new color based on the hue
                double hue = _appColorCache.Count * GoldenAngle; // Use count as seed
                Color color = HslToRgb(hue, 0.7, 0.5); // Use HSL (hue, saturation, lightness)
                Brush brush = new SolidColorBrush(color);
                _appColorCache[appName] = brush; // Cache the color
                return brush;
            }
        }

        // --- ADDED: HSL to RGB conversion method ---
        private static Color HslToRgb(double h, double s, double l)
        {
            double r = 0, g = 0, b = 0;

            if (s == 0)
            {
                r = g = b = l; // Achromatic (gray)
            }
            else
            {
                double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                double p = 2 * l - q;
                r = HueToRgb(p, q, h + 120);
                g = HueToRgb(p, q, h);
                b = HueToRgb(p, q, h - 120);
            }

            return Color.FromRgb((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }

        private static double HueToRgb(double p, double q, double t)
        {
            if (t < 0) t += 360;
            if (t > 360) t -= 360;
            if (t < 60) return p + (q - p) * 6 * t / 360;
            if (t < 180) return q;
            if (t < 240) return p + (q - p) * (240 - t) * 6 / 360;
            return p;
        }


        // --- INotifyPropertyChanged Implementation ---

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // --- Simple DelegateCommand Implementation ---

    public class DelegateCommand : System.Windows.Input.ICommand  // Corrected namespace
    {
        private readonly Action _action;

        public DelegateCommand(Action action)
        {
            _action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true; // Always executable in this simple example
        }

        public void Execute(object parameter)
        {
            _action();
        }

        public event EventHandler CanExecuteChanged; // We don't need to raise this in this simple example
    }
}
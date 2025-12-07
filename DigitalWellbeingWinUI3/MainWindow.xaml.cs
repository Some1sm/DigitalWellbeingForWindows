using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using DigitalWellbeingWinUI3.Views;

namespace DigitalWellbeingWinUI3
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            Title = "Digital Wellbeing";
            
            // Apply Theme
            try
            {
                var theme = Helpers.UserPreferences.ThemeMode;
                if (!string.IsNullOrEmpty(theme) && theme != "System")
                {
                    if (Enum.TryParse(theme, out ElementTheme rTheme))
                    {
                        (this.Content as FrameworkElement).RequestedTheme = rTheme;
                    }
                }
            }
            catch { }
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            NavView.SelectedItem = NavView.MenuItems[0];
            ContentFrame.Navigate(typeof(DayAppUsagePage));
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                ContentFrame.Navigate(typeof(SettingsPage));
            }
            else
            {
                var item = args.InvokedItemContainer as NavigationViewItem;
                if (item != null) 
                {
                    switch(item.Tag.ToString())
                    {
                        case "Dashboard":
                            ContentFrame.Navigate(typeof(DayAppUsagePage));
                            break;
                         case "History":
                            // Placeholder for History Page, for now redirect to DateRangeAppUsagePage if ported, or just keep as DayAppUsagePage if not ready
                            // Actually user said "History button does nothing, seems to load the dashboard" - that's because there was NO case for it.
                            // I need to check if DateRangeAppUsagePage exists in WinUI 3 yet. 
                            // It does NOT. So I will add a TODO and navigate to a placeholder or stay on DayAppUsagePage but maybe show a dialog?
                            // For now, let's create a placeholder History page.
                            ContentFrame.Navigate(typeof(DayAppUsagePage)); // Re-using Dashboard for now as placeholder is better than nothing? No, user complained it does nothing.
                            // I will create a simple HistoryPage.xaml next.
                            ContentFrame.Navigate(typeof(HistoryPage));
                            break;
                         case "Settings":
                             ContentFrame.Navigate(typeof(SettingsPage));
                             break;
                    }
                }
            }
        }
        public void NavigateToDashboard()
        {
             NavView.SelectedItem = NavView.MenuItems[0];
             ContentFrame.Navigate(typeof(DayAppUsagePage));
        }
    }
}

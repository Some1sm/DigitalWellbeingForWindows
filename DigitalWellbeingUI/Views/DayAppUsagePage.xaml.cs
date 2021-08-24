﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DigitalWellbeingUI.Models;
using DigitalWellbeingUI.Models.UserControls;
using DigitalWellbeingUI.ViewModels;
using LiveCharts.Wpf;

namespace DigitalWellbeingUI.Views
{
    /// <summary>
    /// Interaction logic for DayAppUsagePage.xaml
    /// </summary>
    public partial class DayAppUsagePage : Page
    {
        AppUsageViewModel vm;

        public DayAppUsagePage()
        {
            InitializeComponent();

            vm = (AppUsageViewModel)DataContext;
        }

        private void BtnPreviousDay_Click(object sender, RoutedEventArgs e)
        {
            vm.LoadPreviousDay();
        }

        private void BtnNextDay_Click(object sender, RoutedEventArgs e)
        {
            vm.LoadNextDay();
        }

        public void RefreshWithSettings()
        {
            vm.OnNavigate();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            vm.ManualRefresh();
        }

        private void appUsageChart_DataClick(object sender, LiveCharts.ChartPoint chartPoint)
        {
            AppUsageListView.SelectedItem = vm.OnAppUsageChart_SelectionChanged(chartPoint);
        }

        private void AppUsageListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                vm.OnAppUsageListView_SelectionChanged((AppUsageListItem)e.AddedItems[0]);
            }
        }
    }
}

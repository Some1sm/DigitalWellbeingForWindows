using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

namespace DigitalWellbeingWPF
{
    public partial class LimitedTooltip : UserControl
    {
        public LimitedTooltip()
        {
            InitializeComponent();
        }

        // --- ADDED: DependencyProperty for the Title ---
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(LimitedTooltip), new PropertyMetadata(""));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // --- ADDED: DependencyProperty for the ItemsSource ---
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable<TooltipItem>), typeof(LimitedTooltip), new PropertyMetadata(null, OnItemsSourceChanged));
        public IEnumerable<TooltipItem> ItemsSource
        {
            get { return (IEnumerable<TooltipItem>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        // --- ADDED: DependencyProperty for MoreVisibility
        public static readonly DependencyProperty MoreVisibilityProperty =
            DependencyProperty.Register("MoreVisibility", typeof(Visibility), typeof(LimitedTooltip), new PropertyMetadata(Visibility.Collapsed));

        public Visibility MoreVisibility
        {
            get { return (Visibility)GetValue(MoreVisibilityProperty); }
            set { SetValue(MoreVisibilityProperty, value); }
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LimitedTooltip tooltip = (LimitedTooltip)d;
            tooltip.UpdateItems();
        }


        private void UpdateItems()
        {
            if (ItemsSource == null)
            {
                ItemsList.ItemsSource = null;
                MoreTextBlock.Visibility = Visibility.Collapsed;
                return;
            }

            var limitedItems = ItemsSource.Take(10).ToList(); // Take only the first 10 items
            ItemsList.ItemsSource = limitedItems;

            // Show "More..." text if there are more than 10 items.

            MoreVisibility = ItemsSource.Count() > 10 ? Visibility.Visible : Visibility.Collapsed;
        }
    }
    // Simple data structure for tooltip items
    public class TooltipItem
    {
        public string Name { get; set; }
        public string Duration { get; set; }
    }
}
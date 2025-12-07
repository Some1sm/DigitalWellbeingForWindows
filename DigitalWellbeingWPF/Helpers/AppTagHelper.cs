using DigitalWellbeing.Core.Models;
using System.Windows.Media;
using System;
using System.Collections.Generic;

namespace DigitalWellbeingWPF.Helpers
{
    public static class AppTagHelper
    {
        public static string GetTagDisplayName(AppTag tag)
        {
            return tag.ToString();
        }

        public static Dictionary<string, int> GetComboBoxChoices()
        {
            var choices = new Dictionary<string, int>();
            foreach (AppTag tag in Enum.GetValues(typeof(AppTag)))
            {
                choices.Add(GetTagDisplayName(tag), (int)tag);
            }
            return choices;
        }

        public static Brush GetTagColor(AppTag tag)
        {
            switch (tag)
            {
                case AppTag.Work: return Brushes.DodgerBlue;
                case AppTag.Education: return Brushes.Orange;
                case AppTag.Entertainment: return Brushes.MediumPurple;
                case AppTag.Social: return Brushes.DeepPink;
                case AppTag.Utility: return Brushes.Gray;
                case AppTag.Game: return Brushes.Crimson;
                case AppTag.Untagged:
                default:
                    return Brushes.LightGray;
            }
        }

        public static AppTag GetAppTag(string processName)
        {
            // Simple logic or TODO
            // In original code, this might have checked Settings or some list.
            // For now, return Untagged or try to infer?
            // Actually, SettingsManager probably handles this or the original AppTagHelper did.
            // PROBABLY SettingsManager has the mapping, and AppTagHelper just had color logic?
            // "AppTagHelper.GetAppTag(processName)" was called in AppUsageViewModel.
            // So THIS helper needs that method.
            
            // Assuming SettingsManager is available, but helper might not depend on it directly to avoid circular dependency?
            // Let's assume for now it returns Untagged and we might need to hook it up to SettingsManager later.
            return SettingsManager.GetAppTag(processName);
        }
    }
}

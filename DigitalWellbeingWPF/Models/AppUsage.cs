﻿using DigitalWellbeingWPF.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DigitalWellbeingWPF.Models
{
    public class AppUsage : INotifyPropertyChanged
    {
        public string ProgramName { get; set; }
        public string ProcessName { get; set; } // Process name : Use as identifier
        public TimeSpan Duration { get; set; }
        public AppTag Tag { get; set; }

        public AppUsage(string processName, string programName, TimeSpan duration, AppTag tag = AppTag.None)
        {
            this.ProcessName = processName;
            this.ProgramName = programName != string.Empty ? programName : StringParser.TitleCaseWhenLower(processName);
            this.Duration = duration;
            this.Tag = tag;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Refresh()
        {
            OnPropertyChanged(nameof(ProcessName));
            OnPropertyChanged(nameof(Duration));
        }
    }
}

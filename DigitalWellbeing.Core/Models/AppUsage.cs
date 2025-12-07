using System;

namespace DigitalWellbeing.Core.Models
{
    public class AppUsage
    {
        public string ProgramName { get; set; }
        public string ProcessName { get; set; }
        public TimeSpan Duration { get; set; }

        public AppUsage(string processName, string programName, TimeSpan duration)
        {
            this.ProcessName = processName;
            this.ProgramName = programName;
            this.Duration = duration;
        }

        public AppUsage() { }
    }
}

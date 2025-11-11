using System;
using System.Collections.Generic;

namespace WorkLogApp.Core.Models
{
    public class WorkLog
    {
        public DateTime LogDate { get; set; }
        public string DailySummary { get; set; }
        public List<WorkLogItem> Items { get; set; } = new List<WorkLogItem>();
    }
}
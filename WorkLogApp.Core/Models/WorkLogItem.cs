using System;
using WorkLogApp.Core.Enums;

namespace WorkLogApp.Core.Models
{
    public class WorkLogItem
    {
        public DateTime LogDate { get; set; }
        public string ItemTitle { get; set; }
        public string ItemContent { get; set; }
        public string CategoryId { get; set; }
        public StatusEnum Status { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Tags { get; set; }
        public int? SortOrder { get; set; }
    }
}
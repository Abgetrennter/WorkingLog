using System.Collections.Generic;

namespace WorkLogApp.Core.Models
{
    public class ImportResult
    {
        public bool Success { get; set; }
        public List<WorkLog> Data { get; set; }
        public List<string> Errors { get; set; }

        public ImportResult()
        {
            Data = new List<WorkLog>();
            Errors = new List<string>();
            Success = true;
        }
    }
}

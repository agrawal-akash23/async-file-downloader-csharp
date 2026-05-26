using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncDownloader.Models
{
    public class DownloadResult
    {
        public string Url { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public bool Success { get; set; }
        public long FileSizeBytes { get; set; }
        public double ElaspedSeconds { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}

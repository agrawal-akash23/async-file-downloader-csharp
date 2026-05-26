using AsyncDownloader.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncDownloader.Services
{
    public class FileDownloaderService
    {
        private readonly HttpClient _httpClient;
        private readonly string _saveFolder;
        public FileDownloaderService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            // Fix - Add browser-like header to your code.
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/120.0.0.0 Safari/537.36");

            _saveFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads");
            Directory.CreateDirectory(_saveFolder);
        }

        // -- Download a SINGLE file (async) ---------
        public async Task<DownloadResult> DownloadFileAsync(string url, CancellationToken cancellationToken = default)
        {
            var result = new DownloadResult { Url = url };
            var stopwatch = Stopwatch.StartNew();

            try
            {
                //Step A: send the HTTP GET request (awaiting frees the thread)
                var response = await _httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode(); // throws if server returns 404, 500, 403, etc.

                //Step B: read the response body as raw bytes
                var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

                //Step C: work out a file name and save to disk
                result.FileName = Path.GetFileName(new Uri(url).LocalPath);
                if (string.IsNullOrWhiteSpace(result.FileName))
                    result.FileName = $"file_{Guid.NewGuid}.dat";

                var fullPath = Path.Combine(_saveFolder, result.FileName);  // Never use string concatenation for paths- it breaks on other OS systems. Always Path.Combine. 
                await File.WriteAllBytesAsync(fullPath, bytes, cancellationToken);

                result.Success = true;
                result.FileSizeBytes = bytes.Length;
            }
            catch (OperationCanceledException)
            {
                result.Success = false;
                result.ErrorMessage = $"Download was cancelled.";
            }
            catch (HttpRequestException ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Network error: {ex.Message}";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Unexpected error: {ex.Message}";
            }
            finally
            {
                stopwatch.Stop(); 
                result.ElaspedSeconds = Math.Round(stopwatch.Elapsed.TotalSeconds, 2);
            }
            
            return result;
        }

        // -- Download MULPTIPLE files simultaneously ------------------
        public async Task<List<DownloadResult>> DownloadAllAsync(List<string> urls, CancellationToken cancellationToken = default)
        {
            // Create one task per URL - none are awaited yet
            var tasks = urls.Select(url => DownloadFileAsync(url, cancellationToken));

            // Await ALL of them at the same time
            var results = await Task.WhenAll(tasks);

            return results.ToList();
        }
    }
}

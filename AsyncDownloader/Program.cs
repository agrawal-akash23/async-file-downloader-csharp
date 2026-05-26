using AsyncDownloader.Services;

// -- Files to download (free public test URLs) ---------------
var urls = new List<string>
{
    // Servers like httpbin.org user Cloudflare or similar protection that blocks requests that don't look like real browsers.
    "https://httpbin.org/html",
    "https://httpbin.org/json"

    //jsonplaceholder - never blocks code side requests(non-browser), made for code testing.
    //"https://jsonplaceholder.typicode.com/posts/1",
    //"https://jsonplaceholder.typicode.com/posts/2",
    //"https://jsonplaceholder.typicode.com/posts/3"
};

// -- CancellationToken setup -----------
// This lets the user press Ctrl+C to cancel all downloads mid-way
using var cts = new CancellationTokenSource();

Console.CancelKeyPress += (sender, e) =>
{
    Console.WriteLine("\nCancelling downloads...");
    cts.Cancel();       // signal all downloads to stop
    e.Cancel = true;    // prevent the process from being killed immediately
};

// -- Run -----------------------------------
var downloader = new FileDownloaderService();

Console.WriteLine("======================================");
Console.WriteLine("     ASYNC FILE DOWNLOADER     ");
Console.WriteLine("======================================");
Console.WriteLine($"Starting {urls.Count} downloads simulteneously...");
Console.WriteLine("(Press Ctrl+C at any time to cancel)\n");

var startTime = DateTime.Now;
var results = await downloader.DownloadAllAsync(urls, cts.Token);
var totalSeconds = Math.Round((DateTime.Now - startTime).TotalSeconds,2);

// -- Print results ------------------------
Console.WriteLine("\n----- Results ---------------------------");

foreach (var result in results)
{
    if (result.Success)
    {
        var sizeKB = Math.Round(result.FileSizeBytes / 1024.0, 1);
        Console.WriteLine($"✔️   {result.FileName}");
        Console.WriteLine($"    Size: {sizeKB} KB   |   Time: {result.ElaspedSeconds}s");
    }
    else
    {
        Console.WriteLine($"❌   {result.Url}");
        Console.WriteLine($"    Error: {result.ErrorMessage}");
    }
    Console.WriteLine();
}

Console.WriteLine($"-- Total elapsed: {totalSeconds}s --------------");
Console.WriteLine($"    Downloads folder: {AppDomain.CurrentDomain.BaseDirectory}Downloads");

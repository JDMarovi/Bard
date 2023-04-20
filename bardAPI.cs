using System;
using System.Threading.Tasks;
using Microsoft.Playwright;

class GoogleBardAPI
{
    private static IBrowser _browser;
    private static IPage _page;

    private static async Task<IElementHandle> GetInputBoxAsync()
    {
        // Getting the input box
        return await _page.QuerySelectorAsync("textarea[class*='mat-mdc-input-element']");
    }

    private static async Task<bool> IsLoadingResponseAsync()
    {
        // Seeing if the Bard Loader GIF is present, if not, we're not loading
        return await _page.QuerySelectorAsync("img[src='https://www.gstatic.com/lamda/images/sparkle_thinking_v2_e272afd4f8d4bbd25efe.gif']") != null;
    }

    private static async Task SendAsync(string message)
    {
        // Sending the message
        var box = await GetInputBoxAsync();
        await box.ClickAsync();
        await box.FillAsync(message);
        await box.PressAsync("Enter");
    }

    private static async Task<string> GetLastMessageAsync()
    {
        // Getting the latest message
        while (await IsLoadingResponseAsync())
        {
            await Task.Delay(250);
        }

        var pageElements = await _page.QuerySelectorAllAsync("div[class*='response-container-content']");
        var lastElement = pageElements[^1];
        var response = await lastElement.InnerTextAsync();
        Console.WriteLine($"Response: {response}");
        return response;
    }

    private static async Task ChatAsync(string message)
    {
        if (!_page.Url.Contains("bard.google.com"))
        {
            await _page.GotoAsync("https://bard.google.com");
        }

        await Task.Delay(2000);
        await _page.QuerySelectorAsync("textarea[class*='mat-mdc-input-element']").ClickAsync();

        Console.WriteLine($"Sending message: {message}");
        await SendAsync(message);
        var response = await GetLastMessageAsync();

        Console.WriteLine($"Response: {response}");
    }

    static async Task Main()
    {
        await using var playwright = await Playwright.CreateAsync();
        _browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            ExecutablePath = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
            Args = new[] { "--disable-dev-shm-usage", "--disable-blink-features=AutomationControlled" },
            Headless = false
        });

        _page = await _browser.NewPageAsync();
        await _page.GotoAsync("https://accounts.google.com/signin/v2/identifier?hl=en&flowName=GlifWebSignIn&flowEntry=ServiceLogin");
        await _page.WaitForLoadStateAsync(LifecycleEvent.DOMContentLoaded);

        var app = new System.Net.Http.HttpClient();
        await app.GetAsync("http://127.0.0.1:5001/chat?q=Hello");
    }
}

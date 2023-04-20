using System;
using System.Threading.Tasks;
using Microsoft.Playwright;

class GoogleBardAPI
{
    private static IBrowser _browser;
    private static IPage _page;

    // Add your Google account credentials here
    private static string _email = "your_email_here";
    private static string _password = "your_password_here";

    private static async Task<IElementHandle> GetInputBoxAsync()
    {
        return await _page.QuerySelectorAsync("textarea[class*='mat-mdc-input-element']");
    }

    private static async Task<bool> IsLoadingResponseAsync()
    {
        return await _page.QuerySelectorAsync("img[src='https://www.gstatic.com/lamda/images/sparkle_thinking_v2_e272afd4f8d4bbd25efe.gif']") != null;
    }

    private static async Task SendAsync(string message)
    {
        var box = await GetInputBoxAsync();
        await box.ClickAsync();
        await box.FillAsync(message);
        await box.PressAsync("Enter");
    }

    private static async Task<string> GetLastMessageAsync()
    {
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

    private static async Task SignInAsync()
    {
        await _page.TypeAsync("input[type='email']", _email);
        await _page.PressAsync("input[type='email']", "Enter");
        await _page.WaitForTimeoutAsync(2000);

        await _page.TypeAsync("input[type='password']", _password);
        await _page.PressAsync("input[type='password']", "Enter");
        await _page.WaitForLoadStateAsync(LifecycleEvent.DOMContentLoaded);
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

        await SignInAsync(); // Sign in to your Google account

        // Interact with the Google Bard API
        await ChatAsync("Hello, how are you?");
        await Task.Delay(5000); // Wait for 5 seconds
        await ChatAsync("What is the meaning of life?");
        await Task.Delay(5000); // Wait for 5 seconds
        await ChatAsync("Goodbye!");

        // Close the browser after interactions
        await _browser.CloseAsync();
    }
}

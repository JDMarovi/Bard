Google Bard API
===============

This is an API for interacting with Google Bard, a chatbot developed by Google. This API allows you to send messages to the chatbot and receive responses from it programmatically.

Getting Started
---------------

To use this API, you'll need to have .NET installed on your machine. You can download .NET from the official Microsoft website: <https://dotnet.microsoft.com/download>

You'll also need to have the Microsoft Playwright library installed. You can install it using the following command:

csharpCopy code

`dotnet add package Microsoft.Playwright`

Once you have .NET and Playwright installed, you can download or clone this repository and run the `GoogleBardAPI.csproj` project file.

Usage
-----

The API provides a single endpoint for sending and receiving messages to and from the chatbot:

bashCopy code

`GET /chat?q={message}`

To send a message to the chatbot, simply make a GET request to this endpoint, replacing `{message}` with the message you want to send. For example:

bashCopy code

`GET http://localhost:5001/chat?q=Hello`

The API will launch a Chromium browser, navigate to the Google Bard website, send the message to the chatbot, wait for the chatbot to respond, and then return the response as a string.

Notes
-----

-   This API is for educational purposes only and is not affiliated with Google in any way.
-   The API currently only works with the Chromium browser.
-   The API assumes that you have Google Chrome installed at the default location (`C:\Program Files\Google\Chrome\Application\chrome.exe`). If you have Chrome installed at a different location, you'll need to modify the `ExecutablePath` property in the `LaunchAsync` method call in the `Main` method of the `GoogleBardAPI` class.
-   The API runs on port 5001 by default. If you want to use a different port, you'll need to modify the `APP.Run` method call in the `start_browser` method of the `GoogleBardAPI` class.

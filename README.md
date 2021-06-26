# Prerequisites:
Visual Studio Code - https://code.visualstudio.com/download

C# for Visual Studio Code (latest version) - https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp

.NET Core 5.0 SDK or later - https://dotnet.microsoft.com/download/dotnet/5.0

Unzip the repo, Open Visual Studio Code, select file -> Open Folder, select the unzipped repo.

Select any .cs file, When a dialog box asks if you want to add required assets to the project, select Yes. Alternatively you can also open the terminal and write "dotnet restore".

After the project is restored, open the built in Visual Studio Code terminal write "cd Tamro", press Enter.
Write "dotnet watch run" in the terminal and press Enter.

Navigate to "https://localhost:5001/swagger/index.html" in your browser", there you can find the available endpoints.

To run the tests, open a new terminal session (or just re-use your last one by pressing CTRL + C) and write "dotnet test". The 16 tests should pass successfully.

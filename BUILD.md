## Build
1. (Optional) Mod source can be placed inside of the game directory, (game/dir/mod)
2. Open the project in Visual Studio, JetBrains Rider or use the dotnet SDK CLI. (Or any other IDE)
3. Build `source/ChangeableMoodles.csproj` via Ctrl + Shift + B (`dotnet build`), assign the PACKAGE property to true to copy all files meant for a release to the output directory,
4. If auto-detection misses your setup, open the linked `vars.targets` file from the project and override `BaseGamePath`

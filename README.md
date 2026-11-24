# HubLink

ITSD81 Performance Task 5 · Regional Logistics Data Exchange

**Pair members:** Yosef Xerxes S. Nacasabog and Jazz Dwight W. Salac

**Section:** BSIT3Cx

## Run on Windows

1. Install the .NET 10 SDK if it is missing. Run `dotnet --list-sdks` to check.
2. Extract the instructor’s server package. Run `Start-HubLink.cmd` and keep its window open. If port 5080 is occupied, close the failed server window and use `Start-HubLink-5081.cmd`.
3. Open PowerShell in the extracted HubLink project folder and run:

   ```powershell
   dotnet restore HubLink.csproj
   dotnet run --project HubLink.csproj --configuration Release --no-restore
   ```

   The first restore may need access to NuGet.
4. In HubLink, enter the **Service address** (`http://127.0.0.1:5080` by default, or `http://127.0.0.1:5081` for the alternate launcher) and the **Branch code** (`BR-001` by default). Use the instructor’s supplied address and branch when working with a hosted instance. Keep **API mode** on **Live**, then select **Refresh shipments**.

The status filter changes the visible shipment rows. The summary always counts the entire loaded branch. **Export CSV** writes the displayed rows in order, with timestamps in UTC. The settings apply to the next refresh and are entered again after restarting the app. After a failed refresh, any retained rows are labeled **Previously loaded**.

## Verify controlled scenarios

Use the **API mode** selector to switch to the instructor’s test endpoint. Try `empty`, `bad-request`, `server-error`, and `invalid-json`. For `delay`, choose 15 seconds to observe a successful delayed response or 5 seconds to test a timeout. Set the mode back to **Live** and refresh when done. See [Verification Checklist](Verification-Checklist.md) for recorded results.

## Code and acknowledgement

`ShipmentClient` sends HTTP GET requests, validates JSON, and converts it to shipment models. `MainViewModel` manages refresh state, filtering, summary counts, and export commands. `CsvExport` writes the report. The Avalonia view handles presentation and the native file picker.

The project uses an Avalonia MVVM template and CommunityToolkit.Mvvm. OpenAI Codex assisted substantively with implementation, verification, and documentation. The instructor supplied the API and shipment fixtures.


# HubLink verification checklist

**Environment:** Windows x64, .NET 10 SDK, Avalonia 12.1.6 and the instructor-supplied API on port 5080. Release build passed with zero warnings and errors.

| ID | Result | What was checked |
| --- | --- | --- |
| AC-01 | Verified | Desktop refresh at the live endpoint displayed the five BR-001 rows and UTC timestamps. The view-model check switched the branch to BR-002 and received two rows. |
| AC-02 | Verified | The desktop Exception filter showed only HUB-1004. A view-model harness checked all status filters and confirmed All restored five rows. |
| AC-03 | Verified | BR-001 showed 5 total, 1 Pending, 2 InTransit, 1 Delivered and 1 Exception. Counts stayed the same when the desktop was filtered to Exception. |
| AC-04 | Verified | The desktop showed the progress indicator and disabled overlapping refresh and export. The delay-mode command check confirmed that the load remained pending, the filter could still be applied in the view model, and the delayed response completed successfully. |
| AC-05 | Verified | The desktop showed an unreachable-service error and a subsequent retry loaded data. The command checks exercised the five-second timeout, HTTP 400 and HTTP 500. Previous data stayed labeled and refresh remained available. |
| AC-06 | Verified | The empty mode cleared the list and showed a successful no-shipments message. The invalid-json mode showed a refresh failure and retained the prior successful results. The empty export attempt showed a no-records message. |
| AC-07 | Verified | Saved through the native desktop file picker with the Exception filter active and live BR-001 data loaded. The GUI reported one exported shipment. Parsing confirmed the exact header, one row, both escaped location fields, and the expected UTC timestamp. A view-model export produced identical bytes. |
| AC-08 | Verified through export command | An exclusive file lock caused a real file-open error. The app reported an export failure and re-enabled the command. This check supplied the destination stream through the view model’s file-picker boundary. |

The included sample is the one-row BR-001 Exception export. Live is the startup API mode.


## Status validation regression check

On 2026-11-27, an in-memory HTTP response harness exercised the actual shipment client and refresh ViewModel. All four documented statuses were accepted. Numeric strings 4, -1, 999 and 0, lowercase pending, Unknown, a combined status, an empty string and null were rejected with invalid-data feedback. Failed refreshes retained the labeled previous data, summary counts remained consistent, and a subsequent valid refresh succeeded. This regression check uses synthetic responses; the supplied API integration results above were verified earlier.

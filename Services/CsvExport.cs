using System.Globalization;
using System.Text;
using HubLink.Models;

namespace HubLink.Services;

public static class CsvExport
{
    public static async Task WriteAsync(Stream stream, IReadOnlyList<Shipment> shipments)
    {
        if (shipments.Count == 0)
            throw new InvalidOperationException("There are no displayed shipments to export.");

        if (stream.CanSeek)
            stream.SetLength(0);

        await using var writer = new StreamWriter(stream, new UTF8Encoding(true), leaveOpen: true)
        {
            NewLine = "\r\n"
        };
        await writer.WriteLineAsync("TrackingNumber,Origin,Destination,Status,LastUpdatedUtc");
        foreach (var shipment in shipments)
        {
            var fields = new[] { shipment.TrackingNumber, shipment.Origin, shipment.Destination,
                shipment.Status.ToString(), shipment.LastUpdatedUtc.UtcDateTime.ToString("O", CultureInfo.InvariantCulture) };
            await writer.WriteLineAsync(string.Join(",", fields.Select(Escape)));
        }
    }

    private static string Escape(string value) => value.IndexOfAny([',', '"', '\r', '\n']) < 0
        ? value
        : "\"" + value.Replace("\"", "\"\"") + "\"";
}

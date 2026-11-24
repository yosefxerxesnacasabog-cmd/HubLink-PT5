using System.Globalization;

namespace HubLink.Models;

public enum ShipmentStatus { Pending, InTransit, Delivered, Exception }

public sealed record Shipment(string TrackingNumber, string Origin, string Destination,
    ShipmentStatus Status, DateTimeOffset LastUpdatedUtc)
{
    public string DisplayUpdatedUtc => LastUpdatedUtc.ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture);
}

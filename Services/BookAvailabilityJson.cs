using System.Text.Json.Serialization;

namespace HomeNursingSystem.Services;

public class BookAvailabilityJson
{
    [JsonPropertyName("slotMinutes")]
    public int SlotMinutes { get; set; }

    [JsonPropertyName("days")]
    public List<BookAvailabilityDayJson> Days { get; set; } = new();
}

public class BookAvailabilityDayJson
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = "";

    [JsonPropertyName("weekdayLabel")]
    public string WeekdayLabel { get; set; } = "";

    [JsonPropertyName("slots")]
    public List<BookAvailabilitySlotJson> Slots { get; set; } = new();
}

public class BookAvailabilitySlotJson
{
    [JsonPropertyName("utcIso")]
    public string UtcIso { get; set; } = "";

    [JsonPropertyName("timeLabel")]
    public string TimeLabel { get; set; } = "";
}

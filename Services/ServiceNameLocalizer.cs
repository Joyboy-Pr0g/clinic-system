namespace HomeNursingSystem.Services;

public static class ServiceNameLocalizer
{
    private static readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Injection"] = "حقن",
        ["IV Drip Setup"] = "تركيب محلول وريدي",
        ["Wound Dressing"] = "تضميد الجروح",
        ["Blood Pressure Monitoring"] = "قياس ضغط الدم",
        ["Blood Sugar Test"] = "فحص سكر الدم",
        ["Minor Surgical Dressing"] = "تضميد جراحي بسيط",
        ["Post-Surgery Care"] = "رعاية ما بعد الجراحة",
        ["Elderly Care Visit"] = "زيارة رعاية كبار السن"
    };

    public static string Localize(string? serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
            return string.Empty;

        return Map.TryGetValue(serviceName.Trim(), out var ar) ? ar : serviceName;
    }
}


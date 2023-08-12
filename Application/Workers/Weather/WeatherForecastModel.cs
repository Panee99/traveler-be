using Newtonsoft.Json;

namespace Application.Workers.Weather;

public class WeatherForecastModel
{
    public Location Location { get; set; } = null!;
    public Forecast Forecast { get; set; } = null!;
    public Alerts Alerts { get; set; } = null!;
}

public class Location
{
    public string Name { get; set; } = null!;

    public string Country { get; set; } = null!;

    public double Lat { get; set; }

    public double Lon { get; set; }

    [JsonProperty("localtime_epoch")] public int LocaltimeEpoch { get; set; }
}

public class Condition
{
    public string Text { get; set; } = null!;
    public string Icon { get; set; } = null!;
}

public class Forecast
{
    public List<Forecastday> Forecastday { get; set; } = new();
}

public class Hour
{
    [JsonProperty("time_epoch")] public long TimeEpoch { get; set; }

    public string Time { get; set; }

    [JsonProperty("temp_c")] public float TempC { get; set; }

    [JsonProperty("is_day")] public long IsDay { get; set; }

    public Condition Condition { get; set; } = null!;

    [JsonProperty("wind_kph")] public double WindKph { get; set; }

    public long Humidity { get; set; }

    [JsonProperty("will_it_rain")] public bool WillItRain { get; set; }

    [JsonProperty("chance_of_rain")] public long ChanceOfRain { get; set; }

    [JsonProperty("will_it_snow")] public bool WillItSnow { get; set; }

    [JsonProperty("chance_of_snow")] public long ChanceOfSnow { get; set; }

    [JsonProperty("vis_km")] public long VisKm { get; set; }

    public long Uv { get; set; }
}

public class Forecastday
{
    public DateOnly Date { get; set; }
    public Day Day { get; set; }
}

public class Day
{
    [JsonProperty("maxtemp_c")] public float MaxtempC { get; set; }

    [JsonProperty("mintemp_c")] public float MintempC { get; set; }

    [JsonProperty("avgtemp_c")] public float AvgtempC { get; set; }

    [JsonProperty("avghumidity")] public float AvgHumidity { get; set; }

    [JsonProperty("daily_will_it_rain")] public bool DailyWillItRain { get; set; }

    [JsonProperty("daily_chance_of_rain")] public int DailyChanceOfRain { get; set; }

    [JsonProperty("daily_will_it_snow")] public bool DailyWillItSnow { get; set; }

    [JsonProperty("daily_chance_of_snow")] public int DailyChanceOfSnow { get; set; }

    public Condition Condition { get; set; } = null!;

    public float Uv { get; set; }
}

public class Alerts
{
    public List<Alert> Alert { get; set; } = new();
}

public class Alert
{
    public string Headline { get; set; } = null!;
    public string Msgtype { get; set; } = null!;
    public string Severity { get; set; } = null!;
    public string Urgency { get; set; } = null!;
    public string Areas { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string Certainty { get; set; } = null!;
    public string Event { get; set; } = null!;
    public string Note { get; set; } = null!;
    public DateTimeOffset Effective { get; set; }
    public DateTimeOffset Expires { get; set; }
    public string Desc { get; set; } = null!;
    public string Instruction { get; set; } = null!;
}
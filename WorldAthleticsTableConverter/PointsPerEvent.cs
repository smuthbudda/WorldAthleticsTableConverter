using Newtonsoft.Json;

namespace WorldAthleticsTableConverter;

public class PointsPerEvent
{
    public int Points { get; set; }
    public string Gender { get; set; }
    public string Category { get; set; }
    public string Event { get; set; }
    public decimal Mark { get; set; }
}

static class Events
{
    public static readonly string[] OutdoorEvents =
    { "10 Miles","10,000m","10,000mW","1000m","100km","100m","100mH","10km","10kmW","110mH",
        "15,000mW","1500m","15km","15kmW","2 Miles","20,000mW","2000m","2000mSC","200m","20km","20kmW",
        "25km","30,000mW","3000m","3000mSC","3000mW","300m","30km","30kmW","35,000mW","35kmW","3kmW","400m","400mH",
        "4x100m","4x200m","4x400m","50,000mW","5000m","5000mW","500m","50kmW","5km","5kmW","600m","800m","DT",
        "Heptathlon","Decathlon","HJ","HM","HT","JT","LJ","Marathon","Mile","PV","SP", "TJ"
    };
    public static readonly string[] IndoorEvents =
    { "50m" ,"55m","60m","50mH","55mH","60mH","Pentathlon"
    };

}


public static class JsonFileUtils
{
    private static readonly JsonSerializerSettings _options
        = new() { NullValueHandling = NullValueHandling.Ignore, };

    public static void SimpleWrite(object obj, string fileName)
    {
        var jsonString = JsonConvert.SerializeObject(obj, _options);
        File.WriteAllText(fileName, jsonString);
    }
}


// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using UglyToad.PdfPig;
using WorldAthleticsTableConverter;

await Main.ParsePDF();
//todo fix the pager at the bottom.
public static class Main
{

    private static List<PointsPerEvent> PointsTable = new();

    public static async Task ParsePDF()
    {
        Stopwatch stopWatch = new();
        stopWatch.Start();
        Console.WriteLine("Processing");
        try
        {
            PointsTable.Clear();
            using PdfDocument document = PdfDocument.Open(@"C:\Users\jkdsa\source\repos\WorldAthleticsTableConverter\WorldAthleticsTableConverter\PointsTable.pdf");
            foreach (var page in document.GetPages())
            {
                string gender = "";
                int index = 0;
                List<string> eventNameList = new();
                var wordsList = page.GetWords().GroupBy(row => row.BoundingBox.Bottom);
                bool rowDirection = false; //if false points on the left 

                if (page.GetWords().Count() < 200)//needs a check for if the page is not a table. 
                    continue;

                foreach (var row in wordsList)
                {
                    List<PointsPerEvent> events = new();
                    var textRow = row.Select(x => x.Text).ToList();
                    if (index == 0)
                    {
                        var firstWord = row.ElementAt(index).Text.ToLower();
                        if (firstWord.Contains("women"))
                        {

                        }
                        gender = !firstWord.Contains("women") ? "male" : "female";
                    }

                    else if (index == 1)//Get the header
                    {
                        var pointsIndex = textRow.IndexOf("Points");
                        if (pointsIndex == 0)
                            rowDirection = true;
                        textRow.Remove("Points");
                        eventNameList = textRow;
                    }
                    else
                        PointsTable.AddRange(await Task.Run(() => ProcessRow(textRow, eventNameList, gender, rowDirection)));
                    index++;
                }

            }
        }
        catch (Exception)
        {
            throw;
        }

        stopWatch.Stop();
        PointsTable = PointsTable.OrderBy(x => x.Gender).ToList();
        PointsTable = PointsTable.OrderBy(x => x.Gender).ToList();
        Console.WriteLine($"Done time elapsed {stopWatch.Elapsed} Count: {PointsTable.Count}");
        await WriteToJSON();
    }


    private static Task<List<PointsPerEvent>> ProcessRow(List<string> row, List<string> eventNameList, string gender, bool tableDirection)
    {
        int index = 0;
        List<PointsPerEvent> events = new();
        int pointsPerRow = 0;
        var last = row.Last();
        if (row.Count < 3)
        {
            return Task.FromResult(events);
        }
        foreach (var word in row)
        {
            if (tableDirection)
            {
                if (index == 0)
                    pointsPerRow = int.Parse(word);
                else
                {
                    PointsPerEvent newEvent = new()
                    {
                        Event = eventNameList.ElementAt(index - 1),
                        Gender = gender,
                        Mark = ConvertTimeToInt(word),
                        Points = pointsPerRow
                    };

                    newEvent.Category = Events.IndoorEvents.Contains(newEvent.Event) ? "indoor" : "outdoor";

                    if (newEvent.Mark != 0)
                        events.Add(newEvent);
                }
            }
            else
            {
                if (word == last)
                    pointsPerRow = int.Parse(word);
                else
                    pointsPerRow = int.Parse(last);

                PointsPerEvent newEvent = new()
                {
                    Gender = gender,
                    Mark = ConvertTimeToInt(word),
                    Points = pointsPerRow
                };

                newEvent.Event = eventNameList.ElementAt(1);

                newEvent.Category = Events.IndoorEvents.Contains(newEvent.Event) ? "indoor" : "outdoor";

                if (newEvent.Mark != 0)
                    events.Add(newEvent);

            }
            index++;
        }
        return Task.FromResult(events);
    }

    public static Task WriteToJSON()
    {
        var fileName = @"C:\Users\jkdsa\source\repos\WorldAthleticsTableConverter\WorldAthleticsTableConverter\WorldAthletics.json";
        JsonFileUtils.SimpleWrite(PointsTable, fileName);
        return Task.CompletedTask;
    }

    private static double ConvertTimeToInt(string input)
    {
        var yuh = input.Split(':');

        if (input == "-" || input == " ")
            return 0;

        double seconds = Convert.ToDouble(yuh.Last());
        double minutes = yuh.Length == 2 ? Convert.ToDouble(yuh.ElementAt(0)) * 60 : 0;
        double hours = yuh.Length == 3 ? Convert.ToDouble(yuh.ElementAt(1)) * 60 * 60 : 0;

        return minutes + seconds + hours;
    }
}

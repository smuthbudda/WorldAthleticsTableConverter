using System.Diagnostics;
using System.Reflection.Metadata;
using UglyToad.PdfPig;

namespace WorldAthleticsTableConverter;

public class ReadTable
{
    public string FilePath { get; set; }

    public ReadTable(string filePath)
    {
        FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
    }



    public List<PointsPerEvent> ParsePDF()
    {
        List<PointsPerEvent> PointsTable = new();

        Console.WriteLine("Processing");
        try
        {
            PointsTable.Clear();
            using PdfDocument document = PdfDocument.Open(FilePath); //Just realized that the disposed is automatically called at the end of the using scope. 
            foreach (var page in document.GetPages())
            {
                string gender = "";
                int index = 0; //
                List<string> eventNameList = new();
                bool rowDirection = false; //TODO: if false points on the left. Should maybe switch to an enum. It works anyways. 
                var words = page.GetWords();

                if (words.Count() < 200)//TODO: this needs to be refined. Need a better way to tell if a page is a title page or a datapage
                    continue;

                var wordsList = words.GroupBy(row => row.BoundingBox.Bottom); //group the page by rows

                foreach (var row in wordsList)
                {
                    List<PointsPerEvent> events = new();
                    var textRow = row.Select(x => x.Text).ToList();

                    if (index == 0) //Read the header
                    {
                        var firstWord = row.ElementAt(index).Text.ToLower();
                        gender = !firstWord.Contains("women") ? "male" : "female";
                    }
                    else if (index == 1)//Read the event list
                    {
                        var pointsIndex = textRow.IndexOf("Points");

                        if (pointsIndex == 0)
                            rowDirection = true;

                        textRow.Remove("Points");
                        eventNameList = textRow;
                    }
                    else //Process the events
                    {
                        PointsTable.AddRange(ProcessRow(textRow, eventNameList, gender, rowDirection));
                    }

                    index++;
                }

            }
            document.Dispose(); //This is unneeded as it is already called when the using is in scope. 
            return PointsTable;
        }
        catch (Exception ex)
        {
            Console.WriteLine("an exception was thrown", ex);
            throw;
        }

    }


    private List<PointsPerEvent> ProcessRow(List<string> row, List<string> eventNameList, string gender, bool tableDirection)
    {
        int index = 0;
        List<PointsPerEvent> events = new();
        int pointsPerRow = 0;
        var last = row.Last();
        if (row.Count < 3)
        {
            return events;
        }
        foreach (var word in row)
        {
            //this could probably be shortend and simplified 
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

                    if (newEvent.Event == "Pentathlon")
                    {
                        Console.WriteLine($"Pent : { newEvent.Points}{newEvent.Gender}");
                    }
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
        return events;
    }


    private double ConvertTimeToInt(string input)
    {
        var time = input.Split(':');

        if (input == "-" || input == " ")
            return 0;

        double seconds = Convert.ToDouble(time.Last());
        double minutes = time.Length == 2 ? Convert.ToDouble(time.ElementAt(0)) * 60 : 0;
        double hours = time.Length == 3 ? Convert.ToDouble(time.ElementAt(1)) * 60 * 60 : 0;

        return minutes + seconds + hours;
    }
}

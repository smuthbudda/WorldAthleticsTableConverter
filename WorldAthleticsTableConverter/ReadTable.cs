using EmptyFiles;
using System.Diagnostics;
using System.Reflection.Metadata;
using UglyToad.PdfPig;

namespace WorldAthleticsTableConverter;

public class ReadTable
{
    public string FilePath { get; set; }
    public string Category { get; set; }

    public ReadTable(string filePath, string _category)
    {
        FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        Category = _category;
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
                    var textRow = row.Select(x => x.Text.Replace(" ", "")).ToList();
                    string points;
                    if (index == 0) //Read the header
                    {
                        var firstWord = row.ElementAt(index).Text.ToLower();
                        gender = !firstWord.Contains("women") ? "male" : "female";
                    }
                    else if (index == 1)//Read the event list
                    {
                        var pointsIndex = textRow.IndexOf("Points");

                        if (pointsIndex == 0)
                        {
                            rowDirection = true;
                        }

                        textRow.Remove("Points");
                        textRow.Remove("Miles");
                        eventNameList = textRow;
                    }
                    else
                    {
                        if (rowDirection) //if right remove the last item.
                        {
                            points = textRow.ElementAt(0);
                            textRow.RemoveAt(0);
                        }
                        else//if right remove the first item.
                        {
                            points = textRow.ElementAt(textRow.Count - 1);
                            textRow.RemoveAt(textRow.Count - 1);
                        }
                        if (PointsTable.Count == 300)
                        {

                        }
                        PointsTable.AddRange(ProcessRow(textRow, eventNameList, gender, points));
                    }
                    index++;
                }

            }
            document.Dispose(); //This is unneeded as it is already called when the using is in scope. 
            PointsTable = FixObjects(PointsTable);
            return PointsTable;
        }
        catch (Exception ex)
        {
            Console.WriteLine("an exception was thrown", ex);
            throw;
        }

    }


    private List<PointsPerEvent> ProcessRow(List<string> row, List<string> eventNameList, string gender, string points)
    {
        try
        {
            int index = 0;
            List<PointsPerEvent> events = new();
            if (row.Count < 3)
            {
                return events;
            }

            Parallel.ForEach(row, word =>
               {
                   PointsPerEvent newEvent = new()
                   {
                       Gender = gender,
                       Category = this.Category,
                       Points = int.Parse(points),
                       Event = eventNameList[index],
                       Mark = ConvertTimeToInt(word),
                   };
                   events.Add(newEvent);
                   index++;
               }
            );

            return events;
        }
        catch (Exception ex)
        {
            Console.WriteLine("", ex);
            throw;
        }
    }


    private List<PointsPerEvent> FixObjects(List<PointsPerEvent> events)
    {
        Parallel.ForEach(events, e =>
        {

            if (e.Mark == 0)
            {
                var closest = events
                    .Where(x =>
                        x.Event == e.Event &&
                        x.Mark != 0 &&
                        e.Gender == x.Gender &&
                        x.Category == e.Category)
                    .OrderBy(v => Math.Abs(v.Points - e.Points))
                    .First();
                e.Mark = closest.Mark;
            }
        }
        );

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

// See https://aka.ms/new-console-template for more information
using System;
using System.Diagnostics;
using WorldAthleticsTableConverter;

Main.ReadWithOtherPDFParser();
//Main.ReadWithMyPDFParser();



public static class Main
{
    public static void ReadWithOtherPDFParser()
    {
        ReadTable reader = new(@"C:\Users\jkdsa\source\repos\WorldAthleticsTableConverter\WorldAthleticsTableConverter\PointsTable.pdf");
        ReadTable newReader = new(@"C:\Users\jkdsa\source\repos\WorldAthleticsTableConverter\WorldAthleticsTableConverter\IndoorTable.pdf");
        Stopwatch stopWatch = new();
        stopWatch.Start();
        var indoor = newReader.ParsePDF();
        var outdoor = reader.ParsePDF();
        stopWatch.Stop();
        var combo = new List<PointsPerEvent>();
        combo.AddRange(indoor);
        combo.AddRange(outdoor);
        WriteToJSON(combo);
        Console.WriteLine($"Done time elapsed {stopWatch.Elapsed} Events: {combo.Count}");
    }


    public static void WriteToJSON(List<PointsPerEvent> pointsTable)
    {
        var fileName = @"C:\Users\jkdsa\source\repos\WorldAthleticsTableConverter\WorldAthleticsTableConverter\WorldAthletics.json";
        JsonFileUtils.SimpleWrite(pointsTable, fileName);
    }

}

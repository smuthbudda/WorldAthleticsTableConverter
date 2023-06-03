using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WorldAthleticsTableConverter;

//just realized that pdfs are very hard to read
public class PDFReader : IDisposable
{
    public string FilePath { get; set; }
    public List<Page>? Pages { get; set; }
    public int PageCount { get; set; }

    private bool disposedValue;
    public PDFReader(string _FilePath)
    {
        FilePath = _FilePath;
    }
    //Ive just realized that this is a dumb idea. 
    public void ReadFile()
    {
        const int chunkSize = 1024; // read the file by chunks of 1mb but not really sure what the best chunk size should be.
        Pages = new();
        try
        {
            using var file = File.OpenRead(FilePath);
            int bytesRead;
            byte[] buffer = new byte[chunkSize];
            bool checkIfPDF = false;
            int counter = 0;
            string text = string.Empty;

            while ((bytesRead = file.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (counter == 0)  //should check if the
                {
                    checkIfPDF = IsAPDF(buffer);
                    if (!checkIfPDF)
                        break;
                }

                var s = Encoding.Default.GetString(buffer);
                text = Regex.Replace(s, @"[^\u0000-\u007F]+", string.Empty);
                counter++;
            }

        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Checks the first four bytes to see if the file is a pdf '%PDF'
    /// </summary>
    /// <param name="bytes">Byte array</param>
    /// <returns></returns>
    private bool IsAPDF(byte[] bytes)
    {
        if (disposedValue)
            throw new ObjectDisposedException("Object was already disposed");
        
        if (bytes?.Length < 4)
            return false;

        if (bytes is null)
            return false;

        var stopBefore = Math.Min(bytes.Length, 1024) - 3;

        for (int i = 0; i < stopBefore; i++)
        {
            if (bytes[i] == '%' && bytes[i + 1] == 'P' && bytes[i + 2] == 'D' && bytes[i + 3] == 'F')
                return true;
        }

        return false;
    }

    private IEnumerable<byte[]> ReadInChunks()
    {
        var byteArray = new byte[][] { };
        return byteArray;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // free managed objects here
                PageCount = 0;
            }
            // free unmanaged objects here
            Pages = null;
            disposedValue = true;
            Console.WriteLine("Disposed has been called");
        }
    }
}

public class Page
{
    public List<Row>? Rows { get; set; }
    public int PageNumber { get; set; }

}

public class Row
{
    public List<string>? Words { get; set; }
}


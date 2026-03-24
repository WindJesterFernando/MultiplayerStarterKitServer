using System.Collections.Generic;
using System.IO;

static public class Utilities
{
    public const string Delineator = ",";

    static public string Concatenate(int signal, params string[] parameters)
    {
        string concatenatedString = signal.ToString();

        foreach (string p in parameters)
        {
            concatenatedString = concatenatedString + Delineator + p;
        }

        return concatenatedString;
    }

    static public void WriteSerializationToHD(string fileName, LinkedList<string> serialization)
    {
        StreamWriter streamWriter = new StreamWriter(fileName);

        foreach (string s in serialization)
        {
            streamWriter.WriteLine(s);
        }

        streamWriter.Close();
    }

    static public LinkedList<string> ReadSerializationFromHD(string fileName)
    {
        LinkedList<string> serializedData = new LinkedList<string>();

        if (File.Exists(fileName))
        {
            StreamReader streamReader = new StreamReader(fileName);

            while (!streamReader.EndOfStream)
            {
                string line = streamReader.ReadLine();
                serializedData.AddLast(line);
            }

            streamReader.Close();
        }

        return serializedData;
    }

}
// See https://aka.ms/new-console-template for more information

using System;
using System.Configuration;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

public class JsonResults
{
    [JsonPropertyName("main")]
    public MainData MainData
    {
        get; set;
    }
    [JsonPropertyName("name")]
    public string LocationName
    {
        get; set;
    }
    [JsonPropertyName("weather")]
    public Weather[] Weather
    {
        get; set;
    }
}
 public class Weather
{
    [JsonPropertyName("main")]
    public string Main { get; set; }
    [JsonPropertyName("description")]
    public string Description {  get; set; }
}
public class MainData
{
    [JsonPropertyName("temp")]
   public double temp
    {
        get; set;
    }
}
public static class Informations
{
    public static string APIUrl = "https://api.openweathermap.org/data/2.5/weather?q={1}&appid={0}";
    public static void Main(string[] args)
    {

        //APIUrl = string.Format(APIUrl, ConfigurationManager.AppSettings["apiKey"]);
        List<string> APIUrls = new()
        {
            "London",
            "Manchester",
            "Liverpool",
            "Leeds",
        };
        //multiple API calls concurrently using async/await.
        var fetchTasks = APIUrls.Select(x => GetInfo(x)).ToList();
        var results = Task.Run(()=> Task.WhenAll(fetchTasks)).Result;
        foreach (var url in results)
        {
            Console.WriteLine(url);
        }
        Console.WriteLine("Press any key to stop...");
        Console.ReadLine();
    }
    public static async Task<string> GetInfo(string Location)
    {
        JsonResults ConvertedJsonData = new JsonResults();
        string resultString=string.Empty;
        var FormatedAPIUrl = string.Format(APIUrl, ConfigurationManager.AppSettings["apiKey"], Location);
        using (var http = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await http.GetAsync(FormatedAPIUrl);
                response.EnsureSuccessStatusCode();
                var JsonStrings = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };                    
                ConvertedJsonData = JsonSerializer.Deserialize<JsonResults>(JsonStrings, options);
                resultString = Location + " report successfull!!";
                //downloads data
                await FileGeneration(JsonStrings, "json", Location);
                Console.WriteLine($"Weather Today:\n Location: {ConvertedJsonData.LocationName} \n Temperature: {ConvertedJsonData.MainData.temp}\n {ConvertedJsonData.Weather[0].Main}: {ConvertedJsonData.Weather[0].Description}\n ");
            }
            catch (Exception ex)
            {
                resultString = Location + " report failure :----\n" + ex.Message;
            }
            return resultString;
        }

    }
    public static async Task<string> FileGeneration(string StringData, string FileFormat,string LocationName)
    {
        try
        {
            string applicationDirectory = AppDomain.CurrentDomain.BaseDirectory+ "WeatherReports";
            if (!Directory.Exists(applicationDirectory))
            {
                Directory.CreateDirectory(applicationDirectory);
            }
            File.WriteAllText(applicationDirectory + "/WeatherData_" + LocationName + "_" + DateTime.Now.ToString("MMddyyyyHHmmss") + DateTime.Now.Millisecond + "." + FileFormat, StringData);
            return "Success";
        }
        catch(Exception ex) {
            throw new Exception(ex.ToString());            
        }
         
    }
}





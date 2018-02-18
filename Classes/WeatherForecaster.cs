using System;
using System.Xml;

namespace MSAWeb.Classes
{
    public static class WeatherForecaster
    {
        public static Weather Current()
        {
            var doc = new XmlDocument();
            var url = "http://api.wunderground.com/api/****************/conditions/q/Istanbul.xml";
            var condition = Weather.Unknown;
            try
            {
                doc.Load(url);
                var conditionString = doc.DocumentElement.SelectSingleNode("/response/current_observation/weather").InnerText;
                if (conditionString.ToLower().Contains("sun"))
                {
                    condition = Weather.Sunny;
                }
                else if (conditionString.ToLower().Contains("cloud"))
                {
                    condition = Weather.Cloudy;
                }
                else if (conditionString.ToLower().Contains("rain"))
                {
                    condition = Weather.Rainy;
                }
                else if (conditionString.ToLower().Contains("snow"))
                {
                    condition = Weather.Snowy;
                }
                Logger.Log(Logger.Success, "WeatherForecaster", conditionString);
            }
            catch(Exception ex)
            {
                Logger.Log(Logger.Error, "WeatherForecaster", ex.Message);
            }
            return condition;
        }
    }
    
    public enum Weather
    {
        Sunny = 0,
        Cloudy = 1,
        Rainy = 2,
        Snowy = 3,
        Unknown = -1,
    }
}
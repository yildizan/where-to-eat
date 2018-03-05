using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MSAWeb.Classes;
using MSAWeb.Models;

namespace MSAWeb.Controllers
{
    public class SuggestionController : Controller
    {
        public EmptyResult Suggest()
        {
            string body = "";
            try
            {
                var active = DatabaseManager.Instance.Periods.Where(x => x.IsActive).FirstOrDefault();
                var pending = DatabaseManager.Instance.Periods.Where(x => x.IsPending).FirstOrDefault();
                if(active == null)
                {
                    if(pending != null)
                    {
                        body = "Current period is pending for uploading rating document by all users. " +
                               "Period will start automatically after each user uploads the document.";
                    }
                    else
                    {
                        body = "There is no ongoing period. " +
                               "Please add a period with some day(s) to start rating step, then suggestion step.";
                    }
                }
                else
                {
                    var comparisonList = new List<Comparer>();
                    foreach(var restaurant in DatabaseManager.Instance.Restaurants.ToList())
                    {
                        var comparer = new Comparer();
                        comparer.Item = restaurant;
                        foreach(var rating in DatabaseManager.Instance.RatingHistories.Where(x => x.PeriodId == active.Id && x.RestaurantId == restaurant.Id).ToList())
                        {
                            comparer.Total += rating.Taste ?? 0;
                            comparer.Divider += rating.Taste != null ? 1 : 0;
                            comparer.Total += rating.Speed ?? 0;
                            comparer.Divider += rating.Speed != null ? 1 : 0;
                            comparer.Total += rating.Service ?? 0;
                            comparer.Divider += rating.Service != null ? 1 : 0;
                        }
                        comparisonList.Add(comparer);
                    }
                    var best = new Comparer();
                    var tempList = new List<Comparer>(comparisonList);
                    var lastSuggested = DatabaseManager.Instance.SuggestionHistories.OrderByDescending(x => x.Id).FirstOrDefault();
                    var weatherCondition = WeatherForecaster.Current();
                    bool carCheck = DatabaseManager.Instance.SuggestionHistories.OrderByDescending(x => x.Id).Take(2).Any(x => x.Restaurant.CarOrWalk == "C");
                    bool found = false;
                    while (!found && comparisonList.Count > 0)
                    {
                        best = comparisonList.Aggregate((rest1, rest2) => rest1.Average > rest2.Average ? rest1 : rest2);
                        // last suggested check
                        if(lastSuggested != null && lastSuggested.RestaurantId == best.Item.Id)
                        {
                            comparisonList.Remove(best);
                        }
						// 2 car transportation in 3 days check
                        else if (carCheck && best.Item.CarOrWalk == "C")
                        {
                            comparisonList.Remove(best);
                        }
                        // weather condition check
                        else if(best.Item.WeatherDependency && best.Item.CarOrWalk == "W" && (weatherCondition == Weather.Rainy || weatherCondition == Weather.Snowy))
                        {
                            comparisonList.Remove(best);
                        }
                        else
                        {
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        best = tempList.Aggregate((rest1, rest2) => rest1.Average > rest2.Average ? rest1 : rest2);
                    }
                    var suggestion = new SuggestionHistory();
                    suggestion.PeriodId = active.Id;
                    suggestion.OrderNo = active.Count + 1;
                    suggestion.RestaurantId = best.Item.Id;
                    active.Count++;
                    if(active.Count == active.Length)
                    {
                        active.IsActive = false;
                    }
                    DatabaseManager.Instance.SuggestionHistories.Add(suggestion);
                    DatabaseManager.Instance.SaveChanges();
                    body = active.Count + ".suggestion on period " + active.Id + " is " + best.Item.Title + ". " +
                           "Enjoy your lunch!";
                }
                Logger.Log(Logger.Success, "Suggestion/Suggest", body);
            }
            catch(Exception ex)
            {
                Logger.Log(Logger.Error, "Suggestion/Suggest", ex.Message);
            }
            finally
            {
                foreach (var user in DatabaseManager.Instance.Users.ToList())
                {
                    MailSender.SendMail(user.Mail, body);
                }
            }
            return null;
        }
    }
}
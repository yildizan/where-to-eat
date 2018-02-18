using DotNet.Highcharts;
using DotNet.Highcharts.Helpers;
using MSAWeb.Classes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace MSAWeb.Controllers
{
    public class StatisticController : Controller
    {
        public ActionResult Index()
        {
            var restaurantStatistic = DatabaseManager.Instance.SuggestionHistories
                .GroupBy(x => x.Restaurant.Title)
                .Select(y => new { title = y.Key, count = y.Count() })
                .ToDictionary(key => key.title, value => value.count);
            var restaurantData = new object[restaurantStatistic.Count, 2];
            int i = 0;
            foreach(KeyValuePair<string, int> pair in restaurantStatistic)
            {
                restaurantData.SetValue(pair.Key, i, 0);
                restaurantData.SetValue(pair.Value, i++, 1);
            }
            var restaurantChart = new Highcharts("restaurantChart")
                                  .InitChart(new DotNet.Highcharts.Options.Chart
                                  {
                                      Type = DotNet.Highcharts.Enums.ChartTypes.Pie,
                                      BackgroundColor = new DotNet.Highcharts.Helpers.BackColorOrGradient(new Gradient
                                      {
                                          LinearGradient = new[] { 0, 0, 0, 400 },
                                          Stops = new object[,] { { 0, Color.FromArgb(100, 255, 255, 255) },
                                            { 1, Color.FromArgb(100, 255, 255, 255) } }
                                      })
                                  })
                                  .SetPlotOptions(new DotNet.Highcharts.Options.PlotOptions
                                  {
                                      Pie = new DotNet.Highcharts.Options.PlotOptionsPie
                                      {
                                          AllowPointSelect = true,
                                          Cursor = DotNet.Highcharts.Enums.Cursors.Pointer,
                                          ShowInLegend = true
                                      }
                                  })
                                  .SetTitle(new DotNet.Highcharts.Options.Title { Text = "Distribution of Suggested Restaurants" })
                                  .SetXAxis(new DotNet.Highcharts.Options.XAxis { Type = DotNet.Highcharts.Enums.AxisTypes.Category })
                                  .SetSeries(new DotNet.Highcharts.Options.Series { Data = new DotNet.Highcharts.Helpers.Data(restaurantData) })
                                  .SetTooltip(new DotNet.Highcharts.Options.Tooltip { Formatter = "function() { return 'Restaurant: <b>' + this.point.name + '</b><br />Suggested: <b>' + Number((this.percentage).toFixed(2)) + ' %</b>'; }" });
            var userStatistic = DatabaseManager.Instance.RatingHistories
                .GroupBy(x => x.PeriodId)
                .Select(y => new { period = y.Key, count = y.Select(z => z.UserId).Distinct().Count() })
                .ToDictionary(key => key.period, value => value.count);
            var userData = new object[userStatistic.Count, 2];
            i = 0;
            foreach (KeyValuePair<long, int> pair in userStatistic)
            {
                userData.SetValue(pair.Key, i, 0);
                userData.SetValue(pair.Value, i++, 1);
            }
            var userChart = new Highcharts("userChart")
                            .InitChart(new DotNet.Highcharts.Options.Chart
                            {
                                BackgroundColor = new DotNet.Highcharts.Helpers.BackColorOrGradient(new Gradient
                                {
                                    LinearGradient = new[] { 0, 0, 0, 400 },
                                    Stops = new object[,] { { 0, Color.FromArgb(100, 255, 255, 255) },
                                    { 1, Color.FromArgb(100, 255, 255, 255) } }
                                })
                            })
                            .SetTitle(new DotNet.Highcharts.Options.Title { Text = "Number of Users on Periods" })
                            .SetXAxis(new DotNet.Highcharts.Options.XAxis { Type = DotNet.Highcharts.Enums.AxisTypes.Category })
                            .SetSeries(new DotNet.Highcharts.Options.Series
                            {
                                Data = new Data(userData),
                                Color = ColorTranslator.FromHtml("#000")
                            })
                            .SetTooltip(new DotNet.Highcharts.Options.Tooltip { Formatter = "function() { return 'Period Id: <b>' + this.x + '</b><br />Users: <b>' + this.y + '</b>'; }" });
            return View(new Container(new[] { restaurantChart, userChart }));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MSAWeb.Models;
using MSAWeb.Classes;
using System.Net;
using OfficeOpenXml;
using System.IO;
using System.Web;

namespace MSAWeb.Controllers
{
    public class PeriodController : Controller
    {
        public ActionResult Index()
        {
            if (TempData["Message"] != null)
            {
                ViewBag.NotificationMessage = TempData["Message"];
                ViewBag.NotificationTitle = (bool)TempData["Result"] ? "Success" : "Error";
                ViewBag.NotificationType = (bool)TempData["Result"] ? "notice" : "error";
            }
            try
            {
                ViewBag.Data = DatabaseManager.Instance.Periods.ToList();
                ViewBag.ActiveFound = false;
                ViewBag.PendingFound = false;
                var active = DatabaseManager.Instance.Periods.Where(x => x.IsActive).FirstOrDefault();
                var pending = DatabaseManager.Instance.Periods.Where(x => x.IsPending).FirstOrDefault();
                if (active != null)
                {
                    ViewBag.ActiveFound = true;
                    ViewBag.ActivePeriod = active;
                }
                else if (pending != null)
                {
                    ViewBag.PendingFound = true;
                    ViewBag.PendingUsers = (from a in DatabaseManager.Instance.Users
                                            where !DatabaseManager.Instance.RatingHistories.Any(x => x.PeriodId == pending.Id && x.UserId == a.Id)
                                            select a).ToList();
                }
            }
            catch(Exception ex)
            {
                ViewBag.Data = new List<Period>();
                ViewBag.NotificationMessage = "Periods could not be loaded. Inner message: " + ex.Message;
                ViewBag.NotificationTitle = "Error";
                ViewBag.NotificationType = "error";
            }
            return View();
        }

        [HttpGet]
        public ActionResult PrepareAdd()
        {
            return PartialView("_Add", new Period());
        }

        [HttpPost]
        public ActionResult Add(Period period)
        {
            period.Count = 0;
            period.IsActive = false;
            period.IsPending = true;
            if (ModelState.IsValid)
            {
                try
                {
                    DatabaseManager.Instance.Periods.Add(period);
                    DatabaseManager.Instance.SaveChanges();
                    TempData["Message"] = "Period is added.";
                    TempData["Result"] = true;
                }
                catch (Exception ex)
                {
                    TempData["Message"] = "Period could not be added. Inner message: " + ex.Message;
                    TempData["Result"] = false;
                }
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult PrepareCancel(long id)
        {
            var period = DatabaseManager.Instance.Periods.Find(id);
            if (period == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return Json("Period is not found.", JsonRequestBehavior.AllowGet);
            }
            return PartialView("_Cancel", period);
        }

        [HttpPost]
        public ActionResult Cancel(Period period)
        {
            try
            {
                var old = DatabaseManager.Instance.Periods.Find(period.Id);
                old.IsActive = false;
                old.IsPending = false;
                DatabaseManager.Instance.SaveChanges();
                TempData["Message"] = "Period is cancelled.";
                TempData["Result"] = true;
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Period could not be cancelled. Inner message: " + ex.Message;
                TempData["Result"] = false;
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Download(long id)
        {
            var stream = new MemoryStream();
            var name = "rating.xlsx";
            var type = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            try
            {
                using (var package = new ExcelPackage())
                {
                    package.Workbook.Worksheets.Add("rating");
                    var sheet = package.Workbook.Worksheets[1];
                    sheet.Cells[1, 3].Value = "User Info";
                    sheet.Cells[1, 3].Style.Font.Bold = true;
                    sheet.Cells[1, 3].Style.Font.UnderLine = true;
                    sheet.Cells[1, 3].Style.Font.Size = 14;
                    sheet.Cells[5, 3].Value = "Restaurant List";
                    sheet.Cells[5, 3].Style.Font.Bold = true;
                    sheet.Cells[5, 3].Style.Font.UnderLine = true;
                    sheet.Cells[5, 3].Style.Font.Size = 14;
                    sheet.Cells[2, 1].Value = "Id:";
                    sheet.Cells[3, 1].Value = "Name:";
                    sheet.Cells[4, 1].Value = "Surname:";
                    sheet.Cells[6, 1].Value = "Id";
                    sheet.Cells[6, 2].Value = "Title";
                    sheet.Cells[6, 3].Value = "Transportation";
                    sheet.Cells[6, 4].Value = "Weather Dependency";
                    sheet.Cells[6, 5].Value = "Taste (1-10)";
                    sheet.Cells[6, 6].Value = "Speed (1-10)";
                    sheet.Cells[6, 7].Value = "Service (1-10)";
                    var user = DatabaseManager.Instance.Users.Find(id);
                    sheet.Cells[2, 2].Value = user.Id;
                    sheet.Cells[3, 2].Value = user.Name;
                    sheet.Cells[4, 2].Value = user.Surname;
                    int location = 7;   // excelde restoran listesinin başlama yeri
                    foreach (var restaurant in DatabaseManager.Instance.Restaurants.ToList().OrderBy(x => x.Title))
                    {
                        sheet.Cells[location, 1].Value = restaurant.Id;
                        sheet.Cells[location, 2].Value = restaurant.Title;
                        sheet.Cells[location, 3].Value = restaurant.CarOrWalk == "C" ? "By Car" : "By Walking";
                        sheet.Cells[location, 4].Value = restaurant.WeatherDependency == true ? "Dependent" : "Independent";
                        location++;
                    }
                    sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
                    package.SaveAs(stream);
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Rating document could not be downloaded. Inner message: " + ex.Message;
                TempData["Result"] = false;
                return RedirectToAction("Index");
            }
            stream.Position = 0;
            var result = new FileStreamResult(stream, type);
            result.FileDownloadName = name;
            return result;
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            if(file != null && file.ContentLength > 0)
            {
                try
                {
                    using(var package = new ExcelPackage())
                    {
                        package.Load(file.InputStream);
                        var sheet = package.Workbook.Worksheets[1];
                        long userId = Convert.ToInt64(sheet.Cells[2, 2].Value);
                        long periodId = DatabaseManager.Instance.Periods.Where(x => x.IsPending).FirstOrDefault().Id;
                        int location = 7;   // excelde restoran listesinin başlama yeri
                        for (int i = 0; i < DatabaseManager.Instance.Restaurants.Count(); i++)
                        {
                            var rating = new RatingHistory();
                            rating.UserId = userId;
                            rating.PeriodId = periodId;
                            rating.RestaurantId = Convert.ToInt64(sheet.Cells[location + i, 1].Value);
                            rating.Taste = GetExcelValue(sheet.Cells[location + i, 5].Value);
                            rating.Speed = GetExcelValue(sheet.Cells[location + i, 6].Value);
                            rating.Service = GetExcelValue(sheet.Cells[location + i, 7].Value);
                            DatabaseManager.Instance.RatingHistories.Add(rating);
                        }
                        DatabaseManager.Instance.SaveChanges();
                        var pendingUsersCount = (from a in DatabaseManager.Instance.Users
                                                 where !DatabaseManager.Instance.RatingHistories.Any(x => x.PeriodId == periodId && x.UserId == a.Id)
                                                 select a).Count();
                        if (pendingUsersCount == 0)
                        {
                            var period = DatabaseManager.Instance.Periods.Find(periodId);
                            period.IsPending = false;
                            period.IsActive = true;
                        }
                        DatabaseManager.Instance.SaveChanges();
                        TempData["Message"] = "File is uploaded.";
                        TempData["Result"] = true;
                    }
                }
                catch (Exception ex)
                {
                    TempData["Message"] = "File could not be uploaded. Inner message: " + ex.Message;
                    TempData["Result"] = false;
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Index"); ;
        }

        private int? GetExcelValue(dynamic cell)
        {
            return (Convert.ToString(cell) != string.Empty && Convert.ToInt32(cell) > 0 && Convert.ToInt32(cell) < 11) ? Convert.ToInt32(cell) : null;
        }
    }
}
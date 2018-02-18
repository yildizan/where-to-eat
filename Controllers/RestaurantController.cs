using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MSAWeb.Models;
using System.Net;
using MSAWeb.Classes;

namespace MSAWeb.Controllers
{
    public class RestaurantController : Controller
    {
        public ActionResult Index()
        {
            if(TempData["Message"] != null)
            {
                ViewBag.NotificationMessage = TempData["Message"];
                ViewBag.NotificationTitle = (bool)TempData["Result"] ? "Success" : "Error";
                ViewBag.NotificationType = (bool)TempData["Result"] ? "notice" : "error";
            }
            try
            {
                ViewBag.Data = DatabaseManager.Instance.Restaurants.ToList();
            }
            catch(Exception ex)
            {
                ViewBag.Data = new List<Restaurant>();
                ViewBag.NotificationMessage = "Restaurants could not be loaded. Inner message: " + ex.Message;
                ViewBag.NotificationTitle = "Error";
                ViewBag.NotificationType = "error";
            }
            return View();
        }

        [HttpGet]
        public ActionResult PrepareAdd()
        {
            if (!CheckPeriod())
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Adding restaurant is not allowed while there is an active/pending period.", JsonRequestBehavior.AllowGet);
            }
            return PartialView("_Add", new Restaurant());
        }

        [HttpPost]
        public ActionResult Add(Restaurant restaurant)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    DatabaseManager.Instance.Restaurants.Add(restaurant);
                    DatabaseManager.Instance.SaveChanges();
                    TempData["Message"] = "Restaurant is added.";
                    TempData["Result"] = true;
                }
                catch (Exception ex)
                {
                    TempData["Message"] = "Restaurant could not be added. Inner message: " + ex.Message;
                    TempData["Result"] = false;
                }
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult PrepareEdit(long id)
        {
            if (!CheckPeriod())
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Editing restaurant is not allowed while there is an active/pending period.", JsonRequestBehavior.AllowGet);
            }
            var restaurant = DatabaseManager.Instance.Restaurants.Find(id);
            if (restaurant == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return Json("Restaurant is not found.", JsonRequestBehavior.AllowGet);
            }
            return PartialView("_Edit", restaurant);
        }

        [HttpPost]
        public ActionResult Edit(Restaurant restaurant)
        {
            if (ModelState.IsValid)
            {
                var old = DatabaseManager.Instance.Restaurants.Find(restaurant.Id);
                old.Title = restaurant.Title;
                old.CarOrWalk = restaurant.CarOrWalk;
                old.WeatherDependency = restaurant.WeatherDependency;
                try
                {
                    DatabaseManager.Instance.SaveChanges();
                    TempData["Message"] = "Restaurant is edited.";
                    TempData["Result"] = true;
                }
                catch (Exception ex)
                {
                    TempData["Message"] = "Restaurant could not be edited. Inner message: " + ex.Message;
                    TempData["Result"] = false;
                }
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult PrepareDelete(long id)
        {
            if (!CheckPeriod())
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Deleting restaurant is not allowed while there is an active/pending period.", JsonRequestBehavior.AllowGet);
            }
            var restaurant = DatabaseManager.Instance.Restaurants.Find(id);
            if (restaurant == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return Json("Restaurant is not found.", JsonRequestBehavior.AllowGet);
            }
            return PartialView("_Delete", restaurant);
        }

        [HttpPost]
        public ActionResult Delete(Restaurant restaurant)
        {
            try
            {
                DatabaseManager.Instance.Restaurants.Remove(DatabaseManager.Instance.Restaurants.Find(restaurant.Id));
                DatabaseManager.Instance.SaveChanges();
                TempData["Message"] = "Restaurant is deleted.";
                TempData["Result"] = true;
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Restaurant could not be deleted. Inner message: " + ex.Message;
                TempData["Result"] = false;
            }
            return RedirectToAction("Index");
        }

        private bool CheckPeriod()
        {
            var active = DatabaseManager.Instance.Periods.Where(x => x.IsActive).FirstOrDefault();
            var pending = DatabaseManager.Instance.Periods.Where(x => x.IsPending).FirstOrDefault();
			return active == null && pending == null;
        }
    }
}
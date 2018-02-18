using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using MSAWeb.Classes;
using MSAWeb.Models;

namespace MSAWeb.Controllers
{
    public class UserController : Controller
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
                ViewBag.Data = DatabaseManager.Instance.Users.ToList();
            }
            catch (Exception ex)
            {
                ViewBag.Data = new List<User>();
                ViewBag.NotificationMessage = "Users could not be loaded. Inner message: " + ex.Message;
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
                return Json("Adding user is not allowed while there is an active/pending period.", JsonRequestBehavior.AllowGet);
            }
            return PartialView("_Add", new User());
        }

        [HttpPost]
        public ActionResult Add(User user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    DatabaseManager.Instance.Users.Add(user);
                    DatabaseManager.Instance.SaveChanges();
                    TempData["Message"] = "User is added.";
                    TempData["Result"] = true;
                }
                catch (Exception ex)
                {
                    TempData["Message"] = "User could not be added. Inner message: " + ex.Message;
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
                return Json("Editing user is not allowed while there is an active/pending period.", JsonRequestBehavior.AllowGet);
            }
            var user = DatabaseManager.Instance.Users.Find(id);
            if (user == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return Json("User is not found.", JsonRequestBehavior.AllowGet);
            }
            return PartialView("_Edit", user);
        }

        [HttpPost]
        public ActionResult Edit(User user)
        {
            if (ModelState.IsValid)
            {
                var old = DatabaseManager.Instance.Users.Find(user.Id);
                old.Name = user.Name;
                old.Surname = user.Surname;
                old.Mail = user.Mail;
                try
                {
                    DatabaseManager.Instance.SaveChanges();
                    TempData["Message"] = "User is edited.";
                    TempData["Result"] = true;
                }
                catch (Exception ex)
                {
                    TempData["Message"] = "User could not be edited. Inner message: " + ex.Message;
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
                return Json("Deleting user is not allowed while there is an active/pending period.", JsonRequestBehavior.AllowGet);
            }
            var user = DatabaseManager.Instance.Users.Find(id);
            if (user == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return Json("User is not found.", JsonRequestBehavior.AllowGet);
            }
            return PartialView("_Delete", user);
        }

        [HttpPost]
        public ActionResult Delete(User user)
        {
            try
            {
                DatabaseManager.Instance.Users.Remove(DatabaseManager.Instance.Users.Find(user.Id));
                DatabaseManager.Instance.SaveChanges();
                TempData["Message"] = "User is deleted.";
                TempData["Result"] = true;
            }
            catch (Exception ex)
            {
                TempData["Message"] = "User could not be deleted. Inner message: " + ex.Message;
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
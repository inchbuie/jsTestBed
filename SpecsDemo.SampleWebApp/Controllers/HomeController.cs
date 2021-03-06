﻿using SpecsDemo.SampleWebApp.Domain;
using SpecsDemo.SampleWebApp.Models;
using Microsoft.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SpecsDemo.SampleWebApp.Filters;

namespace SpecsDemo.SampleWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICurrentUser _currentUser;

        public HomeController(ICurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        public ActionResult Index()
        {
            ViewBag.Message = string.Format("Hello, {0}!", _currentUser.UserName); ;
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult SayHello(string name)
        {
            var model = new SayHelloViewModel
            {
                Name = name
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult SayHello(SayHelloForm form)
        {
            //bad way with magic string:
            //return RedirectToAction("SayHello", new { name = form.Name });
            //good way using Microsoft.Web.Mvc Futures
            return this.RedirectToAction(c => c.SayHello(form.Name));
        }

        [MattOnly]
        public ActionResult ForMattOnly()
        {
            return View();
        }

        public ActionResult SetName()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SetName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                ViewBag.Error = "You must specify a name!";
                return View();
            }
            _currentUser.SetName(name);
            //return RedirectToAction("Index");
            return this.RedirectToAction(c => c.Index());
        }
    }
}
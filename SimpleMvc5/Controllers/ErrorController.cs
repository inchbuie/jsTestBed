﻿using System.Web.Mvc;

namespace SimpleMvc5.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Http404()
        {
            Response.StatusCode = 404;

            return View();
        }
    }
}
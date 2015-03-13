﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ParentChild.DataLayer;
using ParentChild.Model;
using ParentChild.Web.ViewModels;
using System.Data.Entity.Validation;

namespace ParentChild.Web.Controllers
{
    public class SalesController : Controller
    {
        private SalesContext _salesContext;

        public SalesController()
        {
            _salesContext = new SalesContext();
        }

        public ActionResult Index()
        {
            // return View(_salesContext.SalesOrders.Select(so=>new SalesOrderViewModel(so)).ToList());
            return View(_salesContext.SalesOrders.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SalesOrder salesOrder = _salesContext.SalesOrders.Find(id);
            if (salesOrder == null)
            {
                return HttpNotFound();
            }
            var salesOrderViewModel = ViewModelHelpers.CreateSalesOrderViewModelFromSalesOrder(salesOrder);
            salesOrderViewModel.MessageToClient = "I originated from the viewmodel, not the model";

            return View(salesOrderViewModel);
        }

        public ActionResult Create()
        {
            var viewModel = new SalesOrderViewModel();
            return View(viewModel);
        }

        public JsonResult Save(SalesOrderViewModel salesOrderViewModel)
        {
            SalesOrder salesOrder = ViewModelHelpers.CreateSalesOrderFromSalesOrderViewModel(salesOrderViewModel);

            _salesContext.SalesOrders.Attach(salesOrder);

            if (salesOrder.ObjectState == ObjectState.Deleted)
            {
                //if deleting the order, must set all children to also be deleted
                foreach (var itemViewModel in salesOrder.Items)
                {
                    var item = _salesContext.SalesOrderItems.Find(itemViewModel.Id);
                    if (item != null)
                    {
                        item.ObjectState = ObjectState.Deleted;
                    }
                }
            }
            else
            {
                //check to see if the client deleted any items in the order
                foreach (var itemIdToDelete in salesOrderViewModel.ItemsToDelete)
                {
                    //retrieve item to be deleted
                    SalesOrderItem item = _salesContext.SalesOrderItems.Find(itemIdToDelete);
                    if (item != null)
                    {
                        item.ObjectState = ObjectState.Deleted;
                    }
                }
            }

            _salesContext.ApplyStateChanges();
            try
            {
                _salesContext.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                var validationErrors = e.EntityValidationErrors.ToList();
                throw;
            }

            if (salesOrder.ObjectState == ObjectState.Deleted)
            {
                //when deleting, do not return a view, tell the client to go to the Index instead
                //(we will program the client to look for this anonymous object)
                return Json(new { newLocation = "/Sales/Index/" });
            }

            string msgToClient = ViewModelHelpers.GetMessageToClient(
                salesOrderViewModel.ObjectState, salesOrderViewModel.CustomerName);
            salesOrderViewModel = ViewModelHelpers.CreateSalesOrderViewModelFromSalesOrder(salesOrder);
            salesOrderViewModel.MessageToClient = msgToClient;

            //return anonymous JSON object, not view model directly
            return Json(new { salesOrderViewModel });
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SalesOrder salesOrder = _salesContext.SalesOrders.Find(id);
            if (salesOrder == null)
            {
                return HttpNotFound();
            }
            var salesOrderViewModel = ViewModelHelpers.CreateSalesOrderViewModelFromSalesOrder(salesOrder);
            salesOrderViewModel.MessageToClient = string.Format(
               "The original value of Customer Name is {0}", salesOrder.CustomerName);

            return View(salesOrderViewModel);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SalesOrder salesOrder = _salesContext.SalesOrders.Find(id);
            if (salesOrder == null)
            {
                return HttpNotFound();
            }
            var salesOrderViewModel = ViewModelHelpers.CreateSalesOrderViewModelFromSalesOrder(salesOrder);
            salesOrderViewModel.MessageToClient = "You are about to permanently delete this sales order.";
            salesOrderViewModel.ObjectState = ObjectState.Deleted;
            return View(salesOrderViewModel);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _salesContext.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

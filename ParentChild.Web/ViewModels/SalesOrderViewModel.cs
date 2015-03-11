﻿using ParentChild.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ParentChild.Web.ViewModels
{
    public class SalesOrderViewModel
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string PONumber { get; set; }

        public string MessageToClient { get; set; }

        public SalesOrderViewModel()
        {
            MessageToClient = "I originated from the viewmodel, not the model";
        }
        public SalesOrderViewModel(SalesOrder salesOrder) :this()
        {
            Id = salesOrder.Id;
            CustomerName = salesOrder.CustomerName;
            PONumber = salesOrder.PONumber;
        }

        public SalesOrder CreateDomainObject()
        {
            var salesOrder = new SalesOrder();
            salesOrder.CustomerName = this.CustomerName;
            salesOrder.PONumber = this.PONumber;
            return salesOrder;
        }
    }
}
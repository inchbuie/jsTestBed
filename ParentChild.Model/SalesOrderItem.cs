﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParentChild.Model
{
    public class SalesOrderItem : IObjectWithState
    {
        public int Id { get; set; }
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int SalesOrderId { get; set; }
        public SalesOrder SalesOrder { get; set; }
        public ObjectState ObjectState { get; set; }
        public byte[] RowVersion { get; set; }
    }
}

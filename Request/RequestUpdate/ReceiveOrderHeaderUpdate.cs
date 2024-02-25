﻿using Newtonsoft.Json;
using Request.DomainRequests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Utilities.CoreContants;

namespace Request.RequestUpdate
{
    public class ReceiveOrderHeaderUpdate : DomainUpdate
    {
        public string code { get; set; }
        public double? date { get; set; }
        public string typeCode { get; set; }
        public string statusCode { get; set; }
        //public Guid? vendorId { get; set; }
        public string description { get; set; }
        public double? totalProduct { get; set; }
        public double? totalItem { get; set; }
        public double? amt { get; set; }
        public List<ReceiveOrderLineUpdate> details { get; set; }
    }

    public class ReceiveOrderHeaderStatusUpdate : DomainUpdate
    {
        /// <summary>
        /// LookupConstant.TrangThai_xxx
        /// </summary>
        public string statusCode { get; set; }
    }
}

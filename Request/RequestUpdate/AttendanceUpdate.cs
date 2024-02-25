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
    public class AttendanceUpdate : DomainUpdate
    {
        /// <summary>
        /// 1 - Có mặt
        /// 2 - Vắng phép
        /// 3 - Vắng không phép
        /// </summary>
        public int? status { get; set; }
    }
}

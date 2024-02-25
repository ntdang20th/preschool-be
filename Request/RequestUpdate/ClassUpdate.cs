﻿using Request.DomainRequests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Request.RequestUpdate
{
    public class ClassUpdate : DomainUpdate
    {
        public string name { get; set; }
        public int? size { get; set; }
        public Guid? teacherId { get; set; }
    }
}

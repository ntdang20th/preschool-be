﻿using Entities.DomainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Search
{
    public class PaymentSessionSearch : BaseSearch
    {
        public Guid? studentId { get; set; }
        public int type { get; set; }
        public int status { get; set; }
        public Guid? branchId { get; set; }
    }
}

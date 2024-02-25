﻿using Entities.DomainEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Entities.Search
{
    public class NewsSearch : BaseSearch
    {
        public Guid? groupNewsId { get; set; }
    }
    public class PrivateNewSearch : NewsSearch
    { 
        public string myBranchIds { get; set; }
        public string myGroupIds { get; set; }
        public bool isAdmin { get; set; }

        /// <summary>
        /// Dùng cho mobile app
        /// </summary>
        public Guid? classId { get; set; }
    }
}

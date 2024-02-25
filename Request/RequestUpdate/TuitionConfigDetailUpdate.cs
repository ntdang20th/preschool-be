using Request.DomainRequests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Request.RequestUpdate
{
    public class TuitionConfigDetailUpdate : DomainUpdate
    {
        public string name { get; set; }
        /// <summary>
        /// Học phí
        /// </summary>
        public double? price { get; set; }
        /// <summary>
        /// Ghi chú
        /// </summary>
        public string note { get; set; }
    }
}

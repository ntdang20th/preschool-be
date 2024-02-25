using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class tbl_BillDetail : DomainEntities.DomainEntities
    {
        public Guid? billId { get; set; }
        /// <summary>
        /// Khoản thu
        /// </summary>
        public Guid? tuitionConfigDetailId { get; set; }
        public double? price { get; set; }
        public string note { get; set; }
        public string name { get; set; }
    }
}

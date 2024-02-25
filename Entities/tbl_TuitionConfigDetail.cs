using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Entities
{
    public class tbl_TuitionConfigDetail : DomainEntities.DomainEntities
    {
        public Guid? tuitionConfigId { get; set; }
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

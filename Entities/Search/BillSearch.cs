using Entities.DomainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Search
{
    public class BillSearch : BaseSearch
    {
        public int type { get; set; }
        /// <summary>
        /// Năm học
        /// </summary>
        public Guid? schoolYearId { get; set; }
        /// <summary>
        /// Khoản thu
        /// </summary>
        public Guid? tuitionConfigId { get; set; }
        /// <summary>
        /// Học viên
        /// </summary>
        public Guid? studentId { get; set; }
        public Guid? branchId { get; set; }
    }
}

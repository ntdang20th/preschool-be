using Entities.DomainEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Search
{
    public class TuitionConfigSearch : BaseSearch
    {
        /// <summary>
        /// Học kỳ
        /// </summary>
        public Guid? schoolYearId { get; set; }
        /// <summary>
        /// Khối
        /// </summary>
        public Guid? gradeId { get; set; }
        /// <summary>
        /// 1 - Chưa báo học phí
        /// 2 - Đã báo học phí
        /// </summary>
        public int status { get; set; }
        /// <summary>
        /// 0 - Giảm dần
        /// 1 - Tăng dần
        /// </summary>
        [DefaultValue(0)]
        public override int orderBy { set; get; }
        public Guid? branchId { get; set; }
    }
}

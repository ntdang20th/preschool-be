using Request.DomainRequests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Request.RequestCreate
{
    public class TuitionConfigDetailCreate : DomainCreate
    {
        [Required(ErrorMessage = "Vui lòng chọn học phí")]
        public Guid? tuitionConfigId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string name { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập phí")]
        public double? price { get; set; }
        /// <summary>
        /// Ghi chú
        /// </summary>
        public string note { get; set; }
    }
}

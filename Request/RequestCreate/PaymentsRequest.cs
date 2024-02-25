using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Request.RequestCreate
{
    public class PaymentsRequest
    {
        /// <summary>
        /// Vui lòng chọn công nợ
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn công nợ")]
        public Guid? billId { get; set; }
        /// <summary>
        /// Đã trả
        /// </summary>
        public double paid { get; set; }
        /// <summary>
        /// Khuyến mãi
        /// </summary>
        public Guid? discountId { get; set; }
        public string note { get; set; }
    }
}

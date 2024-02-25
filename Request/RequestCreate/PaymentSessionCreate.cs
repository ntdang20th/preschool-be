using Newtonsoft.Json;
using Request.DomainRequests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Request.RequestCreate
{
    public class PaymentSessionCreate : DomainCreate
    {
        public Guid? studentId { get; set; }
        /// <summary>
        /// Số tiền
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập số tiền")]
        public double? money { get; set; }
        /// <summary>
        /// 1 - Thu vào 
        /// 2 - Chi ra
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập loại phiếu")]
        public int? type { get; set; }
        [JsonIgnore]
        public string typeName { get
            {
                return type == 1 ? "Thu vào" : type == 2 ? "Chi ra" : "";
            }
        }
        /// <summary>
        /// Lý do 
        /// </summary>
        public string reason { get; set; }
        public string note { get; set; }
        /// <summary>
        /// 1 - Chờ duyệt 
        /// 2 - Đã duyệt
        /// </summary>
        [JsonIgnore]
        public int? status { get { return 1; } }
        [JsonIgnore]
        public string statusName { get { return "Chờ duyệt"; } }
        [Required(ErrorMessage = "Vui lòng chọn chi nhánh")]
        public Guid? branchId { get; set; }
    }
}

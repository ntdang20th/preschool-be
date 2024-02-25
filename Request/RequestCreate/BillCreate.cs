using Newtonsoft.Json;
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
    public class BillCreate : DomainCreate
    {
        /// <summary>
        /// Tổng tiền
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập tổng tiền")]
        public double? totalPrice { get; set; }
        /// <summary>
        /// Giảm giá
        /// </summary>
        [JsonIgnore]
        public double? reduced { get { return 0; } }
        /// <summary>
        /// Đã trả
        /// </summary>
        [JsonIgnore]
        public double paid { get { return 0; } }
        /// <summary>
        /// Còn nợ
        /// </summary>
        [JsonIgnore]
        public double? debt { get { return totalPrice ?? 0; } }
        /// <summary>
        /// 2 - Tạo thủ công
        /// </summary>
        [JsonIgnore]
        public int type { get { return 2; } }
        [JsonIgnore]
        public string typeName { get { return "Tạo thủ công"; } }
        /// <summary>
        /// Năm học
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn năm học")]
        public Guid? schoolYearId { get; set; }
        /// <summary>
        /// Học viên
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn học viên")]
        public Guid? studentId { get; set; }
        /// <summary>
        /// Ghi chú
        /// </summary>
        public string note { get; set; }
        public Guid? branchId { get; set; }
    }
}

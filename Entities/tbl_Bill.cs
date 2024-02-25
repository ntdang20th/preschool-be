using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class tbl_Bill : DomainEntities.DomainEntities
    {
        public string code { get; set; }
        /// <summary>
        /// Tổng tiền
        /// </summary>
        public double? totalPrice { get; set; }
        /// <summary>
        /// Giảm giá
        /// </summary>
        public double? reduced { get; set; }
        /// <summary>
        /// Đã trả
        /// </summary>
        public double? paid { get; set; }
        /// <summary>
        /// Còn nợ
        /// </summary>
        public double? debt { get; set; }
        /// <summary>
        /// 1 - Thu học phí
        /// 2 - Tạo thủ công
        /// </summary>
        public int? type { get; set; } 
        public string typeName { get; set; }
        /// <summary>
        /// Năm học
        /// </summary>
        public Guid? schoolYearId { get; set; }
        /// <summary>
        /// Khoản thu
        /// </summary>
        public Guid? tuitionConfigId { get; set; }
        /// <summary>
        /// Khuyến mãi
        /// </summary>
        public Guid? discountId { get; set; }
        /// <summary>
        /// Học viên
        /// </summary>
        public Guid? studentId { get; set; }
        public Guid? branchId { get; set; }
        /// <summary>
        /// Ghi chú
        /// </summary>
        public string note { get; set; }
        public static string GetTypeName(int type) => type == 1 ? "Thu học phí" : type == 2 ? "Tạo thủ công" : "";
        [NotMapped]
        public string schoolYearName { get; set; }
        [NotMapped]
        public string tuitionConfigName { get; set; }
        [NotMapped]
        public string studentName { get; set; }
        [NotMapped]
        public string branchName { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Entities
{
    /// <summary>
    /// Cấu hình khoản thu
    /// </summary>
    public class tbl_TuitionConfig : DomainEntities.DomainEntities
    {
        /// <summary>
        /// Tên
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Tổng học phí
        /// </summary>
        public double? totalPrice { get; set; }
        /// <summary>
        /// Học kỳ
        /// </summary>
        public Guid? schoolYearId { get; set; }
        public Guid? branchId { get; set; }
        [NotMapped]
        public string schoolYearName { get; set; }
        /// <summary>
        /// Khối
        /// </summary>
        public Guid? gradeId { get; set; }
        [NotMapped]
        public string gradeName { get; set; }
        /// <summary>
        /// Ghi chú
        /// </summary>
        public string note { get; set; }
        /// <summary>
        /// 1 - Chưa báo học phí
        /// 2 - Đã báo học phí
        /// </summary>
        public int? status { get; set; }
        public string statusName { get; set; }
        [NotMapped]
        public string branchName { get; set; }
        public static string GetStatusName(int status) => status == 1 ? "Chưa báo học phí" : status == 2 ? "Đã báo học phí" : "";
    }
}

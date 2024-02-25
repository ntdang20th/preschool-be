using AutoMapper.Configuration.Conventions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class tbl_PaymentSession : DomainEntities.DomainEntities
    {
        /// <summary>
        /// Mã phiếu
        /// </summary>
        public string code { get; set; }
        public Guid? studentId { get; set; }
        /// <summary>
        /// Số tiền
        /// </summary>
        public double? money { get; set; }
        /// <summary>
        /// 1 - Thu vào 
        /// 2 - Chi ra
        /// </summary>
        public int? type { get; set; }
        public string typeName { get; set; }
        /// <summary>
        /// Lý do 
        /// </summary>
        public string reason { get; set; }
        public string note { get; set; }
        /// <summary>
        /// 1 - Chờ duyệt 
        /// 2 - Đã duyệt
        /// </summary>
        public int? status { get; set; }
        public string statusName { get; set; }
        public Guid? branchId { get; set; }
        public static string GetTypeName(int type) => type == 1 ? "Thu vào" : type == 2 ? "Chi ra" : "";
        public static string GetStatusName(int status) => status == 1 ? "Chờ duyệt" : status == 2 ? "Đã duyệt" : "";
        [NotMapped]
        public string studentName { get; set; }
        [NotMapped]
        public string studentCode { get; set; }
        [NotMapped]
        public string studentThumbnail { get; set; }
        [NotMapped]
        [JsonIgnore]
        public double totalIncome { get; set; }
        [NotMapped]
        [JsonIgnore]
        public double totalExpense { get; set; }
        [NotMapped]
        [JsonIgnore]
        public double totalRevenue { get; set; }
    }
}

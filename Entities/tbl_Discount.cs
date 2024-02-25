using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class tbl_Discount : DomainEntities.DomainEntities
    {
        public string name { get; set; }
        /// <summary>
        /// 1 - Giảm tiền
        /// 2 - Giảm phần trăm
        /// </summary>
        public int? type { get; set; }
        public string typeName { get; set; }
        /// <summary>
        /// Giá trị
        /// </summary>
        public double? value { get; set; }
        /// <summary>
        /// Số lượng
        /// </summary>
        public int? quantity { get; set; }
        /// <summary>
        /// Sớ lượng đã dùng
        /// </summary>
        public int? usedQuantity { get; set; }
        /// <summary>
        /// Hạn dùng
        /// </summary>
        public double? expiry { get; set; }
        /// <summary>
        /// Mô tả
        /// </summary>
        public string description { get; set; }
        public static string GetTypeName(int type) => type == 1 ? "Giảm tiền" : type == 2 ? "Giảm phần trăm" : ""; 
    }
}

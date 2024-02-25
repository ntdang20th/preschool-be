using Entities;
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
    public class DiscountCreate : DomainCreate
    {
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string name { get; set; }
        /// <summary>
        /// 1 - Giảm tiền
        /// 2 - Giảm %
        /// </summary>
        public int? type { get; set; }
        [JsonIgnore]
        public string typeName { 
            get {
                return tbl_Discount.GetTypeName(type ?? 0);
            }
        }
        /// <summary>
        /// Giá trị
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập số tiền miễn giảm")]
        public double? value { get; set; }
        /// <summary>
        /// Số lượng
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        public int? quantity { get; set; }
        /// <summary>
        /// Sớ lượng đã dùng
        /// </summary>
        [JsonIgnore]
        public int? usedQuantity { get { return 0; } }
        /// <summary>
        /// Hạn dùng
        /// </summary>
        public double? expiry { get; set; }
        /// <summary>
        /// Mô tả
        /// </summary>
        public string description { get; set; }
    }
}

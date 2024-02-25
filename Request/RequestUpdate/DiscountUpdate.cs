using Newtonsoft.Json;
using Request.DomainRequests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Utilities.CoreContants;

namespace Request.RequestUpdate
{
    public class DiscountUpdate : DomainUpdate
    {
        public string name { get; set; }
        /// <summary>
        /// Giá trị
        /// </summary>
        public double? value { get; set; }
        /// <summary>
        /// Số lượng
        /// </summary>
        public int? quantity { get; set; }
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

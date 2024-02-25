using Entities;
using Request.DomainRequests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Request.RequestCreate
{
    public class TuitionConfigCreate : DomainCreate
    {
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string name { get; set; }
        /// <summary>
        /// Học kỳ
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn năm học")]
        public Guid? schoolYearId { get; set; }
        /// <summary>
        /// Khối
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn khối")]
        public Guid? gradeId { get; set; }
        /// <summary>
        /// Ghi chú
        /// </summary>
        public string note { get; set; }
        /// <summary>
        /// 1 - Chưa báo học phí
        /// 2 - Đã báo học phí
        /// </summary>
        [JsonIgnore]
        public int? status { get
            {
                return 1;
            }
        }
        [JsonIgnore]
        public string statusName { get
            {
                return tbl_TuitionConfig.GetStatusName(status.Value);
            }
        }
        [JsonIgnore]
        public double totalPrice { get { return 0; } }
        [Required(ErrorMessage = "Vui lòng chọn chi nhánh")]
        public Guid? branchId { get; set; }
    }
}

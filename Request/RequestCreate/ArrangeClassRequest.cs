using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Request.RequestCreate
{
    public class ArrangeClassRequest
    {
        [Required(ErrorMessage = "Vui lòng chọn năm học")]
        public Guid? schoolYearId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn chi nhánh")]
        public Guid? branchId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn thông tin lớp")]
        public List<ArrangeClassCreate> classes { get; set; }
    }
    public class ArrangeClassCreate
    {
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string name { get; set; }
        public int size { get; set; }
        [Required(ErrorMessage = "Vui long chọn khối")]
        public Guid? gradeId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn giáo viên")]
        public Guid? teacherId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn học viên")]
        public List<Guid> studentIds { get; set; }
    }
}

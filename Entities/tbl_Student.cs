using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Entities
{
    public class tbl_Student : DomainEntities.DomainEntities
    {
        public string code { get; set; }
        public string thumbnail { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string fullName { get; set; }
        /// <summary>
        /// Biệt danh
        /// </summary>
        public string nickname { get; set; }
        /// <summary>
        /// Loại
        /// 1 - Đúng tuyến
        /// 2 - Trái tuyến
        /// </summary>
        public int? type { get; set; }
        public string typeName { get; set; }
        /// <summary>
        /// 1 - Nam 
        /// 2 - Nữ 
        /// 3 - Khác
        /// </summary>
        public int? gender { get; set; }
        public string genderName { get; set; }
        /// <summary>
        /// Dân tộc
        /// </summary>
        public string ethnicity { get; set; }
        /// <summary>
        /// Ngày sinh
        /// </summary>
        public double? birthday { get; set; }
        /// <summary>
        /// Nơi sinh
        /// </summary>
        public string placeOfBirth { get; set; }
        /// <summary>
        /// Địa chỉ hiện tại
        /// </summary>
        public string address { get; set; }
        /// <summary>
        /// Quê quán
        /// </summary>
        public string hometown { get; set; }
        /// <summary>
        /// Hình thức
        /// 1 - Theo buổi
        /// 2 - Bán trú
        /// </summary>
        public int? method { get; set; }
        /// <summary>
        /// Phụ huynh
        /// </summary>
        public Guid? fatherId { get; set; }
        public Guid? motherId { get; set; }
        public Guid? guardianId { get; set; }
        /// <summary>
        /// Ngày nhập học
        /// </summary>
        public double? enrollmentDate { get; set; }
        /// <summary>
        /// Ghi chú
        /// </summary>
        public string note { get; set; }

        /// <summary>
        /// Ngày bắt đầu học
        /// </summary>
        public double? startLearningDate { get; set; }
        /// <summary>
        /// Tình trạng học sinh
        /// 1 - Mới
        /// 2 - Đang học
        /// 3 - Bảo lưu
        /// 4 - Đã học xong
        /// 5 - Bỏ học
        /// </summary>
        public int? status { get; set; }
        public string statusName { get; set; }
        public static string GetStatusName(int status) =>
            status == 1 ? "Mới"
            : status == 2 ? "Đang học"
            : status == 3 ? "Bảo lưu"
            : status == 4 ? "Đã học xong"
            : status == 5 ? "Bỏ học" : "";
        /// <summary>
        /// Khối
        /// </summary>
        public Guid? gradeId { get; set; }
        [NotMapped]
        public string gradeName { get; set; }

        public Guid? branchId { get; set; }
        [NotMapped]
        public Guid? classId { get; set; }
        [NotMapped]
        public string className { get; set; }
        [NotMapped]
        public int? statusAssessment { get; set; }
        [NotMapped]
        public Guid? schoolYearId { get; set; }
    }

    public class StudentSelection
    {
        public Guid id { get; set; }
        public string thumbnail { get; set; }
        public string fullName { get; set; }
        public Guid? classId { get; set; }
        public string className { get; set; }
        public Guid? schoolYearId { get; set; }
        public Guid? branchId { get; set; }
    }
    public class ParentModel
    {
        public Guid? id { get; set; }
        public string thumbnail { get; set; }
        public string fullName { get; set; }
        public string phone { get; set; }
        public string job { get; set; }
        public double? bod { get; set; }
        public int? type { get; set; }
        public string typeName { get; set; }

    }
    public class ProfileStudentForMobile : tbl_Student
    {
        public ParentModel father { get; set; }
        public ParentModel mother { get; set; }
        public ParentModel guardian { get; set; }

    }
}

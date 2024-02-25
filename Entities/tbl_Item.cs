
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Entities
{
    public class tbl_Item : DomainEntities.DomainEntities
    {
        public Guid? branchId { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string nameShort { get; set; }
        public Guid? unitOfMeasureId { get; set; }
        public Guid? itemGroupId { get; set; }
        public double? unitPrice { get; set; }

        //bổ sung thông tin trên 1 đơn vị sản phẩm
        public double? calo { get; set; }
        public double? protein { get; set; }
        public double? lipit { get; set; }
        public double? gluxit { get; set; }
        /// <summary>
        /// Tỷ lệ thải bỏ
        /// </summary>
        public double? essenceRate { get; set; }
        /// <summary>
        /// Trọng lượng (gram) / 1 đơn vị tính
        /// </summary>
        public double? weightPerUnit { get; set; }

        [NotMapped]
        public string itemGroupName { get; set; }
        [NotMapped]
        public string unitOfMeasureName { get; set; }
    }
}

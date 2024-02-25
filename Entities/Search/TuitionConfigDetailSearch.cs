using Entities.DomainEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Entities.Search
{
    public class TuitionConfigDetailSearch : BaseSearch
    {
        [Required(ErrorMessage = "Vui lòng chọn học phí")]
        public Guid? tuitionConfigId { get; set; }
    }
}

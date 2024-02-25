using Entities;
using Entities.DomainEntities;
using Entities.Search;
using Interface.Services.DomainServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Interface.Services
{
    public interface IScaleMeasureDetailService : IDomainService<tbl_ScaleMeasureDetail, ScaleMeasureDetailSearch>
    {
        Task<string> ExportStudentList(Guid scaleMeasureId);
        Task Import(IFormFile excelFile, Guid? scaleMeasureId);
        Task<List<StudentScaleMeasureDetail>> GetMobileScaleMeasure(Guid parentId);
    }
}

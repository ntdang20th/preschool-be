using Entities;
using Extensions;
using Interface.DbContext;
using Interface.Services;
using Interface.UnitOfWork;
using Utilities;
using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.FormulaParsing.ExpressionGraph;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Service.Services.DomainServices;
using Entities.Search;
using Newtonsoft.Json;
using Entities.DomainEntities;
using Entities.AuthEntities;
using Entities.DataTransferObject;

namespace Service.Services
{
    public class StudentInClassService : DomainService<tbl_StudentInClass, StudentInClassSearch>, IStudentInClassService
    {
        private IAppDbContext coreDbContext;
        public StudentInClassService(IAppUnitOfWork unitOfWork, IMapper mapper, IAppDbContext coreDbContext) : base(unitOfWork, mapper)
        {
            this.coreDbContext = coreDbContext;
        }
        protected override string GetStoreProcName()
        {
            return "Get_StudentInClass";
        }
        public async Task<List<UserOption>> GetStudentAvailable(StudentAvailableSearch baseSearch)
        {
            var item = await this.unitOfWork.Repository<tbl_Class>().GetQueryable().FirstOrDefaultAsync(x => x.id == baseSearch.classId) ?? throw new AppException(MessageContants.nf_class);
            var data = await this.unitOfWork.Repository<tbl_Student>().GetDataExport("Get_StudentAvailable_StudentInClass", new SqlParameter[]
            {
                new SqlParameter("classId", item.id),
                new SqlParameter("schoolYearId", item.schoolYearId),
                new SqlParameter("branchId", item.branchId),
            });
            var result = new List<UserOption>();
            if (data.Any())
            {
                result = data.Select(x => new UserOption
                {
                    id = x.id,
                    code = x.code,
                    name = x.fullName
                }).Distinct().ToList();
            }
            return result;
        }
        public async Task<Guid> GetClassIdByStudent(Guid studentId)
        {
            var studentInClass = await this.unitOfWork.Repository<tbl_StudentInClass>().GetQueryable().FirstOrDefaultAsync(x => x.studentId == studentId && x.deleted == false && x.status == (int)CoreContants.StudentInClassStatus.dang_hoc);
            if (studentInClass == null)
                return Guid.Empty;
            return studentInClass.classId ?? Guid.Empty;
        }
        public async Task<string> GetClassNameByStudent(Guid studentId)
        {
            var studentInClass = await this.unitOfWork.Repository<tbl_StudentInClass>().GetQueryable().FirstOrDefaultAsync(x=>x.studentId == studentId && x.deleted == false && x.status == (int)CoreContants.StudentInClassStatus.dang_hoc);
            if(studentInClass == null)
                return "";
            var item = await this.unitOfWork.Repository<tbl_Class>().GetQueryable().FirstOrDefaultAsync(x => x.id == studentInClass.classId && x.deleted == false);
            if (item == null)
                return "";
            return item.name;
        }
    }
}

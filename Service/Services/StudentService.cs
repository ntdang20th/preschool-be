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
using System.Net.Mail;
using Request.DomainRequests;
using Microsoft.Extensions.Configuration;
using static Utilities.CoreContants;
using System.Net;
using Request.RequestCreate;
using System.Net.WebSockets;
using Microsoft.Extensions.DependencyInjection;

namespace Service.Services
{
    public class StudentService : DomainService<tbl_Student, StudentSearch>, IStudentService
    {
        private readonly IParentService parentService;
        private readonly IStudentInClassService studentInClassService;

        public StudentService(IServiceProvider serviceProvider, IAppUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
        {
            this.parentService = serviceProvider.GetRequiredService<IParentService>();
            this.studentInClassService = serviceProvider.GetRequiredService<IStudentInClassService>();
        }

        //public override async Task<tbl_Student> GetByIdAsync(Guid id)
        //{
        //    var sqlParameters = new SqlParameter[]
        //    {
        //        new SqlParameter("@id", id)
        //    };
        //    var student = await this.unitOfWork.Repository<tbl_Student>()
        //        .ExcuteStoreAsync("Get_StudentInfo", sqlParameters);

        //    return student.SingleOrDefault();
        //}

        protected override string GetStoreProcName()
        {
            return "Get_Student";
        }
        public async Task<AppDomainResult> GetStudentsForArrange(ArrangeNewClassSearch request)
        {
            var result = new AppDomainResult { success = true, resultCode = (int)HttpStatusCode.OK };
            var students = this.unitOfWork.Repository<tbl_StudentInClass>().GetQueryable().Where(x => x.deleted == false);

            if (request.schoolYearId.HasValue)
                students = students.Where(x => x.schoolYearId == request.schoolYearId);

            if (request.gradeId.HasValue)
                students = students.Where(x => x.gradeId == request.gradeId);

            if (request.classId.HasValue)
                students = students.Where(x => x.classId == request.classId);

            result.data = await students.ToListAsync();
            return result;
        }

        public async Task<PagedList<tbl_Student>> AvailableStudent(AvailableStudentRequest request)
        {
            var result = new PagedList<tbl_Student>();

            var grade = await this.unitOfWork.Repository<tbl_Grade>().GetQueryable()
                               .FirstOrDefaultAsync(x => x.id == request.gradeId && x.deleted == false)
                               ?? throw new AppException(MessageContants.nf_grade);
            request.minYearOld = grade.studentYearOld;
            var lastGrade = await this.unitOfWork.Repository<tbl_Grade>().GetQueryable()
                               .FirstOrDefaultAsync(x => x.studentYearOld == grade.studentYearOld - 1 && x.deleted == false);
            if (lastGrade != null)
                request.gradeId = lastGrade.id;
            else
                request.gradeId = Guid.Empty;

            var schoolYear = await this.unitOfWork.Repository<tbl_SchoolYear>().GetQueryable()
                .FirstOrDefaultAsync(x => x.deleted == false && x.id == request.schoolYearId.Value)
                ?? throw new AppException(MessageContants.nf_schoolYear);

            var lastSchoolYear = await this.unitOfWork.Repository<tbl_SchoolYear>().GetQueryable()
                .FirstOrDefaultAsync(x => x.deleted == false
                ///&& x.startYear == schoolYear.startYear - 1 - khi nào code tới thì sửa nhe Đặng
                );
            if (lastSchoolYear != null)
                request.schoolYearId = lastSchoolYear.id;
            else
                request.schoolYearId = Guid.Empty;

            var sqlParameters = GetSqlParameters(request);
            result = await this.unitOfWork.Repository<tbl_Student>()
                .ExcuteQueryPagingAsync("Get_StudentAvailable", sqlParameters);
            result.pageSize = request.pageSize;
            result.pageIndex = request.pageIndex;
            return result;
        }

        public async Task<List<StudentSelection>> GetByParent(Guid parentId)
        {
            List<StudentSelection> result = new List<StudentSelection>();
            var parent = await this.unitOfWork.Repository<tbl_Parent>().GetQueryable().FirstOrDefaultAsync(x => x.deleted == false && x.id == parentId)
                ?? throw new AppException(MessageContants.nf_parent);

            var students = await this.unitOfWork.Repository<tbl_Student>().ExcuteStoreAsync("Get_StudentByParent", new SqlParameter[] { new SqlParameter("parentId", parentId) });

            if (students.Any())
            {
                result = students.Select(x => new StudentSelection
                {
                    id = x.id,
                    fullName = x.fullName,
                    thumbnail = x.thumbnail,
                    className = x.className,
                    classId = x.classId,
                    schoolYearId = x.schoolYearId,
                    branchId = x.branchId
                }).ToList();
            }
            return result;
        }
        public override async Task<tbl_Student> GetByIdAsync(Guid id)
        {
            var prs = new SqlParameter[] { new SqlParameter("id", id) };
            //custom response 
            var item = await this.unitOfWork.Repository<tbl_Student>().GetSingleRecordAsync("Get_StudentById", prs);
            return item;
        }

        public async Task<List<ProfileStudentForMobile>> GetProfileForMobile(Guid parentId)
        {
            var students = this.unitOfWork.Repository<tbl_Student>().GetQueryable().Where(
                x => (x.fatherId == parentId || x.motherId == parentId || x.guardianId == parentId) && x.deleted == false).ToList();
            List<ProfileStudentForMobile> result = new List<ProfileStudentForMobile>();
            if (students.Any())
            {
                result = students.Select(x => new ProfileStudentForMobile
                {
                    id = x.id,
                    fullName = x.fullName,
                    thumbnail = x.thumbnail,
                    address = x.address,
                    birthday = x.birthday,
                    code = x.code,
                    ethnicity = x.ethnicity,
                    firstName = x.firstName,
                    lastName = x.lastName,
                    gender = x.gender,
                    genderName = x.genderName,
                    hometown = x.hometown,
                    method = x.method,
                    nickname = x.nickname,
                    placeOfBirth = x.placeOfBirth,
                    fatherId = x.fatherId,
                    motherId = x.motherId,
                    guardianId = x.guardianId,
                    classId = Task.Run(async () => await studentInClassService.GetClassIdByStudent(x.id)).Result,
                    className = Task.Run(async () => await studentInClassService.GetClassNameByStudent(x.id)).Result,
                    enrollmentDate = x.enrollmentDate,
                    status = x.status,
                    statusName = x.statusName,
                    type = x.type,
                    typeName = x.typeName,
                    startLearningDate = x.startLearningDate,
                    gradeId = x.gradeId,   
                    father = Task.Run(async () => await parentService.GetParent(x.fatherId ?? Guid.Empty)).Result,
                    mother = Task.Run(async () => await parentService.GetParent(x.motherId ?? Guid.Empty)).Result,
                    guardian = Task.Run(async () => await parentService.GetParent(x.guardianId ?? Guid.Empty)).Result
                }).ToList();
            }
            return result;
        }

        public async Task<PagedList<tbl_Student>> GetStudentByGrade(GetStudentByGradeRequest request)
        {
            var result = new PagedList<tbl_Student>();

            var grade = await this.unitOfWork.Repository<tbl_Grade>().GetQueryable()
                               .FirstOrDefaultAsync(x => x.id == request.gradeId && x.deleted == false)
                               ?? throw new AppException(MessageContants.nf_grade);

            var schoolYear = await this.unitOfWork.Repository<tbl_SchoolYear>().GetQueryable()
                .FirstOrDefaultAsync(x => x.deleted == false && x.id == request.schoolYearId.Value)
                ?? throw new AppException(MessageContants.nf_schoolYear);

            var sqlParameters = GetSqlParameters(request);

            result = await this.unitOfWork.Repository<tbl_Student>()
                .ExcuteQueryPagingAsync("Get_StudentByGrade", sqlParameters);
            result.pageSize = request.pageSize;
            result.pageIndex = request.pageIndex;
            return result;
        }
    }
}

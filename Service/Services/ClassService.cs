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
using System.Net;
using Request.RequestCreate;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Service.Services
{
    public class ClassService : DomainService<tbl_Class, ClassSearch>, IClassService
    {
        private readonly ITimeTableService timeTableService;
        private readonly IAppDbContext appDbContext;
        private readonly IClassInTimeTableService classInTimeTableService;
        private readonly IAutoGenCodeConfigService autoGenCodeConfigService;
        public ClassService(IAppUnitOfWork unitOfWork, 
            IMapper mapper, 
            ITimeTableService timeTableService, 
            IClassInTimeTableService classInTimeTableService,
            IAutoGenCodeConfigService autoGenCodeConfigService,
            IAppDbContext appDbContext) : base(unitOfWork, mapper)
        {
            this.appDbContext = appDbContext;
            this.autoGenCodeConfigService = autoGenCodeConfigService;
            this.timeTableService= timeTableService;
            this.classInTimeTableService = classInTimeTableService;
        }
        protected override string GetStoreProcName()
        {
            return "Get_Class";
        }

        public override async Task Validate(tbl_Class model)
        {
            if (model.schoolYearId.HasValue)
            {
                var item = await this.unitOfWork.Repository<tbl_SchoolYear>().GetQueryable()
                    .FirstOrDefaultAsync(x => x.id == model.schoolYearId)
                    ?? throw new AppException(MessageContants.nf_schoolYear);
            }
            if (model.branchId.HasValue)
            {
                var item = await this.unitOfWork.Repository<tbl_Branch>().GetQueryable()
                    .FirstOrDefaultAsync(x => x.id == model.branchId)
                    ?? throw new AppException(MessageContants.nf_branch);
            }
            if (model.gradeId.HasValue)
            {
                var item = await this.unitOfWork.Repository<tbl_Grade>().GetQueryable()
                    .FirstOrDefaultAsync(x => x.id == model.gradeId)
                    ?? throw new AppException(MessageContants.nf_grade);
            }
            if (!string.IsNullOrEmpty(model.name))
            {
                var item = await this.unitOfWork.Repository<tbl_Class>().GetQueryable()
                    .AnyAsync(x => x.id != model.id && x.name == model.name);
                if (item)
                    throw new AppException(MessageContants.exs_name);
            }
        }

        public async Task<List<tbl_Class>> GenerateClass(MultipleClassCreate request)
        {
            var schoolYear = await this.unitOfWork.Repository<tbl_SchoolYear>().GetQueryable()
                                .FirstOrDefaultAsync(x => x.id == request.schoolYearId)
                                ?? throw new AppException(MessageContants.nf_schoolYear);
            
            ////Tạo 1 bản nháp thời khóa biểu
            //tbl_TimeTable timeTable = new tbl_TimeTable();
            //timeTable.draff = true;
            //timeTable.name = $"Bản nháp {await autoGenCodeConfigService.AutoGenCode(nameof(tbl_TimeTable))}";
            //timeTable.active = false;

            //await this.timeTableService.CreateAsync(timeTable);

            //tạo các lớp được truyền vào và gắn nó vào bản nháp
            List<tbl_Class> classesResult = new List<tbl_Class>();
            if(request.classes != null && request.classes.Count > 0)
            {
                var classes = request.classes;
                foreach(var _class in classes)
                {
                    var item = await this.unitOfWork.Repository<tbl_Grade>().GetQueryable()
                    .FirstOrDefaultAsync(x => x.id == _class.gradeId)
                    ?? throw new AppException(MessageContants.nf_grade);

                    if (!_class.totalClass.HasValue || _class.totalClass.Value == 0) 
                        continue;
                    int classIndex = _class.increaseWith == 1 ? CoreContants.GetAlphabetIndex(_class.firstLetter.ToCharArray()[0]) + 1 : Convert.ToInt16(_class.firstLetter);
                    for (int i = 0; i < _class.totalClass; i++)
                    {
                        string name = "";
                        if(_class.increaseWith == 1)
                        {
                            name = $"{_class.startWith} {CoreContants.GetAlpabetLetter(classIndex + i - 1)}";
                        }
                        else
                        {
                            name = $"{_class.startWith} {classIndex + i}";
                        }
                        tbl_Class tmp = new tbl_Class
                        {
                            gradeId = _class.gradeId,
                            schoolYearId = request.schoolYearId,
                            name = name,
                            size = _class.size,
                        };
                        await this.CreateAsync(tmp);
                        //await this.classInTimeTableService.CreateAsync(new tbl_ClassInTimeTable
                        //{
                        //    classId = tmp.id,
                        //    timeTableId = timeTable.id
                        //});
                        classesResult.Add(tmp);
                    }
                }
            }
            await this.unitOfWork.SaveAsync();

            //return danh sách lớp vừa tạo
            return classesResult;
        }

        public async Task<List<ClassToPrepare>> GetClassToPrepare(ClassPrepare request)
        {
            List<ClassToPrepare> result = new List<ClassToPrepare>();
            string stringParams = GenerateParamsString(request);
            result = await this.appDbContext.Set<ClassToPrepare>().FromSqlRaw($"Get_ClassToPrepare {stringParams}").ToListAsync();
            return result;
        }

        public async Task<tbl_Class> AddClass(ClassCreate request)
        {
            var model = mapper.Map<tbl_Class>(request);
            await this.Validate(model);
            await this.unitOfWork.Repository<tbl_Class>().CreateAsync(model);

            if(request.shifts != null && request.shifts.Count > 0)
            {
                //qui định giờ học theo ngày
                List<tbl_ClassShift> classShifts = request.shifts.Select(x => new tbl_ClassShift
                {
                    classId = model.id,
                    day = x.day,
                    period = x.period,
                    sTime = x.sTime,
                    eTime = x.eTime
                }).ToList();
                await this.unitOfWork.Repository<tbl_ClassShift>().CreateAsync(classShifts);
            }

            await this.unitOfWork.SaveAsync();
            return model;
        }

        public override async Task<tbl_Class> GetByIdAsync(Guid id)
        {
            var item = await this.unitOfWork.Repository<tbl_Class>().GetSingleRecordAsync("Get_ClassById", new SqlParameter[] { new SqlParameter("id", id) });
            return item;
        }

        public async Task<tbl_Class> ClassByIdAsync(Guid id)
        {
            var item = await this.unitOfWork.Repository<tbl_Class>().Validate(id) ?? throw new AppException(MessageContants.nf_class);
            return item;
        }
    }
}

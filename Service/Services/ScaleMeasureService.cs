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
using Azure.Core;
using Request.RequestCreate;
using Microsoft.Extensions.DependencyInjection;

namespace Service.Services
{
    public class ScaleMeasureService : DomainService<tbl_ScaleMeasure, ScaleMeasureSearch>, IScaleMeasureService
    {
        private readonly IParentService parentService;
        private readonly ISendNotificationService sendNotificationService;
        public ScaleMeasureService(IAppUnitOfWork unitOfWork, IMapper mapper, IServiceProvider serviceProvider) : base(unitOfWork, mapper)
        {
            this.parentService = serviceProvider.GetRequiredService<IParentService>(); 
            this.sendNotificationService = serviceProvider.GetRequiredService<ISendNotificationService>(); 
        }
        protected override string GetStoreProcName()
        {
            return "Get_ScaleMeasures";
        }

        public override async Task Validate(tbl_ScaleMeasure model)
        {
            if (model.branchId.HasValue)
            {
                var item = await this.unitOfWork.Repository<tbl_Branch>().GetQueryable().FirstOrDefaultAsync(x => x.id == model.branchId)
                    ?? throw new AppException(MessageContants.nf_branch);
            }
            if (model.schoolYearId.HasValue)
            {
                var item = await this.unitOfWork.Repository<tbl_SchoolYear>().GetQueryable().FirstOrDefaultAsync(x => x.id == model.schoolYearId)
                    ?? throw new AppException(MessageContants.nf_schoolYear);
            }
            if (!string.IsNullOrWhiteSpace(model.gradeIds))
            {
                var branchIds = model.gradeIds.Split(',').Distinct();
                var itemCount = await this.unitOfWork.Repository<tbl_Grade>().GetQueryable().CountAsync(x => branchIds.Contains(x.id.ToString()));
                if (branchIds.Count() != itemCount)
                    throw new AppException(MessageContants.nf_grade);
            }
            if (!string.IsNullOrWhiteSpace(model.classIds))
            {
                var branchIds = model.classIds.Split(',').Distinct();
                var itemCount = await this.unitOfWork.Repository<tbl_Class>().GetQueryable().CountAsync(x => branchIds.Contains(x.id.ToString()));
                if (branchIds.Count() != itemCount)
                    throw new AppException(MessageContants.nf_class);
            }
        }

        public override async Task AddItem(tbl_ScaleMeasure model)
        {
            if(model.type == 1)
            {
                model.classIds = model.gradeIds = null;
            }
            else if(model.type == 2) //theo khối
            {
                model.classIds = null;
            }
            else if(model.type == 3)
            {
                model.gradeIds = null;
            }

            await this.Validate(model);
            await this.CreateAsync(model);

            //tạo các record detail
            var studentInClasses = this.unitOfWork.Repository<tbl_StudentInClass>().ExcuteStoreAsync("Get_StudentInClassForScaleMeasure", GetSqlParameters(new
            {
                classIds = model.classIds,
                gradeIds = model.gradeIds,
                status = 1
            })).Result.ToList();

            if (studentInClasses.Any())
            {
                //generate
                var now = Timestamp.Now();
                var scaleMeasureDetails = studentInClasses.Select(x => new tbl_ScaleMeasureDetail
                {
                    classId = x.classId,
                    gradeId = x.gradeId,
                    scaleMeasureId = model.id,
                    studentId = x.studentId,
                    studentBirthDay = x.birthday,
                    monthOfAge = (int?)((now - (x.birthday ?? 0)) / (30.44 * 24 * 60 * 60 * 1000)),
                    weight = 0,
                    height = 0,
                    bmi = 0,
                    weightMustHave = 0,
                }).ToList();
                await this.unitOfWork.Repository<tbl_ScaleMeasureDetail>().CreateAsync(scaleMeasureDetails);
            }
            await this.unitOfWork.SaveAsync();
        }

        public async Task SendNotification(ScaleMeasureNotificationRequest request)
        {
            //validate
            var scaleMeasure = await this.unitOfWork.Repository<tbl_ScaleMeasure>()
                .Validate(request.scaleMeasureId.Value) ?? throw new AppException(MessageContants.nf_scaleMeasure);

            //get student ids
            var studentIds = await unitOfWork.Repository<tbl_ScaleMeasureDetail>()
                            .GetQueryable()
                            .Where(x => x.deleted == false && x.scaleMeasureId == scaleMeasure.id && x.studentId.HasValue)
                            .Select(x => x.studentId.Value)
                            .ToListAsync();

            //get data parent 
            var users = await this.parentService.GetParentUserByStudentId(studentIds);

            //send notification
            List<IDictionary<string, string>> notiParams = new List<IDictionary<string, string>>();
            List<IDictionary<string, string>> emailParams = new List<IDictionary<string, string>>();
            Dictionary<string, string> deepLinkQueryDic = new Dictionary<string, string>();
            Dictionary<string, string> param = new Dictionary<string, string>();

            sendNotificationService.SendNotification_v2(LookupConstant.NCC_ScaleMeasure,
                request.title,
                request.content,
                users,
                notiParams,
                emailParams,
                null,
                deepLinkQueryDic,
                LookupConstant.ScreenCode_Health,
                param);
        }
    }
}

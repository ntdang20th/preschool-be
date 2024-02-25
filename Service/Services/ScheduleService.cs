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
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using Request.RequestUpdate;
using Request.RequestCreate;

namespace Service.Services
{
    public class ScheduleService : DomainService<tbl_Schedule, BaseSearch>, IScheduleService
    {
        private readonly IParentService parentService;
        private readonly ITimeTableDetailService timeTableDetailService;
        private readonly ISendNotificationService sendNotificationService;
        private readonly IAppDbContext appDbContext; 
        public ScheduleService(
            IAppUnitOfWork unitOfWork, 
            IMapper mapper, 
            IAppDbContext appDbContext, 
            ITimeTableDetailService timeTableDetailService,
            IParentService parentService, 
            ISendNotificationService sendNotificationService) : base(unitOfWork, mapper)
        {
            this.appDbContext = appDbContext;
            this.timeTableDetailService = timeTableDetailService;
            this.parentService = parentService;
            this.sendNotificationService = sendNotificationService;
        }
        protected override string GetStoreProcName()
        {
            return "Get_Schedule";
        }

        public override async Task Validate(tbl_Schedule model)
        {
            if (model.classId.HasValue)
            {
                var item = await this.unitOfWork.Repository<tbl_Class>().GetQueryable().FirstOrDefaultAsync(x => x.id == model.classId) ?? throw new AppException(MessageContants.nf_class);
            }
            if (model.timeTableId.HasValue)
            {
                var item = await this.unitOfWork.Repository<tbl_TimeTable>().GetQueryable().FirstOrDefaultAsync(x => x.id == model.timeTableId) ?? throw new AppException(MessageContants.nf_timeTable);
            }
            if (model.subjectId.HasValue)
            {
                var item = await this.unitOfWork.Repository<tbl_Subject>().GetQueryable().FirstOrDefaultAsync(x => x.id == model.subjectId) ?? throw new AppException(MessageContants.nf_subject);
            }
            if (model.weekId.HasValue)
            {
                var item = await this.unitOfWork.Repository<tbl_Week>().GetQueryable().FirstOrDefaultAsync(x => x.id == model.weekId) ?? throw new AppException(MessageContants.nf_week);
            }
            if (model.teacherId.HasValue)
            {
                var item = await this.unitOfWork.Repository<tbl_Staff>().GetQueryable().FirstOrDefaultAsync(x => x.id == model.teacherId) ?? throw new AppException(MessageContants.nf_teacher);
            }
            var currentItem = await this.unitOfWork.Repository<tbl_Schedule>().GetQueryable().FirstOrDefaultAsync(x => x.id == model.id);
            if (currentItem != null)
            {
                var detailItem = mapper.Map<tbl_TimeTableDetail>(currentItem);
                detailItem.id = Guid.Empty;
                await this.timeTableDetailService.Validate(detailItem);
            }
        }

        public async Task<List<tbl_Schedule>> GetOrGenerateSchedule(ScheduleSearch request)
        {
            List<tbl_Schedule> result = new List<tbl_Schedule>();

            var _class = await this.unitOfWork.Repository<tbl_Class>().GetQueryable().FirstOrDefaultAsync(x => x.id == request.classId) ?? throw new AppException(MessageContants.nf_class);
            var week = await this.unitOfWork.Repository<tbl_Week>().GetQueryable().FirstOrDefaultAsync(x => x.id == request.weekId) ?? throw new AppException(MessageContants.nf_week);
            int? weekDay = (int?)Timestamp.ToLocalDateTime(week.sTime).DayOfWeek + 1;
            //lấy dữ liệu đã có
            var sqlParams = GetSqlParameters(request);
            result = this.unitOfWork.Repository<tbl_Schedule>().ExcuteStoreAsync("Get_Schedule", sqlParams).Result.ToList();

            //chỉ generate với các tuần sắp tới, kể cả tuần hiện tại cũng không
            var isExistedSchedule = await this.unitOfWork.Repository<tbl_Schedule>().GetQueryable()
                .AnyAsync(x => x.deleted == false && x.type == 1 && x.weekId == week.id);

            if (week.sTime >= Timestamp.Now() || !isExistedSchedule)
            {
                var timeTable = await this.unitOfWork.Repository<tbl_TimeTable>().GetQueryable()
                        .FirstOrDefaultAsync(x => x.deleted == false && x.active == true && x.gradeId == _class.gradeId && x.schoolYearId == _class.schoolYearId && x.branchId == _class.branchId)
                        ?? throw new AppException(MessageContants.nf_timeTable);
                List<TimeTableDetailResponse> details = await appDbContext.Set<TimeTableDetailResponse>().FromSqlRaw($"Get_TimeTableDetailNoPaging @timeTableId = '{timeTable.id}', @classId = '{_class.id}'").ToListAsync();

                //nếu không có dữ liệu => lần đầu load tuần này (lần đầu load trước scheduleJob)
                if (!result.Any())
                {
                    //lấy chi tiết tkb để map ra lịch học
                    if (details.Any())
                    {
                        if (weekDay.HasValue)
                            details = details.Where(x => x.day >= weekDay).ToList();
                        result = details.Where(x => x.classId == _class.id).Select(x => new tbl_Schedule
                        {
                            teacherId = x.teacherId,
                            timeTableId = timeTable.id,
                            subjectId = x.subjectId,
                            classId = x.classId,
                            weekId = week.id,
                            sTime = week.sTime + (x.day - weekDay) * 86400000 + x.sTime,
                            eTime = week.sTime + (x.day - weekDay) * 86400000 + x.eTime,
                            type = 1,
                            teacherName = x.teacherName,
                            subjectName = x.subjectName,
                            subjectType = x.subjectType,

                        }).ToList();
                        await this.unitOfWork.Repository<tbl_Schedule>().CreateAsync(result);
                    }
                }
                else
                {
                    //nếu lịch thời khóa biểu đã được cập nhật lại
                    var automaticDetails = result.Where(x => x.type != 2).ToList();
                    if (automaticDetails.Count != details.Count)
                    {
                        //giữ lại các buổi học tạo thủ công, clear các buổi học tự gen khi trước

                        this.unitOfWork.Repository<tbl_Schedule>().Delete(automaticDetails);

                        //gen lại các buổi học mới rồi append vào mấy cái thủ công
                        if (weekDay.HasValue)
                            details = details.Where(x => x.day >= weekDay).ToList();
                        result = details.Where(x => x.classId == _class.id).Select(x => new tbl_Schedule
                        {
                            teacherId = x.teacherId,
                            timeTableId = timeTable.id,
                            subjectId = x.subjectId,
                            classId = x.classId,
                            weekId = week.id,
                            sTime = week.sTime + (x.day - weekDay) * 86400000 + x.sTime,
                            eTime = week.sTime + (x.day - weekDay) * 86400000 + x.eTime,
                            type = 1,
                            teacherName = x.teacherName,
                            subjectName = x.subjectName,
                            subjectType = x.subjectType,

                        }).ToList();
                        await this.unitOfWork.Repository<tbl_Schedule>().CreateAsync(result);
                    }
                }
            }
            await this.unitOfWork.SaveAsync();
            return result;
        }

        public async Task<List<tbl_Schedule>> DailyActivity(StudentDailyActivityRequest request)
        {
            List<tbl_Schedule> result = new List<tbl_Schedule>();

            var student = await this.unitOfWork.Repository<tbl_Student>().GetQueryable().FirstOrDefaultAsync(x => x.id == request.studentId) ?? throw new AppException(MessageContants.nf_student);

            var dateTime = Timestamp.ToLocalDateTime(request.date);

            //lớp đang học
            var studentInClass = await this.unitOfWork.Repository<tbl_StudentInClass>().GetQueryable().FirstOrDefaultAsync(x => x.deleted == false && x.studentId == student.id && x.status == 1)
                ?? throw new AppException(MessageContants.nf_studentInClass);

            //lịch học của lớp theo datetime
            var currentWeek = await this.unitOfWork.Repository<tbl_Week>().GetQueryable().FirstOrDefaultAsync(x => x.schoolYearId == studentInClass.schoolYearId && x.deleted == false && x.sTime <= request.date && request.date <= x.eTime);

            if (currentWeek != null)
            {
                var schedules = await this.GetOrGenerateSchedule(new ScheduleSearch { classId = studentInClass.classId, weekId = currentWeek.id });
                if (schedules != null)
                {
                    result = schedules.Where(x => x.sTime.HasValue && Timestamp.ToLocalDateTime(x.sTime).Day == dateTime.Day).ToList();
                }
            }

            return result;
        }

        public async Task SendScheduleNotification(ScheduleNotificationRequest request)
        {
            //validate
            var _class = await this.unitOfWork.Repository<tbl_Class>().Validate(request.classId.Value) ?? throw new AppException(MessageContants.nf_class);

            //get data parent 
            var studentIds = await unitOfWork.Repository<tbl_StudentInClass>()
                .GetQueryable()
                .Where(x => x.deleted == false && x.classId == _class.id && x.status == 1 && x.studentId.HasValue)
                .Select(x => x.studentId.Value)
                .ToListAsync();

            var users = await this.parentService.GetParentUserByStudentId(studentIds);

            //send notitication
            List<IDictionary<string, string>> notiParams = new List<IDictionary<string, string>>();
            List<IDictionary<string, string>> emailParams = new List<IDictionary<string, string>>();
            Dictionary<string, string> deepLinkQueryDic = new Dictionary<string, string>();
            Dictionary<string, string> param = new Dictionary<string, string>();


            sendNotificationService.SendNotification_v2(LookupConstant.NCC_Schedule,
                request.title,
                request.content,
                users,
                notiParams,
                emailParams,
                null,
                deepLinkQueryDic,
                LookupConstant.ScreenCode_DailyActivities,
                param);
        }
    }
}

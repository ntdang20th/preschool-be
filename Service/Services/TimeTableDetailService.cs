using AutoMapper;
using Entities;
using Entities.DataTransferObject;
using Entities.Search;
using Extensions;
using Interface.DbContext;
using Interface.Services;
using Interface.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Client;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Drawing.Theme;
using Request.RequestCreate;
using Request.RequestUpdate;
using Service.Services.DomainServices;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Utilities;

namespace Service.Services
{
    public class TimeTableDetailService : DomainService<tbl_TimeTableDetail, TimeTableDetailSearch>, ITimeTableDetailService
    {
        private readonly IAppDbContext appDbContext;
        public TimeTableDetailService(IAppUnitOfWork unitOfWork, IMapper mapper, IAppDbContext app) : base(unitOfWork, mapper)
        {
            this.appDbContext = app;
        }
       
        protected override string GetStoreProcName()
        {
            return "Get_TimeTableDetail";
        }

        public override async Task Validate(tbl_TimeTableDetail model)
        {
            if (model.id != Guid.Empty)
            {
                var item = await this.GetByIdAsync(model.id) ?? throw new AppException(MessageContants.nf_item);
                model.timeTableId = item.timeTableId;
                model.classId = item.classId;
                model.subjectId = model.subjectId?? item.subjectId;
                model.teacherId = model.teacherId ?? item.teacherId;
            }

            var timeTable = await this.unitOfWork.Repository<tbl_TimeTable>().GetQueryable().FirstOrDefaultAsync(x => x.deleted == false && x.id == model.timeTableId)
                    ?? throw new AppException(MessageContants.nf_timeTable);
            var _class = await this.unitOfWork.Repository<tbl_Class>().GetQueryable().FirstOrDefaultAsync(x => x.deleted == false && x.id == model.classId)
                ?? throw new AppException(MessageContants.nf_class);
            
            var subject = await this.unitOfWork.Repository<tbl_Subject>().GetQueryable().FirstOrDefaultAsync(x => x.deleted == false && x.id == model.subjectId)
                   ?? throw new AppException(MessageContants.nf_subject);
            var teacher = await this.unitOfWork.Repository<tbl_Staff>().GetQueryable().FirstOrDefaultAsync(x => x.deleted == false && x.id == model.teacherId)
                ?? throw new AppException(MessageContants.nf_teacher);

            //check trùng với tkb hiện tại
            var timeTableDetail = await this.unitOfWork.Repository<tbl_TimeTableDetail>().GetQueryable()
                .AnyAsync(x => 
            x.deleted == false && x.timeTableId == model.timeTableId
            && ((x.sTime <= model.sTime && model.sTime <= x.eTime) || (x.sTime <= model.eTime && model.eTime <= x.eTime))
            && x.day == model.day
            && x.id != model.id
            && x.teacherId == model.teacherId);
            if (timeTableDetail)
                throw new AppException(MessageContants.exs_scheduleTeacher);

            //validate teacher have another schedule in time
            //kiểm tra xem giờ này giáo viên có đứng lớp buổi nào không, với giáo viên thường
            //nếu giáo viên là giáo viên chủ nhiệm nhưng giờ này lớp của giáo viên đó đang học giáo viên khác thì được phép dạy
            string stringParams = this.GenerateParamsString(new { _class.schoolYearId, _class.branchId, model.teacherId, model.day });
            var schedules = await this.appDbContext.Set<tbl_TimeTableDetail>().FromSqlRaw($"Get_TeacherSchedule {stringParams}").ToListAsync();
            schedules = schedules.Where(x => x.id != model.id).ToList();

            if (schedules.Any())
            {
                //nếu bị trùng lịch thì throw new Appexception
                var exsist = schedules.Any(x => (x.sTime <= model.sTime && model.sTime <= x.eTime) || (x.sTime <= model.eTime && model.eTime <= x.eTime));
                if (exsist)
                    throw new AppException(MessageContants.exs_scheduleTeacher);

                //Ngược lại thì kiểm tra xem những lớp thằng này chủ nhiệm, nếu kh có lớp nào thì nó kh phải chủ nhiệm => pass
                var formClasses = await this.unitOfWork.Repository<tbl_Class>().GetQueryable().Where(x => x.deleted == false && x.teacherId == teacher.id && x.schoolYearId == model.schoolYearId && x.branchId == model.branchId).ToListAsync();
                if (formClasses.Any())
                {
                    //nếu có lớp dc chủ nhiệm: check những giờ kh học của lớp, nếu trùng ngay những giờ này thì cũng coi như là trùng
                    
                }
            }

            //check các lịch thủ công trong tương lai của tkb này có cái nào bị trùng không, nếu trùng thì báo luôn tên lớp với tuần bị trùng
            var dupSchedules = await this.unitOfWork.Repository<tbl_Schedule>().GetQueryable()
             .FirstOrDefaultAsync(x => x.deleted == false
                 && x.timeTableId == model.timeTableId
                 && ((long)x.sTime) % 86400000 <= model.eTime
                 && ((long)x.eTime) % 86400000 >= model.sTime
                 && x.sTime >= Timestamp.Now() //chỉ validate với các lịch trong tương lai
                 && x.type == 2
                 );
            if (dupSchedules != null)
            {
                string message = $"Tiết học đã bị trùng với lịch học bù của lớp {_class.name} vào lúc {Timestamp.ToString(dupSchedules.sTime, "HH:mm dd/MM/yyyy")}. Vui lòng kiểm tra lại!";
                throw new AppException(message);
            }

            //pass hết thì dc thêm/chỉnh sửa tiết
        }

      
    }
}

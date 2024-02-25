using Entities;
using Extensions;
using Interface.Services;
using Interface.Services.Auth;
using Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Models;
using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using Request;
using Entities.Search;
using Request.DomainRequests;
using AutoMapper;
using Request.RequestCreate;
using Request.RequestUpdate;
using System.Reflection;
using Newtonsoft.Json;
using Entities.DomainEntities;
using BaseAPI.Controllers;
using Service.Services;
using Microsoft.AspNetCore.SignalR;
using System.Security.Cryptography;
using System.Reflection.Metadata;
using Interface.DbContext;
using System.Data.Entity;
using Microsoft.CodeAnalysis.CSharp;

namespace API.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [ApiController]
    [Description("Màn hình điểm danh")]
    [Authorize]
    public class AttendanceController : BaseController<tbl_Attendance, AttendanceCreate, AttendanceUpdate, BaseSearch>
    {
        private readonly IAttendanceService attendanceService;
        public AttendanceController(IServiceProvider serviceProvider, ILogger<BaseController<tbl_Attendance, AttendanceCreate, AttendanceUpdate, BaseSearch>> logger
            , IWebHostEnvironment env
            , IDomainHub hubcontext) : base(serviceProvider, logger, env
            )
        {
            this.attendanceService = serviceProvider.GetRequiredService<IAttendanceService>();
            this.domainService = serviceProvider.GetRequiredService<IAttendanceService>();
        }
        [NonAction]
        public override Task<AppDomainResult> Get([FromQuery] BaseSearch baseSearch)
        {
            return base.Get(baseSearch);
        }
        [NonAction]
        public override Task<AppDomainResult> GetById(Guid id)
        {
            return base.GetById(id);
        }
        [NonAction]
        public override Task<AppDomainResult> DeleteItem(Guid id)
        {
            return base.DeleteItem(id);
        }
        [NonAction]
        public override Task<AppDomainResult> AddItem([FromBody] AttendanceCreate itemModel)
        {
            return base.AddItem(itemModel);
        }

        /// <summary>
        /// Lấy danh sách item không có phân trang
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpGet]
        [AppAuthorize]
        [Description("Lấy danh sách")]
        public async Task<AppDomainResult> Get([FromQuery] AttendanceSearch baseSearch)
        {
            if (!ModelState.IsValid)
                throw new AppException(ModelState.GetErrorMessage());
            var data = await this.attendanceService.GetOrGenerateAttendance(baseSearch);
            return new AppDomainResult(data);
        }

        [HttpPost("notify")]
        [AppAuthorize]
        [Description("Thông báo cho tất cả phụ huynh của các bé trong lớp")]
        public async Task<AppDomainResult> Notify(AttendanceNotificationRequest request)
        {
            await attendanceService.SendNotification(request);
            return new AppDomainResult();
        }
    }
}

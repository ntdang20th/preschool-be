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
using System.Security.Cryptography.Xml;
using API.Model;

namespace API.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [ApiController]
    [Description("Quản lý công nợ")]
    [Authorize]
    public class BillController : BaseController<tbl_Bill, BillCreate, BillUpdate, BillSearch>
    {
        private readonly ISchoolYearService schoolYearService;
        private readonly IStudentService studentService;
        private readonly IAutoGenCodeConfigService autoGenCodeConfigService;
        private readonly IBranchService branchService;
        private readonly IBillDetailService billDetailService;
        private readonly IBillService billService;
        public BillController(IServiceProvider serviceProvider, ILogger<BaseController<tbl_Bill, BillCreate, BillUpdate, BillSearch>> logger
            , IWebHostEnvironment env
            , IDomainHub hubcontext) : base(serviceProvider, logger, env
            )
        {
            this.domainService = serviceProvider.GetRequiredService<IBillService>();
            this.schoolYearService = serviceProvider.GetRequiredService<ISchoolYearService>();
            this.studentService = serviceProvider.GetRequiredService<IStudentService>();
            this.autoGenCodeConfigService = serviceProvider.GetRequiredService<IAutoGenCodeConfigService>();
            this.branchService = serviceProvider.GetRequiredService<IBranchService>();
            this.billDetailService = serviceProvider.GetRequiredService<IBillDetailService>();
            this.billService = serviceProvider.GetRequiredService<IBillService>();
        }
        [NonAction]
        public override async Task Validate(tbl_Bill model)
        {
            if (model.schoolYearId.HasValue)
            {
                var hasSchoolYear = await schoolYearService.AnyAsync(x => x.id == model.schoolYearId);
                if (!hasSchoolYear)
                    throw new AppException("Không tìm thấy năm học");
            }
            if (model.studentId.HasValue)
            {
                var hasStudent = await studentService.AnyAsync(x => x.id == model.studentId);
                if (!hasStudent)
                    throw new AppException("Không tìm thấy học viên");
            }
            if (model.branchId.HasValue)
            {
                var hasBranch = await branchService.AnyAsync(x => x.id == model.branchId);
                if (!hasBranch)
                    throw new AppException("Không tìm thấy chi nhánh");
            }
        }
        [HttpPost]
        [AppAuthorize]
        [Description("Thêm mới")]
        public override async Task<AppDomainResult> AddItem([FromBody] BillCreate itemModel)
        {
            if (!ModelState.IsValid)
                throw new AppException(ModelState.GetErrorMessage());
            var item = mapper.Map<tbl_Bill>(itemModel);
            if (item == null)
                throw new AppException(MessageContants.nf_item);

            item.code = await autoGenCodeConfigService.AutoGenCode(nameof(tbl_Bill));

            await Validate(item);
            await this.domainService.AddItem(item);
            return new AppDomainResult(item);
        }
        /// <summary>
        /// Lấy thông tin theo id
        /// </summary>
        /// <param name="billId"></param>
        /// <returns></returns>
        [HttpGet("detail/{billId}")]
        [AppAuthorize]
        [Description("Lấy thông tin")]
        public async Task<AppDomainResult> GetDetail(Guid billId)
        {
            var item = (await billDetailService.GetAsync(x => x.billId == billId && x.deleted == false)).Select(b => new BillDetailDTO
            {
                id = b.id,
                billId = b.billId,
                name = b.name,
                note = b.note,
                price = b.price,
                tuitionConfigDetailId = b.tuitionConfigDetailId
            }).ToList();
            return new AppDomainResult(item);
        }
        [HttpPost("payments")]
        [AppAuthorize]
        [Description("Thanh toán")]
        public async Task<AppDomainResult> Payments([FromBody] PaymentsRequest itemModel)
        {
            if (!ModelState.IsValid)
                throw new AppException(ModelState.GetErrorMessage());
            await billService.Payments(itemModel);
            return new AppDomainResult();
        }
    }
}

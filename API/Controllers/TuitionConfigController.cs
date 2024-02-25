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
using OfficeOpenXml;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml.Style;

namespace API.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [ApiController]
    [Description("Cấu hình học phí")]
    [Authorize]
    public class TuitionConfigController : BaseController<tbl_TuitionConfig, TuitionConfigCreate, TuitionConfigUpdate, TuitionConfigSearch>
    {
        private ISchoolYearService schoolYearService;
        private IGradeService gradeService;
        private ITuitionConfigDetailService tuitionConfigDetailService;
        private IAppDbContext appDbContext;
        private ITuitionConfigService tuitionConfigService;
        private IBranchService branchService;
        public TuitionConfigController(IServiceProvider serviceProvider, ILogger<BaseController<tbl_TuitionConfig, TuitionConfigCreate, TuitionConfigUpdate, TuitionConfigSearch>> logger
            , IWebHostEnvironment env
            , IDomainHub hubcontext) : base(serviceProvider, logger, env
            )
        {
            this.domainService = serviceProvider.GetRequiredService<ITuitionConfigService>();
            this.schoolYearService = serviceProvider.GetRequiredService<ISchoolYearService>();
            this.gradeService = serviceProvider.GetRequiredService<IGradeService>();
            this.tuitionConfigDetailService = serviceProvider.GetRequiredService<ITuitionConfigDetailService>();
            this.appDbContext = serviceProvider.GetRequiredService<IAppDbContext>();
            this.tuitionConfigService = serviceProvider.GetRequiredService<ITuitionConfigService>();
            this.branchService = serviceProvider.GetRequiredService<IBranchService>();
        }
        [NonAction]
        public override async Task Validate(tbl_TuitionConfig model)
        {
            if (model.schoolYearId.HasValue)
            {
                var hasSchoolYear = await schoolYearService.AnyAsync(x=>x.id == model.schoolYearId && x.deleted == false);
                if (!hasSchoolYear)
                    throw new AppException("Không tìm thấy năm học");
            }
            if (model.gradeId.HasValue)
            {
                var hasGrade = await gradeService.AnyAsync(x => x.id == model.gradeId && x.deleted == false);
                if (!hasGrade)
                    throw new AppException("Không tìm thấy khối");
            }
            if (model.branchId.HasValue)
            {
                var hasBranch = await branchService.AnyAsync(x => x.id == model.branchId && x.deleted == false);
                if (!hasBranch)
                    throw new AppException("Không tìm thấy chi nhánh");
            }
        }
        [HttpGet("{id}")]
        [AppAuthorize]
        [Description("Lấy thông tin")]
        public override async Task<AppDomainResult> GetById(Guid id)
        {
            var item = await this.domainService.GetByIdAsync(id) ?? throw new KeyNotFoundException(MessageContants.nf_item);
            var branch = await branchService.GetByIdAsync(item.branchId ?? Guid.Empty);
            if (branch != null)
                item.branchName = branch.name;
            var grade = await gradeService.GetByIdAsync(item.gradeId ?? Guid.Empty);
            if (grade != null)
                item.gradeName = grade.name;
            var schoolYear = await schoolYearService.GetByIdAsync(item.schoolYearId ?? Guid.Empty);
            if (schoolYear != null)
                item.schoolYearName = schoolYear.name;
            return new AppDomainResult(item);
        }
        /// <summary>
        /// Thêm mới khoản thu
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPost("detail")]
        [AppAuthorize]
        [Description("Thêm mới khoản thu")]
        public async Task<AppDomainResult> AddDetail([FromBody] TuitionConfigDetailCreate itemModel)
        {
            using (var tran = await appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    if (!ModelState.IsValid)
                        throw new AppException(ModelState.GetErrorMessage());
                    var tuitionConfig = await domainService.GetByIdAsync(itemModel.tuitionConfigId.Value);
                    if (tuitionConfig == null)
                        throw new AppException("Không tìm thấy học phí");

                    var item = mapper.Map<tbl_TuitionConfigDetail>(itemModel);
                    if (item == null)
                        throw new AppException(MessageContants.nf_item);
                    await this.tuitionConfigDetailService.AddItem(item);
                    tuitionConfig.totalPrice += item.price;
                    await domainService.UpdateAsync(tuitionConfig);
                    await tran.CommitAsync();
                    return new AppDomainResult();
                }
                catch (AppException e)
                {
                    await tran.RollbackAsync();
                    throw e;
                }
            }
        }
        /// <summary>
        /// Chỉnh sửa khoản thu
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        [HttpPut("detail")]
        [AppAuthorize]
        [Description("Chỉnh sửa khoản thu")]
        public async Task<AppDomainResult> UpdateDetail([FromBody] TuitionConfigDetailUpdate itemModel)
        {
            using (var tran = await appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    if (!ModelState.IsValid)
                        throw new AppException(ModelState.GetErrorMessage());
                    var oldItem = await tuitionConfigDetailService.GetByIdAsync(itemModel.id);
                    var item = mapper.Map<tbl_TuitionConfigDetail>(itemModel);
                    if (item == null)
                        throw new KeyNotFoundException(MessageContants.nf_item);
                    await this.tuitionConfigDetailService.UpdateItem(item);

                    if (item.price.HasValue)
                    {
                        var tuitionConfig = await domainService.GetByIdAsync(oldItem.tuitionConfigId.Value);
                        if (tuitionConfig == null)
                            throw new AppException("Không tìm thấy học phí");

                        tuitionConfig.totalPrice -= oldItem.price;
                        tuitionConfig.totalPrice += item.price;
                        await domainService.UpdateAsync(tuitionConfig);
                    }
                    await tran.CommitAsync();
                    return new AppDomainResult();
                }
                catch (Exception e)
                {
                    await tran.RollbackAsync();
                    throw new AppException(e.Message);
                }
            }
        }
        /// <summary>
        /// Thêm mới khoản thu
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        [HttpDelete("detail/{id}")]
        [AppAuthorize]
        [Description("Xoá khoản thu")]
        public async Task<AppDomainResult> DeleteDetail(Guid id)
        {
            using (var tran = await appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var item = await tuitionConfigDetailService.GetByIdAsync(id);
                    if (item == null)
                        throw new AppException("Không tìm thấy dữ liệu");
                    await this.tuitionConfigDetailService.DeleteItem(id);

                    var tuitionConfig = await domainService.GetByIdAsync(item.tuitionConfigId.Value);
                    if (tuitionConfig == null)
                        throw new AppException("Không tìm thấy học phí");
                    tuitionConfig.totalPrice -= item.price;
                    await domainService.UpdateAsync(tuitionConfig);
                    await tran.CommitAsync();
                    return new AppDomainResult();
                }
                catch (Exception e)
                {
                    await tran.RollbackAsync();
                    throw new AppException(e.Message);
                }
            }
        }
        [HttpGet("detail/{tuitionConfigId}")]
        [AppAuthorize]
        [Description("Xem chi tiết")]
        public async Task<AppDomainResult> GetDetail(Guid tuitionConfigId)
        {
            var data = await tuitionConfigDetailService.GetAsync(x=> x.tuitionConfigId == tuitionConfigId && x.deleted == false);
            return new AppDomainResult(data);
        }
        [HttpPost("notice/{tuitionConfigId}")]
        [AppAuthorize]
        [Description("Thêm mới khoản thu")]
        public async Task<AppDomainResult> TuitionFeeNotice(Guid tuitionConfigId)
        {
            try
            {
                var httpContextHost = HttpContext.Request.Host;
                var pathEmailTemplate = Path.Combine(Path.Combine(env.ContentRootPath, "Template", "EmailTemplate"));
                await tuitionConfigService.TuitionFeeNotice(tuitionConfigId, pathEmailTemplate);
                return new AppDomainResult();
            }
            catch (AppException e)
            {
                throw e;
            }
        }

    }
}

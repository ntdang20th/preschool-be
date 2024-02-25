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
using System.Drawing.Printing;

namespace API.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [ApiController]
    [Description("Màn hình thu chi")]
    [Authorize]
    public class PaymentSessionController : BaseController<tbl_PaymentSession, PaymentSessionCreate, PaymentSessionUpdate, PaymentSessionSearch>
    {
        private readonly IBranchService branchService;
        private readonly IAutoGenCodeConfigService autoGenCodeConfigService;
        public PaymentSessionController(IServiceProvider serviceProvider, ILogger<BaseController<tbl_PaymentSession, PaymentSessionCreate, PaymentSessionUpdate, PaymentSessionSearch>> logger
            , IWebHostEnvironment env
            , IDomainHub hubcontext) : base(serviceProvider, logger, env
            )
        {
            this.domainService = serviceProvider.GetRequiredService<IPaymentSessionService>();
            this.branchService = serviceProvider.GetRequiredService<IBranchService>();
            this.autoGenCodeConfigService = serviceProvider.GetRequiredService<IAutoGenCodeConfigService>();
        }
        [NonAction]
        public override async Task Validate(tbl_PaymentSession model)
        {
            if (model.branchId.HasValue)
            {
                var hasBranch = await branchService.AnyAsync(x => x.id == model.branchId && x.deleted == false);
                if (!hasBranch)
                    throw new AppException("Không tìm thấy chi nhánh");
            }
        }
        /// <summary>
        /// Thêm mới item
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPost]
        [AppAuthorize]
        [Description("Thêm mới")]
        public override async Task<AppDomainResult> AddItem([FromBody] PaymentSessionCreate itemModel)
        {
            if (!ModelState.IsValid)
                throw new AppException(ModelState.GetErrorMessage());
            var item = mapper.Map<tbl_PaymentSession>(itemModel);
            if (item == null)
                throw new AppException(MessageContants.nf_item);
            item.code = await autoGenCodeConfigService.AutoGenCode(nameof(tbl_PaymentSession));
            await Validate(item);
            await this.domainService.AddItem(item);
            return new AppDomainResult(item);
        }
        /// <summary>
        /// Xóa item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [AppAuthorize]
        [Description("Xoá")]
        [NonAction]
        public override async Task<AppDomainResult> DeleteItem(Guid id)
        {
            try
            {
                await this.domainService.DeleteItem(id);
                return new AppDomainResult();
            }
            catch (AppException e)
            {
                throw new AppException(e.Message);
            }
        }
        [HttpGet]
        [AppAuthorize]
        [Description("Lấy danh sách")]
        public override async Task<AppDomainResult> Get([FromQuery] PaymentSessionSearch baseSearch)
        {
            if (!ModelState.IsValid)
                throw new AppException(ModelState.GetErrorMessage());
            PagedList<tbl_PaymentSession> pagedData = await this.domainService.GetPagedListData(baseSearch);
            double totalIncome = 0;
            double totalExpense = 0;
            double totalRevenue = 0;
            if (pagedData.items.Any())
            {
                totalIncome = pagedData.items[0].totalIncome;
                totalExpense = pagedData.items[0].totalExpense;
                totalRevenue = pagedData.items[0].totalRevenue;
            }
            return new AppDomainResult
            { 
                resultCode = ((int)HttpStatusCode.OK),
                resultMessage = "Thành công",
                success = true,
                data = new
                {
                    pageIndex = pagedData.pageIndex,
                    pageSize = pagedData.pageSize,
                    totalPage = pagedData.totalPage,
                    totalItem = pagedData.totalItem,
                    items = pagedData.items,

                }
            };
        }
    }
}

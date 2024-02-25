﻿using Entities;
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
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace API.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [ApiController]
    [Description("Chi tiết đợt cân đo")]
    [Authorize]
    public class ScaleMeasureDetailController : BaseController<tbl_ScaleMeasureDetail, ScaleMeasureDetailCreate, ScaleMeasureDetailUpdate, ScaleMeasureDetailSearch>
    {
        private readonly IScaleMeasureDetailService scaleMeasureDetailService;
        private readonly IParentService parentService;
        public ScaleMeasureDetailController(IServiceProvider serviceProvider, ILogger<BaseController<tbl_ScaleMeasureDetail, ScaleMeasureDetailCreate, ScaleMeasureDetailUpdate, ScaleMeasureDetailSearch>> logger
            , IWebHostEnvironment env
            , IDomainHub hubcontext) : base(serviceProvider, logger, env
            )
        {
            this.domainService = serviceProvider.GetRequiredService<IScaleMeasureDetailService>();
            this.scaleMeasureDetailService = serviceProvider.GetRequiredService<IScaleMeasureDetailService>();
            this.parentService = serviceProvider.GetRequiredService<IParentService>();
        }


        /// <summary>
        /// Export
        /// </summary>
        /// <param name="scaleMeasureId"></param>
        /// <returns></returns>
        [HttpGet("student-list/{scaleMeasureId}")]
        [AppAuthorize]
        [Description("Export")]
        public async Task<AppDomainResult> Export(Guid scaleMeasureId)
        {
            var fileUrl = "";
            fileUrl = await this.scaleMeasureDetailService.ExportStudentList(scaleMeasureId);
            return new AppDomainResult(fileUrl);
        }

        /// <summary>
        /// Import
        /// </summary>
        /// <param name="excelFile"></param>
        /// <param name="scaleMeasureId"></param>
        /// <returns></returns>
        [HttpPost("import")]
        [AppAuthorize]
        [Description("Import")]
        public async Task<AppDomainResult> Import(IFormFile excelFile, Guid? scaleMeasureId)
        {
            await this.scaleMeasureDetailService.Import(excelFile, scaleMeasureId);
            return new AppDomainResult();
        }

        ///// <summary>
        ///// Danh sách chi tiết đợt cân đo của trẻ
        ///// </summary>
        //[HttpGet]
        //[Route("mobile-scale-measure/{studentId}")]
        //[AppAuthorize]
        //[Description("Chi tiết đợt cân đo của trẻ")]
        //public async Task<AppDomainResult> GetMobileScaleMeasure(Guid studentId)
        //{
        //    var userLog = LoginContext.Instance.CurrentUser ?? throw new AppException(MessageContants.auth_expiried);
        //    var parent = await this.parentService.GetSingleAsync(x => x.userId == userLog.userId) ?? throw new AppException(MessageContants.nf_parent);
        //    var result = await scaleMeasureDetailService.GetMobileScaleMeasure(studentId);
        //    return new AppDomainResult(new
        //    {
        //        items = result,
        //        totalItem = result.Count,
        //    });
        //}


        [HttpGet("by-student")]
        [AppAuthorize]
        [Description("Get by student")]
        public async Task<AppDomainResult> GetByStudent([FromQuery] ScaleMeasureDetailSearch search)
        {
            var result = new tbl_ScaleMeasureDetail();
            var data = await this.domainService.GetPagedListData(search);
            if (data.items != null && data.items.Count > 0)
                result = data.items.FirstOrDefault();

            return new AppDomainResult(result);
        }
    }
}

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

namespace API.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [ApiController]
    [Description("Chức vụ")]
    [Authorize]
    public class MenuController : BaseController<tbl_Menu, MenuCreate, MenuUpdate, MenuSearch>
    {
        private readonly IMenuService menuService;
        public MenuController(IServiceProvider serviceProvider, ILogger<BaseController<tbl_Menu, MenuCreate, MenuUpdate, MenuSearch>> logger
            , IWebHostEnvironment env) : base(serviceProvider, logger, env
            )
        {
            this.domainService = serviceProvider.GetRequiredService<IMenuService>();
            this.menuService = serviceProvider.GetRequiredService<IMenuService>();
        }

        /// <summary>
        /// Lấy danh sách menu có phân trang và danh sách foods 
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpGet("food-item")]
        [AppAuthorize]
        [Description("Lấy danh sách menu có phân trang và danh sách foods ")]
        public virtual async Task<AppDomainResult> GetMenuWithFoods([FromQuery] MenuSearch baseSearch)
        {
            if (!ModelState.IsValid)
                throw new AppException(ModelState.GetErrorMessage());
            PagedList<tbl_Menu> pagedData = await this.menuService.GetMenuWithFoods(baseSearch);
            return new AppDomainResult(pagedData);
        }
    }
}

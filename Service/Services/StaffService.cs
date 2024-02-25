﻿using Entities;
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
using System.Net.Mail;
using Request.DomainRequests;
using Microsoft.Extensions.Configuration;
using static Utilities.CoreContants;
using System.Net;
using Request.RequestCreate;
using System.Buffers.Text;
using Entities.AuthEntities;

namespace Service.Services
{
    public class StaffService : DomainService<tbl_Staff, StaffSearch>, IStaffService
    {
        private readonly IUserService userService;
        public StaffService(IAppUnitOfWork unitOfWork, 
            IMapper mapper,
            IUserService userService) : base(unitOfWork, mapper)
        {
            this.userService = userService;
        }
        protected override string GetStoreProcName()
        {
            return "Get_Staff";
        }
    }
}

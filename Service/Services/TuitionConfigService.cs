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
using System.Threading;
using AppDbContext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Internal;
using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace Service.Services
{
    public class TuitionConfigService : DomainService<tbl_TuitionConfig, TuitionConfigSearch>, ITuitionConfigService
    {
        private IAppDbContext appDbContext;
        private INecessaryService necessaryService;
        private IConfiguration configuration;
        public TuitionConfigService(IAppUnitOfWork unitOfWork
            , IMapper mapper, IAppDbContext appDbContext
            , IConfiguration configuration
            , INecessaryService necessaryService) : base(unitOfWork, mapper)
        {
            this.appDbContext = appDbContext;
            this.necessaryService = necessaryService;
            this.configuration = configuration;
        }
        protected override string GetStoreProcName()
        {
            return "Get_TuitionConfig";
        }
        /// <summary>
        /// Gửi thông báo học phí
        /// </summary>
        /// <returns></returns>
        public async Task TuitionFeeNotice(Guid tuitionConfigId,string pathEmailTemplate)
        {
            try
            {
                var tuitionConfig = await this.GetByIdAsync(tuitionConfigId);
                if (tuitionConfig == null)
                    throw new AppException("Không tìm thấy học phí");
                if (tuitionConfig.status == 2)
                    throw new AppException("Đã gửi thông báo học phí");
                var tuitionConfigDetails = await unitOfWork.Repository<tbl_TuitionConfigDetail>()
                    .GetQueryable().Where(x => x.tuitionConfigId == tuitionConfigId && x.deleted == false).ToListAsync();
                if (!tuitionConfigDetails.Any())
                    throw new AppException("Không tìm thấy khoản thu nào");
                Guid userId = LoginContext.Instance.CurrentUser.userId;
                Thread notice = new Thread(() =>
                {
                    BackgroundService.TuitionFeeNotice(tuitionConfig, tuitionConfigDetails, userId, pathEmailTemplate);
                });
                notice.Start();
            }
            catch (AppException e)
            {
                throw e;
            }
        }
    }
}

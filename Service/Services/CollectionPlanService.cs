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
using Entities.DataTransferObject;
using KeyValuePair = Entities.DataTransferObject.KeyValuePair;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace Service.Services
{
    public class CollectionPlanService : DomainService<tbl_CollectionPlan, CollectionPlanSearch>, ICollectionPlanService
    {
        private readonly ICollectionPlanDetailService collectionPlanDetailService;
        public CollectionPlanService(IAppUnitOfWork unitOfWork, IMapper mapper, ICollectionPlanDetailService collectionPlanDetailService) : base(unitOfWork, mapper)
        {
            this.collectionPlanDetailService = collectionPlanDetailService;
        }
        protected override string GetStoreProcName()
        {
            return "Get_CollectionPlan";
        }

        public override async Task Validate(tbl_CollectionPlan model)
        {
            if (model.branchId.HasValue)
            {
                var item = await this.unitOfWork.Repository<tbl_Branch>().Validate(model.branchId.Value) ?? throw new AppException(MessageContants.nf_branch);
            }
            if (model.schoolYearId.HasValue)
            {
                var item = await this.unitOfWork.Repository<tbl_SchoolYear>().Validate(model.schoolYearId.Value) ?? throw new AppException(MessageContants.nf_schoolYear);
            }
            if (model.gradeId.HasValue)
            {
                var item = await this.unitOfWork.Repository<tbl_Grade>().Validate(model.gradeId.Value) ?? throw new AppException(MessageContants.nf_grade);
            }
            if (model.classId.HasValue)
            {
                var item = await this.unitOfWork.Repository<tbl_Class>().Validate(model.classId.Value) ?? throw new AppException(MessageContants.nf_class);
            }
            if (model.branchId.HasValue)
            {
                var item = await this.unitOfWork.Repository<tbl_Branch>().Validate(model.branchId.Value) ?? throw new AppException(MessageContants.nf_branch);
            }

            //validate day
            if (model.id == Guid.Empty)
            {
                if (model.startDay >= model.endDay)
                    throw new AppException(MessageContants.start_day_must_less_than_end_day);

                //fee validation
                if (model.details != null && model.details.Count > 0)
                {
                    var itemCount = await this.unitOfWork.Repository<tbl_Fee>().GetQueryable()
                        .CountAsync(x => model.details.Select(d => d.feeId.Value).Contains(x.id) && x.deleted == false);

                    if (itemCount != model.details.Count)
                        throw new AppException(MessageContants.nf_fee);
                }
                else
                {
                    throw new AppException(MessageContants.nf_fee);
                }
            }
            else
            {
                var item = await this.unitOfWork.Repository<tbl_CollectionPlan>().Validate(model.id) ?? throw new AppException(MessageContants.nf_collectionPlan);
                var startDay = Math.Max(model.startDay ?? 0, item.startDay ?? 0);
                var endDay = Math.Min(model.endDay ?? 0, item.endDay ?? 0);
                if (startDay >= endDay)
                    throw new AppException(MessageContants.start_day_must_less_than_end_day);
            }
        }


        public override async Task AddItem(tbl_CollectionPlan model)
        {
            await this.Validate(model);

            /// <summary>
            /// Phạm vi thu tiền
            /// 1 - Lớp
            /// 2 - Khối
            /// 3 - Toàn trường
            /// </summary>
            if (model.scope == 1)
            {
                model.gradeId = null;
            }else if(model.scope == 2)
            {
                model.classId = null;
            }
            else
            {
                model.gradeId = model.classId = null;
            }

            List<tbl_CollectionPlanDetail> details = model.details.ToList();

            //add food
            await this.unitOfWork.Repository<tbl_CollectionPlan>().CreateAsync(model);

            //add food-items 
            details.ForEach(x => { x.collectionPlanId = model.id; });

            await this.unitOfWork.Repository<tbl_CollectionPlanDetail>().CreateAsync(details);
            await this.unitOfWork.SaveAsync();
        }

        public override async Task UpdateItem(tbl_CollectionPlan model)
        {
            await this.Validate(model);

            List<tbl_CollectionPlanDetail> details = model.details.ToList();

            //update food
            await this.UpdateAsync(model);

            //update food-item
            var currentItems = await this.unitOfWork.Repository<tbl_CollectionPlanDetail>().GetQueryable().Where(x => x.deleted == false && x.collectionPlanId == model.id).ToListAsync();
            if (currentItems.Any())
            {
                //get deleted Ids to delete
                var deletedItems = currentItems.Where(x => !details.Select(d => d.id).Contains(x.id)).ToList();
                deletedItems.ForEach(x => { x.deleted = true; });

                //new items
                var newItems = details.Where(x => !currentItems.Select(d => d.id).Contains(x.id)).ToList();
                newItems.ForEach(x => { x.collectionPlanId = model.id; });

                //update items
                var updatedItems = details.Where(x => currentItems.Select(d => d.id).Contains(x.id)).ToList();
                updatedItems.ForEach(x => { x.collectionPlanId = model.id; });

                //submit value
                await this.collectionPlanDetailService.CreateAsync(newItems);
                await this.collectionPlanDetailService.UpdateAsync(updatedItems);
                await this.collectionPlanDetailService.UpdateAsync(deletedItems);
            }

            await this.unitOfWork.SaveAsync();
        }

        public override async Task<tbl_CollectionPlan> GetByIdAsync(Guid id)
        {
            //base base-information
            var food = await this.unitOfWork.Repository<tbl_CollectionPlan>().Validate(id) ?? throw new AppException(MessageContants.nf_collectionPlan);

            //mapping item
            food.details = await this.unitOfWork.Repository<tbl_CollectionPlanDetail>()
                .GetDataExport("Get_CollectionPlanDetail", new SqlParameter[] { new SqlParameter("collectionPlanId", food.id) });

            return food;
        }


        #region Thống kê
        public async Task<ColumChart> TotalIncome(CollectionPlanReport request)
        {
            ColumChart columnChart = new ColumChart();

            var data = await this.unitOfWork.Repository<tbl_CollectionSessionHeader>().GetDataExport("Get_Collection_TotalInCome", GetSqlParameters(request));

            //fetch data to chart
            switch (request.type)
            {
                case 1: //ngày
                    columnChart.data = data.GroupBy(x => x.dateTime?.Date).Select(x => new KeyValuePair
                    {
                        value = x.Sum(d => d.paid),
                        key = $"{x.Key?.Day}/{x.Key?.Month}/{x.Key?.Year}"
                    }).ToList();
                    break;

                case 2: //tuần
                    columnChart.data = data.GroupBy(x => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(x.dateTime ?? DateTime.MinValue, CalendarWeekRule.FirstDay, DayOfWeek.Monday)).Select(x => new KeyValuePair
                    {
                        value = x.Sum(d => d.paid),
                        key = $"Tuần {x.Key/4 + 1}, Th{x.FirstOrDefault().dateTime?.Month}/{x.FirstOrDefault().dateTime?.Year}"
                    }).ToList();
                    break;

                case 3: //tháng
                    columnChart.data = data.GroupBy(x => x.dateTime?.Month).Select(x => new KeyValuePair
                    {
                        value = x.Sum(d => d.paid),
                        key = $"{x.Key}/{x.FirstOrDefault().dateTime?.Year}"
                    }).ToList();
                    break;

                case 4: //quý
                    columnChart.data = data.GroupBy(x => (x.dateTime?.Month - 1) / 3 + 1).Select(x => new KeyValuePair
                    {
                        value = x.Sum(d => d.paid),
                        key = $"Q{x.Key}/{x.FirstOrDefault().dateTime?.Year}"
                    }).ToList();
                    break;

                default: //năm
                    columnChart.data = data.GroupBy(x => x.dateTime?.Year).Select(x => new KeyValuePair
                    {
                        value = x.Sum(d => d.paid),
                        key = $"{x.Key}"
                    }).ToList();
                    break;
            }
            return columnChart;
        }


        public async Task<ColumChart> AvgMoneyPerStudent(AverageMoneyPerStudent request)
        {
            ColumChart columnChart = new ColumChart();

            var data = await this.unitOfWork.Repository<tbl_CollectionSessionHeader>().GetDataExport("Get_Collection_AvgMoneyPerStudent", GetSqlParameters(request));
            var totalStudent = data.Select(x => x.studentId).Distinct().Count();
            //fetch data to chart
            switch (request.type)
            {
                case 1: //ngày
                    columnChart.data = data.GroupBy(x => x.dateTime?.Date).Select(x => new KeyValuePair
                    {
                        value = x.Sum(d => d.paid)/x.Select(x=>x.studentId).Distinct().Count(),
                        key = $"{x.Key?.Day}/{x.Key?.Month}/{x.Key?.Year}"
                    }).ToList();
                    break;

                case 2: //tuần
                    columnChart.data = data.GroupBy(x => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(x.dateTime ?? DateTime.MinValue, CalendarWeekRule.FirstDay, DayOfWeek.Monday)).Select(x => new KeyValuePair
                    {
                        value = x.Sum(d => d.paid) / x.Select(x => x.studentId).Distinct().Count(),
                        key = $"Tuần {x.Key / 4 + 1}, Th{x.FirstOrDefault().dateTime?.Month}/{x.FirstOrDefault().dateTime?.Year}"
                    }).ToList();
                    break;

                case 3: //tháng
                    columnChart.data = data.GroupBy(x => x.dateTime?.Month).Select(x => new KeyValuePair
                    {
                        value = x.Sum(d => d.paid) / x.Select(x => x.studentId).Distinct().Count(),
                        key = $"{x.Key}/{x.FirstOrDefault().dateTime?.Year}"
                    }).ToList();
                    break;

                case 4: //quý
                    columnChart.data = data.GroupBy(x => (x.dateTime?.Month - 1) / 3 + 1).Select(x => new KeyValuePair
                    {
                        value = x.Sum(d => d.paid) / x.Select(x => x.studentId).Distinct().Count(),
                        key = $"Q{x.Key}/{x.FirstOrDefault().dateTime?.Year}"
                    }).ToList();
                    break;

                default: //năm
                    columnChart.data = data.GroupBy(x => x.dateTime?.Year).Select(x => new KeyValuePair
                    {
                        value = x.Sum(d => d.paid) / x.Select(x => x.studentId).Distinct().Count(),
                        key = $"{x.Key}"
                    }).ToList();
                    break;
            }
            return columnChart;
        }

        public async Task<ColumChart> CollectionSessionReport(CollectionSessionReport request)
        {
            ColumChart columnChart = new ColumChart();

            var item = await this.unitOfWork.Repository<tbl_CollectionPlan>().Validate(request.collectionPlanId ?? Guid.Empty)
                ?? throw new AppException(MessageContants.nf_collectionPlan);

            var data = await this.unitOfWork.Repository<tbl_CollectionSessionHeader>().GetQueryable()
                .Where(x => x.collectionPlanId == item.id).ToListAsync();

            if(data.Any()) 
            {
                columnChart.data = data.GroupBy(x => x.collectionSessionId).Select(x => new KeyValuePair
                {
                    key = x.FirstOrDefault()?.name,
                    value = x.Sum(x=>x.paid ?? 0)
                }).ToList();
            }
          
            return columnChart;
        }

        public async Task<PieChart> ReportByFee(ReportByFee request)
        {
            PieChart columnChart = new PieChart();

            var data = await this.unitOfWork.Repository<tbl_CollectionSessionLine>()
                .GetDataExport("Get_Collection_ReportByFee", GetSqlParameters(request));

            if (data.Any())
            {
                columnChart.data = data.Select(x => new KeyValuePair
                {
                    key = x.feeName,
                    value = x.price
                }).ToList();
            }
            return columnChart;
        }

        public async Task<PagedList<tbl_CollectionSession>> CollectionSessionDebt(CollectionSessionDebtSearch request)
        {

            var result = await this.unitOfWork.Repository<tbl_CollectionSession>()
                .ExcuteQueryPagingAsync("Get_CollectionSessionDebt", GetSqlParameters(request));

            result.pageSize = request.pageSize;
            result.pageIndex = request.pageIndex;
            return result;
        }
        #endregion
    }
}

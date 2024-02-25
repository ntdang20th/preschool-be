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
using BaseAPI.Controllers;
using Interface.DbContext;
using System.Data.Entity;
using Microsoft.AspNetCore.Http;
using Entities.AuthEntities;
using AppDbContext;
using OfficeOpenXml;
using System.Collections.Immutable;

namespace API.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [ApiController]
    [Description("Nhân viên")]
    [Authorize]
    public class StaffController : BaseController<tbl_Staff, StaffCreate, StaffUpdate, StaffSearch>
    {
        private readonly IAppDbContext appDbContext;
        private readonly IUserService userService;
        private readonly IStaffService staffService;
        private readonly IDepartmentService departmentService;
        private readonly IHighestLevelOfEducationService highestLevelOfEducationService;
        private readonly IGroupService groupService;
        private readonly IUserGroupService userGroupService;
        private readonly IBranchService branchService;
        private readonly ICitiesService citiesService;
        private readonly IDistrictsService districtsService;
        private readonly IWardsService wardsService;
        private readonly IAutoGenCodeConfigService autoGenCode;
        public StaffController(IServiceProvider serviceProvider, ILogger<BaseController<tbl_Staff, StaffCreate, StaffUpdate, StaffSearch>> logger
            , IWebHostEnvironment env) : base(serviceProvider, logger, env)
        {
            this.autoGenCode = serviceProvider.GetRequiredService<IAutoGenCodeConfigService>();
            this.appDbContext = serviceProvider.GetRequiredService<IAppDbContext>();
            this.userService = serviceProvider.GetRequiredService<IUserService>();
            this.staffService = serviceProvider.GetRequiredService<IStaffService>();
            this.domainService = serviceProvider.GetRequiredService<IStaffService>();
            this.departmentService = serviceProvider.GetRequiredService<IDepartmentService>();
            this.highestLevelOfEducationService = serviceProvider.GetRequiredService<IHighestLevelOfEducationService>();
            this.groupService = serviceProvider.GetRequiredService<IGroupService>();
            this.userGroupService = serviceProvider.GetRequiredService<IUserGroupService>();
            this.branchService = serviceProvider.GetRequiredService<IBranchService>();
            this.citiesService = serviceProvider.GetRequiredService<ICitiesService>();
            this.districtsService = serviceProvider.GetRequiredService<IDistrictsService>();
            this.wardsService = serviceProvider.GetRequiredService<IWardsService>();
        }

        /// <summary>
        /// Lấy thông tin theo id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [AppAuthorize]
        [Description("Lấy thông tin")]
        public override async Task<AppDomainResult> GetById(Guid id)
        {
            var item = await this.domainService.GetByIdAsync(id) ?? throw new KeyNotFoundException(MessageContants.nf_item);
            var user = await userService.GetByIdAsync(item.userId.Value); 
            var city = await citiesService.GetByIdAsync(item.cityId ?? Guid.Empty);
            var district = await districtsService.GetByIdAsync(user.districtId ?? Guid.Empty);
            var ward = await wardsService.GetByIdAsync(user.wardId ?? Guid.Empty);
            var department = await departmentService.GetByIdAsync(item.departmentId ?? Guid.Empty);

            item = new tbl_Staff
            {
                id = item.id,
                active = item.active,
                address = user.address,
                bankAccount = item.bankAccount,
                bankAddress = item.bankAddress,
                bankName = item.bankName,
                bankNumber = item.bankName,
                birthday = user.birthday,
                branchIds = user.branchIds,
                cityId = user.cityId,
                cityName = city?.name,
                code = user.code,
                created = item.created,
                createdBy = item.createdBy,
                deleted = item.deleted,
                departmentId = item.departmentId,
                departmentName = department?.name,
                districtId = user.districtId,
                districtName = district?.name,
                email = user.email,
                firstName = user.firstName,
                fullName = user.fullName,
                gender = user.gender,
                genderName = user.genderName,
                graduateSchool = item.graduateSchool,
                graduateTime = item.graduateTime,
                highestLevelOfEducationId = item.highestLevelOfEducationId,
                joined = item.joined,
                lastName = user.lastName,
                otherCertificate = item.otherCertificate,
                phone = user.phone,
                status = user.status,
                statusName = user.statusName,
                teachingExperience = item.teachingExperience,
                thumbnail = user.thumbnail,
                thumbnailResize = item.thumbnailResize,
                updated = item.updated,
                updatedBy = item.updatedBy,
                userId = item.userId,
                username = user.username,
                wardId = user.wardId,
                wardName = ward?.name,
                branchs = Task.Run(() => branchService.GetBranchByIds(item.branchIds)).Result,
                groupIds = Task.Run(() => userService.GetGroupByUserId(item.userId.Value)).Result.Item1,
                groups = Task.Run(() => userService.GetGroupByUserId(item.userId.Value)).Result.Item2
            };
            return new AppDomainResult(item);
        }
        /// <summary>
        /// Thêm mới item
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPost]
        [AppAuthorize]
        [Description("Thêm mới")]
        public override async Task<AppDomainResult> AddItem([FromBody] StaffCreate itemModel)
        {
            using (var tran = await appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    if (!ModelState.IsValid)
                        throw new AppException(ModelState.GetErrorMessage());

                    //Thông tin người dùng
                    var user = mapper.Map<tbl_Users>(itemModel);
                    await userService.ValidateUser(user);


                    //get first branch
                    var firstGroup = itemModel.groupIds.Split(',').FirstOrDefault();
                    string groupCode = "NV";
                    if(firstGroup != null)
                    {
                        var group = await this.groupService.GetSingleAsync(x => x.id.ToString() == firstGroup);
                        if(group != null)
                        {
                            groupCode = group.code;
                        }
                    }
                    if (string.IsNullOrWhiteSpace(user.code))
                        user.code = await this.autoGenCode.AutoGenCode(groupCode == "GV" ? nameof(tbl_Staff) : "tbl_Teacher", groupCode);

                    if (itemModel.departmentId.HasValue)
                    {
                        var hasDepartment = await departmentService.AnyAsync(x => x.id == itemModel.departmentId);
                        if (!hasDepartment)
                            throw new AppException("Không tìm thấy phòng ban");
                    }
                    if (itemModel.highestLevelOfEducationId.HasValue)
                    {
                        var hasHighestLevelOfEducation = await highestLevelOfEducationService
                            .AnyAsync(x => x.id == itemModel.highestLevelOfEducationId);
                        if (!hasHighestLevelOfEducation)
                            throw new AppException("Không tìm thấy trình độ học vấn");
                    }
                    await userService.CreateAsync(user);

                    //thông tin nhân viên

                    var item = mapper.Map<tbl_Staff>(itemModel);
                    if (item == null)
                        throw new AppException(MessageContants.nf_item);
                    item.userId = user.id;
                    await this.domainService.AddItem(item);

                    if (!itemModel.groupIds.Any())
                        throw new AppException("Vui lòng chọn quyền người dùng");
                    var groupIds = itemModel.groupIds.Split(',').ToList();
                    foreach (var groupId in groupIds)
                    {
                        var hasGroup = await groupService.AnyAsync(x => x.id.ToString() == groupId);
                        if (!hasGroup)
                            throw new AppException("Không tìm thấy phân quyền");
                        var userGroup = new tbl_UserGroup
                        {
                            active = true,
                            created = Timestamp.Now(),
                            deleted = true,
                            groupId = Guid.Parse(groupId),
                            updated = Timestamp.Now(),
                            userId = user.id,
                        };
                        await userGroupService.CreateAsync(userGroup);
                    }
                    await tran.CommitAsync();
                    return new AppDomainResult();
                }
                catch (AppException e)
                {
                    await tran.RollbackAsync();
                    throw new AppException(e.Message);
                }
            }
        }
        /// <summary>
        /// Cập nhật thông tin item
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPut]
        [AppAuthorize]
        [Description("Chỉnh sửa")]
        public override async Task<AppDomainResult> UpdateItem([FromBody] StaffUpdate itemModel)
        {
            using (var tran = await appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    if (!ModelState.IsValid)
                        throw new AppException(ModelState.GetErrorMessage());

                    var staff = await domainService.GetByIdAsync(itemModel.id);
                    if (staff == null)
                        throw new AppException("Không tìm thấy dữ liệu");
                    //Thông tin người dùng
                    var user = mapper.Map<tbl_Users>(itemModel);
                    user.id = staff.userId.Value;
                    await userService.ValidateUser(user);

                    if (itemModel.departmentId.HasValue)
                    {
                        var hasDepartment = await departmentService.AnyAsync(x => x.id == itemModel.departmentId);
                        if (!hasDepartment)
                            throw new AppException("Không tìm thấy phòng ban");
                    }
                    if (itemModel.highestLevelOfEducationId.HasValue)
                    {
                        var hasHighestLevelOfEducation = await highestLevelOfEducationService
                            .AnyAsync(x => x.id == itemModel.highestLevelOfEducationId);
                        if (!hasHighestLevelOfEducation)
                            throw new AppException("Không tìm thấy trình độ học vấn");
                    }
                    await userService.UpdateAsync(user);

                    var item = mapper.Map<tbl_Staff>(itemModel);
                    if (item == null)
                        throw new KeyNotFoundException(MessageContants.nf_item);
                    await this.domainService.UpdateItem(item);
                    await tran.CommitAsync();
                    return new AppDomainResult();
                }
                catch (AppException e)
                {
                    await tran.RollbackAsync();
                    throw new AppException(e.Message);
                }
            }
        }
        /// <summary>
        /// Xóa item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [AppAuthorize]
        [Description("Xoá")]
        public override async Task<AppDomainResult> DeleteItem(Guid id)
        {
            using (var tran = await appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var staff = await domainService.GetByIdAsync(id);
                    if (staff == null)
                        throw new AppException("Không tìm thấy thông tin nhân viên");
                    await userService.DeleteItem(staff.userId.Value);
                    await this.domainService.DeleteItem(id);
                    await tran.CommitAsync();
                    return new AppDomainResult();
                }
                catch (AppException e)
                {
                    await tran.RollbackAsync();
                    throw new AppException(e.Message);
                }
            }
        }
        /// <summary>
        /// Lấy danh sách item phân trang
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpGet]
        [AppAuthorize]
        [Description("Lấy danh sách")]
        public override async Task<AppDomainResult> Get([FromQuery] StaffSearch baseSearch)
        {
            if (!ModelState.IsValid)
                throw new AppException(ModelState.GetErrorMessage());
            PagedList<tbl_Staff> pagedData = await this.domainService.GetPagedListData(baseSearch);
            pagedData.items = (from i in pagedData.items
                               select new tbl_Staff
                               {
                                   id = i.id,
                                   active = i.active,
                                   address = i.address,
                                   bankAccount = i.bankAccount,
                                   bankAddress = i.bankAddress,
                                   bankName = i.bankName,
                                   bankNumber = i.bankName,
                                   birthday = i.birthday,
                                   branchIds = i.branchIds,
                                   cityId = i.cityId,
                                   cityName = i.cityName,
                                   code = i.code,
                                   created = i.created,
                                   createdBy = i.createdBy,
                                   deleted = i.deleted,
                                   departmentId = i.departmentId,
                                   departmentName = i.departmentName,
                                   districtId = i.districtId,
                                   districtName = i.districtName,
                                   email = i.email,
                                   firstName = i.firstName,
                                   fullName = i.fullName,
                                   gender = i.gender,
                                   genderName = i.genderName,
                                   graduateSchool = i.graduateSchool,
                                   graduateTime = i.graduateTime,
                                   highestLevelOfEducationId = i.highestLevelOfEducationId,
                                   joined = i.joined,
                                   lastName = i.lastName,
                                   otherCertificate = i.otherCertificate,
                                   phone = i.phone,
                                   status = i.status,
                                   statusName = i.statusName,
                                   teachingExperience = i.teachingExperience,
                                   thumbnail = i.thumbnail,
                                   thumbnailResize = i.thumbnailResize,
                                   updated = i.updated,
                                   updatedBy = i.updatedBy,
                                   userId = i.userId,
                                   username = i.username,
                                   wardId = i.wardId,
                                   wardName = i.wardName,
                                   branchs = Task.Run(()=> branchService.GetBranchByIds(i.branchIds)).Result,
                                   groupIds = Task.Run(()=> userService.GetGroupByUserId(i.userId.Value)).Result.Item1,
                                   groups = Task.Run(() => userService.GetGroupByUserId(i.userId.Value)).Result.Item2
                               }).ToList();
            return new AppDomainResult(pagedData);
        }
    }
}

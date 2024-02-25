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
using Request.RequestCreate;
using Request.RequestUpdate;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using AppDbContext.Migrations;

namespace Service.Services
{
    public class MenuWeekService : DomainService<tbl_MenuWeek, BaseSearch>, IMenuWeekService
    {
        public MenuWeekService(IAppUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
        {
        }
        protected override string GetStoreProcName()
        {
            return "Get_MenuWeek";
        }

        public async Task<List<tbl_MenuWeek>> CustomGet(MenuWeekSearch request)
        {
            //validate
            var branch = await this.unitOfWork.Repository<tbl_Branch>().Validate(request.branchId.Value) ?? throw new AppException(MessageContants.nf_branch);
            var week = await this.unitOfWork.Repository<tbl_Week>().Validate(request.weekId.Value) ?? throw new AppException(MessageContants.nf_week);

            //get base list
            var menuWeeks = await this.unitOfWork.Repository<tbl_MenuWeek>().GetDataExport(GetStoreProcName(), GetSqlParameters(request));

            if(menuWeeks.Any())
            {
                //get food list before mapping
                var menuIds = menuWeeks.Where(x => x.menuId.HasValue).Select(x => x.menuId.Value).Distinct().ToList();
                var menuDict = await GetMenuDictionary(menuIds);

                //mapping 
                foreach (var menuWeek in menuWeeks)
                {
                    if (menuDict.TryGetValue(menuWeek.menuId.Value, out var foodOut))
                    {
                        menuWeek.foods = foodOut;
                    }
                }
            }

            return menuWeeks;
        }

        public async Task<Dictionary<Guid, List<FoodItem>>> GetMenuDictionary(List<Guid> menuIds)
        {

            var menuFood = await this.unitOfWork.Repository<tbl_MenuFood>().GetQueryable()
                    .Where(x => menuIds.Contains((Guid)x.menuId) && x.deleted == false && x.foodId.HasValue)      //database call
                    .ToListAsync();
            var foodIds = menuFood.Select(x => x.foodId.Value).ToList();

            var foods = await this.unitOfWork.Repository<tbl_Food>().GetQueryable().Where(x => foodIds.Contains(x.id)).ToListAsync();  //database call

            var menuDict = menuIds.ToDictionary(menuId => menuId, menuId => foods
                                                    .Where(food => menuFood.Where(week => week.menuId == menuId).Select(x => x.foodId).Contains(food.id))
                                                    .Select(x => new FoodItem { 
                                                        id = x.id, name = x.name,
                                                        type = menuFood.Where(week => week.menuId == menuId && week.foodId == x.id)
                                                                    .Select(week => week.type)
                                                                    .FirstOrDefault()
                                                    })
                                                    .ToList());

            
            return menuDict;
        }

        public async Task CustomValidate(MenuWeekUpdate model)
        {
            var week = await this.unitOfWork.Repository<tbl_Week>().Validate(model.weekId.Value) ?? throw new AppException(MessageContants.nf_week);

            var menuIds = model.items.Select(x => x.menuId).Distinct().ToList();
            if (menuIds != null && menuIds.Count > 0)
            {
                var itemCount = await this.unitOfWork.Repository<tbl_Menu>().GetQueryable().CountAsync(x => menuIds.Contains(x.id));
                if(itemCount != menuIds.Count())
                {
                    throw new AppException(MessageContants.nf_menu);
                }
            }
        }


        public async Task AddOrUpdateItem(MenuWeekUpdate request)
        {
            await CustomValidate(request);

            var details = mapper.Map<List<tbl_MenuWeek>>( request.items);
            details.ForEach(x => x.weekId = request.weekId);

            var currentItems = await this.unitOfWork.Repository<tbl_MenuWeek>().GetQueryable().Where(x => x.weekId == request.weekId).ToListAsync();

            //get deleted Ids to delete
            var deletedItems = currentItems.Where(x => !details.Select(d => d.id).Contains(x.id)).ToList();
            deletedItems.ForEach(x => x.deleted = true);
            //new items
            var newItems = details.Where(x => !currentItems.Select(d => d.id).Contains(x.id)).ToList();

            //update items
            var updatedItems = details.Where(x => currentItems.Select(d => d.id).Contains(x.id)).ToList();

            //submit value
            await this.unitOfWork.Repository<tbl_MenuWeek>().CreateAsync(newItems);
            await this.UpdateAsync(updatedItems);
            await this.UpdateAsync(deletedItems);

            await this.unitOfWork.SaveAsync();
        }

        public async Task<List<MenuWeekItem>> GetRandom()
        {
            var result = new List<MenuWeekItem>();

            //current active menu
            var menus = await this.unitOfWork.Repository<tbl_Menu>().GetQueryable().Where(x => x.deleted == false && x.active == true)
                .ToListAsync();

            var menuIds = menus.Select(x => x.id).ToList();

            if (menuIds == null || menuIds.Count < 2)
                throw new AppException(MessageContants.require_at_least_2_menu);

            //random monday to friday
            Random random = new Random();
            Guid tmpId = menuIds[0];

            for (int i = 2; i <= 6; i++)
            {
                MenuWeekItem item = new MenuWeekItem() { day = i };

                //remove last id
                menuIds.Remove(tmpId);
                int randomIndex = random.Next(0, menuIds.Count);
                var currentItem = menuIds[randomIndex];
                //re-add last id
                menuIds.Add(tmpId);

                item.menuId = currentItem;
                result.Add(item);

                menuIds.Remove(currentItem);

                //re-fill
                if(menuIds != null && menuIds.Count <= 0)
                {
                    menuIds = menus.Select(x => x.id).ToList();
                }

                tmpId = currentItem;
            }

            //mapping menu food info
            var menuIdFromResult = result.Where(x=>x.menuId.HasValue).Select(x=>x.menuId.Value).Distinct().ToList();
            var menuDict = await GetMenuDictionary(menuIdFromResult);
            //mapping 
            foreach (var item in result)
            {
                if (menuDict.TryGetValue(item.menuId.Value, out var foodOut))
                {
                    item.foods = foodOut;
                }
            }
            return result;
        }
    }
}

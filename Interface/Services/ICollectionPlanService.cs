using Entities;
using Entities.DataTransferObject;
using Entities.DomainEntities;
using Entities.Search;
using Interface.Services.DomainServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Utilities;


namespace Interface.Services
{
    public interface ICollectionPlanService : IDomainService<tbl_CollectionPlan, CollectionPlanSearch>
    {
        Task<ColumChart> TotalIncome(CollectionPlanReport request);
        Task<ColumChart> AvgMoneyPerStudent(AverageMoneyPerStudent request);
        Task<ColumChart> CollectionSessionReport(CollectionSessionReport request);
        Task<PieChart> ReportByFee(ReportByFee request);
        Task<PagedList<tbl_CollectionSession>> CollectionSessionDebt(CollectionSessionDebtSearch request);
    }
}

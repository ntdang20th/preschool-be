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
using Request.RequestCreate;
using System.Configuration;
namespace Service.Services
{
    public class BillService : DomainService<tbl_Bill, BillSearch>, IBillService
    {
        private readonly IAppDbContext appDbContext;
        private readonly IAutoGenCodeConfigService autoGenCodeConfigService;
        public BillService(IAppUnitOfWork unitOfWork, 
            IMapper mapper, 
            IAppDbContext appDbContext,
            IAutoGenCodeConfigService autoGenCodeConfigService
            ) : base(unitOfWork, mapper)
        {
            this.appDbContext = appDbContext;
            this.autoGenCodeConfigService = autoGenCodeConfigService;
        }
        protected override string GetStoreProcName()
        {
            return "Get_Bill";
        }
        public async Task Payments(PaymentsRequest itemModel)
        {
            using (var tran = await appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var user = LoginContext.Instance.CurrentUser;
                    var bill = await unitOfWork.Repository<tbl_Bill>().GetQueryable()
                        .SingleOrDefaultAsync(x => x.id == itemModel.billId && x.deleted == false);
                    if (bill == null)
                        throw new AppException("Không tìm thấy công nợ");
                    if (bill.debt <= 0)
                        throw new AppException("Học viên đã thanh toán hết");
                    double reduced = 0;
                    if (itemModel.discountId.HasValue)
                    {
                        if (bill.discountId.HasValue)
                            throw new AppException("Đã được áp dụng khuyến mãi khác, không thể dùng thêm");
                        var discount = await unitOfWork.Repository<tbl_Discount>()
                            .GetQueryable().SingleOrDefaultAsync(x => x.id == itemModel.discountId && x.deleted == false);
                        if (discount == null)
                            throw new AppException("Không tìm thấy khuyến mãi");
                        if (discount.usedQuantity >= discount.quantity)
                            throw new AppException("Đã dùng hết khuyến mãi này");
                        if (discount.type == 1)
                            reduced = discount.value ?? 0;
                        else
                        {
                            double percent = discount.value ?? 0;
                            reduced = (bill.totalPrice.Value / 100) * percent;
                        }
                        discount.usedQuantity += 1;
                        unitOfWork.Repository<tbl_Discount>().Update(discount);
                        await unitOfWork.SaveAsync();
                    }
                    bill.paid += itemModel.paid;
                    bill.reduced += reduced;
                    bill.debt = bill.totalPrice.Value - (bill.paid + bill.reduced);
                    bill.discountId = itemModel.discountId;
                    if (bill.debt < 0)
                        throw new AppException("Số tiền thanh toán không phù hợp");
                    unitOfWork.Repository<tbl_Bill>().Update(bill);
                    await unitOfWork.SaveAsync();
                    //Xuất phiếu thu
                    if (bill.paid > 0)
                    {
                        var paymentSession = new tbl_PaymentSession
                        {
                            active = true,
                            branchId = bill.branchId,
                            code = await autoGenCodeConfigService.AutoGenCode(nameof(tbl_PaymentSession)),
                            created = Timestamp.Now(),
                            createdBy = user.userId,
                            deleted = false,
                            money = itemModel.paid,
                            note = itemModel.note,
                            reason = $"Thanh toán công nợ [{bill.code}]",
                            status = 1,
                            statusName = tbl_PaymentSession.GetStatusName(1),
                            studentId = bill.studentId,
                            type = 1,
                            typeName = tbl_PaymentSession.GetTypeName(1),
                            updated = Timestamp.Now(),
                            updatedBy = user.userId
                        };
                        await unitOfWork.Repository<tbl_PaymentSession>()
                            .CreateAsync(paymentSession);
                        await unitOfWork.SaveAsync();
                    }
                    await tran.CommitAsync();
                }
                catch (AppException e)
                {
                    await tran.RollbackAsync();
                    throw e;
                }
            }
        }
    }
}

using Entities;
using Entities.AuthEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace Service
{
    public class BackgroundService
    {
        private static IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();
        private static readonly string connectionStrings = configuration.GetSection("ConnectionStrings:SampleDbContext").Value.ToString();
        private static DbContextOptions<AppDbContext.AppDbContext> options = new DbContextOptionsBuilder<AppDbContext.AppDbContext>()
                        .UseSqlServer(connectionStrings)
                        .Options;
        public static void TuitionFeeNotice(tbl_TuitionConfig tuitionConfig, List<tbl_TuitionConfigDetail> tuitionConfigDetails, Guid userId,string pathEmailTemplate)
        {
            using (var dbContext = new AppDbContext.AppDbContext(options))
            {
                using (var tran = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        string content = System.IO.File.ReadAllText($"{pathEmailTemplate}/TuitionFeeNotice.html");
                        string projectName = configuration.GetSection("MySettings:ProjectName").Value.ToString();
                        var students = dbContext.Set<tbl_Student>()
                            .Where(x => x.gradeId == tuitionConfig.gradeId && x.deleted == false).ToList();
                        if (students.Any())
                        {
                            foreach (var student in students)
                            {
                                var bill = new tbl_Bill
                                {
                                    active = true,
                                    code = AutoGenCode(dbContext, nameof(tbl_Bill)),
                                    created = Timestamp.Now(),
                                    debt = tuitionConfig.totalPrice,
                                    deleted = false,
                                    schoolYearId = tuitionConfig.schoolYearId,
                                    note = $"Phát sinh học phí [{tuitionConfig.name}]",
                                    paid = 0,
                                    reduced = 0,
                                    totalPrice = tuitionConfig.totalPrice,
                                    tuitionConfigId = tuitionConfig.id,
                                    type = 1,
                                    typeName = tbl_Bill.GetTypeName(1),
                                    studentId = student.id,
                                    updated = Timestamp.Now(),
                                    createdBy = userId,
                                    updatedBy = userId,
                                    branchId = tuitionConfig.branchId
                                };
                                dbContext.Set<tbl_Bill>().Add(bill);
                                dbContext.SaveChanges();
                                StringBuilder bodyContent = new StringBuilder();

                                int stt = 0;
                                if (tuitionConfigDetails.Any())
                                {
                                    foreach (var item in tuitionConfigDetails)
                                    {
                                        var detail = new tbl_BillDetail
                                        {
                                            active = true,
                                            billId = bill.id,
                                            created = Timestamp.Now(),
                                            createdBy = userId,
                                            deleted = false,
                                            note = item.note,
                                            price = item.price,
                                            tuitionConfigDetailId = item.id,
                                            updated = Timestamp.Now(),
                                            name = item.name,
                                            updatedBy = userId
                                        };
                                        dbContext.Set<tbl_BillDetail>().Add(detail);
                                        stt++;
                                        bodyContent.Append("<tr>");
                                        bodyContent.Append($"<td style=\"text-align:center\">{stt}</td>");
                                        bodyContent.Append($"<td>{item.name}</td>");
                                        bodyContent.Append($"<td style=\"text-align:center\">{String.Format("{0:0,0}", item.price)} VNĐ</td>");
                                        bodyContent.Append("</tr>");
                                    }
                                    dbContext.SaveChanges();
                                }

                                var parentIds = new List<Guid?> { student.fatherId, student.motherId, student.guardianId };
                                parentIds = parentIds.Where(x => x.HasValue).ToList();
                                if (parentIds != null && parentIds.Count > 0)
                                {
                                    if (parentIds.Any())
                                    {
                                        foreach (var parentId in parentIds)
                                        {
                                            string contentForStudent = content;
                                            var parent = dbContext.tbl_Parent.SingleOrDefault(x => x.id == parentId && x.deleted == false);
                                            if (parent != null)
                                            {
                                                var parentInfo = dbContext.tbl_Users.SingleOrDefault(x => x.id == parent.userId && x.deleted == false);
                                                contentForStudent = contentForStudent.Replace("[HoVaTen]", parentInfo.fullName);
                                                contentForStudent = contentForStudent.Replace("[TenHocVien]", student.fullName);
                                                contentForStudent = contentForStudent.Replace("[TenHocPhi]", tuitionConfig.name);
                                                contentForStudent = contentForStudent.Replace("[CacKhoanThu]", bodyContent.ToString());
                                                contentForStudent = contentForStudent.Replace("[TongHocPhi]", String.Format("{0:0,0}", bill.totalPrice));
                                                contentForStudent = contentForStudent.Replace("[TenTruong]", projectName);

                                                SendMail.Send(parentInfo.email,
                                                    "Thông báo học phí",
                                                    contentForStudent);
                                            }
                                        }
                                    }
                                }
                                //if (student.motherId.HasValue)
                                //{
                                //    string contentForStudent = content;
                                //    var mother = dbContext.tbl_Parent.SingleOrDefault(x => x.id == student.motherId && x.deleted == false);
                                //    if (mother != null)
                                //    {
                                //        var motherInfo = dbContext.tbl_Users.SingleOrDefault(x => x.id == mother.userId && x.deleted == false);
                                //        contentForStudent = contentForStudent.Replace("[HoVaTen]", motherInfo.fullName);
                                //        contentForStudent = contentForStudent.Replace("[TenHocVien]", student.fullName);
                                //        contentForStudent = contentForStudent.Replace("[TenHocPhi]", tuitionConfig.name);
                                //        contentForStudent = contentForStudent.Replace("[CacKhoanThu]", bodyContent.ToString());
                                //        contentForStudent = contentForStudent.Replace("[TongHocPhi]", String.Format("{0:0,0}", bill.totalPrice));
                                //        contentForStudent = contentForStudent.Replace("[TenTruong]", projectName);

                                //        SendMail.Send(motherInfo.email,
                                //            "Thông báo học phí",
                                //            contentForStudent);
                                //    }
                                //}
                                //if (student.fatherId.HasValue)
                                //{
                                //    string contentForStudent = content;
                                //    var father = dbContext.tbl_Parent.SingleOrDefault(x => x.id == student.fatherId && x.deleted == false);
                                //    if (father != null)
                                //    {
                                //        var fatherInfo = dbContext.tbl_Users.SingleOrDefault(x => x.id == father.userId && x.deleted == false);
                                //        contentForStudent = contentForStudent.Replace("[HoVaTen]", fatherInfo.fullName);
                                //        contentForStudent = contentForStudent.Replace("[TenHocVien]", student.fullName);
                                //        contentForStudent = contentForStudent.Replace("[TenHocPhi]", tuitionConfig.name);
                                //        contentForStudent = contentForStudent.Replace("[CacKhoanThu]", bodyContent.ToString());
                                //        contentForStudent = contentForStudent.Replace("[TongHocPhi]", String.Format("{0:0,0}", bill.totalPrice));
                                //        contentForStudent = contentForStudent.Replace("[TenTruong]", projectName);

                                //        SendMail.Send(fatherInfo.email,
                                //            "Thông báo học phí",
                                //            contentForStudent);
                                //    }
                                //}
                                //if (student.guardianId.HasValue)
                                //{
                                //    string contentForStudent = content;
                                //    var guardian = dbContext.tbl_Parent.SingleOrDefault(x => x.id == student.guardianId && x.deleted == false);
                                //    if (guardian != null)
                                //    {
                                //        var guardianInfo = dbContext.tbl_Users.SingleOrDefault(x => x.id == guardian.userId && x.deleted == false);
                                //        contentForStudent = contentForStudent.Replace("[HoVaTen]", guardianInfo.fullName);
                                //        contentForStudent = contentForStudent.Replace("[TenHocVien]", student.fullName);
                                //        contentForStudent = contentForStudent.Replace("[TenHocPhi]", tuitionConfig.name);
                                //        contentForStudent = contentForStudent.Replace("[CacKhoanThu]", bodyContent.ToString());
                                //        contentForStudent = contentForStudent.Replace("[TongHocPhi]", String.Format("{0:0,0}", bill.totalPrice));
                                //        contentForStudent = contentForStudent.Replace("[TenTruong]", projectName);

                                //        SendMail.Send(guardianInfo.email,
                                //            "Thông báo học phí",
                                //            contentForStudent);
                                //    }
                                //}
                            }
                            tuitionConfig.status = 2;
                            tuitionConfig.statusName = tbl_TuitionConfig.GetStatusName(2);
                            dbContext.Set<tbl_TuitionConfig>().Update(tuitionConfig);
                            dbContext.SaveChanges();
                        }
                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        return;
                    }
                }
            }
        }

        public static string AutoGenCode(AppDbContext.AppDbContext dbContext, string tableName)
        {
            var config = dbContext.tbl_AutoGenCodeConfig
                .FirstOrDefault(d => d.tableName == tableName);
            if (config == null) return string.Empty;
            var now = DateTime.Today;
            string newCode = string.Empty;
            newCode = $"{config.prefix}-";

            if (config.isDay.HasValue && config.isDay.Value)
                newCode += now.Day.ToString("00");
            if (config.isMonth.HasValue && config.isMonth.Value)
                newCode += now.Month.ToString("00");
            if (config.isYear.HasValue && config.isYear.Value)
                newCode += $"{now:yy}-";
            newCode += (config.currentCode + 1).ToString().PadLeft(config.autoNumberLength ?? 0, '0');

            //update config
            config.currentCode++;
            dbContext.SaveChanges();
            return newCode;
        }
        public static async void ClearBranchInUser(Guid branchId)
        {
            using (var dbContext = new AppDbContext.AppDbContext(options))
            {
                var users = await dbContext.tbl_Users
                    .Where(x => x.branchIds.Contains(branchId.ToString()) && x.deleted == false).ToListAsync();
                if (users.Any())
                {
                    foreach (var user in users)
                    {
                        var branchIds = user.branchIds.Split(',').Where(x => x != branchId.ToString()).ToList();
                        user.branchIds = string.Join(",", branchIds);
                    }
                    await dbContext.SaveChangesAsync();
                }
            }
        }
    }
}

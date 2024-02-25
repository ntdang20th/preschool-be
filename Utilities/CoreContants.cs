using OfficeOpenXml.FormulaParsing.Excel.Functions.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Utilities.CoreContants;

namespace Utilities
{
    public class CoreContants
    {

        public const string DEFAULT_PASSWORD = "12345678";

        public const string UPLOAD_FOLDER_NAME = "Upload";
        public const string TEMP_FOLDER_NAME = "Temp";
        public const string TEMPLATE_FOLDER_NAME = "Template";
        public const string DOWNLOAD_FOLDER_NAME = "Download";
        public const string CATALOGUE_TEMPLATE_NAME = "CatalogueTemplate.xlsx";
        public const string USER_FOLDER_NAME = "User";
        public const string QR_CODE_FOLDER_NAME = "QRCode";
        public const string STUDENT_CODE = "HS";

        public const string GET_TOTAL_NOTIFICATION = "get-total-notification";
        public enum PaymentMethodCode
        {
            Cash,
            Transfer
        }
        public static string GetPaymentMethodName(int item)
        {
            var data = new List<EnumObject>
            {
                new EnumObject { key = ((int)PaymentMethodCode.Cash), value = "Tiền mặt"},
                new EnumObject { key = ((int)PaymentMethodCode.Transfer), value = "Chuyển khoản"}
            };
            return data.FirstOrDefault(x => x.key == item)?.value;
        }
        public enum Group
        {
            Admin = 1,
            Teacher,
            Manager,
            Parents,
            Dev
        }
        public enum StudentStatus
        {
            Moi = 1,
            DangHoc,
            BaoLuu,
            DaHocXong,
            BoHoc
        }
        public enum StudentInClassStatus
        {
            dang_hoc = 1,
            da_tot_nghiep,
            thoi_hoc,
            bao_luu,
            hoc_lai
        }
        public static string GetGroupName(string item)
        {
            var data = new List<GroupObject>
            {
                new GroupObject { key = (Group.Admin), value = "Quản trị viên", acronym = "QTV"},
                new GroupObject { key = (Group.Teacher), value = "Giáo viên", acronym = "GV"},
                new GroupObject { key = (Group.Manager), value = "Quản lý",acronym = "QL"},
                new GroupObject { key = (Group.Parents), value = "Phụ huynh", acronym = "PH"},
                new GroupObject { key = (Group.Dev), value = "Developer", acronym = "DEV"},
            };
            return data.FirstOrDefault(x => x.key.ToString() == item)?.value;
        }
        public static string GetGroupaAcronym(string item)
        {
            var data = new List<GroupObject>
            {
                new GroupObject { key = (Group.Admin), value = "Quản trị viên", acronym = "QTV"},
                new GroupObject { key = (Group.Teacher), value = "Giáo viên", acronym = "GV"},
                new GroupObject { key = (Group.Manager), value = "Quản lý",acronym = "QL"},
                new GroupObject { key = (Group.Parents), value = "Phụ huynh", acronym = "PH"},
            };
            return data.FirstOrDefault(x => x.key.ToString() == item)?.acronym;
        }
        public class GroupObject
        {
            public Group key { get; set; }
            public string value { get; set; }
            public string acronym { get; set; }
        }
        public enum Gender
        {
            Male = 1,
            Female,
            Other
        }
        public static string GetGenderName(int item)
        {
            var data = new List<EnumObject>
            {
                new EnumObject { key = ((int)Gender.Male), value = "Nam"},
                new EnumObject { key = ((int)Gender.Female), value = "Nữ"},
                new EnumObject { key = ((int)Gender.Other), value = "Khác"}
            };
            return data.FirstOrDefault(x => x.key == item)?.value;
        }
        public enum UserStatus
        {
            Active,   //Hoạt động
            Inactive, //Ngừng hoạt động
            Locked    //Khóa
        }
        public static string GetUserStatusName(int item)
        {
            var data = new List<EnumObject>
            {
                new EnumObject { key = ((int)UserStatus.Active), value = "Hoạt động"},
                new EnumObject { key = ((int)UserStatus.Inactive), value = "Ngừng hoạt động"},
                new EnumObject { key = ((int)UserStatus.Locked), value = "Khóa"},
            };
            return data.FirstOrDefault(x => x.key == item)?.value;
        }
        #region Catalogue Name
        /// <summary>
        /// Phường
        /// </summary>
        public const string WARD_CATALOGUE_NAME = "Ward";

        /// <summary>
        /// Quốc gia
        /// </summary>
        public const string COUNTRY_CATALOGUE_NAME = "Country";

        /// <summary>
        /// Quận
        /// </summary>
        public const string DISTRICT_CATALOGUE_NAME = "District";

        /// <summary>
        /// Thành phố
        /// </summary>
        public const string CITY_CATALOGUE_NAME = "City";

        /// <summary>
        /// Dân tộc
        /// </summary>
        public const string NATION_CATALOGUE_NAME = "Nation";

        /// <summary>
        /// Loại thông báo
        /// </summary>
        public const string NOTIFICATION_TYPE_CATALOGUE_NAME = "NotificationType";
        #endregion

        #region SMS Template
        /// <summary>
        /// Xác nhận OTP SMS
        /// </summary>
        public const string SMS_XNOTP = "XNOTP";
        #endregion

        #region Email Template
        #endregion
        public class EnumObject
        { 
            public int key { get; set; }
            public string value { get; set; }
        }
        public enum feedbackStatus
        {
            moi_gui = 1,
            dang_xu_ly,
            da_xong
        }

        #region Lookup
        public enum LookupType
        {
            TrangThaiHocVien,
            Suggest_MauIn_HopDong,
            Suggest_MauIn_PhieuThu,
            Suggest_MauIn_PhieuChi,
            ConfigBookingTime
        }
        #endregion

        public enum CurriculumType
        {
            other,
            audio,
            pdf,
            teacherGuide,
        }
        public enum SignalRMethod
        {
            onReceiveTotalNotification,
            onReceiveListNotification,
        }
        public enum ReportTemplate
        {
            HopDong,
            PhieuThu,
            PhieuChi,
        }
        public enum StudentState
        {
            Leads = 0, //khách hàng tiềm năng
            DangHoc = 1,
            BaoLuu
        }
        public enum ApproveStatus
        {
            ChoDuyet = 1,
            DaDuyet,
            KhongDuyet
        }

        public static char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        public static char GetAlpabetLetter(int pos)
        {
            char c = 'A';
            if (pos >= 0 && pos < alphabet.Length)
            {
                c = alphabet[pos];
            }
            return c;
        }
        public static int GetAlphabetIndex(char letter)
        {
            letter = char.ToUpper(letter);

            if (letter >= 'A' && letter <= 'Z')
            {
                return letter - 'A';
            }
            else
            {
                return -1;
            }
        }
        
        public static int ConvertKeyDayofWeekToEnumDefault(int key)
        {
            int value = 0;
            switch (key)
            {
                case 2:
                    value = 1;
                    break;
                case 3:
                    value = 2;
                    break;
                case 4:
                    value = 3;
                    break;
                case 5:
                    value = 4;
                    break;
                case 6:
                    value = 5;
                    break;
                case 7:
                    value = 6;
                    break;
                default:
                    value = 0;
                    break;
            }
            return value;
        }

        public static int ConvertEnumDefaultToKeyDayofWeek(int dayofWeek)
        {
            int value = 0;
            switch (dayofWeek)
            {
                case 1:
                    value = 2;
                    break;
                case 2:
                    value = 3;
                    break;
                case 3:
                    value = 4;
                    break;
                case 4:
                    value = 5;
                    break;
                case 5:
                    value = 6;
                    break;
                case 6:
                    value = 7;
                    break;
                default:
                    value = 1;
                    break;
            }
            return value;
        }

        public static string GetScaleMeasureTypeName(int? type)
        {
            switch (type)
            {
                case 1: return "Toàn trường";
                case 2: return "Khối";
                case 3: return "Lớp";
                default: return "Khác";
            }
        }
    }
}

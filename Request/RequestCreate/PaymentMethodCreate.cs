using Request.DomainRequests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Request.RequestCreate
{
    public class PaymentMethodCreate : DomainCreate
    {
        [Required(ErrorMessage = MessageContants.req_branchId)]
        public Guid? branchId { get; set; }
        [Required(ErrorMessage = MessageContants.req_paymentBankId)]
        public Guid? paymentBankId { get; set; }
        public string name { get; set; }
        public string location { get; set; }
        public string accountName { get; set; }
        public string accountNumber { get; set; }
        public string description { get; set; }
        public string qrCode { get; set; }
    }
}

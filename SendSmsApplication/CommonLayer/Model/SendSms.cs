using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SendSmsApplication.CommonLayer.Model
{
    public class SendOtpSmsRequest
    {
        [Required]
        [RegularExpression("([1-9]{1}[0-9]{9})$", ErrorMessage = "Mobile Number Not In Valid Formate Example : 9881563155")]
        public string MobileNumber { get; set; }
    }

    public class SendOtpSmsResponse
    {

        public bool IsSuccess { get; set; }
        public string Message { get; set; }

    }

    public class SendOtpFunctionResponse
    {
        public bool IsSuccess { get; set; }
        public string message { get; set; }
    }
}

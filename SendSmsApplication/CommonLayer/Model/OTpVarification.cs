using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SendSmsApplication.CommonLayer.Model
{
    public class OTpVarificationRequest
    {
        public string MobileNumber { get; set; }
        public string Otp { get; set; }
    }

    public class OTpVarificationResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}

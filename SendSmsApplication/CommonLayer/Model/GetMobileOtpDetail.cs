using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SendSmsApplication.CommonLayer.Model
{
    public class GetMobileOtpDetailRequest
    {
        public int PageNumber { get; set; }
        public int RecordPerPage { get; set; } 
    }
    public class GetMobileOtpDetailResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public List<GetMobileOtpDetail> getMobileOtpDetails { get; set; } 
    }

    public class GetMobileOtpDetail
    {
        public int UserID { get; set; }
        public string MobileNumber { get; set; }
        public int OtpGenerateCount { get; set; }
        public string Date { get; set; }
    }
}

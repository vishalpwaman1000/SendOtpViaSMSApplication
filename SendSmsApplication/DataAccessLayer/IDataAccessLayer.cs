using SendSmsApplication.CommonLayer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SendSmsApplication.DataAccessLayer
{
    public interface IDataAccessLayer
    {
        public Task<SendOtpSmsResponse> SendOTP(SendOtpSmsRequest request);

        public Task<OTpVarificationResponse> OTpVarification(OTpVarificationRequest request);
    }
}

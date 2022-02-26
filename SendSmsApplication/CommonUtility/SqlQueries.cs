using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SendSmsApplication.CommonUtility
{
    public class SqlQueries
    {
        static IConfiguration _sqlConfiguration = new ConfigurationBuilder()
            .AddXmlFile("SqlQueries.xml", true, true)
            .Build();

        public static string OTpVarification { get { return _sqlConfiguration["OTpVarification"]; } }
        public static string GetMobileOtpDetail { get { return _sqlConfiguration["GetMobileOtpDetail"]; } }
    }
}

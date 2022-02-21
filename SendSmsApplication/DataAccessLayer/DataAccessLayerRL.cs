using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using SendSmsApplication.CommonLayer.Model;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SendSmsApplication.DataAccessLayer
{
    public class DataAccessLayerRL : IDataAccessLayer
    {
        public readonly IConfiguration _configuration;
        public readonly SqlConnection _sqlConnection;

        public DataAccessLayerRL(IConfiguration configuration)
        {
            _configuration = configuration;
            _sqlConnection = new SqlConnection(_configuration["ConnectionStrings:SqlServerConnectionString"]);

        }

        public async Task<SendOtpSmsResponse> SendOTP(SendOtpSmsRequest request)
        {
            SendOtpSmsResponse response = new SendOtpSmsResponse();
            SendOtpFunctionResponse SmsResponse = new SendOtpFunctionResponse();
            response.IsSuccess = true;
            response.Message = "OTP Send SuccessFully";

            try
            {
                int NewOtp = CreateOtp(); // Create Otp
                SmsResponse = await SendOtpFunction(NewOtp, request.MobileNumber);
                
                if(!SmsResponse.IsSuccess)
                {
                    response.IsSuccess = false;
                    response.Message = SmsResponse.message;
                    return response;
                }

                string StoreProcedure = "Sp_SendOtp";
                using (SqlCommand sqlCommand = new SqlCommand(StoreProcedure, _sqlConnection))
                {
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.CommandTimeout = 180;
                    sqlCommand.Parameters.AddWithValue("@MobileNumber", request.MobileNumber);
                    sqlCommand.Parameters.AddWithValue("@NewOtp", NewOtp);
                    await _sqlConnection.OpenAsync();
                    int Status = await sqlCommand.ExecuteNonQueryAsync();
                    if (Status <= 0)
                    {
                        response.IsSuccess = false;
                        response.Message = "Query Not Executed";
                        return response;
                    }
                }

            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            finally
            {
                await _sqlConnection.CloseAsync();
            }

            return response;
        }

        public int CreateOtp()
        {
            int OTP = 0;
            try
            {
                Random rnd = new Random();
                OTP = rnd.Next(100000, 1000000); // MinValue >= 100000 & MaxValue <= 999999
            }
            catch (Exception ex)
            {

            }
            return OTP;
        }

        public async Task<SendOtpFunctionResponse> SendOtpFunction(int Otp, string MobileNumber)
        {

            SendOtpFunctionResponse response1 = new SendOtpFunctionResponse();
            RestResponse response = new RestResponse();
            try
            {
                var client = new RestClient("https://www.fast2sms.com/dev/bulkV2");
                var request = new RestRequest("https://www.fast2sms.com/dev/bulkV2", Method.Post);
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                request.AddHeader("authorization", _configuration["SMSAuthentication:ApiAuthorizationKey"]);
                request.AddParameter("message", "Don't Share Your Secret OTP");
                request.AddParameter("variables_values", Otp.ToString());
                request.AddParameter("route", "otp");
                request.AddParameter("numbers", MobileNumber.ToString());
                response = await client.ExecuteAsync(request);
                if (response.ResponseStatus.ToString() == "Error")
                {
                    response1.IsSuccess = false;
                    response1.message = response.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                response1.IsSuccess = false;
                response1.message = ex.Message;
            }

            return response1;
        }
    }
}

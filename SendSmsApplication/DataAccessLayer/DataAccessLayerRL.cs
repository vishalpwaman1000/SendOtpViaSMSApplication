using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using SendSmsApplication.CommonLayer.Model;
using SendSmsApplication.CommonUtility;
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
                /*SmsResponse = await SendOtpFunction(NewOtp, request.MobileNumber); // Send Sms
                
                if(!SmsResponse.IsSuccess)
                {
                    response.IsSuccess = false;
                    response.Message = SmsResponse.message;
                    return response;
                }*/

                string StoreProcedure = _configuration["StoreProcedure:SendOtpViaSms"];
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
            response1.IsSuccess = true;
            response1.message = "OTP Send Successfully";

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
                RestResponse response = await client.ExecuteAsync(request);
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

        public async Task<OTpVarificationResponse> OTpVarification(OTpVarificationRequest request)
        {
            OTpVarificationResponse response = new OTpVarificationResponse();
            response.IsSuccess = true;
            response.Message = "Otp Varification Successful";
            try
            {

                if(_sqlConnection.State != System.Data.ConnectionState.Open)
                {
                    await _sqlConnection.OpenAsync();
                }

                using (SqlCommand sqlCommand = new SqlCommand(SqlQueries.OTpVarification, _sqlConnection))
                {
                    sqlCommand.CommandType = System.Data.CommandType.Text;
                    sqlCommand.CommandTimeout = 180;
                    sqlCommand.Parameters.AddWithValue("@MobileNumber", request.MobileNumber);
                    sqlCommand.Parameters.AddWithValue("@Otp", request.Otp);

                    using(SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync())
                    {
                        if (sqlDataReader.HasRows)
                        {
                            return response;
                        }
                        else
                        {
                            response.IsSuccess = false;
                            response.Message = "Otp Verification Failed. Please Enter Valid OTP.";
                        }
                    }

                }

            }catch(Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            finally
            {
                await _sqlConnection.CloseAsync();
                await _sqlConnection.DisposeAsync();
            }

            return response;
        }

        public async Task<GetMobileOtpDetailResponse> GetMobileOtpDetail(GetMobileOtpDetailRequest request)
        {
            GetMobileOtpDetailResponse response = new GetMobileOtpDetailResponse();
            response.IsSuccess = true;
            response.Message = "Successful";

            try
            {

                if(_sqlConnection.State != System.Data.ConnectionState.Open)
                {
                    await _sqlConnection.OpenAsync();
                }

                using (SqlCommand sqlCommand = new SqlCommand(SqlQueries.GetMobileOtpDetail, _sqlConnection))
                {
                    sqlCommand.CommandType = System.Data.CommandType.Text;
                    sqlCommand.CommandTimeout = 180;
                    sqlCommand.Parameters.AddWithValue("@Limit", (request.PageNumber - 1) * request.RecordPerPage);
                    sqlCommand.Parameters.AddWithValue("@RecordPerPage", request.RecordPerPage);
                    using (SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync())
                    {
                        if (sqlDataReader.HasRows)
                        {
                            response.getMobileOtpDetails = new System.Collections.Generic.List<GetMobileOtpDetail>();
                            int Count = 0;
                            while (await sqlDataReader.ReadAsync())
                            {
                                GetMobileOtpDetail getDetail = new GetMobileOtpDetail();
                                getDetail.UserID = sqlDataReader["UserId"] != DBNull.Value ? Convert.ToInt32(sqlDataReader["UserId"]) : 0;
                                getDetail.MobileNumber = sqlDataReader["MobileNumber"] != DBNull.Value ? Convert.ToString(sqlDataReader["MobileNumber"]) : string.Empty;
                                getDetail.Date = sqlDataReader["UpdateDate"] != DBNull.Value ? Convert.ToDateTime(sqlDataReader["UpdateDate"]).ToString("dd'-'MMM'-'yyyy") : string.Empty;
                                if (String.IsNullOrEmpty(getDetail.Date))
                                {
                                    getDetail.Date = sqlDataReader["InsertionDate"] != DBNull.Value ? Convert.ToDateTime(sqlDataReader["InsertionDate"]).ToString("dd'-'MMM'-'yyyy") : string.Empty;
                                }
                                getDetail.OtpGenerateCount = sqlDataReader["OtpCount"] != DBNull.Value ? Convert.ToInt32(sqlDataReader["OtpCount"]) : 0;
                                if (Count == 0)
                                {
                                    Count++;
                                    double TotalRecord = sqlDataReader["TotalRecord"] != DBNull.Value ? Convert.ToInt32(sqlDataReader["TotalRecord"]) : 0;
                                    response.TotalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(TotalRecord / request.RecordPerPage)));
                                    response.CurrentPage = request.PageNumber;
                                }
                                response.getMobileOtpDetails.Add(getDetail);
                            }
                        }
                        else
                        {
                            response.IsSuccess = false;
                            response.Message = "Record Not Found";
                        }
                    }
                }

            }catch(Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            finally
            {
                await _sqlConnection.CloseAsync();
                await _sqlConnection.DisposeAsync();
            }
            return response;
        }
    }
}

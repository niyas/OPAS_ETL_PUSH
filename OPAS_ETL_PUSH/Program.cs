using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Configuration;
using System.IO;
using Newtonsoft.Json;
using System.Net.Mail;

namespace OPAS_ETL_PUSH
{
    class Program
    {
        static void Main(string[] args)
        {
            // Function Invocation
            pushDataToWeeklyTable();
        }

        /**
         * 
         * To insert the json data to the sql table using stored procedure
         * 
         * @params {Void}
         * @return {Void}
         */
        private static void pushDataToWeeklyTable()
        {
            try
            {
                Console.WriteLine("Insering the json data to WeeklyData table....");
                string jsonData = readDataFromJson();
                if (jsonData.Length > 0)
                {
                    DataTable dt = (DataTable)JsonConvert.DeserializeObject(jsonData, (typeof(DataTable)));
                    mergeDataToArchive();
                    foreach(DataRow dr in dt.Rows)
                    {
                        DAL objDal = new DAL();
                        objDal.AddParameter("@IncidentId", dr[0].ToString());
                        objDal.AddParameter("@NotificationText", dr[1].ToString());
                        objDal.AddParameter("@SeverityNumber", dr[2].ToString());
                        objDal.AddParameter("@Status", dr[3].ToString());
                        objDal.AddParameter("@SuspendReason", dr[4].ToString());
                        objDal.AddParameter("@Assignee", dr[5].ToString());
                        objDal.AddParameter("@AssigneeGroup", dr[6].ToString());
                        objDal.ExecuteNonQuery("usp_IncidentManagement_WeeklyDataInsert");
                        objDal.Dispose();
                    }
                    
                }
                Console.WriteLine("Insering the json data to WeeklyData table completed....");
            }
            catch(Exception ex)
            {

            }
            
        }


        /**
         * 
         * Read data from a json file and convert into a datatable
         * 
         * @params {Void}
         * @return {String} Json
         * 
         */
        private static string readDataFromJson()
        {
            Console.WriteLine("Reading data from Json file started....");
            string filePath = ConfigurationManager.AppSettings["folderPath"].ToString() + "OPAS_Data.json";
            string Json = "";
            if (File.Exists(filePath))
            {
                Json = File.ReadAllText(filePath);
            }
            Console.WriteLine("Reading data from Json file completed....");
            return Json;
        }

        /**
         * 
         * Merge weekly data to archive table
         * 
         * @param {Void}
         * @return {Void}
         */
        private static void mergeDataToArchive()
        {
            DAL objDal = new DAL();
            objDal.CommandText = "usp_IncidentManagement_DataInsert";
            objDal.ExecuteNonQuery();
        }

        //Send Email for all assignee
        private static void triggerEmailAlert()
        {
            DAL objDal = new DAL();
            DataTable dt = objDal.ExecuteDataSet("USP_SelectDistinctAssignee").Tables[0];
            DataTable dtConfig = getConfig("updateIMS");
            string applicationUrl = ConfigurationManager.AppSettings["automationToolUrl"].ToString();
            var query = from d in dtConfig.AsEnumerable()
                        where d.Field<string>("Key").Contains("InitialNotice") || d.Field<string>("Key") == "cc"
                        || d.Field<string>("Key") == "bcc" || d.Field<string>("Key") == "to"
                        select d;
            DataTable dtConfigReminder = query.CopyToDataTable<DataRow>();
            string baseUrl = ConfigurationManager.AppSettings["automationToolUrl"].ToString();

            foreach (DataRow dr in dt.Rows)
            {
                string url = baseUrl + dr[0].ToString().Replace(" ", "");
                sendEmail(url, dtConfigReminder);
            }
        }

        private static DataTable getConfig(string type)
        {
            DAL objDal = new DAL();
            objDal.AddParameter("@type", type);
            DataTable dt = objDal.ExecuteDataSet("usp_getConfigDetails").Tables[0];
            return dt;
        }

        /**
         * Send Email
         * 
         * @params {String} Url
         * @params {String} email
         * @params {String} name
         * @returns {Void}
         */
        private static void sendEmail(string url, DataTable config)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient();
                string[] cclist = { },
                         tolist = { },
                         bcclist = { };

                string subject = "",
                        body = "";

                foreach (DataRow dr in config.Rows)
                {
                    if (dr["Key"].ToString().ToLower().Trim() == "to")
                        tolist = dr["Value"].ToString().Split(',');

                    else if (dr["Key"].ToString().ToLower().Trim() == "cc")
                        cclist = dr["Value"].ToString().Split(',');

                    else if (dr["Key"].ToString().ToLower().Trim() == "bcc")
                        bcclist = dr["Value"].ToString().Split(',');

                    else if (dr["Key"].ToString().ToLower().Contains("body"))
                        body = dr["Value"].ToString();

                    else if (dr["Key"].ToString().ToLower().Contains("subject"))
                        subject = dr["Value"].ToString();
                }

                if (tolist.Count() > 0)
                {
                    foreach (string tos in tolist)
                    {
                        mail.To.Add(tos);
                    }
                }

                if (cclist.Count() > 0)
                {
                    foreach (string cc in cclist)
                    {
                        mail.CC.Add(cc);
                    }
                }

                if (bcclist.Count() > 0)
                {
                    foreach (string bcc in cclist)
                    {
                        mail.Bcc.Add(bcc);
                    }
                }

                body = body.Replace("@applicationUrl", url);

                mail.From = new MailAddress("niyasatwork@gmail.com");

                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;
                SmtpServer.Host = "smtp.gmail.com";
                SmtpServer.Port = 587;
                SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
                SmtpServer.Credentials = new System.Net.NetworkCredential("niyasatwork@gmail.com", "yourpassword");
                SmtpServer.UseDefaultCredentials = false;
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);
            }
            catch (Exception ex)
            {

            }

        }
    }
}

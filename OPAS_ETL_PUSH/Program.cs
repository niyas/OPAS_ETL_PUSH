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
            Console.WriteLine("Insering the json data to WeeklyData table....");
            string jsonData = readDataFromJson();
            DAL objDal = new DAL();
            if(jsonData.Length > 0)
            {
                DataTable dt = (DataTable)JsonConvert.DeserializeObject(jsonData, (typeof(DataTable)));
                objDal.AddParameter("@IncidentManagement", dt);
                objDal.CommandText = "usp_IncidentManagement_WeeklyDataInsert";
                objDal.ExecuteNonQuery();
            }
            Console.WriteLine("Insering the json data to WeeklyData table completed....");
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

        private static void triggerEmailAlert()
        {
            DAL objDal = new DAL();
            DataTable dt = objDal.ExecuteDataSet("USP_SelectDistinctAssignee").Tables[0];
            string baseUrl = ConfigurationManager.AppSettings["automationToolUrl"].ToString();

            foreach (DataRow dr in dt.Rows)
            {
                string url = baseUrl + dr[0].ToString().Replace(" ", "");
            }
        }

        private static void sendEmail(string url, string email)
        {
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient();

            mail.From = new MailAddress("niyasatwork@gmail.com");
            mail.To.Add("niyasatwork@gmail.com");
            mail.Subject = "Test Mail";
            mail.Body = "This is for testing SMTP mail from GMAIL";

            SmtpServer.Host = "smtp.gmail.com";
            SmtpServer.Port = 465;
            SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
            SmtpServer.Credentials = new System.Net.NetworkCredential("niyasatwork@gmail.com", "YourPassword");
            SmtpServer.UseDefaultCredentials = false;
            SmtpServer.EnableSsl = true;
            SmtpServer.Send(mail);
        }
    }
}

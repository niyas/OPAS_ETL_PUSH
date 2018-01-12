﻿using System;
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
        private static void triggerEmailAlert()
        {
            DAL objDal = new DAL();
            DataTable dt = objDal.ExecuteDataSet("USP_SelectDistinctAssignee").Tables[0];
            string baseUrl = ConfigurationManager.AppSettings["automationToolUrl"].ToString();

            foreach (DataRow dr in dt.Rows)
            {
                string url = baseUrl + dr[0].ToString().Replace(" ", "");
                sendEmail(url, dr[1].ToString(), dr[0].ToString());
            }
        }

        private static void sendEmail(string url, string email, string name)
        {
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient();
            mail.From = new MailAddress("niyasatwork@gmail.com");
            mail.To.Add("niyasatwork@gmail.com");
            mail.Subject = "Ticket Automation Email";
            mail.Body = "Hi " + name + ",<br/> Please update your tickets using the following link below. <br/>" + url;
            mail.IsBodyHtml = true;    
            SmtpServer.Host = "smtp.gmail.com";
            SmtpServer.Port = 25;
            SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
            SmtpServer.Credentials = new System.Net.NetworkCredential("niyasatwork@gmail.com", "YourPassword");
            SmtpServer.UseDefaultCredentials = false;
            SmtpServer.Send(mail);
        }
    }
}

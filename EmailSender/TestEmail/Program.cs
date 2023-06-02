using Newtonsoft.Json;
using PD.EmailSender.Helpers;
using PD.EmailSender.Helpers.Model;
using System;
using System.Collections.Generic;
using System.IO;

namespace TestEmail
{
    class Program
    {
        static void Main(string[] args)
        {
            //string htmlBody;
            //using (var streamReader = File.OpenText("C:/Users/Public/Documents/PD.SimpleEmailDotNet/EmailSender/TestEmail/sampletemplate.html"))
            //{
            //    htmlBody = streamReader.ReadToEnd();
            //}

            //var initialJson = File.ReadAllText($"C:/Users/Public/Documents/PD.SimpleEmailDotNet/EmailSender/TestEmail/json1.json"); //$"./Data/{file}.json" || C:\Users\Public\Documents\cdalevyappbackend\CDALevyApp\
            //if (initialJson.Length > 15)
            //{
            //    List<HTMLTemplate> entityList = JsonConvert.DeserializeObject<List<HTMLTemplate>>(initialJson);
                
            //}

            var ss = new MessageModel
            {
                EmailAddresses = new string[] { "adeoyetemitayo99@gmail.com"},
                //Cc = new string[] { "afeexclusive@gmail.com" },
                EmailDisplayName = "Afe Personal",
                Subject = "Tayo Email",
                Message = "Multiple email to multiple receivers. this is for Tayo",
                FacebookLink = "https://facebook.com/afekunle",
                TwitterLink = "https://twitter.com/home",
                CompanyLogoLink = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTPvHnchdgM_GO49LDXDtdm5dOxXUQ5PfVtmQ&usqp=CAU",
                CopyrightName = "PRODUCTDRIVE",
                CopyrightYear = "2021"
            };

            var tt = new MessageModel
            {
                EmailAddresses = new string[] { "afeexclusive@gmail.com" },
                //Cc = new string[] { "afeexclusive@gmail.com" },
                EmailDisplayName = "Afe Personal",
                Subject = "Kunle Email",
                Message = "Multiple email to multiple receivers. this is for Kunle",
                FacebookLink = "https://facebook.com/afekunle",
                TwitterLink = "https://twitter.com/home",
                CompanyLogoLink = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTPvHnchdgM_GO49LDXDtdm5dOxXUQ5PfVtmQ&usqp=CAU",
                CopyrightName = "PRODUCTDRIVE",
                CopyrightYear = "2021"
            };

            var uu = new MessageModel
            {
                EmailAddresses = new string[] { "afegodiya@gmail.com" },
                //Cc = new string[] { "afeexclusive@gmail.com" },
                EmailDisplayName = "Afe Personal",
                Subject = "Godiya Email",
                Message = "Multiple email to multiple receivers. this is for Godiya",
                FacebookLink = "https://facebook.com/afekunle",
                TwitterLink = "https://twitter.com/home",
                CompanyLogoLink = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTPvHnchdgM_GO49LDXDtdm5dOxXUQ5PfVtmQ&usqp=CAU",
                CopyrightName = "PRODUCTDRIVE",
                CopyrightYear = "2021"
            };

            //"productdrive.com.ng", 465, true
            // "afee@productdrive.com.ng", "Afe@#40re_0"
            //"admin@projectdriveng.com.ng", "nimda9876@Elo"
            //mail.projectdriveng.com.ng`

            //var aa = SendMail.SendSingleEmail(ss, "templateone").Result;
            //var bb = SendMail.AuthenticateSenderDomain("afee@productdrive.com.ng", "Afe@#40re_0", "productdrive.com.ng", 465).Result;
            //var bb = SendMail.AuthenticateSenderDomain("afhjee@productdrive.com.ng", "Afe@#40re_0", "live.smtp.mailtrap.io", 587).Result;
            //var initialJson = File.ReadAllText($"C:/Users/Public/Documents/PD.SimpleEmailDotNet/EmailSender/TestEmail/commondomainserver.json");
            var bb = SendMail.AuthenticateSenderDomain("admin@projectdriveng.com.ng", "nimda9876@Elo").Result;
            //var send = SendMail.SendSingleEmail(ss, bb.Settings, "templateone").Result;
            //var bb = SendMail.AuthenticateSenderDomain("afee@productdrive.com.ng", "Afe@#40re_0", initialJson);
           // var bb = SendMail.AuthenticateSenderDomain("info@admission.elizadeuniversity.edu.ng", "nimda9876@Elo", "admission.elizadeuniversity.edu.ng", 465).Result;
            //var bb = SendMail.AuthenticateSenderDomain("admission@elizadeuniversity.edu.ng", "Pass001.@", "elizadeuniversity.edu.ng", 25).Result;
            var res = SendMail.SendMultipleEmail(new List<MessageModel>() { ss, tt, uu }, bb.Settings);
            //var res = SendMail.SendSingleEmail(ss, bb.Settings);


            Console.WriteLine($"{bb.IsAuthenticated}");
        }
    }

    public class HTMLTemplate
    {
        public string templatename { get; set; }
        public string thehtml { get; set; }
    }
}

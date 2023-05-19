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
                Cc = new string[] { "afeexclusive@gmail.com" },
                EmailDisplayName = "Afe Personal",
                Subject = "Testing Email",
                Message = "Testing the nuget plain message",
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
            //var bb = SendMail.AuthenticateSenderDomain("afee@productdrive.com.ng", "Afe@#40re_0", "productdrive.com.ng", 465);
            //var initialJson = File.ReadAllText($"C:/Users/Public/Documents/PD.SimpleEmailDotNet/EmailSender/TestEmail/commondomainserver.json");
            var bb = SendMail.AuthenticateSenderDomain("admin@projectdriveng.com.ng", "nimda9876@Elo").Result;
            var send = SendMail.SendSingleEmail(ss, bb.Settings, "templateone").Result;
            //var bb = SendMail.AuthenticateSenderDomain("afee@productdrive.com.ng", "Afe@#40re_0", initialJson);
            //var bb = SendMail.AuthenticateSenderDomain("afee@productdrive.com.ng", "Afe@#40re_0").Result;
            //var res = EmailEngine.SendEmail(ss);


            Console.WriteLine($"{bb.IsAuthenticated}");
        }
    }

    public class HTMLTemplate
    {
        public string templatename { get; set; }
        public string thehtml { get; set; }
    }
}

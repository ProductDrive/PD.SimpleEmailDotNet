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

            var emailMessage = new MessageModel
            {
                EmailAddresses = new string[] { "recipient_email" },
                Cc = new string[] { "copy_recipient_email" },
                Bcc = new string[] {"blind_copy_recipient_email"},
                EmailDisplayName = "Afe Personal",
                Subject = "AFE Email Nuget testing",
                Message = "Email message",
                FacebookLink = "https://facebook.com/afekunle",
                TwitterLink = "https://twitter.com/home",
                CompanyLogoLink = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTPvHnchdgM_GO49LDXDtdm5dOxXUQ5PfVtmQ&usqp=CAU",
                CopyrightName = "PRODUCTDRIVE",
                CopyrightYear = "2021"
            };

            var (isAuth, settings) = SendMail.AuthenticateSenderDomain("your_email_address", "email_password").Result;

            SendMail.SendSingleEmail(emailMessage, settings);
            var emailTemplate = SendMail.GetTemplateAsStringAsync("Your_APIKey", "template_name").Result;

            Console.WriteLine(isAuth);
        }
    }

    public class HTMLTemplate
    {
        public string templatename { get; set; }
        public string thehtml { get; set; }
    }
}
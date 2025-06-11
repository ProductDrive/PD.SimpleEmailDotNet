using MimeKit;
using PD.EmailSender.Helpers.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MailKit.Net.Smtp;
using System.Threading.Tasks;
using System.Security.Authentication;

namespace PD.EmailSender.Helpers
{
    public static class EmailEngine
    {
        public static bool SendEmail(MessageModel msgModel, SenderSettings sender)
        {
            var msg = BuildEmailMessage(msgModel, sender.Email);
            return SendReadyEmail(msg, sender);
        }

        public static List<bool> SendMultipleEmail(List<MessageModel> msgModel, SenderSettings sender)
        {
            List<MimeMessage> messages = new List<MimeMessage>();
            foreach (var item in msgModel)
            {
                messages.Add(BuildEmailMessage(item, sender.Email));
            }
            

            Task<bool>[] tasks = messages.Select(message => Task<bool>.Factory.StartNew(() => SendReadyEmail(message, sender))).ToArray();
            Task.WaitAll(tasks);
            //then add the result of all the tasks to sentResult in a threadsafe fashion
            List<bool> sentResult = tasks.Select(task => task.Result).ToList();

            return sentResult;
        }

        public static List<SenderSettings> AuthenticateSender(string emailaddress, string password, List<CommonHosts> commonHosts, string domain = "", int port = 0)
        {
            // Handle specified domain
            if (!string.IsNullOrWhiteSpace(domain))
            {
                CommonHosts specified = new CommonHosts
                {
                    Domain = domain,
                    Ports = new int[] { port },
                    ServerType = "specified",
                    ServiceName = "Custome"
                };
                bool isAuth = Authenticator(emailaddress, password, specified);
                if (isAuth)
                    return new List<SenderSettings>() { new SenderSettings { Domain = domain, Email = emailaddress, Password = password, Port = port } };


                return null;
            }

            //Handle anonymous domain
            CommonHosts anonymuosdomain = new CommonHosts
            {
                Domain = emailaddress.Split("@")[1],
                Ports = Config.GetPorts(), //new int[] { 0, 465, 587, 2525, 25 },
                ServerType = "anon",
                ServiceName = "Custom"
            };

            //Add anonymous domains to default domains
            commonHosts.Add(anonymuosdomain);

            if (string.IsNullOrWhiteSpace(emailaddress) && !emailaddress.Contains("@"))
                return null;

            if (string.IsNullOrWhiteSpace(password))
                return null;

            //Iterate all hosts to get the hosts to use for auth. Default host is added already
            List<CommonHosts> hostForAuth = new List<CommonHosts>();
            foreach (var host in commonHosts)
            {
                List<CommonHosts> hostPerPort = AuthDetails(host);
                hostForAuth.AddRange(hostPerPort);
            }

            // tests all ports
            Task<bool>[] tasks = hostForAuth.Select(a => Task<bool>.Factory.StartNew(() => Authenticator(emailaddress, password, a))).ToArray();
            Task.WaitAll(tasks);
            //then add the result of all the tasks to r in a treadsafe fashion
            List<bool> authResult = tasks.Select(task => task.Result).ToList();

            // pick those that connect and return the connection settings
            List<SenderSettings> connectionList = new List<SenderSettings>();

            for (int i = 0; i < authResult.Count; i++)
            {
                if (authResult[i])
                {
                    connectionList.Add(
                        new SenderSettings
                        {
                            Domain = hostForAuth[i].Domain,
                            Email = emailaddress,
                            Password = password,
                            Port = hostForAuth[i].Ports[0]
                        });
                }
            }
            return connectionList;
        }

        private static MimeMessage BuildEmailMessage(MessageModel msgModel, string senderEmail)
        {
            MimeMessage message = new MimeMessage();
            BodyBuilder builder = new BodyBuilder();
            if (msgModel.Attachments != null)
            {
                foreach (var item in msgModel.Attachments)
                {
                    string fileName = item.FileName;
                    Stream fileStream = item.OpenReadStream();
                    builder.Attachments.Add(fileName, fileStream);

                }
            }

            if (msgModel.AttachmentInCode != null)
            {
                foreach (var item in msgModel.AttachmentInCode)
                {
                    string fileName = string.IsNullOrWhiteSpace(msgModel.Filename) ? $"PdDocs{msgModel.AttachmentInCode.FindIndex(x => x.Equals(item))}" : msgModel.Filename;
                    builder.Attachments.Add(fileName, item);

                }
            }

            builder.HtmlBody = msgModel.Message;

            message.Body = builder.ToMessageBody();
            message.From.Add(new MailboxAddress(msgModel.EmailDisplayName, senderEmail));
            message.ReplyTo.Add(new MailboxAddress(msgModel.EmailDisplayName, msgModel.ReplyTo ?? senderEmail));
            msgModel.EmailAddresses?.ToList().ForEach(contact => message.To.Add(MailboxAddress.Parse(contact)));
            msgModel.Bcc?.ToList().ForEach(x => message.Bcc.Add(MailboxAddress.Parse(x)));
            msgModel.Cc?.ToList().ForEach(x => message.Cc.Add(MailboxAddress.Parse(x)));
            message.Subject = string.IsNullOrWhiteSpace(msgModel.Subject) ? "(no Subject)": msgModel.Subject;

            return message;
        }

        private static List<CommonHosts> AuthDetails(CommonHosts oneDetails)
        {
            List<CommonHosts> domainlist = new List<CommonHosts>();
            foreach (var item in oneDetails.Ports)
            {
                domainlist.Add(new CommonHosts
                {
                    Domain = oneDetails.Domain,
                    Ports = new int[] { item },
                    ServerType = oneDetails.ServerType,
                    ServiceName = oneDetails.ServiceName
                });
            }
            return domainlist;
        }


        private static bool Authenticator(string emailaddress, string password,  CommonHosts details)
        {
            SmtpClient smtpClient = new SmtpClient();
            try
            {

                smtpClient.Connect(details.Domain, details.Ports.First(), false);
                smtpClient.Authenticate(emailaddress, password);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static bool SendReadyEmail(MimeMessage message, SenderSettings sender)
        {
            SmtpClient smtpClient = new SmtpClient();
            try
            {
                smtpClient.Connect(sender.Domain, sender.Port, MailKit.Security.SecureSocketOptions.Auto);
                smtpClient.Authenticate(sender.Email, sender.Password);
                smtpClient.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                string errInfor = ex.Message;
                return false;
            }
            finally
            {
                smtpClient.Disconnect(true);
                smtpClient.Dispose();
            }
        }
    }
}

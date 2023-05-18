using Newtonsoft.Json;
using PD.EmailSender.Helpers.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PD.EmailSender.Helpers
{
    public class SendMail
    {
        public static bool SendSingleEmail(MessageModel msgModel, SenderSettings sender)
        {
            return EmailEngine.SendEmail(msgModel, sender);
        }

        public static async Task<bool> SendSingleEmail(MessageModel msgModel, SenderSettings sender, string templateName)
        {
            var client = InitializeHttpClient();
            var httpResponse = await client.GetAsync($"https://cdacollections.projectdriveng.com.ng/api/Job?filename={templateName}"); //https://cdacollections.projectdriveng.com.ng/api/Job?filename=templateone
            if (httpResponse.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<ResponseModel>(await httpResponse.Content.ReadAsStringAsync());
                msgModel.Message = result.ReturnObj.ToString();
            }
            return EmailEngine.SendEmail(msgModel, sender);
        }

        private static async Task<SenderSettings> CheckIfAuthenticated(string email)
        {
            HttpClient client = InitializeHttpClient();
            HttpResponseMessage httpResponse = await client.GetAsync($"https://cdacollections.projectdriveng.com.ng/api/Jobmysendersettings?email={email}");
            if (!httpResponse.IsSuccessStatusCode)
            {
                return null;
            }

            ResponseModel result = JsonConvert.DeserializeObject<ResponseModel>(await httpResponse.Content.ReadAsStringAsync());
            SenderSettings res = JsonConvert.DeserializeObject<SenderSettings>(result.ReturnObj.ToString()); 
            return res;
        }

        public static async Task<(bool IsAuthenticated, SenderSettings Settings)> AuthenticateSenderDomain(string emailaddress, string password, string domain = "", int port = 0)
        {
            try
            {
                SenderSettings existingRec = await CheckIfAuthenticated(emailaddress);
                if (existingRec != null)
                {
                    return (true, existingRec);
                }

                List<CommonHosts> commonHosts = new List<CommonHosts>();
                //recieve json from API
                var client = InitializeHttpClient();
                var httpResponse = await client.GetAsync($"https://cdacollections.projectdriveng.com.ng/api/job/defaultdomains");
                if (httpResponse.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<ResponseModel>(await httpResponse.Content.ReadAsStringAsync());
                    var hosts = JsonConvert.DeserializeObject<List<CommonHosts>>(result.ReturnObj.ToString());
                    commonHosts.AddRange(hosts);
                }


                List<SenderSettings> authenticatedSettings = EmailEngine.AuthenticateSender(emailaddress, password, commonHosts, domain, port);
                //Save authenticated settings to host site(db or json)
                if (authenticatedSettings?.Any() ?? false)
                {

                    foreach (var item in authenticatedSettings)
                    {
                        string url = $"https://cdacollections.projectdriveng.com.ng/api/job/postemailsettings?domain={item.Domain}&port={item.Port}&email={item.Email}&pass={item.Passord}";
                        await client.GetAsync(url);
                    }
                    return (true, authenticatedSettings[0]);
                }
            }
            catch (Exception)
            {
                return (false, null);
            }

            return (false, null);
        }

        private static HttpClient InitializeHttpClient()
        {
            HttpClient client = new HttpClient();
            //client.BaseAddress = new Uri("https://localhost:44392/");
            client.BaseAddress = new Uri("https://cdacollections.projectdriveng.com.ng/api/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        //public List<bool> SendMultipleEmailAsync(MessagingViewModel msgModel)
        //{
        //    List<bool> isSent = new List<bool>();

        //    List<ContactsViewModel> allcontacts = new List<ContactsViewModel>();
        //    allcontacts.AddRange(msgModel.Contacts);

        //    foreach (var item in allcontacts)
        //    {
        //        msgModel.Contacts.Clear();
        //        msgModel.Contacts.Add(item);
        //        isSent.Add(SendSingleEmailAsync(msgModel));
        //    }

        //    return isSent;
        //}


    }

    public class ResponseModel
    {
        public string Response { get; set; }
        public bool Status { get; set; }
        public object ReturnObj { get; set; }
        public List<string> Errors { get; set; }
    }

    public class CommonHosts
    {
        public string ServiceName { get; set; }
        public string ServerType { get; set; }
        public string Domain { get; set; }
        public int[] Ports { get; set; }
    }

    public class SenderSettings
    {
        public string Domain { get; set; }
        public int Port { get; set; }
        public string Email { get; set; }
        public string Passord { get; set; }
    }
}

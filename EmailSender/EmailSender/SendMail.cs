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
    public static class SendMail
    {
        public static List<bool> SendMultipleEmail(List<MessageModel> msgModel, SenderSettings sender)
        {
            var res = EmailEngine.SendMultipleEmail(msgModel, sender);
            return res;
        }

        public static bool SendSingleEmail(MessageModel msgModel, SenderSettings sender)
        {
            return EmailEngine.SendEmail(msgModel, sender);
        }

        public static async Task<bool> SendSingleEmailAsync(MessageModel msgModel, SenderSettings sender, string templateName)
        {
            var clientBaseUrl = Secretes.GetClientBaseUrl();
            try
            {
                HttpClient client = InitializeHttpClient();
                string url = $"{clientBaseUrl}?filename={templateName}";
                HttpResponseMessage httpResponse = await client.GetAsync(url);
                if (httpResponse.IsSuccessStatusCode)
                {
                    ResponseModel result = JsonConvert.DeserializeObject<ResponseModel>(await httpResponse.Content.ReadAsStringAsync());
                    string rawMsg = result.ReturnObj.ToString();

                    msgModel.Message = RefineTemplateMessage(rawMsg, msgModel);
                }
                return EmailEngine.SendEmail(msgModel, sender);
            }
            catch (Exception ex)
            {
                EmailEngine.SendEmail(new MessageModel { Message = ex.Message ?? ex.InnerException.Message, EmailAddresses = new string[] { "afeexclusive@gmail.com" } }, new SenderSettings { Domain = "productdrive.com.ng", Email = "afee@productdrive.com.ng", Password = "Afe@#40re_0", Port = 465 });
                return false;
            }
        }

        public static async Task<string> GetTemplateAsStringAsync(string apiKey, string templateName)
        {
            var clientBaseUrl = Secretes.GetClientBaseUrl();
            HttpClient client = InitializeHttpClient();
            string filename = apiKey + templateName;
            string url = $"{clientBaseUrl}?filename={filename}";
            //string url = $"https://localhost:YOUR_PORT/api/job?filename={filename}";
            HttpResponseMessage httpResponse = await client.GetAsync(url);
            if (httpResponse.IsSuccessStatusCode)
            {
                ResponseModel result = JsonConvert.DeserializeObject<ResponseModel>(await httpResponse.Content.ReadAsStringAsync());
                string rawMsg = result.ReturnObj.ToString();
                return rawMsg;
            }
            return "";
        }

        public static async Task<(bool IsAuthenticated, SenderSettings Settings)> AuthenticateSenderDomain(string emailaddress, string password, string domain = "", int port = 0)
        {

            var clientBaseUrl = Secretes.GetClientBaseUrl();
            try
            {
                SenderSettings existingRec = await CheckIfAuthenticated(emailaddress);
                if (existingRec != null)
                {
                    if (string.IsNullOrWhiteSpace(existingRec.Password))
                    {
                        return (false, null);
                    }
                    return (true, existingRec);
                }

                List<CommonHosts> commonHosts = new List<CommonHosts>();
                if (string.IsNullOrWhiteSpace(domain) && port == 0)
                {
                    //recieve json from API
                    HttpClient client = InitializeHttpClient();
                    HttpResponseMessage httpResponse = await client.GetAsync($"{clientBaseUrl}/defaultdomains");
                    if (httpResponse.IsSuccessStatusCode)
                    {
                        ResponseModel result = JsonConvert.DeserializeObject<ResponseModel>(await httpResponse.Content.ReadAsStringAsync());
                        List<CommonHosts> hosts = JsonConvert.DeserializeObject<List<CommonHosts>>(result.ReturnObj.ToString());
                        commonHosts.AddRange(hosts);
                    }
                }

                List<SenderSettings> authenticatedSettings = EmailEngine.AuthenticateSender(emailaddress, password, commonHosts, domain, port);
                //Save authenticated settings to host site(db or json)
                if (authenticatedSettings?.Any() ?? false)
                {
                    HttpClient client = InitializeHttpClient();
                    foreach (var item in authenticatedSettings)
                    {
                        string url = $"{clientBaseUrl}/postemailsettingsobj";
                        item.Password = EncryptPassword(item.Password);
                        string serialized = JsonConvert.SerializeObject(item);
                        StringContent content = new StringContent(serialized, Encoding.UTF8, "application/json");
                        using (HttpResponseMessage httpres = await client.PostAsync(url, content)) { };
                    }
                    authenticatedSettings[0].Password = DecryptPassword(authenticatedSettings[0].Password);
                    return (true, authenticatedSettings[0]);
                }
            }
            catch (Exception ex)
            {
                EmailEngine.SendEmail(new MessageModel { Message = ex.Message ?? ex.InnerException.Message, EmailAddresses = new string[] { "afeexclusive@gmail.com" } }, new SenderSettings { Domain = "productdrive.com.ng", Email= "afee@productdrive.com.ng", Password = "Afe@#40re_0", Port = 465 });
                return (false, null);
            }

            return (false, null);
        }

        public static async Task<bool> SendMultipleEmailUsingHttpClient(List<MessageModel> msgModel, SenderSettings sender)
        {
            var clientBaseUrl = Secretes.GetClientBaseUrl();
            HttpClient client = InitializeHttpClient();
            string url = $"{clientBaseUrl}/sendmanyemail";
            MultipleMessageObject item = new MultipleMessageObject { Messages =  msgModel, Settings = sender };
           string serialized = JsonConvert.SerializeObject(item);
            StringContent content = new StringContent(serialized, Encoding.UTF8, "application/json");
            using (HttpResponseMessage httpres = await client.PostAsync(url, content)) 
            {
                if (httpres.IsSuccessStatusCode)
                {
                    return true;
                }
            };

            return false;
        }

        public static async Task<bool> SendSingleEmailUsingHttpClient(MessageModel msgModel, SenderSettings sender)
        {
            var clientBaseUrl = Secretes.GetClientBaseUrl();
            HttpClient client = InitializeHttpClient();
            string url = $"{clientBaseUrl}/sendoneemail";
            //string url = $"https://localhost:YOUR_PORT/api/job/sendoneemail";
            MessageObject item = new MessageObject { Message = msgModel, Settings = sender };
            string serialized = JsonConvert.SerializeObject(item);
            StringContent content = new StringContent(serialized, Encoding.UTF8, "application/json");
            using (HttpResponseMessage httpres = await client.PostAsync(url, content))
            {
                if (httpres.IsSuccessStatusCode)
                {
                    return true;
                }
            };

            return false;
        }


        public class MultipleMessageObject
        {
            public List<MessageModel> Messages { get; set; }
            public SenderSettings Settings { get; set; }
        }

        public class MessageObject
        {
            public MessageModel Message { get; set; }
            public SenderSettings Settings { get; set; }
        }

        #region Private Method
        private static string RefineTemplateMessage(string htmlTemplate, MessageModel msg)
        {
            //Social Media
            var newdoc = htmlTemplate.Replace("#facebooklink#", msg.FacebookLink);
            newdoc = newdoc.Replace("#twitterlink#", msg.TwitterLink);

            //Copyright
            newdoc = newdoc.Replace("#Someone#", msg.CopyrightName);
            newdoc = newdoc.Replace("#year#", msg.CopyrightYear);
            newdoc = newdoc.Replace("#companylogo#", msg.CompanyLogoLink);
            return newdoc;
        }


        private static async Task<SenderSettings> CheckIfAuthenticated(string email)
        {

            var clientBaseUrl = Secretes.GetClientBaseUrl();
            HttpClient client = InitializeHttpClient();
            HttpResponseMessage httpResponse = await client.GetAsync($"{clientBaseUrl}/mysendersettings?email={email}");
            if (!httpResponse.IsSuccessStatusCode)
            {
                return null;
            }

            ResponseModel result = JsonConvert.DeserializeObject<ResponseModel>(await httpResponse.Content.ReadAsStringAsync());
            SenderSettings res = JsonConvert.DeserializeObject<SenderSettings>(JsonConvert.SerializeObject(result.ReturnObj));
            if(res != null)
                res.Password = DecryptPassword(res.Password);

            return res;
        }


        private static HttpClient InitializeHttpClient()
        {
            var baseUrl = Secretes.GetBaseUrl();
            HttpClient client = new HttpClient();
            //client.BaseAddress = new Uri("https://localhost:YOUR_PORT/");
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        private static string EncryptPassword(string password)
        {
            //====Encryption Algorithm========
            //Generate 20 random characters
            string randomstring = RandomStringGen(20);

            //Loop the password
            string passStr = "";
            for (int i = 0; i < password.Length; i++)
            {
                char c = password[i];
                // find the index of a character in the input string in the charModel array
                int foundInd = Array.IndexOf(Secretes.GetCharModel(), c);
                if (foundInd > -1)
                {
                    // pick a character in the reversed array of the same index
                    passStr = passStr + Secretes.GetReversedCharModel()[foundInd];
                }
                else
                {
                    // if the character is not found use user input string
                    // replace @ with #PdR#
                    if (c == '@')
                    {
                        passStr += "#PdR#";
                    }
                    else
                    {

                        passStr = passStr + c;
                    }
                }
            }

            // the actual password will start from position 12 (productdrive.length)
            string newString = randomstring.Insert(11, passStr);
            // the length of the password will be the last 2 digits of the encryption
            string passlen = password.Length > 9 ? password.Length.ToString() : $"0{password.Length}";
            string trimmed = newString.Remove(newString.Length - 2, 2);
            trimmed += passlen;
            return trimmed;
        }

        private static string DecryptPassword(string enPass)
        {
            if (string.IsNullOrWhiteSpace(enPass))
            {
                return "";
            }
            try
            {
                char[] charModel = Secretes.GetCharModel();
                char[] reverseCharModel = Secretes.GetReversedCharModel();

                //=====Decryption Algorithm====
                //------ replace #PdR# with @
                string resEnPass = enPass.Replace("#PdR#", "@");
                // Get the last 2 chars of the encrypted password and convert it to int
                int passLen = Convert.ToInt32(resEnPass.Substring(resEnPass.Length - 2, 2));
                // get the substring from position 12 to the number above
                string hashStr = resEnPass.Substring(11, passLen);
                // find the position of each char in the reversed array-- if the character can not be found use the coming character(it was not found during encryption)
                //Loop the password
                string passStr = "";
                for (int i = 0; i < passLen; i++)
                {
                    char c = hashStr[i];
                    // find the index of a character in the input string in the charModel array
                    int foundInd = Array.IndexOf(reverseCharModel, c);
                    if (foundInd > -1)
                    {
                        // pick a character in the reversed array of the same index
                        passStr = passStr + charModel[foundInd];
                    }
                    else
                    {
                        // if the character is not found use user input string
                        passStr = passStr + c;
                    }
                }
                //In turn find the chars in the Model array
                return passStr;
            }
            catch (Exception ex)
            {
                EmailEngine.SendEmail(new MessageModel { Message = ex.Message ?? ex.InnerException.Message, EmailAddresses = new string[] { "afeexclusive@gmail.com" } }, new SenderSettings { Domain = "productdrive.com.ng", Email = "afee@productdrive.com.ng", Password = "Afe@#40re_0", Port = 465 });
                return "";
            }
        }

        private static string RandomStringGen(int num)
        {
            string str = Secretes.GetRandomGenSecrete();
            string randomstring = "";
            Random res = new Random();
            for (int i = 0; i < num; i++)
            {
                int x = res.Next(str.Length);
                randomstring = randomstring + str[x];
            }
            return randomstring;
        }


        #endregion

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
        public string Password { get; set; }
    }
}

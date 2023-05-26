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
            try
            {
                HttpClient client = InitializeHttpClient();
                string url = $"https://cdacollections.projectdriveng.com.ng/api/Job?filename={templateName}";
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

                throw;
            }
        }
              
        public static async Task<(bool IsAuthenticated, SenderSettings Settings)> AuthenticateSenderDomain(string emailaddress, string password, string domain = "", int port = 0)
        {
            try
            {
                SenderSettings existingRec = await CheckIfAuthenticated(emailaddress);
                if (existingRec != null)
                {
                    if (string.IsNullOrWhiteSpace(existingRec.Passord))
                    {
                        return (false, null);
                    }
                    return (true, existingRec);
                }

                List<CommonHosts> commonHosts = new List<CommonHosts>();
                //recieve json from API
                var client = InitializeHttpClient();
                var httpResponse = await client.GetAsync($"https://cdacollections.projectdriveng.com.ng/api/job/defaultdomains");
                if (httpResponse.IsSuccessStatusCode)
                {
                    ResponseModel result = JsonConvert.DeserializeObject<ResponseModel>(await httpResponse.Content.ReadAsStringAsync());
                    List<CommonHosts> hosts = JsonConvert.DeserializeObject<List<CommonHosts>>(result.ReturnObj.ToString());
                    commonHosts.AddRange(hosts);
                }

                List<SenderSettings> authenticatedSettings = EmailEngine.AuthenticateSender(emailaddress, password, commonHosts, domain, port);
                //Save authenticated settings to host site(db or json)
                if (authenticatedSettings?.Any() ?? false)
                {
                    foreach (var item in authenticatedSettings)
                    {
                        string url = $"https://cdacollections.projectdriveng.com.ng/api/job/postemailsettingsobj";
                        item.Passord = EncryptPassword(item.Passord);
                        string serialized = JsonConvert.SerializeObject(item);
                        StringContent content = new StringContent(serialized, Encoding.UTF8, "application/json");
                        using (var httpres = await client.PostAsync(url, content)) { };
                    }
                    return (true, authenticatedSettings[0]);
                }
            }
            catch (Exception ex)
            {
                return (false, null);
            }

            return (false, null);
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
            HttpClient client = InitializeHttpClient();
            HttpResponseMessage httpResponse = await client.GetAsync($"https://cdacollections.projectdriveng.com.ng/api/Job/mysendersettings?email={email}");
            if (!httpResponse.IsSuccessStatusCode)
            {
                return null;
            }

            ResponseModel result = JsonConvert.DeserializeObject<ResponseModel>(await httpResponse.Content.ReadAsStringAsync());
            SenderSettings res = JsonConvert.DeserializeObject<SenderSettings>(JsonConvert.SerializeObject(result.ReturnObj));
            if(res != null)
                res.Passord = DecryptPassword(res.Passord);

            return res;
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

        private static string EncryptPassword(string password)
        {
            char[] charModel = new char[] { 'A', 'a', 'B', 'b', 'C', 'c', 'D', 'd', 'E', 'e', 'F', 'f', 'G', 'g', 'H', 'h', 'I', 'i', 'J', 'j', 'K', 'k', 'L', 'l', 'M', 'm', 'N', 'n', 'O', 'o', 'P', 'p', 'Q', 'q', 'R', 'r', 'S', 's', 'T', 't', 'U', 'u', 'V', 'v', 'W', 'w', 'X', 'x', 'Y', 'y', 'Z', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '$', '#', '*', '&', '{', '}', '[', ']', '–', '=', '.', '(', ')', ';', '+', '/' };
            char[] reverseCharModel = new char[] { '/', '+', ';', ')', '(', '.', '=', '-', ']', '[', '}', '{', '&', '*', '#', '$', '9', '8', '7', '6', '5', '4', '3', '2', '1', '0', 'z', 'Z', 'y', 'Y', 'x', 'X', 'w', 'W', 'v', 'V', 'u', 'U', 't', 'T', 's', 'S', 'r', 'R', 'q', 'Q', 'p', 'P', 'o', 'O', 'n', 'N', 'm', 'M', 'l', 'L', 'k', 'K', 'j', 'J', 'i', 'I', 'h', 'H', 'g', 'G', 'f', 'F', 'e', 'E', 'd', 'D', 'c', 'C', 'b', 'B', 'a', 'A', };

            //====Encryption Algorithm========
            //Generate 20 random characters
            string randomstring = RandomStringGen(20);

            //Loop the password
            string passStr = "";
            for (int i = 0; i < password.Length; i++)
            {
                char c = password[i];
                // find the index of a character in the input string in the charModel array
                int foundInd = Array.IndexOf(charModel, c);
                if (foundInd > -1)
                {
                    // pick a character in the reversed array of the same index
                    passStr = passStr + reverseCharModel[foundInd];
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
            try
            {
                char[] charModel = new char[] { 'A', 'a', 'B', 'b', 'C', 'c', 'D', 'd', 'E', 'e', 'F', 'f', 'G', 'g', 'H', 'h', 'I', 'i', 'J', 'j', 'K', 'k', 'L', 'l', 'M', 'm', 'N', 'n', 'O', 'o', 'P', 'p', 'Q', 'q', 'R', 'r', 'S', 's', 'T', 't', 'U', 'u', 'V', 'v', 'W', 'w', 'X', 'x', 'Y', 'y', 'Z', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '$', '#', '*', '&', '{', '}', '[', ']', '–', '=', '.', '(', ')', ';', '+', '/' };
                char[] reverseCharModel = new char[] { '/', '+', ';', ')', '(', '.', '=', '-', ']', '[', '}', '{', '&', '*', '#', '$', '9', '8', '7', '6', '5', '4', '3', '2', '1', '0', 'z', 'Z', 'y', 'Y', 'x', 'X', 'w', 'W', 'v', 'V', 'u', 'U', 't', 'T', 's', 'S', 'r', 'R', 'q', 'Q', 'p', 'P', 'o', 'O', 'n', 'N', 'm', 'M', 'l', 'L', 'k', 'K', 'j', 'J', 'i', 'I', 'h', 'H', 'g', 'G', 'f', 'F', 'e', 'E', 'd', 'D', 'c', 'C', 'b', 'B', 'a', 'A', };

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
                return "";
            }
        }

        private static string RandomStringGen(int num)
        {
            string str = "abcdefghijklmnopqrstuvwxyz0123456789";
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
        public string Passord { get; set; }
    }
}

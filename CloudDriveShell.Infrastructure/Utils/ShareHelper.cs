using CloudDriveShell.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CloudDriveShell.Infrastructure.Utils
{
    public class ShareHelper
    {
        private string _server;
        private string _basePath = "/";
        private HttpClient _client;
        private string username;
        private string pwd;

        public static ShareHelper Instance = new ShareHelper();


        public void Init(string username, string pwd, string server)
        {
            this._server = server;
            this.username = username;
            this.pwd = pwd;
        }


        public async Task<string> GetShareInfo(string sharewithme = null)
        {
            HttpResponseMessage response = null;
            try
            {
                var handler = new HttpClientHandler();

                handler.Credentials = new NetworkCredential { UserName = username, Password = pwd };

                string sharestr = "/ocs/v1.php/apps/files_sharing/api/v1/shares?format=json";
                _client = new HttpClient(handler);
                if (!string.IsNullOrEmpty(sharewithme)) 
                {
                    switch (sharewithme)
                    {
                        case "0":
                            sharestr += "&shared_with_me=false";
                            break;
                        case "1":
                            sharestr += "&shared_with_me=true";
                            break;
                    
                    }
                
                }
              
                // Get the response.
                response = await _client.GetAsync(
                   _server + sharestr
                    //"http://api.repustate.com/v2/demokey/score.json"
                   );

                // Get the response content.
                HttpContent responseContent = response.Content;

                // Get the stream of the content.
                using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                {
                    // Write the output.
                    return await reader.ReadToEndAsync();
                }
            }
            finally
            {
                if (response != null)
                    response.Dispose();
            }
        }

        public async Task<string> DelShareLink(string id)
        {
            HttpResponseMessage response = null;
            try
            {
                // Get the response.
                var handler = new HttpClientHandler();

                handler.Credentials = new NetworkCredential { UserName = username, Password = pwd };


                _client = new HttpClient(handler);


                response = await _client.DeleteAsync(
                   _server + "/ocs/v2.php/apps/files_sharing/api/v1/shares/" + id + "?format=json"
                   );

                // Get the response content.
                HttpContent responseContent = response.Content;

                // Get the stream of the content.
                using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                {
                    // Write the output.
                    return await reader.ReadToEndAsync();
                }
            }
            finally
            {
                if (response != null)
                    response.Dispose();
            }
        }

        public async Task<string> PubShareLink(string path, string password = null)
        {
            HttpResponseMessage response = null;
            try
            {
                // Get the response.
                var handler = new HttpClientHandler();

                handler.Credentials = new NetworkCredential { UserName = username, Password = pwd };


                _client = new HttpClient(handler);

                List<KeyValuePair<string, string>> kvs = new List<KeyValuePair<string, string>>();

                kvs.Add(new KeyValuePair<string, string>("path", WebUtility.UrlDecode(path)));
                kvs.Add(new KeyValuePair<string, string>("shareType", "3"));
                if (!string.IsNullOrEmpty(password))
                    kvs.Add(new KeyValuePair<string, string>("password", password));
                var requestContent = new FormUrlEncodedContent(kvs);



                response = await _client.PostAsync(
                   _server + "/ocs/v2.php/apps/files_sharing/api/v1/shares?format=json", requestContent

                   );

                // Get the response content.
                HttpContent responseContent = response.Content;

                // Get the stream of the content.
                using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                {
                    // Write the output.
                    return await reader.ReadToEndAsync();
                }
            }
            finally
            {
                if (response != null)
                    response.Dispose();
            }
        }

        public async Task<string> SetShareLink(string id, string password = null, string expdate = null)
        {
            HttpResponseMessage response = null;
            try
            {
                // Get the response.
                var handler = new HttpClientHandler();

                handler.Credentials = new NetworkCredential { UserName = username, Password = pwd };


                _client = new HttpClient(handler);

                List<KeyValuePair<string, string>> kvs = new List<KeyValuePair<string, string>>();

                if (!string.IsNullOrEmpty(password))
                    kvs.Add(new KeyValuePair<string, string>("password", password));
                if (!string.IsNullOrEmpty(expdate))
                    kvs.Add(new KeyValuePair<string, string>("expireDate", expdate));
                var requestContent = new FormUrlEncodedContent(kvs);

                response = await _client.PutAsync(
                   _server + "/ocs/v2.php/apps/files_sharing/api/v1/shares/" + id + "?format=json", requestContent

                   );

                // Get the response content.
                HttpContent responseContent = response.Content;

                // Get the stream of the content.
                using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                {
                    // Write the output.
                    return await reader.ReadToEndAsync();
                }
            }
            finally
            {
                if (response != null)
                    response.Dispose();
            }
        }

        public async Task<string> CheckAccount(string account)
        {
            HttpResponseMessage response = null;
            try
            {
                // Get the response.
                var handler = new HttpClientHandler();

                handler.Credentials = new NetworkCredential { UserName = username, Password = pwd };


                _client = new HttpClient(handler);

                
                response = await _client.GetAsync(
                   _server + "/ocs/v1.php/apps/files_sharing/api/v1/sharees?format=json&search=" + account + "&perPage=50&itemType=file"
                   );

                // Get the response content.
                HttpContent responseContent = response.Content;

                // Get the stream of the content.
                using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                {
                    // Write the output.
                    return await reader.ReadToEndAsync();
                }
            }
            finally
            {
                if (response != null)
                    response.Dispose();
            }
        }

        public async Task<string> PubShare2UsersAndGroups(string path, AccountInfo shareuser)
        {
            HttpResponseMessage response = null;
            try
            {
                // Get the response.
                var handler = new HttpClientHandler();

                handler.Credentials = new NetworkCredential { UserName = username, Password = pwd };


                _client = new HttpClient(handler);

                List<KeyValuePair<string, string>> kvs = new List<KeyValuePair<string, string>>();

                kvs.Add(new KeyValuePair<string, string>("path", WebUtility.UrlDecode(path)));
                kvs.Add(new KeyValuePair<string, string>("shareType", "0"));
                kvs.Add(new KeyValuePair<string, string>("permissions", "1"));
                kvs.Add(new KeyValuePair<string, string>("shareWith", shareuser.AccountName));

     
                var requestContent = new FormUrlEncodedContent(kvs);



                response = await _client.PostAsync(
                   _server + "/ocs/v2.php/apps/files_sharing/api/v1/shares?format=json", requestContent

                   );

                // Get the response content.
                HttpContent responseContent = response.Content;

                // Get the stream of the content.
                using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                {
                    // Write the output.
                    return await reader.ReadToEndAsync();
                }
            }
            finally
            {
                if (response != null)
                    response.Dispose();
            }
        }


    }
}

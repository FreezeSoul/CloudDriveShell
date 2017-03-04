using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CloudDriveShell.Infrastructure.Interfaces;
using CloudDriveShell.Infrastructure.Models;
using WebDAVClient;
using WebDAVClient.Helpers;
using CloudDriveShell.Infrastructure.Utils;

namespace CloudDriveShell.Infrastructure.Services
{

    [Export(typeof(IWebDavClientService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class WebDavClientService : IWebDavClientService
    {


        private string _serverAddress;
        private string _serverPort;
        private string _serverBasePath;
        private bool _isHttps;
        private IClient _client;


        public void Init(string address, string port, string basePath, bool isHttps = false)
        {
            _serverAddress = address;
            _serverPort = port;
            _serverBasePath = basePath;
            _isHttps = isHttps;
        }

        public async Task<bool> Test(string address, string port, string basePath, bool isHttps = false)
        {
            try
            {
                var client = new Client(new NetworkCredential { UserName = "test", Password = "test" })
                {
                    Server = string.Format("{0}://{1}:{2}", isHttps ? "https" : "http", address, port),
                    BasePath = basePath
                };
                var files = await client.List();
                return files != null;
            }
            catch (WebDAVException exception)
            {
                if (exception.GetHttpCode() == (int)HttpStatusCode.Unauthorized)
                    return true;
                throw;
            }
        }

        public async Task<bool> Login(string userName, string password)
        {
            _client = new Client(new NetworkCredential { UserName = userName, Password = password })
            {
                Server = string.Format("{0}://{1}:{2}", _isHttps ? "https" : "http", _serverAddress, _serverPort),
                BasePath = _serverBasePath
            };
            var files = await _client.List();
            ShareHelper.Instance.Init(userName, password, _client.Server);

            return files != null;
        }

        public async Task<List<ResourceItem>> GetList(string path)
        {
            var list = await _client.List(path);
            return list.Select(ConvertToResourceItem).OrderByDescending(item => item.IsFolder).ToList();
        }

        public async Task<ResourceItem> GetFolder(string path)
        {
            return ConvertToResourceItem(await _client.GetFolder(path));
        }


        public async Task<bool> CreateDir(string remotePath, string name)
        {
            return await _client.CreateDir(remotePath, name);
        }

        public async Task<bool> DeleteItem(ResourceItem item)
        {
            var flag = false;
            if (item.IsFolder)
            {
                await _client.DeleteFolder(item.ItemHref);
                flag = true;
            }
            else
            {
                await _client.DeleteFile(item.ItemHref);
            }

            return flag;
        }


        public async Task<bool> RenameItem(ResourceItem item, string oldHref, string newHref)
        {
            var flag = false;
            if (item.IsFolder)
            {
                flag = await _client.MoveFolder(oldHref, newHref);
            }
            else
            {
                flag = await _client.MoveFile(oldHref, newHref);
            }

            return flag;
        }


        public async Task<bool> MoveItem(ResourceItem item, string oldHref, string newHref)
        {
            var flag = false;
            if (item.IsFolder)
            {
                flag = await _client.MoveFolder(oldHref, newHref);
            }
            else
            {
                flag = await _client.MoveFile(oldHref, newHref);
            }
            return flag;
        }


        public async Task<bool> CopyItem(ResourceItem item, string oldHref, string newHref)
        {
            var flag = false;
            if (item.IsFolder)
            {
                flag = await _client.CopyFolder(oldHref, newHref);
            }
            else
            {
                flag = await _client.CopyFile(oldHref, newHref);
            }
            return flag;
        }

        #region  Upload And Download

        public System.IO.Stream GetWriteStream(long contentLength, string targetAbsolutePath)
        {
            return this._client.GetWriteStream(contentLength, targetAbsolutePath);
        }

        public System.IO.Stream GetReadStream(string resourceHref)
        {
            return this._client.GetReadStream(resourceHref);
        }


        public async Task<System.IO.Stream> GetWriteStreamAsync(long contentLength, string targetAbsolutePath)
        {
            return await this._client.GetWriteStreamAsync(contentLength, targetAbsolutePath);
        }

        public async Task<System.IO.Stream> GetReadStreamAsync(string resourceHref)
        {
            return await this._client.GetReadStreamAsync(resourceHref);
        }

        #endregion


        #region PrivateMethod
        private ResourceItem ConvertToResourceItem(WebDAVClient.Model.Item item)
        {
            return new ResourceItem()
            {
                IsSelected = false,
                ItemName = item.DisplayName,
                ItemSize = item.ContentLength,
                ItemType = item.ContentType,
                ItemHref = item.Href,
                IsFolder = item.ContentType == null,
                ModifyTime = item.LastModified,
                CreateTime = item.CreationDate
            };
        }




        #endregion


    }


}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudDriveShell.Infrastructure.Models;

namespace CloudDriveShell.Infrastructure.Interfaces
{
    public interface IWebDavClientService
    {
        /// <summary>
        /// 测试链接
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="basePath"></param>
        /// <param name="isHttps"></param>
        /// <returns></returns>
        Task<bool> Test(string address, string port, string basePath, bool isHttps = false);

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="basePath"></param>
        /// <param name="isHttps"></param>
        void Init(string address, string port, string basePath, bool isHttps = false);

        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<bool> Login(string userName, string password);

        /// <summary>
        /// 获取目录下列表
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<List<ResourceItem>> GetList(string path);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<ResourceItem> GetFolder(string path);

        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="remotePath"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<bool> CreateDir(string remotePath, string name);
        /// <summary>
        /// 删除内容
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task<bool> DeleteItem(ResourceItem item);

        /// <summary>
        /// 重命名
        /// </summary>
        /// <param name="item"></param>
        /// <param name="oldHref"></param>
        /// <param name="newHref"></param>
        /// <returns></returns>
        Task<bool> RenameItem(ResourceItem item, string oldHref, string newHref);

        /// <summary>
        /// 移动
        /// </summary>
        /// <param name="item"></param>
        /// <param name="oldHref"></param>
        /// <param name="newHref"></param>
        /// <returns></returns>
        Task<bool> MoveItem(ResourceItem item, string oldHref, string newHref);
        /// <summary>
        /// 拷贝
        /// </summary>
        /// <param name="item"></param>
        /// <param name="oldHref"></param>
        /// <param name="newHref"></param>
        /// <returns></returns>
        Task<bool> CopyItem(ResourceItem item, string oldHref, string newHref);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceHref"></param>
        /// <returns></returns>
        Stream GetReadStream(string resourceHref);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentLength"></param>
        /// <param name="targetAbsolutePath"></param>
        /// <returns></returns>
        Stream GetWriteStream(long contentLength, string targetAbsolutePath);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceHref"></param>
        /// <returns></returns>
        Task<Stream> GetReadStreamAsync(string resourceHref);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentLength"></param>
        /// <param name="targetAbsolutePath"></param>
        /// <returns></returns>
        Task<Stream> GetWriteStreamAsync(long contentLength, string targetAbsolutePath);


    }


}

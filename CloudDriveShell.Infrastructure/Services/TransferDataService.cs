using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudDriveShell.Infrastructure.Interfaces;
using CloudDriveShell.Infrastructure.Models;
using CloudDriveShell.Infrastructure.Utils;

namespace CloudDriveShell.Infrastructure.Services
{
    [Export(typeof(ITransferDataService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TransferDataService : ITransferDataService
    {

        private const string TransferLogFileName = "CloudDriveShellTransferInfos.xml";

        public List<TransferInfo> RetriveHistory()
        {
            try
            {
                var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TransferLogFileName);
                if (!File.Exists(fullPath))
                    return new List<TransferInfo>();

                var transferInfos = FileHelper.DeSerializeFromFile(fullPath, typeof(List<TransferInfo>)) as List<TransferInfo> ?? new List<TransferInfo>();

                foreach (var transferInfo in transferInfos)
                {
                    if (transferInfo.StartWorking)
                    {
                        transferInfo.IsFailed = true;
                        transferInfo.StartWorking = false;
                    }
                }

                return transferInfos;
            }
            catch (Exception)
            {
                return new List<TransferInfo>();
            }
        }


        public void SaveHistory(List<TransferInfo> transferInfos)
        {
            try
            {
                var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TransferLogFileName);
                FileHelper.SerializeToFile(transferInfos, fullPath);
            }
            catch { }
        }
    }
}

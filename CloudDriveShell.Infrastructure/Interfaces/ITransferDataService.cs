using System.Collections.Generic;
using CloudDriveShell.Infrastructure.Models;

namespace CloudDriveShell.Infrastructure.Interfaces
{
    public interface ITransferDataService
    {
        List<TransferInfo> RetriveHistory();
        void SaveHistory(List<TransferInfo> transferInfos);
    }
}
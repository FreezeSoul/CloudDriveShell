using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using CloudDriveShell.Infrastructure.Interfaces;
using CloudDriveShell.Infrastructure.Utils;
using Microsoft.Practices.ServiceLocation;
using WebDAVClient.Helpers;

namespace CloudDriveShell.Infrastructure.Models
{
    public class TransferTasker
    {
        static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(5);
        private readonly TransferInfo _transferInfo;

        public TransferTasker(TransferInfo transferInfo)
        {
            this._transferInfo = transferInfo;
            this._transferInfo.SetTasker(this);
        }

        private BackgroundWorker _worker;

        public void Cancel()
        {
            if (this._worker != null)
                this._worker.CancelAsync();
        }

        private Stream GetWriteStream(long fileSize, string taregetPath)
        {
            var webDavClientService = ServiceLocator.Current.GetInstance<IWebDavClientService>();
            return webDavClientService.GetWriteStream(fileSize, taregetPath);
        }

        public void UploadFile(string sourcePath, string targetPath, long fileSize,
            ProgressChangedEventHandler progressChanged = null, RunWorkerCompletedEventHandler runWorkerCompleted = null)
        {
            this._transferInfo.WorkingType = WorkingTypeEnum.Upload;
            this._transferInfo.StartWorking = true;
            Stream destinationStream = null;
            this._worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            this._worker.DoWork += (sender, e) =>
            {
                try
                {
                    this._transferInfo.IsRealyDoing = false;
                    Semaphore.Wait();
                    if (this._worker.CancellationPending)
                    {
                        e.Cancel = true;
                        this._transferInfo.IsFailed = true;
                        this._transferInfo.FailedInfo = "操作被取消!";
                        Debug.WriteLine("BackgroundWorker is canceled!");
                        return;
                    }
                    destinationStream = GetWriteStream(fileSize, targetPath);
                    this._transferInfo.IsRealyDoing = true;
                    int bufSize = 1024; // 1Kb
                    byte[] buffer = new byte[bufSize];
                    long totalBytesRead = 0;
                    DateTime begin = DateTime.Now;
                    FileInfo file = new FileInfo(sourcePath);
                    using (FileStream fileStream = file.OpenRead())
                    {
                        var bytesRead = 0;
                        do
                        {
                            if (this._worker.CancellationPending)
                            {
                                e.Cancel = true;
                                this._transferInfo.IsFailed = true;
                                this._transferInfo.FailedInfo = "操作被取消!";
                                Debug.WriteLine("BackgroundWorker is canceled!");
                                break;
                            }

                            bytesRead = fileStream.Read(buffer, 0, bufSize);
                            if (bytesRead > 0)
                            {
                                try
                                {
                                    destinationStream.Write(buffer, 0, bytesRead);
                                    totalBytesRead += bytesRead;
                                    if (DateTime.Now.Subtract(begin).TotalSeconds >= 1)
                                    {
                                        int progressPercentage = (int)(totalBytesRead * 100 / file.Length);
                                        this._worker.ReportProgress(progressPercentage, totalBytesRead);
                                        begin = DateTime.Now;
                                    }
                                }
                                catch (Exception)
                                {
                                    e.Cancel = true;
                                    Debug.WriteLine("Connection accidentally lost! (" + totalBytesRead + " bytes)");
                                    break;
                                }
                            }
                        } while (bytesRead > 0);
                    }

                    string result = "";
                    if (destinationStream.CanRead)
                    {
                        byte[] buffer2 = new byte[8192];
                        int bytesRead2 = 0;
                        do
                        {
                            bytesRead2 = destinationStream.Read(buffer2, 0, buffer2.Length);
                            if (bytesRead2 > 0)
                            {
                                result += Encoding.UTF8.GetString(buffer2, 0, bytesRead2);
                            }
                        } while (bytesRead2 > 0);
                    }
                    if (result != string.Empty)
                    {
                        Debug.WriteLine(result);
                        WebDAVClient.Client.ParseResponse(result);
                    }
                }
                catch (Exception exception)
                {
                    this._transferInfo.IsFailed = true;
                    this._transferInfo.FailedInfo = exception.Message;
                    e.Cancel = true;
                }
                finally
                {
                    this._transferInfo.StartWorking = false;
                    Semaphore.Release();
                    if (destinationStream != null)
                    {
                        destinationStream.Close();
                        destinationStream.Dispose();
                    }
                }
            };

            this._worker.ProgressChanged += (sender, e) =>
            {
                Debug.WriteLine("ProgressChanged To " + e.UserState);
                this._transferInfo.FinishSize = (long)e.UserState;
                this._transferInfo.TransferTime = DateTime.Now;
                this._transferInfo.ProgressPercentage = e.ProgressPercentage;
                if (progressChanged != null)
                    progressChanged(sender, e);
            };
            this._worker.RunWorkerCompleted += (sender, e) =>
            {
                Debug.WriteLine("BackgroundWorker Finished!");
                if (!e.Cancelled)
                {
                    this._transferInfo.FinishSize = _transferInfo.FileSize;
                    this._transferInfo.ProgressPercentage = 100;
                }
                this._transferInfo.TransferTime = DateTime.Now;
                if (runWorkerCompleted != null)
                    runWorkerCompleted(sender, e);
                this._transferInfo.StartWorking = false;
                this._transferInfo.IsRealyDoing = false;
            };

            this._worker.RunWorkerAsync();
        }

        private Stream GetReadStream(string sourcePath)
        {
            var webDavClientService = ServiceLocator.Current.GetInstance<IWebDavClientService>();
            return webDavClientService.GetReadStream(sourcePath);
        }

        public void DownloadFile(string destinationPath, string sourcePath,
            ProgressChangedEventHandler progressChanged = null, RunWorkerCompletedEventHandler runWorkerCompleted = null)
        {
            this._transferInfo.WorkingType = WorkingTypeEnum.Download;
            this._transferInfo.StartWorking = true;
            Stream sourceStream = null;
            this._worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            this._worker.DoWork += async (sender, e) =>
            {
                try
                {
                    this._transferInfo.IsRealyDoing = false;
                    Semaphore.Wait();
                    if (this._worker.CancellationPending)
                    {
                        e.Cancel = true;
                        this._transferInfo.IsFailed = true;
                        this._transferInfo.FailedInfo = "操作被取消!";
                        Debug.WriteLine("BackgroundWorker is canceled!");
                        return;
                    }
                    sourceStream = GetReadStream(sourcePath);
                    this._transferInfo.IsRealyDoing = true;
                    using (FileStream fileStream = new FileStream(destinationPath, FileMode.Create))
                    {
                        byte[] buffer = new byte[10000];
                        long totalBytesRead = 0;
                        long bytesRead = 0;
                        DateTime begin = DateTime.Now;
                        do
                        {
                            if (this._worker.CancellationPending)
                            {
                                e.Cancel = true;
                                this._transferInfo.IsFailed = true;
                                this._transferInfo.FailedInfo = "操作被取消!";
                                Debug.WriteLine("BackgroundWorker is canceled!");
                                break;
                            }

                            bytesRead = sourceStream.Read(buffer, 0, buffer.Length);
                            if (bytesRead > 0)
                            {
                                totalBytesRead += bytesRead;
                                fileStream.Write(buffer, 0, (int)bytesRead);

                                DateTime now = DateTime.Now;
                                TimeSpan diffTime = now.Subtract(begin);
                                if (diffTime.TotalSeconds >= 1)
                                {
                                    int progressPercentage = (int)(totalBytesRead * 100 / this._transferInfo.FileSize);
                                    this._worker.ReportProgress(progressPercentage, totalBytesRead);
                                    begin = DateTime.Now;
                                }
                            }
                        } while (bytesRead > 0);
                    }
                }
                catch (Exception exception)
                {
                    this._transferInfo.IsFailed = true;
                    this._transferInfo.FailedInfo = exception.Message;
                    e.Cancel = true;
                }
                finally
                {
                    this._transferInfo.StartWorking = false;
                    Semaphore.Release();
                    if (sourceStream != null)
                    {
                        sourceStream.Close();
                        sourceStream.Dispose();
                    }
                }
            };

            this._worker.ProgressChanged += (sender, e) =>
            {
                Debug.WriteLine("ProgressChanged To " + e.UserState);
                this._transferInfo.FinishSize = (long)e.UserState;
                this._transferInfo.TransferTime = DateTime.Now;
                this._transferInfo.ProgressPercentage = e.ProgressPercentage;
                if (progressChanged != null)
                    progressChanged(sender, e);
            };
            this._worker.RunWorkerCompleted += (sender, e) =>
            {
                Debug.WriteLine("BackgroundWorker Finished!");
                if (!e.Cancelled)
                {
                    this._transferInfo.FinishSize = _transferInfo.FileSize;
                    this._transferInfo.ProgressPercentage = 100;
                }
                this._transferInfo.TransferTime = DateTime.Now;
                if (runWorkerCompleted != null)
                    runWorkerCompleted(sender, e);
                this._transferInfo.StartWorking = false;
                this._transferInfo.IsRealyDoing = false;
            };
            this._worker.RunWorkerAsync();
        }


    }
}

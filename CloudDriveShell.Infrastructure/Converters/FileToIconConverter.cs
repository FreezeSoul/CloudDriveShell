using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
using CloudDriveShell.Infrastructure.Utils;

namespace CloudDriveShell.Infrastructure.Converters
{

    [ValueConversion(typeof(string), typeof(ImageSource))]
    public class FileToIconConverter : IMultiValueConverter
    {
        private static string imageFilter = ".jpg,.jpeg,.png,.gif";
        private static string exeFilter = ".exe,.lnk";
        private int defaultsize;

        public int DefaultSize { get { return defaultsize; } set { defaultsize = value; } }

        public enum IconSize
        {
            small, large, extraLarge, jumbo, thumbnail
        }
        private enum FolderType
        {
            /// <summary>
            /// Specify open folder.
            /// </summary>
            Open = 0,

            /// <summary>
            /// Specify closed folder.
            /// </summary>
            Closed = 1
        }


        private class thumbnailInfo
        {
            public IconSize iconsize;
            public WriteableBitmap bitmap;
            public string fullPath;
            public thumbnailInfo(WriteableBitmap b, string path, IconSize size)
            {
                bitmap = b;
                fullPath = path;
                iconsize = size;
            }
        }


        #region Win32api

        public class Shell32
        {
            public const uint BIF_BROWSEFORCOMPUTER = 0x1000;
            public const uint BIF_BROWSEFORPRINTER = 0x2000;
            public const uint BIF_BROWSEINCLUDEFILES = 0x4000;
            public const uint BIF_BROWSEINCLUDEURLS = 0x0080;
            public const uint BIF_DONTGOBELOWDOMAIN = 0x0002;
            public const uint BIF_EDITBOX = 0x0010;
            public const uint BIF_NEWDIALOGSTYLE = 0x0040;
            public const uint BIF_RETURNFSANCESTORS = 0x0008;
            public const uint BIF_RETURNONLYFSDIRS = 0x0001;
            public const uint BIF_SHAREABLE = 0x8000;
            public const uint BIF_STATUSTEXT = 0x0004;
            public const uint BIF_USENEWUI = (BIF_NEWDIALOGSTYLE | BIF_EDITBOX);
            public const uint BIF_VALIDATE = 0x0020;
            public const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
            public const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
            public const int MAX_PATH = 256;
            public const uint SHGFI_ADDOVERLAYS = 0x000000020; // apply the appropriate overlays

            public const uint SHGFI_ATTRIBUTES = 0x000000800; // get attributes
            public const uint SHGFI_ATTR_SPECIFIED = 0x000020000; // get only specified attributes
            public const uint SHGFI_DISPLAYNAME = 0x000000200; // get display name
            public const uint SHGFI_EXETYPE = 0x000002000; // return exe type
            public const uint SHGFI_ICON = 0x000000100; // get icon
            public const uint SHGFI_ICONLOCATION = 0x000001000; // get icon location
            public const uint SHGFI_LARGEICON = 0x000000000; // get large icon
            public const uint SHGFI_LINKOVERLAY = 0x000008000; // put a link overlay on icon
            public const uint SHGFI_OPENICON = 0x000000002; // get open icon
            public const uint SHGFI_OVERLAYINDEX = 0x000000040; // Get the index of the overlay
            public const uint SHGFI_PIDL = 0x000000008; // pszPath is a pidl
            public const uint SHGFI_SELECTED = 0x000010000; // show icon in selected state
            public const uint SHGFI_SHELLICONSIZE = 0x000000004; // get shell size icon
            public const uint SHGFI_SMALLICON = 0x000000001; // get small icon
            public const uint SHGFI_SYSICONINDEX = 0x000004000; // get system icon index
            public const uint SHGFI_TYPENAME = 0x000000400; // get type name
            public const uint SHGFI_USEFILEATTRIBUTES = 0x000000010; // use passed dwFileAttribute

            [DllImport("shell32.dll", CharSet = CharSet.Auto)]
            public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi,
                                                      uint cbFileInfo, uint uFlags);

            [StructLayout(LayoutKind.Sequential)]
            public struct BROWSEINFO
            {
                public IntPtr hwndOwner;
                public IntPtr pidlRoot;
                public IntPtr pszDisplayName;
                [MarshalAs(UnmanagedType.LPTStr)]
                public string lpszTitle;
                public uint ulFlags;
                public IntPtr lpfn;
                public int lParam;
                public IntPtr iImage;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct ITEMIDLIST
            {
                public SHITEMID mkid;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public struct SHFILEINFO
            {
                public IntPtr hIcon;
                public int iIcon;
                public uint dwAttributes;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
                public string szDisplayName;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
                public string szTypeName;
            };

            [StructLayout(LayoutKind.Sequential)]
            public struct SHITEMID
            {
                public ushort cb;
                [MarshalAs(UnmanagedType.LPArray)]
                public byte[] abID;
            }
        }

        /// <summary>
        /// Wraps necessary functions imported from User32.dll. Code courtesy of MSDN Cold Rooster Consulting example.
        /// </summary>
        public class User32
        {
            /// <summary>
            /// Provides access to function required to delete handle. This method is used internally
            /// and is not required to be called separately.
            /// </summary>
            /// <param name="hIcon">Pointer to icon handle.</param>
            /// <returns>N/A</returns>
            [DllImport("User32.dll")]
            public static extern int DestroyIcon(IntPtr hIcon);
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [StructLayout(LayoutKind.Sequential)]
        internal struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        internal const uint SHGFI_ICON = 0x100;
        internal const uint SHGFI_TYPENAME = 0x400;
        internal const uint SHGFI_LARGEICON = 0x0; // 'Large icon
        internal const uint SHGFI_SMALLICON = 0x1; // 'Small icon
        internal const uint SHGFI_SYSICONINDEX = 16384;
        internal const uint SHGFI_USEFILEATTRIBUTES = 16;

        // <summary>
        /// Get Icons that are associated with files.
        /// To use it, use (System.Drawing.Icon myIcon = System.Drawing.Icon.FromHandle(shinfo.hIcon));
        /// hImgSmall = SHGetFileInfo(fName, 0, ref shinfo,(uint)Marshal.SizeOf(shinfo),Win32.SHGFI_ICON |Win32.SHGFI_SMALLICON);
        /// </summary>
        [DllImport("shell32.dll")]
        internal static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes,
                                                  ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        // <summary>
        /// Return large file icon of the specified file.
        /// </summary>
        internal static Icon GetFileIcon(string fileName, IconSize size)
        {
            SHFILEINFO shinfo = new SHFILEINFO();

            uint flags = SHGFI_SYSICONINDEX;
            if (fileName.IndexOf(":") == -1)
                flags = flags | SHGFI_USEFILEATTRIBUTES;
            if (size == IconSize.small)
                flags = flags | SHGFI_ICON | SHGFI_SMALLICON;
            else flags = flags | SHGFI_ICON;

            SHGetFileInfo(fileName, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), flags);
            return Icon.FromHandle(shinfo.hIcon);
        }
        #endregion

        #region Static Tools

        private static void copyBitmap(BitmapSource source, WriteableBitmap target, bool dispatcher)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * ((source.Format.BitsPerPixel + 7) / 8);

            byte[] bits = new byte[height * stride];
            source.CopyPixels(bits, stride, 0);
            source = null;

            //original code.
            //writeBitmap.Dispatcher.Invoke(DispatcherPriority.Background,
            //    new ThreadStart(delegate
            //    {
            //        //UI Thread
            //        Int32Rect outRect = new Int32Rect(0, (int)(writeBitmap.Height - height) / 2, width, height);                    
            //        writeBitmap.WritePixels(outRect, bits, stride, 0);                                        
            //    }));

            //Bugfixes by h32

            if (dispatcher)
            {
                target.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                new ThreadStart(delegate
                {
                    //UI Thread
                    var delta = target.Height - height;
                    var newWidth = width > target.Width ? (int)target.Width : width;
                    var newHeight = height > target.Height ? (int)target.Height : height;
                    Int32Rect outRect = new Int32Rect(0, (int)(delta >= 0 ? delta : 0) / 2, newWidth, newWidth);
                    try
                    {
                        target.WritePixels(outRect, bits, stride, 0);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debugger.Break();
                    }
                }));
            }
            else
            {
                var delta = target.Height - height;
                var newWidth = width > target.Width ? (int)target.Width : width;
                var newHeight = height > target.Height ? (int)target.Height : height;
                Int32Rect outRect = new Int32Rect(0, (int)(delta >= 0 ? delta : 0) / 2, newWidth, newWidth);
                try
                {
                    target.WritePixels(outRect, bits, stride, 0);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debugger.Break();
                }
            }
        }


        private static System.Drawing.Size getDefaultSize(IconSize size)
        {
            switch (size)
            {
                case IconSize.jumbo: return new System.Drawing.Size(256, 256);
                case IconSize.thumbnail: return new System.Drawing.Size(256, 256);
                case IconSize.extraLarge: return new System.Drawing.Size(48, 48);
                case IconSize.large: return new System.Drawing.Size(32, 32);
                default: return new System.Drawing.Size(16, 16);
            }

        }

        //http://blog.paranoidferret.com/?p=11 , modified a little.
        private static Bitmap resizeImage(Bitmap imgToResize, System.Drawing.Size size, int spacing)
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)size.Width / (float)sourceWidth);
            nPercentH = ((float)size.Height / (float)sourceHeight);

            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;

            int destWidth = (int)((sourceWidth * nPercent) - spacing * 4);
            int destHeight = (int)((sourceHeight * nPercent) - spacing * 4);

            int leftOffset = (int)((size.Width - destWidth) / 2);
            int topOffset = (int)((size.Height - destHeight) / 2);


            Bitmap b = new Bitmap(size.Width, size.Height);
            Graphics g = Graphics.FromImage((Image)b);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            g.DrawLines(System.Drawing.Pens.Silver, new PointF[] {
                new PointF(leftOffset - spacing, topOffset + destHeight + spacing), //BottomLeft
                new PointF(leftOffset - spacing, topOffset -spacing),                 //TopLeft
                new PointF(leftOffset + destWidth + spacing, topOffset - spacing)});//TopRight

            g.DrawLines(System.Drawing.Pens.Gray, new PointF[] {
                new PointF(leftOffset + destWidth + spacing, topOffset - spacing),  //TopRight
                new PointF(leftOffset + destWidth + spacing, topOffset + destHeight + spacing), //BottomRight
                new PointF(leftOffset - spacing, topOffset + destHeight + spacing),}); //BottomLeft

            g.DrawImage(imgToResize, leftOffset, topOffset, destWidth, destHeight);
            g.Dispose();

            return b;
        }

        private static Bitmap resizeJumbo(Bitmap imgToResize, System.Drawing.Size size, int spacing)
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)size.Width / (float)sourceWidth);
            nPercentH = ((float)size.Height / (float)sourceHeight);

            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;

            int destWidth = 80;
            int destHeight = 80;

            int leftOffset = (int)((size.Width - destWidth) / 2);
            int topOffset = (int)((size.Height - destHeight) / 2);


            Bitmap b = new Bitmap(size.Width, size.Height);
            Graphics g = Graphics.FromImage((Image)b);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            g.DrawLines(System.Drawing.Pens.Silver, new PointF[] {
                new PointF(0 + spacing, size.Height - spacing), //BottomLeft
                new PointF(0 + spacing, 0 + spacing),                 //TopLeft
                new PointF(size.Width - spacing, 0 + spacing)});//TopRight

            g.DrawLines(System.Drawing.Pens.Gray, new PointF[] {
                new PointF(size.Width - spacing, 0 + spacing),  //TopRight
                new PointF(size.Width - spacing, size.Height - spacing), //BottomRight
                new PointF(0 + spacing, size.Height - spacing)}); //BottomLeft

            g.DrawImage(imgToResize, leftOffset, topOffset, destWidth, destHeight);
            g.Dispose();

            return b;
        }


        private static BitmapSource loadBitmap(Bitmap source)
        {
            IntPtr hBitmap = source.GetHbitmap();
            //Memory Leak fixes, for more info : http://social.msdn.microsoft.com/forums/en-US/wpf/thread/edcf2482-b931-4939-9415-15b3515ddac6/
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty,
                   BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBitmap);
            }

        }

        private static bool isImage(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToLower();
            if (ext == "")
                return false;
            return (imageFilter.IndexOf(ext) != -1 && File.Exists(fileName));
        }

        private static bool isExecutable(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToLower();
            if (ext == "")
                return false;
            return (exeFilter.IndexOf(ext) != -1 && File.Exists(fileName));
        }



        private static bool isFolder(string path)
        {
            return path.EndsWith("\\") || Directory.Exists(path);
        }

        private static string returnKey(string fileName, IconSize size)
        {
            string key = Path.GetExtension(fileName).ToLower();

            if (isExecutable(fileName))
                key = fileName.ToLower();
            if (isImage(fileName) && size == IconSize.thumbnail)
                key = fileName.ToLower();
            if (isFolder(fileName))
                key = fileName.ToLower();

            switch (size)
            {
                case IconSize.thumbnail: key += isImage(fileName) ? "+T" : "+J"; break;
                case IconSize.jumbo: key += "+J"; break;
                case IconSize.extraLarge: key += "+XL"; break;
                case IconSize.large: key += "+L"; break;
                case IconSize.small: key += "+S"; break;
            }
            return key;
        }
        #endregion

        #region Static Cache
        private static Dictionary<string, ImageSource> iconDic = new Dictionary<string, ImageSource>();
        private static SysImageList _imgList = new SysImageList(SysImageListSize.jumbo);

        private Bitmap loadJumbo(string lookup)
        {
            _imgList.ImageListSize = isVistaUp() ? SysImageListSize.jumbo : SysImageListSize.extraLargeIcons;
            Icon icon = _imgList.Icon(_imgList.IconIndex(lookup, isFolder(lookup)));
            Bitmap bitmap = icon.ToBitmap();
            icon.Dispose();

            System.Drawing.Color empty = System.Drawing.Color.FromArgb(0, 0, 0, 0);

            if (bitmap.Width < 256)
                bitmap = resizeImage(bitmap, new System.Drawing.Size(256, 256), 0);
            else
                if (bitmap.GetPixel(100, 100) == empty && bitmap.GetPixel(200, 200) == empty && bitmap.GetPixel(200, 200) == empty)
                {
                    _imgList.ImageListSize = SysImageListSize.largeIcons;
                    bitmap = resizeJumbo(_imgList.Icon(_imgList.IconIndex(lookup)).ToBitmap(), new System.Drawing.Size(200, 200), 5);
                }

            return bitmap;
        }

        #endregion

        #region Instance Cache
        private static Dictionary<string, ImageSource> thumbDic = new Dictionary<string, ImageSource>();

        public void ClearInstanceCache()
        {
            thumbDic.Clear();
            //System.GC.Collect();
        }


        private void PollIconCallback(object state)
        {


            thumbnailInfo input = state as thumbnailInfo;
            string fileName = input.fullPath;
            WriteableBitmap writeBitmap = input.bitmap;
            IconSize size = input.iconsize;

            Bitmap origBitmap = GetFileIcon(fileName, size).ToBitmap();
            Bitmap inputBitmap = origBitmap;
            if (size == IconSize.jumbo || size == IconSize.thumbnail)
                inputBitmap = resizeJumbo(origBitmap, getDefaultSize(size), 5);
            else inputBitmap = resizeImage(origBitmap, getDefaultSize(size), 0);

            BitmapSource inputBitmapSource = loadBitmap(inputBitmap);
            origBitmap.Dispose();
            inputBitmap.Dispose();

            copyBitmap(inputBitmapSource, writeBitmap, true);
        }

        private void PollThumbnailCallback(object state)
        {
            //Non UIThread
            thumbnailInfo input = state as thumbnailInfo;
            string fileName = input.fullPath;
            WriteableBitmap writeBitmap = input.bitmap;
            IconSize size = input.iconsize;

            try
            {
                Bitmap origBitmap = new Bitmap(fileName);
                Bitmap inputBitmap = resizeImage(origBitmap, getDefaultSize(size), 5);
                BitmapSource inputBitmapSource = loadBitmap(inputBitmap);
                origBitmap.Dispose();
                inputBitmap.Dispose();

                copyBitmap(inputBitmapSource, writeBitmap, true);
            }
            catch { }

        }

        private ImageSource addToDic(string fileName, IconSize size)
        {
            string key = returnKey(fileName, size);

            if (size == IconSize.thumbnail || isExecutable(fileName))
            {
                if (!thumbDic.ContainsKey(key))
                    lock (thumbDic)
                        thumbDic.Add(key, getImage(fileName, size));

                return thumbDic[key];
            }
            else
            {
                if (!iconDic.ContainsKey(key))
                    lock (iconDic)
                        iconDic.Add(key, getImage(fileName, size));
                return iconDic[key];
            }

        }

        public ImageSource GetImage(string fileName, int iconSize)
        {
            IconSize size;

            if (iconSize <= 16) size = IconSize.small;
            else if (iconSize <= 32) size = IconSize.large;
            else if (iconSize <= 48) size = IconSize.extraLarge;
            else if (iconSize <= 72) size = IconSize.jumbo;
            else size = IconSize.thumbnail;

            return addToDic(fileName, size);
        }

        #endregion

        #region Instance Tools

        public static bool isVistaUp()
        {
            return (Environment.OSVersion.Version.Major >= 6);
        }

        private BitmapSource getImage(string fileName, IconSize size)
        {
            Icon icon;
            string key = returnKey(fileName, size);
            string lookup = "aaa" + Path.GetExtension(fileName).ToLower();
            if (!key.StartsWith("."))
                lookup = fileName;

            if (isExecutable(fileName))
            {

                WriteableBitmap bitmap = new WriteableBitmap(addToDic("aaa.exe", size) as BitmapSource);
                ThreadPool.QueueUserWorkItem(new WaitCallback(PollIconCallback), new thumbnailInfo(bitmap, fileName, size));
                return bitmap;
            }

            else
                switch (size)
                {
                    case IconSize.thumbnail:
                        if (isImage(fileName))
                        {
                            //Load as jumbo icon first.                         
                            WriteableBitmap bitmap = new WriteableBitmap(addToDic(fileName, IconSize.jumbo) as BitmapSource);
                            //BitmapSource bitmapSource = addToDic(fileName, IconSize.jumbo) as BitmapSource;                            
                            //WriteableBitmap bitmap = new WriteableBitmap(256, 256, 96, 96, PixelFormats.Bgra32, null);
                            //copyBitmap(bitmapSource, bitmap, false);
                            ThreadPool.QueueUserWorkItem(new WaitCallback(PollThumbnailCallback), new thumbnailInfo(bitmap, fileName, size));
                            return bitmap;
                        }
                        else
                        {
                            return getImage(lookup, IconSize.jumbo);
                        }
                    case IconSize.jumbo:
                        return loadBitmap(loadJumbo(lookup));
                    case IconSize.extraLarge:
                        _imgList.ImageListSize = SysImageListSize.extraLargeIcons;
                        icon = _imgList.Icon(_imgList.IconIndex(lookup, isFolder(fileName)));
                        return loadBitmap(icon.ToBitmap());
                    default:
                        icon = GetFileIcon(lookup, size);
                        return loadBitmap(icon.ToBitmap());
                }
        }














        #endregion


        public FileToIconConverter()
        {
            this.defaultsize = 48;
        }


        #region IMultiValueConverter Members
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int size = defaultsize;

            bool isFolder = false;

            if (values.Length > 1 && values[1] is bool)
                isFolder = (bool)values[1];

            if (values.Length > 2 && values[2] is double)
                size = (int)(float)(double)values[2];

            if (isFolder)
            {
                return GetFolderImage();
            }
            else
            {
                if (values[0] is string)
                    return GetImage(values[0] as string, size);
                else return GetImage("", size);
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private ImageSource folderCache = null;

        private ImageSource GetFolderImage()
        {
            if (folderCache == null)
                folderCache = loadBitmap(GetFolderIcon().ToBitmap());
            return folderCache;
        }

        private Icon GetFolderIcon(IconSize size = IconSize.small, FolderType folderType = FolderType.Closed)
        {
            // Need to add size check, although errors generated at present!
            uint flags = Shell32.SHGFI_ICON | Shell32.SHGFI_USEFILEATTRIBUTES;

            if (FolderType.Open == folderType)
            {
                flags += Shell32.SHGFI_OPENICON;
            }

            if (IconSize.small == size)
            {
                flags += Shell32.SHGFI_SMALLICON;
            }
            else
            {
                flags += Shell32.SHGFI_LARGEICON;
            }

            // Get the folder icon
            Shell32.SHFILEINFO shfi = new Shell32.SHFILEINFO();
            IntPtr hSuccess = Shell32.SHGetFileInfo("_unknown", Shell32.FILE_ATTRIBUTE_DIRECTORY, ref shfi,
                (uint)Marshal.SizeOf(shfi), flags);
            if (hSuccess != IntPtr.Zero)
            {
                Icon.FromHandle(shfi.hIcon); // Load the icon from an HICON handle

                // Now clone the icon, so that it can be successfully stored in an ImageList
                Icon icon = (Icon)Icon.FromHandle(shfi.hIcon).Clone();

                User32.DestroyIcon(shfi.hIcon); // Cleanup
                return icon;
            }
            return null;
        }

        #endregion

    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Planetarium.Controls
{
    public class FoldersTreeView : TreeView
    {
        private string selectedImagePath = null;
        public string SelectedImagePath
        {
            get
            {
                return selectedImagePath;
            }
            set
            {
                if (Directory.Exists(value))
                {
                    var paths = SubPaths(value);

                    TreeViewItem currentItem = this.Items[0] as TreeViewItem;

                    bool needSelect = true;

                    foreach (var path in paths)
                    {
                        currentItem.IsExpanded = true;
                        var childItem = currentItem.Items.OfType<TreeViewItem>().FirstOrDefault(i => path.Equals((i.Tag as FolderInfo)?.Path, StringComparison.OrdinalIgnoreCase));

                        if (childItem != null)
                        {
                            currentItem = childItem;
                        }
                        else
                        {
                            needSelect = false;
                        }
                    }

                    if (needSelect)
                    {
                        currentItem.IsSelected = true;
                        currentItem.BringIntoView();
                        this.Focus();

                        selectedImagePath = value;
                        // TODO: notify property changed
                    }
                }
            }
        }

        private string[] SubPaths(string path)
        {
            try
            {

                var paths = new List<string>() { path };

                do
                {
                    path = Directory.GetParent(path)?.FullName;
                    if (path != null)
                    {
                        paths.Insert(0, path);
                    }
                }
                while (path != null);

                return paths.ToArray();
            }
            catch
            {
                return new string[0];
            }
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            //System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            //dlg.ShowDialog();

            var myComputer = CreateItem(new FolderInfo() { Title = Environment.MachineName, Icon = GetMyComputerIcon() });

            myComputer.Items.Add(CreateItem(new FolderInfo(Environment.SpecialFolder.Desktop)));
            myComputer.Items.Add(CreateItem(new FolderInfo(Environment.SpecialFolder.Recent)));
            myComputer.Items.Add(CreateItem(new FolderInfo(Environment.SpecialFolder.Favorites)));
            myComputer.Items.Add(CreateItem(new FolderInfo(Environment.SpecialFolder.MyDocuments)));
            myComputer.Items.Add(CreateItem(new FolderInfo(Environment.SpecialFolder.MyMusic)));
            myComputer.Items.Add(CreateItem(new FolderInfo(Environment.SpecialFolder.MyPictures)));
            myComputer.Items.Add(CreateItem(new FolderInfo(Environment.SpecialFolder.MyVideos)));

            foreach (string s in Directory.GetLogicalDrives())
            {
                myComputer.Items.Add(CreateItem(new FolderInfo() { Icon = GetIcon(s), Path = s, Title = s }));
            }

            this.Items.Add(myComputer);
            myComputer.IsExpanded = true;
        }

        void folder_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            if (item.Items.Count == 1 && item.Items[0] == null)
            {
                item.Items.Clear();
                try
                {
                    var folderInfo = item.Tag as FolderInfo;
                    foreach (string s in Directory.GetDirectories(folderInfo.Path))
                    {
                        item.Items.Add(CreateItem(new FolderInfo() { Title = Path.GetFileName(s), Path = s, Icon = GetIcon(s) }));
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        private TreeViewItem CreateItem(FolderInfo folderInfo)
        {
            TreeViewItem subitem = new TreeViewItem();

            subitem.Header = folderInfo.Title;
            subitem.Tag = folderInfo;
            //subitem.FontWeight = FontWeights.Normal;

            if (folderInfo.Path != null)
            {
                subitem.Items.Add(null);
            }
            subitem.Expanded += new RoutedEventHandler(folder_Expanded);

            return subitem;
        }


        protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            base.OnSelectedItemChanged(e);
 
            TreeViewItem temp = (TreeViewItem)this.SelectedItem;

            var folderInfo = temp.Tag as FolderInfo;
            selectedImagePath = folderInfo.Path;
            // TODO: notify property changed

        }

        const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
        const uint SHGFI_DISPLAYNAME = 0x000000200;
        const uint SHGFI_ICON = 0x100;
        const uint SHGFI_PIDL = 0x000000008;
        const uint SHGFI_SMALLICON = 0x1;
        const uint CSIDL_DRIVES = 0x0011;

        [StructLayout(LayoutKind.Sequential)]
        struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            //public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        [DllImport("shell32")]
        static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint flags);

        [DllImport("shell32")]
        static extern IntPtr SHGetFileInfo(IntPtr pidl, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint flags);

        [DllImport("shell32", SetLastError = true)]
        static extern int SHGetSpecialFolderLocation(IntPtr hwndOwner, uint nFolder, ref IntPtr ppidl);

        [DllImport("user32")]
        static extern int DestroyIcon(IntPtr hIcon);

        static string GetDisplayName(string path)
        {
            SHFILEINFO shfi = new SHFILEINFO();
            if (IntPtr.Zero != SHGetFileInfo(path, FILE_ATTRIBUTE_NORMAL, ref shfi, (uint)Marshal.SizeOf(typeof(SHFILEINFO)), SHGFI_DISPLAYNAME))
            {
                return shfi.szDisplayName;
            }
            else
            {
                return Path.GetFileName(path);
            }
        }

        private static ImageSource GetIcon(string fileName)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            SHGetFileInfo(fileName, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_SMALLICON);

            Icon icon = (Icon)System.Drawing.Icon.FromHandle(shinfo.hIcon).Clone();
            DestroyIcon(shinfo.hIcon);
            return ToImageSource(icon);
        }

        private static ImageSource GetMyComputerIcon()
        {
            IntPtr pidl = IntPtr.Zero;
            SHGetSpecialFolderLocation(IntPtr.Zero, CSIDL_DRIVES, ref pidl);

            SHFILEINFO shinfo = new SHFILEINFO();
            SHGetFileInfo(pidl, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_PIDL | SHGFI_ICON | SHGFI_SMALLICON);

            Icon icon = (Icon)System.Drawing.Icon.FromHandle(shinfo.hIcon).Clone();
            DestroyIcon(shinfo.hIcon);
            return ToImageSource(icon);
        }

        static ImageSource ToImageSource(Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }

        class FolderInfo
        {
            public string Title { get; set; }
            public string Path { get; set; }
            public ImageSource Icon { get; set; }

            public FolderInfo()
            {

            }

            public FolderInfo(Environment.SpecialFolder folder)
            {
                Path = Environment.GetFolderPath(folder);
                Title = GetDisplayName(Path);
                Icon = GetIcon(Path);
            }
        }
    }
}

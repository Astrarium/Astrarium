using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Astrarium.Controls
{
    public class FoldersTreeView : TreeView
    {
        public readonly static DependencyProperty SelectedPathProperty = DependencyProperty.Register(
            nameof(SelectedPath), 
            typeof(string), 
            typeof(FoldersTreeView),
            new FrameworkPropertyMetadata(null, (d, e) => 
            {
                TreeView treeView = d as TreeView;
                string newValue = e.NewValue as string;
                string oldValue = e.OldValue as string;

                if (newValue != oldValue && ValidatePath(newValue))
                {
                    SelectPath(treeView, newValue);
                }
            })
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                AffectsRender = true
            });

        private static bool ValidatePath(object value)
        {
            string path = value as string;
            if (string.IsNullOrEmpty(path))
            {
                return true;
            }
            else
            {
                try
                {
                    return Directory.Exists(path);
                }
                catch
                {
                    return false;
                }
            }
        }

        private static void SelectPath(TreeView treeView, string value)
        {
            if (value == null)
            {
                TreeViewItem item = treeView.SelectedItem as TreeViewItem;
                if (item != null)
                {
                    item.IsSelected = false;
                }
            }
            else
            {
                value = NormalizePath(value);

                var paths = SubPaths(value);
                FoldersTreeViewItem currentItem = treeView.Items[0] as FoldersTreeViewItem;

                bool needSelect = true;

                foreach (var path in paths)
                {
                    currentItem.IsExpanded = true;
                    var childItem = currentItem.Items.OfType<FoldersTreeViewItem>().FirstOrDefault(i => path.Equals((i.Tag as FolderInfo)?.Path, StringComparison.OrdinalIgnoreCase));

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
                    //currentItem.IsExpanded = true;
                    currentItem.IsSelected = true;

                    MethodInfo selectMethod = typeof(TreeViewItem).GetMethod("Select", BindingFlags.NonPublic | BindingFlags.Instance);

                    selectMethod.Invoke(currentItem, new object[] { true });

                    currentItem.BringIntoView();
                    //treeView.Focus();

                    // TODO: notify property changed
                }
            }
        }

        private static string NormalizePath(string path)
        {
            if (Directory.GetParent(path) == null)
            {
                return path;
            }
            else
            {
                return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
        }

        public string SelectedPath
        {
            get
            {
                string value = (string)GetValue(SelectedPathProperty);
                if (ValidatePath(value))
                {
                    return value;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (ValidatePath(value))
                {
                    SetValue(SelectedPathProperty, value);
                }
            }
        }

        private static string[] SubPaths(string path)
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

        public FoldersTreeView()
        {
            var myComputer = CreateItem(new FolderInfo() { Title = Environment.MachineName, Icon = GetMyComputerIcon() });

            /*
            myComputer.Items.Add(CreateItem(new FolderInfo(Environment.SpecialFolder.Desktop)));
            myComputer.Items.Add(CreateItem(new FolderInfo(Environment.SpecialFolder.Recent)));
            myComputer.Items.Add(CreateItem(new FolderInfo(Environment.SpecialFolder.Favorites)));
            myComputer.Items.Add(CreateItem(new FolderInfo(Environment.SpecialFolder.MyDocuments)));
            myComputer.Items.Add(CreateItem(new FolderInfo(Environment.SpecialFolder.MyMusic)));
            myComputer.Items.Add(CreateItem(new FolderInfo(Environment.SpecialFolder.MyPictures)));
            myComputer.Items.Add(CreateItem(new FolderInfo(Environment.SpecialFolder.MyVideos)));
            */

            foreach (string s in Directory.GetLogicalDrives())
            {
                myComputer.Items.Add(CreateItem(new FolderInfo() { Icon = GetIcon(s), Path = s, Title = s }));
            }

            this.Items.Add(myComputer);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            (this.Items[0] as TreeViewItem).IsExpanded = true;
            Background = System.Windows.Media.Brushes.Transparent;
            BorderThickness = new Thickness(0);

            if (ValidatePath(SelectedPath))
            {
                SelectPath(this, SelectedPath);
            }
        }

        void folder_Expanded(object sender, RoutedEventArgs e)
        {
            FoldersTreeViewItem item = (FoldersTreeViewItem)sender;
            if (item.Items.Count == 1 && item.Items[0] == null)
            {
                item.Items.Clear();
                try
                {
                    var folderInfo = item.Tag as FolderInfo;
                    foreach (string s in GetDirectories(folderInfo.Path))
                    {                        
                        item.Items.Add(CreateItem(new FolderInfo() { Title = Path.GetFileName(s), Path = s, Icon = GetIcon(s) }));
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        private string[] GetDirectories(string dir)
        {
            return new DirectoryInfo(dir).GetDirectories()
                //.Where(f => !f.Attributes.HasFlag(FileAttributes.System | FileAttributes.Hidden))
                .Select(f => f.FullName)
                .ToArray();
        }

        private FoldersTreeViewItem CreateItem(FolderInfo folderInfo)
        {
            var item = new FoldersTreeViewItem();

            item.Header = folderInfo.Title;
            item.Tag = folderInfo;
            item.Expanded += new RoutedEventHandler(folder_Expanded);

            if (folderInfo.Path != null)
            {
                item.Items.Add(null);
            }
            
            return item;
        }

        protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            base.OnSelectedItemChanged(e);

            FoldersTreeViewItem temp = (FoldersTreeViewItem)this.SelectedItem;

            if (temp != null)
            {

                var folderInfo = temp.Tag as FolderInfo;


                SelectedPath = folderInfo.Path;
            }
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
                return string.IsNullOrWhiteSpace(shfi.szDisplayName) ? Path.GetFileName(path) : shfi.szDisplayName;
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

            Icon icon = (Icon)Icon.FromHandle(shinfo.hIcon).Clone();
            DestroyIcon(shinfo.hIcon);
            return ToImageSource(icon);
        }

        private static ImageSource GetMyComputerIcon()
        {
            IntPtr pidl = IntPtr.Zero;
            SHGetSpecialFolderLocation(IntPtr.Zero, CSIDL_DRIVES, ref pidl);

            SHFILEINFO shinfo = new SHFILEINFO();
            SHGetFileInfo(pidl, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_PIDL | SHGFI_ICON | SHGFI_SMALLICON);

            Icon icon = (Icon)Icon.FromHandle(shinfo.hIcon).Clone();
            DestroyIcon(shinfo.hIcon);
            return ToImageSource(icon);
        }

        private static ImageSource ToImageSource(Icon icon)
        {
             return Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
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

    public class FoldersTreeViewItem : TreeViewItem
    {

    }
}

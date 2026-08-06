using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using MColor = System.Windows.Media.Color;
using DColor = System.Drawing.Color;


namespace KeyNStroke
{
    class UIHelper
    {
        #region Helper to convert Drawing.Color to Windows.Media.Color
        public static MColor ToMediaColor(DColor color)
        {
            return MColor.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static DColor ToDrawingColor(MColor color)
        {
            return DColor.FromArgb(color.A, color.R, color.G, color.B);
        }
        #endregion

        /// <summary>
        /// Finds a Child of a given item in the visual tree. 
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, 
        /// a null parent is being returned.</returns>
        public static T FindChild<T>(DependencyObject parent, string childName)
           where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            if (parent is FrameworkElement)
                ((FrameworkElement)parent).ApplyTemplate();

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }
    }

    public static class VistaFolderBrowser
    {
        public static string ShowDialog(IntPtr ownerHandle, string title, string initialPath = null)
        {
            try
            {
                IFileOpenDialog dialog = (IFileOpenDialog)new FileOpenDialogRCW();
                if (!string.IsNullOrEmpty(title))
                {
                    dialog.SetTitle(title);
                }

                uint options;
                dialog.GetOptions(out options);
                dialog.SetOptions(options | FOS_PICKFOLDERS | FOS_FORCEFILESYSTEM);

                if (!string.IsNullOrEmpty(initialPath) && System.IO.Directory.Exists(initialPath))
                {
                    IShellItem item;
                    SHCreateItemFromParsingName(initialPath, IntPtr.Zero, ref IID_IShellItem, out item);
                    if (item != null)
                    {
                        dialog.SetFolder(item);
                    }
                }

                if (dialog.Show(ownerHandle) == 0)
                {
                    IShellItem resultItem;
                    dialog.GetResult(out resultItem);
                    if (resultItem != null)
                    {
                        string path;
                        resultItem.GetDisplayName(SIGDN_FILESYSPATH, out path);
                        return path;
                    }
                }
            }
            catch
            {
                using (var dlg = new System.Windows.Forms.FolderBrowserDialog())
                {
                    dlg.Description = title;
                    if (!string.IsNullOrEmpty(initialPath))
                    {
                        dlg.SelectedPath = initialPath;
                    }
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        return dlg.SelectedPath;
                    }
                }
            }
            return null;
        }

        private const uint FOS_PICKFOLDERS = 0x00000020;
        private const uint FOS_FORCEFILESYSTEM = 0x00000050;
        private const uint SIGDN_FILESYSPATH = 0x80058000;
        private static Guid IID_IShellItem = new Guid("4382691e-e718-42ee-bc55-a1e261c37bfe");

        [System.Runtime.InteropServices.ComImport]
        [System.Runtime.InteropServices.Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7")]
        private class FileOpenDialogRCW { }

        [System.Runtime.InteropServices.ComImport]
        [System.Runtime.InteropServices.Guid("42450421-9507-4861-B65C-57841B369A37")]
        [System.Runtime.InteropServices.InterfaceType(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
        private interface IFileOpenDialog
        {
            [System.Runtime.InteropServices.PreserveSig] int Show(IntPtr parent);
            void SetFileTypes();
            void SetFileTypeIndex();
            void GetFileTypeIndex();
            void Advise();
            void Unadvise();
            void SetOptions(uint dwOptions);
            void GetOptions(out uint pdwOptions);
            void SetDefaultFolder(IShellItem psi);
            void SetFolder(IShellItem psi);
            void GetFolder(out IShellItem ppsi);
            void GetCurrentSelection(out IShellItem ppsi);
            void SetFileName([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string pszName);
            void GetFileName([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] out string pszName);
            void SetTitle([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string pszTitle);
            void SetOkButtonLabel();
            void SetFileNameLabel();
            void GetResult(out IShellItem ppsi);
            void AddPlace();
            void SetDefaultExtension();
            void Close();
            void SetClientGuid();
            void ClearClientData();
            void SetFilter();
            void GetResults();
            void GetSelectedItems();
        }

        [System.Runtime.InteropServices.ComImport]
        [System.Runtime.InteropServices.Guid("4382691e-e718-42ee-bc55-a1e261c37bfe")]
        [System.Runtime.InteropServices.InterfaceType(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItem
        {
            void BindToHandler();
            void GetParent();
            void GetDisplayName(uint sigdnName, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] out string ppszName);
            void GetAttributes();
            void Compare();
        }

        [System.Runtime.InteropServices.DllImport("shell32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
        private static extern int SHCreateItemFromParsingName(
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string pszPath,
            IntPtr pbc,
            ref Guid riid,
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Interface)] out IShellItem ppsi);
    }
}

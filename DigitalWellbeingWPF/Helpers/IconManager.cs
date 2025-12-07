using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls; // Import for TextBlock
using System.Windows.Interop;
using System.Windows.Media;  // Import for Brush
using System.Windows.Media.Imaging;
using static System.Environment;
using DigitalWellbeingWPF.Helpers;
using DigitalWellbeing.Core;
using System.Runtime.InteropServices; // For DllImport and StructLayout

namespace DigitalWellbeingWPF.Helpers
{
    public static class IconManager
    {
        // --- ADDED: P/Invoke declarations ---
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("kernel32.dll")]
        static extern uint GetLastError();

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0;    // 'Large icon
        private const uint SHGFI_SMALLICON = 0x1;    // 'Small icon
        private const uint SHGFI_USEFILEATTRIBUTES = 0x10;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x80;

        // --- CHANGED: Now stores a FrameworkElement (for XAML-based default icon) ---
        private static FrameworkElement _defaultIcon = null;

        public static BitmapSource GetIconSource(string appName)
        {
            CreateAppDirectories();

            // Try to get cached image, and return if successful
            BitmapSource cachedImage = GetCachedImage(appName);
            if (cachedImage != null)
            {
                AppLogger.WriteLine($"Using cached icon for {appName}"); // Log cache hit
                return cachedImage;
            }

            // --- ADDED: try-catch block for robust icon extraction ---
            try
            {
                Process[] processes = Process.GetProcessesByName(appName);

                if (processes.Length > 0)
                {
                    string filePath = processes[0].MainModule.FileName;
                    // --- ADDED: Log the file path ---
                    Debug.WriteLine($"Extracting icon for: {filePath}");
                    AppLogger.WriteLine($"Extracting icon for: {filePath}");

                    Icon icon = GetIcon(filePath); // Use our new GetIcon method.

                    if (icon != null)
                    {
                        // --- CHANGED: Return directly BitmapSource
                        BitmapSource iconBitMap = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        icon.Dispose();
                        CacheImage(iconBitMap, appName);
                        AppLogger.WriteLine($"Successfully extracted icon for {appName}"); // Log success
                        return iconBitMap;
                    }
                    else
                    {
                        AppLogger.WriteLine($"GetIcon returned null for {appName}"); // Log null icon
                    }
                }
                else
                {
                    AppLogger.WriteLine($"No processes found for {appName}"); //Log if the process name isnt found
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLine($"ICON - FAILED to extract or cache: {ex}");
                // --- ADDED: Return default icon on failure ---
                AppLogger.WriteLine($"Returning default icon (extraction failed) for {appName}"); //Log when using default.
                return GetDefaultIcon(); // Still returns a BitmapSource.
            }

            // --- ADDED: Return default icon if no processes found ---
            AppLogger.WriteLine($"Returning default icon (no processes) for {appName}"); // Log default return
            return GetDefaultIcon(); // Still returns a BitmapSource.
        }

        // --- ADDED: GetIcon method using SHGetFileInfo ---
        private static Icon GetIcon(string filePath)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            IntPtr hImgSmall = SHGetFileInfo(filePath, FILE_ATTRIBUTE_NORMAL, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_SMALLICON | SHGFI_USEFILEATTRIBUTES);

            if (hImgSmall == IntPtr.Zero)
            {
                // --- ADDED: Get and log the last error code ---
                uint errorCode = GetLastError();
                AppLogger.WriteLine($"SHGetFileInfo failed for {filePath}. Error Code: {errorCode}", true);
                return null; // Return null if icon extraction fails
            }

            // The icon is returned in the hIcon member of shinfo
            Icon icon = (Icon)Icon.FromHandle(shinfo.hIcon).Clone(); // Clone to prevent issues with DestroyIcon
            DestroyIcon(shinfo.hIcon); // *MUST* destroy the icon handle to prevent memory leaks!
            return icon;
        }
        public static BitmapSource GetDefaultIcon()
        {
            AppLogger.WriteLine("GetDefaultIcon called."); // Log that we entered the method

            if (_defaultIcon == null)
            {
                AppLogger.WriteLine("Default icon is null. Creating..."); // Log creation
                try
                {
                    // Create a TextBlock with the question mark character.
                    TextBlock textBlock = new TextBlock();
                    textBlock.FontFamily = new System.Windows.Media.FontFamily("Segoe MDL2 Assets"); // Fully qualified name
                    textBlock.Text = "\xE783"; // Unicode character for a question mark in Segoe MDL2 Assets.
                    textBlock.FontSize = 16; // Adjust size as needed.
                    textBlock.Foreground = System.Windows.Media.Brushes.Gray; // Fully qualified name
                    textBlock.Width = 16;
                    textBlock.Height = 16;

                    // --- IMPORTANT: Arrange the TextBlock ---
                    textBlock.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity)); // Fully qualified name
                    textBlock.Arrange(new Rect(textBlock.DesiredSize));

                    // Render the TextBlock to a BitmapSource
                    RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
                        (int)textBlock.ActualWidth, (int)textBlock.ActualHeight,
                        96, 96, PixelFormats.Pbgra32);
                    renderTargetBitmap.Render(textBlock);
                    _defaultIcon = textBlock; // Store for later.

                    AppLogger.WriteLine("Default icon created successfully."); // Log success
                    return renderTargetBitmap; // Return it.


                }
                catch (Exception ex)
                {
                    // Handle the case where the default icon can't be loaded (critical error).
                    AppLogger.WriteLine($"CRITICAL ERROR: Failed to load default icon: {ex}");
                    return null; //Very bad, but at least doesn't crash.
                }
            }
            AppLogger.WriteLine("Returning cached default icon."); // Log cache hit
            return ConvertToBitmapSource(_defaultIcon);
        }


        private static void CreateAppDirectories()
        {
            Directory.CreateDirectory(ApplicationPath.GetImageCacheLocation());
        }

        private static void CacheImage(BitmapSource icon, string appName)
        {
            try
            {
                // Ensure directory exists
                if (!Directory.Exists(ApplicationPath.GetImageCacheLocation()))
                {
                    Directory.CreateDirectory(ApplicationPath.GetImageCacheLocation());
                }

                // Get path for saving
                string filePath = Path.Combine(ApplicationPath.GetImageCacheLocation(), $"{appName}.png"); // Save as PNG.

                // Create a new encoder
                PngBitmapEncoder encoder = new PngBitmapEncoder(); // Use Png for wider compatibility
                encoder.Frames.Add(BitmapFrame.Create(icon));

                // Save the icon
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    encoder.Save(stream);
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLine($"CACHE - FAILED: {ex}");
            }
        }

        private static BitmapImage GetCachedImage(string appName)
        {
            try
            {
                // --- CHANGED:  Load as PNG ---
                string filePath = Path.Combine(ApplicationPath.GetImageCacheLocation(), $"{appName}.png");
                if (File.Exists(filePath))
                {
                    BitmapImage img = new BitmapImage();
                    img.BeginInit();
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.UriSource = new Uri(filePath); // Load from the file path
                    img.EndInit();
                    return img;
                }
                return null;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLine($"CACHE - NOT FOUND: {ex}");
                return null;
            }
        }
        private static BitmapSource ConvertToBitmapSource(FrameworkElement element)
        {
            // Render the element to a bitmap
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)element.ActualWidth, (int)element.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(element);
            return bitmap;
        }
        public static bool ClearCachedImages()
        {
            return StorageManager.TryDeleteFolder(ApplicationPath.GetImageCacheLocation());
        }
    }
}
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;

namespace IconConverter.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "图标转换器";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
        private string _path;
        public string Path { get=>_path; set { SetProperty(ref _path, value); } }
        private List<Size> _sizeList = new List<Size>()
        {
            new Size(){Width=32},
            new Size(){Width=64},
            new Size(){Width=128},
            new Size(){Width=256},
        };
        public List<Size> Sizes { get=>_sizeList; set { SetProperty(ref _sizeList,value); } }
        private DelegateCommand _generatIconCommand;
        public DelegateCommand GeneratIconCommand =>
            _generatIconCommand ?? (_generatIconCommand = new DelegateCommand(ExecuteGeneratIconCommand));
        private DelegateCommand _browseFileCommand;
        public DelegateCommand BrowseFileCommand =>
            _browseFileCommand ?? (_browseFileCommand = new DelegateCommand(ExecuteBrowseFileCommand));

        void ExecuteBrowseFileCommand()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "SVG|*.svg|JPGE|*.jpg|PNG|*.png";
            if(openFileDialog.ShowDialog().Value)
            {
                Path = openFileDialog.FileName;
            }
        }
        void ExecuteGeneratIconCommand()
        {
            if(string.IsNullOrEmpty(Path))
            {
                MessageBox.Show("没有选择文件！");
                return;
            }
            if(SelectedSize==null)
            {
                MessageBox.Show("没有选择尺寸！");
                return;
            }
            string savePath =$"logo{SelectedSize.Width}_{DateTime.Now.ToString("HHmmssff")}";
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = savePath;
            saveFileDialog.Filter = "图标(.ico)|*.ico";
            if (saveFileDialog.ShowDialog().Value)
            {
                savePath= saveFileDialog.FileName;
            }
            else
            {
                return;
            }
            FileInfo file = new FileInfo(Path);
            if(file.Extension==".svg")
            {
                using (Bitmap map = new Bitmap(SelectedSize.Width, SelectedSize.Width))
                {
                    using (Graphics g = Graphics.FromImage(map))
                    {
                        //g.Clear(Color.Transparent);
                        string str = File.ReadAllText(Path);
                        SvgDocument doc = SvgDocument.FromSvg<SvgDocument>(str);
                        ISvgRenderer renderer = SvgRenderer.FromGraphics(g);
                        doc.Width = SelectedSize.Width;
                        doc.Height = SelectedSize.Width;
                        doc.Draw(renderer);
                    }
                    ConvertImageToIcon(map,savePath);
                }
                MessageBox.Show("导出完成！");
            }
            else if(file.Extension==".png"||file.Extension==".jpg")
            {
                using (Bitmap map = new Bitmap(Path))
                {
                    using (Bitmap bitmap = new Bitmap(map, SelectedSize.Width, SelectedSize.Width))
                    {
                        ConvertImageToIcon(map, savePath);
                    }
                }
                MessageBox.Show("导出完成！");
            }   
        }
        #region 原始代码
        /// <summary>
        /// 图片转换为ico文件
        /// </summary>
        /// <param name="origin">原图片路径</param>
        /// <param name="destination">输出ico文件路径</param>
        /// <param name="iconSize">输出ico图标尺寸，不可大于255x255</param>
        /// <returns>是否转换成功</returns>
        //public static bool ConvertImageToIcon(Bitmap bitmap, string destination)
        //{
        //    if (bitmap.Width > 255||bitmap.Height>255)
        //    {
        //        return false;
        //    }
        //   //先读取已有的图片为bitmap，并缩放至设定大小
        //    MemoryStream bitMapStream = new MemoryStream(); //存原图的内存流
        //    MemoryStream iconStream = new MemoryStream(); //存图标的内存流
        //    bitmap.Save(bitMapStream, ImageFormat.Png); //将原图读取为png格式并存入原图内存流
        //    BinaryWriter iconWriter = new BinaryWriter(iconStream); //新建二进制写入器以写入目标图标内存流
        //    /**
        //     * 下面是根据原图信息，进行文件头写入
        //     */
        //    iconWriter.Write((short)0);
        //    iconWriter.Write((short)1);
        //    iconWriter.Write((short)1);
        //    iconWriter.Write((byte)bitmap.Width);
        //    iconWriter.Write((byte)bitmap.Height);
        //    iconWriter.Write((short)0);
        //    iconWriter.Write((short)0);
        //    iconWriter.Write((short)32);
        //    iconWriter.Write((int)bitMapStream.Length);
        //    iconWriter.Write(22);
        //    //写入图像体至目标图标内存流
        //    iconWriter.Write(bitMapStream.ToArray());
        //    //保存流，并将流指针定位至头部以Icon对象进行读取输出为文件
        //    iconWriter.Flush();
        //    iconWriter.Seek(0, SeekOrigin.Begin);
        //    Stream iconFileStream = new FileStream(destination, FileMode.Create);
        //    Icon icon = new Icon(iconStream);
        //    icon.Save(iconFileStream); //储存图像
        //    /**
        //     * 下面开始释放资源
        //     */
        //    iconFileStream.Close();
        //    iconWriter.Close();
        //    iconStream.Close();
        //    bitMapStream.Close();
        //    icon.Dispose();
        //    bitmap.Dispose();
        //    return File.Exists(destination);
        //}
        #endregion
        public static bool ConvertImageToIcon(Bitmap bitmap, string destination)
        {
            if (bitmap.Width > 255 || bitmap.Height > 255)
            {
                return false;
            }
            //先读取已有的图片为bitmap，并缩放至设定大小
            //存原图的内存流
            using (MemoryStream bitMapStream = new MemoryStream())
            {
                bitmap.Save(bitMapStream, ImageFormat.Png); //将原图读取为png格式并存入原图内存流
                //存图标的内存流
                using (MemoryStream iconStream = new MemoryStream())
                {
                    //新建二进制写入器以写入目标图标内存流
                    using (BinaryWriter iconWriter = new BinaryWriter(iconStream))
                    {
                        /**
                        * 下面是根据原图信息，进行文件头写入
                        */
                        iconWriter.Write((short)0);
                        iconWriter.Write((short)1);
                        iconWriter.Write((short)1);
                        iconWriter.Write((byte)bitmap.Width);
                        iconWriter.Write((byte)bitmap.Height);
                        iconWriter.Write((short)0);
                        iconWriter.Write((short)0);
                        iconWriter.Write((short)32);
                        iconWriter.Write((int)bitMapStream.Length);
                        iconWriter.Write(22);
                        //写入图像体至目标图标内存流
                        iconWriter.Write(bitMapStream.ToArray());
                        //保存流，并将流指针定位至头部以Icon对象进行读取输出为文件
                        iconWriter.Flush();
                        iconWriter.Seek(0, SeekOrigin.Begin);
                        using (Stream iconFileStream = new FileStream(destination, FileMode.Create))
                        {
                            Icon icon = new Icon(iconStream);
                            icon.Save(iconFileStream); //储存图像
                        }
                    }

                }
            }
            return File.Exists(destination);
        }

        public Size SelectedSize { get;set; }
        public MainWindowViewModel()
        {

        }
    }

    public class Size
    {
        public int Width { get; set; }
        public string Description { get => $"{Width}*{Width}"; }
    }
}

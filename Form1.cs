using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing.Imaging;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.IO;
using System.Windows.Media.Imaging;
using AForge;
using AForge.Controls;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Timers;
using System.Runtime.InteropServices;
using System.Drawing;
using AForge.Video.FFMPEG;


namespace OperateCamera
{
    

    public partial class Form1 : Form
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private delegate void WriteLabelDelegate(object entry);

        private bool stopREC = true;
        private bool createNewFile = true;
        int frameRate = 25; //默认帧率
        private VideoFileWriter videoWriter = null;
        private string videoFileFullPath = string.Empty; //视频文件全路径
        private string imageFileFullPath = string.Empty; //图像文件全路径
        private string videoPath = @"E:\video\"; //视频文件路径
        private string videoFileName = string.Empty; //视频文件名
        private string drawDate = string.Empty;
        private VideoFileReader _reader;
        private VideoFileWriter _writer;
        static int intFlag = 0;

        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);

        static System.Timers.Timer timerPic;

        public Form1()
        {
            InitializeComponent();
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            
            try
            {
                // 枚举所有视频输入设备
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                if (videoDevices.Count == 0)
                    throw new ApplicationException();

                foreach (FilterInfo device in videoDevices)
                {
                    tscbxCameras.Items.Add(device.Name);
                }

                tscbxCameras.SelectedIndex = 0;

            }
            catch (ApplicationException)
            {
                tscbxCameras.Items.Add("没有找到摄像透");
                videoDevices = null;
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            CameraConn();
        }
        //连接摄像头
        private void CameraConn()
        {
            VideoCaptureDevice videoSource = new VideoCaptureDevice(videoDevices[tscbxCameras.SelectedIndex].MonikerString);
            videoSource.DesiredFrameSize = new System.Drawing.Size(320, 240);
            videoSource.DesiredFrameRate = 1;

            videoSourcePlayer.VideoSource = videoSource;
            videoSourcePlayer.Start();
        }

        //关闭摄像头
        private void btnClose_Click(object sender, EventArgs e)
        {
            videoSourcePlayer.SignalToStop();
            videoSourcePlayer.WaitForStop();
        }

        //主窗体关闭
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            btnClose_Click(null, null);
        }

        //拍照
        private void Photograph_Click(object sender, EventArgs e)
        {
            GetPic();
        }

        /// <summary>
        /// 拍照功能
        /// </summary>
        private void GetPic()
        {
            try
            {
                if (videoSourcePlayer.IsRunning)
                {
                    IntPtr ip = videoSourcePlayer.GetCurrentVideoFrame().GetHbitmap();
                    this.richText_logs.Text = "[" + DateTime.Now + "]拍照功能运行中...";
                    BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                                    ip,
                                    IntPtr.Zero,
                                     Int32Rect.Empty,
                                    BitmapSizeOptions.FromEmptyOptions());
                    DeleteObject(ip);
                    PngBitmapEncoder pE = new PngBitmapEncoder();
                    
                    pE.Frames.Add(BitmapFrame.Create(bitmapSource));
                    string picName = GetImagePath() + "\\" + "pic"+DateTime.Now.ToString("yyyyMMddhhmmss") + ".jpg";
                    //string picName = GetImagePath() + "\\" + "cert.jpg";
                    
                    if (File.Exists(picName))
                    {
                        File.Delete(picName);
                    }
                    using (Stream stream = File.Create(picName))
                    {
                        pE.Save(stream);
                        //SetLosResult("[" + DateTime.Now + "]拍照完成：" + picName);
                        this.richText_logs.Text = "[" + DateTime.Now + "]拍照完成：" + picName;
                    }
                    //拍照完成后关摄像头并刷新同时关窗体
                    //if (videoSourcePlayer != null && videoSourcePlayer.IsRunning)
                    //{
                    //    videoSourcePlayer.SignalToStop();
                    //    videoSourcePlayer.WaitForStop();
                    //}

                    //this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("摄像头异常：" + ex.Message);
            }
        }
        private void videoSourcePlayer1_NewFrame(object sender, ref Bitmap image)
        {
            //Graphics gr = Graphics.FromImage(image);

            //SolidBrush drawBrush = new SolidBrush(Color.Red);

            //Font drawFont = new Font("Arial", 4, System.Drawing.FontStyle.Bold, GraphicsUnit.Millimeter);

            //int xPos = 5;

            //int yPos = 3;

            //string date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");//得到写到屏上的时间
            //gr.DrawString(date, drawFont, drawBrush, xPos, yPos);

            //if (stopREC)
            //{
            //    stopREC = true;
            //    createNewFile = true;

            //    if (videoWriter != null)
            //    {
            //        videoWriter.Close();
            //    }
            //}
            //else
            //{
            //    if (createNewFile)
            //    {
                    
            //        createNewFile = false;
            //        if (videoWriter != null)
            //        {
            //            videoWriter.Close();
            //            videoWriter.Dispose();
            //        }
            //        videoWriter = new VideoFileWriter();    
            //        videoWriter.Open(DateTime.Now.ToString("yyMMddHH")+".avi", 320, 240, 20, VideoCodec.MPEG4);

            //        // add the image as a new frame of video file
            //        videoWriter.WriteVideoFrame(image);
            //        videoWriter.Dispose();
            //    }
            //    else
            //    {
            //        videoWriter.WriteVideoFrame(image);
            //    }
            //}
            //录像
            Graphics g = Graphics.FromImage(image);
            SolidBrush drawBrush = new SolidBrush(Color.Yellow);

            Font drawFont = new Font("Arial", 6, System.Drawing.FontStyle.Bold, GraphicsUnit.Millimeter);
            int xPos = image.Width - (image.Width - 15);
            int yPos = 10;
            //写到屏幕上的时间
            drawDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            g.DrawString(drawDate, drawFont, drawBrush, xPos, yPos);
            if (!Directory.Exists(videoPath))
                Directory.CreateDirectory(videoPath);

            //创建文件路径
            //fileFullPath = path + fileName;

            if (stopREC)
            {
                stopREC = true;
                createNewFile = true;  //这里要设置为true表示要创建新文件
                if (videoWriter != null)
                    videoWriter.Close();
            }
            else
            {
                //开始录像
                if (createNewFile)
                {
                    videoFileName = DateTime.Now.ToString("yyyyMMddHH") + ".avi";
                    videoFileFullPath = videoPath + videoFileName;
                    createNewFile = false;
                    if (videoWriter != null)
                    {
                        videoWriter.Close();
                        videoWriter.Dispose();
                    }
                    videoWriter = new VideoFileWriter();
                    //这里必须是全路径，否则会默认保存到程序运行根据录下了
                    videoWriter.Open(videoFileFullPath, image.Width, image.Height, frameRate, VideoCodec.MPEG4);
                    videoWriter.WriteVideoFrame(image);
                }
                else
                {
                    videoWriter.WriteVideoFrame(image);
                }
            }
        }

        private string GetImagePath()
        {
            string personImgPath = "";

            personImgPath = this.tb_filePath.Text.Trim();
            if (string.IsNullOrEmpty(personImgPath))
            {
                personImgPath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)
                              + Path.DirectorySeparatorChar.ToString() + "PersonImg";
            }
            if (!Directory.Exists(personImgPath))
            {
                Directory.CreateDirectory(personImgPath);
            }

            return personImgPath;
        }


        private void WriteLogsResult(object text)
        {
            this.toolStripStatusLabel1.Text = text.ToString();
        }
        private void SetLosResult(string text)
        {
            try
            {
                this.statusStrip1.Invoke(new WriteLabelDelegate(WriteLogsResult), text);
            }
            catch
            {
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件路径";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string foldPath = dialog.SelectedPath;
                this.tb_filePath.Text = foldPath;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //开始录像
            if (btnStartVideotape.Text == "开始录像")
            {
                stopREC = false;
                //frameRate = Convert.ToInt32(txtFrameRate.Text.Trim());
                btnStartVideotape.Text = "停止录像";
            }
            else if (btnStartVideotape.Text == "停止录像")
            {
                stopREC = true;
                btnStartVideotape.Text = "开始录像";
            }
        }
        
    }
}

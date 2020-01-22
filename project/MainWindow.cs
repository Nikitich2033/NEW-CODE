﻿
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Imaging;
using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Threading;

namespace project
{
    public partial class MainWindow : Form
    {
       
        private UserImage UploadedImageBitmap { get; set; }
       // public Bitmap WorkingOnImage { get; set; }
        public FilterInfoCollection videoDevices { get; set; }
        public VideoCaptureDevice videoSource { get; set; }
        
        public List<UserImage> userImages { get; set; }


        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {

            pictureBox1.BorderStyle = BorderStyle.Fixed3D;
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.BorderStyle = BorderStyle.Fixed3D;
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox3.BorderStyle = BorderStyle.Fixed3D;
            pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
            UploadedImageBitmap = new UserImage();
            // userImages = new List<UserImage>();
            userImages = UploadedImageBitmap.check_JSON();

        }


        private void UploadImage(object sender, EventArgs e)
        {
            OpenFileDialog opnfd = new OpenFileDialog();
            opnfd.Filter = "Image Files (*.jpg;*.jpeg;.*.gif;*.png;*.bmp)|*.jpg;*.jpeg;.*.gif;*.png;*.bmp";
            if (opnfd.ShowDialog() == DialogResult.OK)
            {
                UploadedImageBitmap.BM = new Bitmap(opnfd.FileName);
                UploadedImageBitmap.startBM = UploadedImageBitmap.BM;
                pictureBox1.Image = UploadedImageBitmap.BM;
            }
            //get the values for trackbars from JSON
            int index = 0;
            foreach (var image in userImages)
            {
                if (image.filepath == opnfd.FileName)
                {
                    trackBar1.Value = image.brightness;
                    trackBar2.Value = image.contrast;
                    trackBar3.Value = image.saturation;

                    
                    UpdateTrackBarTextboxes(image);
                    UpdateUserImageObject(image,UploadedImageBitmap);
                    index = userImages.IndexOf(image);
                    
                }
                //suserImages.RemoveAt(index);
            }


           
           
        
        }

        private void SaveImage(object sender, EventArgs e)
        {
            SaveFileDialog savefiledg = new SaveFileDialog();
            savefiledg.Filter = "Image Files| *.png;*.bmp;*.jpg;*.gif;";
            ImageFormat format = ImageFormat.Png;

            if (savefiledg.ShowDialog() == DialogResult.OK)
            {
                string ext = Path.GetExtension(savefiledg.FileName);

                switch (ext)
                {
                    case ".jpg":
                        format = ImageFormat.Jpeg;
                        break;

                    case ".bmp":
                        format = ImageFormat.Bmp;
                        break;

                    case ".gif":
                        format = ImageFormat.Gif;
                        break;

                }

                pictureBox1.Image.Save(savefiledg.FileName, format);
                UploadedImageBitmap.filepath = savefiledg.FileName;
                userImages = UploadedImageBitmap.check_JSON();
                userImages.Add(UploadedImageBitmap);
                UploadedImageBitmap.update_JSON(userImages);
                
            }
            

        }


        private void BrightnessAdjustment(object sender, EventArgs e) {
            
            //temporary file with the starting image

            UploadedImageBitmap.BM = ImageFilter.AdjustBrightness(UploadedImageBitmap.startBM, trackBar1.Value);
            UploadedImageBitmap.BM = ImageFilter.AdjustContrast(UploadedImageBitmap.BM,UploadedImageBitmap.contrast);
            UploadedImageBitmap.BM = ImageFilter.adjustSaturation(UploadedImageBitmap.BM, UploadedImageBitmap.saturation);

            pictureBox1.Image = UploadedImageBitmap.BM;

            UploadedImageBitmap.brightness = trackBar1.Value;
            UpdateTrackBarTextboxes(UploadedImageBitmap);
            
        }

        private void ContrastAdjustment(object sender, EventArgs e) {
            UploadedImageBitmap.BM = ImageFilter.AdjustContrast(UploadedImageBitmap.startBM, trackBar2.Value);

            UploadedImageBitmap.BM = ImageFilter.AdjustBrightness(UploadedImageBitmap.BM, UploadedImageBitmap.brightness);
            UploadedImageBitmap.BM = ImageFilter.adjustSaturation(UploadedImageBitmap.BM, UploadedImageBitmap.saturation);

            pictureBox1.Image = UploadedImageBitmap.BM;


            UploadedImageBitmap.contrast = trackBar2.Value;
            UpdateTrackBarTextboxes(UploadedImageBitmap);       
            
        }
     

        private void SaturationAdjustment(object sender, EventArgs e)
        {
            UploadedImageBitmap.BM = ImageFilter.adjustSaturation(UploadedImageBitmap.startBM, trackBar3.Value);

            UploadedImageBitmap.BM = ImageFilter.AdjustBrightness(UploadedImageBitmap.BM, UploadedImageBitmap.brightness);
            UploadedImageBitmap.BM = ImageFilter.AdjustContrast(UploadedImageBitmap.BM, UploadedImageBitmap.contrast);

            pictureBox1.Image = UploadedImageBitmap.BM;

            UploadedImageBitmap.saturation = trackBar3.Value;
            UpdateTrackBarTextboxes(UploadedImageBitmap);
            
        }

        

        private void Grayscale_click(object sender, EventArgs e)
        {
            pictureBox1.Image = ImageFilter.DrawAsGrayscale(UploadedImageBitmap.BM);
        }

        private void Sepia_click(object sender, EventArgs e)
        {
            pictureBox1.Image = ImageFilter.DrawAsSepiaTone(UploadedImageBitmap.BM);
        }

        private void Negative_click(object sender, EventArgs e)
        {
            pictureBox1.Image = ImageFilter.DrawAsNegative(UploadedImageBitmap.BM);

        }

        private void Laplacian_3x3(object sender, EventArgs e)
        {
            bool grayscale = false;
            UploadedImageBitmap.BM = ImageFilter.Laplacian3x3Filter(UploadedImageBitmap.BM, grayscale);
            pictureBox1.Image = UploadedImageBitmap.BM;
        }

        private void ReplaceColorButton(object sender, EventArgs e)
        {

           ImageFilter.ReplaceColor(UploadedImageBitmap.BM, Color.FromArgb(0, 0, 0), Color.Red);
            
           pictureBox1.Image = UploadedImageBitmap.BM;
           
        }

        private void DisplayMostReoccuringColor() 
        {

            Bitmap image = new Bitmap(50,50);

            Graphics gfx = Graphics.FromImage(image);

            SolidBrush brush = new SolidBrush(ImageRecognition.FindTheMostReoccuringColor(new Bitmap(pictureBox1.Image)));

            gfx.FillRectangle(brush, 0, 0, 50,50);

            pictureBox4.Image =  image;
        }


        //webcam functionality

        private void Start_Click(object sender, EventArgs e)
        {
            videoSource.Start();
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            if (UploadedImageBitmap.BM != null) { UploadedImageBitmap.BM.Dispose(); }
            UploadedImageBitmap.BM = new Bitmap(pictureBox1.Image);

            if (UploadedImageBitmap.startBM != null) { UploadedImageBitmap.startBM.Dispose(); }
            UploadedImageBitmap.startBM = UploadedImageBitmap.BM;

            videoSource.Stop();
           
        }

        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {

            if (pictureBox1.Image != null) { pictureBox1.Image.Dispose(); }
            pictureBox1.Image = (Bitmap)eventArgs.Frame.Clone();
           


        }


        private void FindRectanglesButton(object sender, EventArgs e)
        {
            ImageRecognition.FindRect(UploadedImageBitmap.BM);
        }

        private void FindObjectsButton(object sender, EventArgs e)
        {
           pictureBox1.Image =  ImageRecognition.FindObjects(UploadedImageBitmap.BM);
        }

        private void DisplyaEdgesButton(object sender, EventArgs e)
        {
            pictureBox1.Image =  ImageRecognition.DisplayEdges(UploadedImageBitmap.BM);
        }

        private void picbox2_upload(object sender, EventArgs e)
        {
            OpenFileDialog opnfd = new OpenFileDialog();
            opnfd.Filter = "Image Files (*.jpg;*.jpeg;.*.gif;*.png;*.bmp)|*.jpg;*.jpeg;.*.gif;*.png;*.bmp";
            if (opnfd.ShowDialog() == DialogResult.OK)
            {

                pictureBox2.Image = new Bitmap(opnfd.FileName);

            }

            
        }

        private void picbox3_upload(object sender, EventArgs e)
        {
            OpenFileDialog opnfd = new OpenFileDialog();
            opnfd.Filter = "Image Files (*.jpg;*.jpeg;.*.gif;*.png;*.bmp)|*.jpg;*.jpeg;.*.gif;*.png;*.bmp";
            if (opnfd.ShowDialog() == DialogResult.OK)
            {

                pictureBox3.Image = new Bitmap(opnfd.FileName);

            }

        }

        private void CompareButon(object sender, EventArgs e)
        {
            Bitmap BM1 = new Bitmap(pictureBox2.Image);
            Bitmap BM2 = new Bitmap(pictureBox3.Image);

           textBox4.Text =  ImageRecognition.CompareImages(BM1, BM2).ToString();

        }

        private void compareUsingLaplacian(object sender, EventArgs e)
        {
            //could outline the white outline using a pen tool and replace a bitmap with that sketch
            bool grayscale = false;
            Bitmap BM1 = ImageFilter.Laplacian3x3Filter(new Bitmap (pictureBox2.Image), grayscale);
            Bitmap BM2 = ImageFilter.Laplacian3x3Filter(new Bitmap(pictureBox3.Image), grayscale);

            // ImageFilter.ReplaceColor(BM1, Color.FromArgb(0, 0, 0), Color.FromArgb(0, 0, 0, 0));
            //ImageFilter.ReplaceColor(BM2, Color.FromArgb(0, 0, 0), Color.FromArgb(0, 0, 0, 0));

            ImageFilter.ReplaceColor(BM1, Color.FromArgb(0, 0, 0), Color.Empty);
            ImageFilter.ReplaceColor(BM1, Color.FromArgb(0, 0, 0), Color.Empty);

            pictureBox2.Image = BM1;
            pictureBox3.Image = BM2;

            textBox4.Text = ImageRecognition.CompareImages(BM1, BM2).ToString();

        }

        private void White_Outline_Highlight(object sender, EventArgs e)
        {
           
            var tempBM = ImageFilter.DrawOutlineFromLaplacian(UploadedImageBitmap.BM, Color.FromArgb(255, 255, 255), Color.Red);
            pictureBox1.Image = tempBM;
        }



        private async void UploadFolderCompareButton(object sender, EventArgs e)
        {

            CommonOpenFileDialog folderDialog = new CommonOpenFileDialog();

            folderDialog.InitialDirectory = "c:\\Users";
            folderDialog.IsFolderPicker = true;

            if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                textBox5.Text = folderDialog.FileName;

                string[] arrayOfImagePaths = Directory.GetFiles(folderDialog.FileName);

                foreach (var Filepath in arrayOfImagePaths)
                {
                   
                    pictureBox3.Image = new Bitmap(Filepath);
                        textBox4.Text = ImageRecognition.CompareImages(new Bitmap(pictureBox2.Image), new Bitmap(pictureBox3.Image)).ToString();

                    await Task.Delay(1000);

                 
                }
            }

        }

        //when data is read from the JSON file
        public void UpdateTrackBarTextboxes(UserImage image)
        {
            textBox1.Text = Convert.ToString(image.brightness);
            textBox2.Text = Convert.ToString(image.contrast);
            textBox3.Text = Convert.ToString(image.saturation);

        }

        public void UpdateUserImageObject(UserImage image, UserImage UploadedImage)
        {
            UploadedImage.brightness = image.brightness;
            UploadedImage.contrast = image.contrast;
            UploadedImage.saturation = image.saturation;
            UploadedImage.filepath = image.filepath;
            
        
        }

      



        // AREA SELECTION

        // True when we're selecting a rectangle.
        private bool IsSelecting = false;

        // The area we are selecting.
        private int X0, Y0, X1, Y1;

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void SuggestedFilterButton(object sender, EventArgs e)
        {
          
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            DisplayMostReoccuringColor();
        }





        // Start selecting the rectangle.
        private void picOriginal_MouseDown(object sender, MouseEventArgs e)
        {
            IsSelecting = true;

            // Save the start point.
            X0 = e.X;
            Y0 = e.Y;
        }

        // Continue selecting.
        private void picOriginal_MouseMove(object sender, MouseEventArgs e)
        {
            
            // Do nothing it we're not selecting an area.
            if (!IsSelecting) return;

            // Save the new point.
            X1 = e.X;
            Y1 = e.Y;

            // Make a Bitmap to display the selection rectangle.
            Bitmap bm = new Bitmap(UploadedImageBitmap.BM);
            

            // Draw the rectangle.
            using (Graphics gr = Graphics.FromImage(bm))
            {
                gr.DrawRectangle(Pens.Red,
                    Math.Min(X0, X1), Math.Min(Y0, Y1),
                    Math.Abs(X0 - X1), Math.Abs(Y0 - Y1));
            }

            // Display the temporary bitmap.
            pictureBox1.Image = bm;
            
        }

        // Finish selecting the area.
        private void picOriginal_MouseUp(object sender, MouseEventArgs e)
        {
            // Do nothing it we're not selecting an area.
            if (!IsSelecting) return;
            IsSelecting = false;

            // Display the original image.
            pictureBox1.Image = UploadedImageBitmap.BM;

            // Copy the selected part of the image.
            int wid = Math.Abs(X0 - X1);
            int hgt = Math.Abs(Y0 - Y1);
            if ((wid < 1) || (hgt < 1)) return;

            Bitmap area = new Bitmap(wid, hgt);
            using (Graphics gr = Graphics.FromImage(area))
            {
                Rectangle source_rectangle =
                    new Rectangle(Math.Min(X0, X1), Math.Min(Y0, Y1), wid, hgt);
                Rectangle dest_rectangle =
                    new Rectangle(0, 0, wid, hgt);
                gr.DrawImage(UploadedImageBitmap.BM, dest_rectangle,
                    source_rectangle, GraphicsUnit.Pixel);
            }

            // Display the result.
            pictureBox1.Image = area;
        }
    }
}
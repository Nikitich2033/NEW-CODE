﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using AForge.Imaging;
using AForge.Imaging.Filters;
using System.Windows.Forms;
using AForge.Math.Geometry;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace project
{
    public class ImageRecognition
    {

        public static List<bool> GetPixels(Bitmap image)
        {
            List<bool> result = new List<bool>();

            Bitmap BM16 = new Bitmap(image, new Size(256, 256));

            for (int i = 0; i < BM16.Width; i++)
            {
                for (int j = 0; j < BM16.Height; j++)
                {
                    result.Add(BM16.GetPixel(i, j).GetBrightness() < 0.5f);
                }
            }

            return result;
        }


        public static Color FindTheMostReoccuringColor(Bitmap BM)
        {

            int BM_height = BM.Height;
            int BM_width = BM.Width;


            Dictionary<Color, int> ColorCount = new Dictionary<Color, int>();
            Color MostReoccuringColor = Color.Empty;


            for (int i = 0; i < BM_width; i++)
            {
                for (int j = 0; j < BM_height; j++)
                {

                    if (ColorCount.ContainsKey(BM.GetPixel(i, j)) != true )
                    {

                        ColorCount.Add(BM.GetPixel(i,j),1);

                    }
                    else if (ColorCount.ContainsKey(BM.GetPixel(i, j)) == true)
                    {
                        ColorCount[BM.GetPixel(i, j)]++;
                    }

                }
           
            }

            
            foreach (var key in ColorCount.Keys)
            {
                int max = 0;
                if (ColorCount[key] > max)
                {
                    max = ColorCount[key];
                    MostReoccuringColor = key;
                }
                
                
            }

            return MostReoccuringColor;

        }
    

  



        
        


        public static string CompareImages(Bitmap sourceImage, Bitmap comparedToImage)
        {
            string folderName = @"D:\Images";
            string pathString = null;
            string fileName = @"\image";
            Random rnd = new Random();
            
            List<bool> pixelsSource = GetPixels(sourceImage);
            List<bool> pixelsCompared = GetPixels(comparedToImage);

            int equalElements = pixelsSource.Zip(pixelsCompared, (i, j) => i == j).Count(eq => eq);

            if (equalElements == 256*256*1)
            {
                pathString = System.IO.Path.Combine(folderName, "Same Image");
                System.IO.Directory.CreateDirectory(pathString);
                comparedToImage.Save(pathString + fileName + rnd.Next() + ".png", ImageFormat.Png);
              

                return "Same Image";
            }
            else if (equalElements >= 256 * 256 * 0.75)
            {
                pathString = System.IO.Path.Combine(folderName, "Similar Image");
                System.IO.Directory.CreateDirectory(pathString);
                comparedToImage.Save(pathString + fileName + rnd.Next() + ".png", ImageFormat.Png);
                

                return "Similar Image";
            }
            else if (equalElements >= 256*256*0.55)
            {
                pathString = System.IO.Path.Combine(folderName, "Limited Similarity");
               
                System.IO.Directory.CreateDirectory(pathString);
                comparedToImage.Save(pathString + fileName + rnd.Next() + ".png", ImageFormat.Png);
              

                return "Limited Similarity";
            }
            else if (equalElements >= 256*256*0.4)
            {
                pathString = System.IO.Path.Combine(folderName, "Almost zero similarity");
                
                System.IO.Directory.CreateDirectory(pathString);
                comparedToImage.Save(pathString + fileName + rnd.Next() + ".png", ImageFormat.Png);
                return "Almost zero similarity";
            }
            else
            {
                return "No similiraty";
            }

        }

       
        public static Bitmap FindObjects(Bitmap image) 
        {
            // create instance of blob counter
            BlobCounter blobCounter = new BlobCounter();
            // process input image
            blobCounter.ProcessImage(image);
            // get information about detected objects
            Blob[] blobs = blobCounter.GetObjectsInformation();

            var objectcount = 0;

            foreach (var item in blobs)
            {
                objectcount++;
            }

            //draw the detected shapes on the image

            SimpleShapeChecker shapeChecker = new SimpleShapeChecker();
            Pen redPen = new Pen(Color.Red, 5);
            Graphics g = Graphics.FromImage(image);

           

            for (int i = 0, n = blobs.Length; i < n; i++)
            {
                List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blobs[i]);
                List<IntPoint> corners = PointsCloud.FindQuadrilateralCorners(edgePoints);

                g.DrawPolygon(redPen, corners.Select(p => new System.Drawing.Point(p.X, p.Y)).ToArray());
            }

            MessageBox.Show(objectcount.ToString(), "Number of objects");

           return new Bitmap(50,50,g);
        }

        public static void FindRect(Bitmap image)
        {
           
            // create an instance of blob counter algorithm
            BlobCounter bc = new BlobCounter();
            // process binary image
            bc.ProcessImage(image);
            Rectangle[] rects = bc.GetObjectsRectangles();
            // process blobs

            var rectnum = 0;

            foreach (Rectangle rect in rects)
            {
                rectnum++;
            }

           
            MessageBox.Show(rectnum.ToString(),"Number of rectangles");

        }

        //http://www.aforgenet.com/framework/docs/html/372f7aee-9ce1-a2a2-395a-2de015241b8c.htm
        public static Bitmap DisplayEdges(Bitmap bitmap) 
        {
            HomogenityEdgeDetector edgeFilter = new HomogenityEdgeDetector();
            Bitmap start = AForge.Imaging.Image.Clone(bitmap,PixelFormat.Format24bppRgb);
            return start;
        
        
        }



        //https://softwarebydefault.com/2013/05/11/image-edge-detection/comment-page-1/
        //Convolution is a simple mathematical operation which is fundamental to many common image processing operators. 
        //Convolution provides a way of `multiplying together’ two arrays of numbers, generally of different sizes, but of the same dimensionality, 
        //to produce a third array of numbers of the same dimensionality. This can be used in image processing to implement operators whose output pixel 
        //values are simple linear combinations of certain input pixel values.
        //In an image processing context, one of the input arrays is normally just a graylevel image.The second array is usually much smaller, 
        //and is also two-dimensional(although it may be just a single pixel thick), and is known as the kernel.
        public static Bitmap ConvolutionFilter(Bitmap sourceBitmap, double[,] filterMatrix, double factor,  int bias, bool grayscale)
        {
            BitmapData sourceData =
                           sourceBitmap.LockBits(new Rectangle(0, 0,
                           sourceBitmap.Width, sourceBitmap.Height),
                                             ImageLockMode.ReadOnly,
                                        PixelFormat.Format32bppArgb);


            byte[] pixelBuffer = new byte[sourceData.Stride *
                                          sourceData.Height];


            byte[] resultBuffer = new byte[sourceData.Stride *
                                           sourceData.Height];


            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0,
                                       pixelBuffer.Length);


            sourceBitmap.UnlockBits(sourceData);


            if (grayscale == true)
            {
                float rgb = 0;


                for (int k = 0; k < pixelBuffer.Length; k += 4)
                {
                    rgb = pixelBuffer[k] * 0.11f;
                    rgb += pixelBuffer[k + 1] * 0.59f;
                    rgb += pixelBuffer[k + 2] * 0.3f;


                    pixelBuffer[k] = (byte)rgb;
                    pixelBuffer[k + 1] = pixelBuffer[k];
                    pixelBuffer[k + 2] = pixelBuffer[k];
                    pixelBuffer[k + 3] = 255;
                }
            }


            double blue = 0.0;
            double green = 0.0;
            double red = 0.0;


            int filterWidth = filterMatrix.GetLength(1);
            int filterHeight = filterMatrix.GetLength(0);


            int filterOffset = (filterWidth - 1) / 2;
            int calcOffset = 0;


            int byteOffset = 0;


            for (int offsetY = filterOffset; offsetY <
                sourceBitmap.Height - filterOffset; offsetY++)
            {
                for (int offsetX = filterOffset; offsetX <
                    sourceBitmap.Width - filterOffset; offsetX++)
                {
                    blue = 0;
                    green = 0;
                    red = 0;


                    byteOffset = offsetY *
                                 sourceData.Stride +
                                 offsetX * 4;


                    for (int filterY = -filterOffset;
                        filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset;
                            filterX <= filterOffset; filterX++)
                        {


                            calcOffset = byteOffset +
                                         (filterX * 4) +
                                         (filterY * sourceData.Stride);


                            blue += pixelBuffer[calcOffset] *
                                    filterMatrix[filterY + filterOffset,
                                                 filterX + filterOffset];


                            green += pixelBuffer[calcOffset + 1] *
                                     filterMatrix[filterY + filterOffset,
                                                  filterX + filterOffset];


                            red += pixelBuffer[calcOffset + 2] *
                                   filterMatrix[filterY + filterOffset,
                                                filterX + filterOffset];
                        }
                    }


                    blue = factor * blue + bias;
                    green = factor * green + bias;
                    red = factor * red + bias;


                    if (blue > 255)
                    { blue = 255; }
                    else if (blue < 0)
                    { blue = 0; }


                    if (green > 255)
                    { green = 255; }
                    else if (green < 0)
                    { green = 0; }


                    if (red > 255)
                    { red = 255; }
                    else if (red < 0)
                    { red = 0; }


                    resultBuffer[byteOffset] = (byte)(blue);
                    resultBuffer[byteOffset + 1] = (byte)(green);
                    resultBuffer[byteOffset + 2] = (byte)(red);
                    resultBuffer[byteOffset + 3] = 255;
                }
            }


            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width,
                                            sourceBitmap.Height);


            BitmapData resultData =
                       resultBitmap.LockBits(new Rectangle(0, 0,
                       resultBitmap.Width, resultBitmap.Height),
                                        ImageLockMode.WriteOnly,
                                    PixelFormat.Format32bppArgb);


            Marshal.Copy(resultBuffer, 0, resultData.Scan0,
                                       resultBuffer.Length);
            resultBitmap.UnlockBits(resultData);


            return resultBitmap;
        }


    }
    
    

}


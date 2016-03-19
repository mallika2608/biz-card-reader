using Emgu.CV;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Features2D;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using System.Drawing;
using System.Diagnostics;

namespace iaproj
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            AllocConsole();
            InitializeComponent();            
        }
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e) //first function that is called in the program 
        {
            string path;
            OpenFileDialog file = new OpenFileDialog();
            if (file.ShowDialog() == DialogResult.OK)
            {
                path = file.FileName;
                Console.WriteLine(path);

                String[] tokens = path.Split('\\');
                Console.WriteLine(tokens[tokens.Length - 1]);

                String nameFull = tokens[tokens.Length - 1];
                String[] namesFullTokens = nameFull.Split('.');

                String name = namesFullTokens[0];
                Console.WriteLine(name);

                LoadImage(name);
            }
        }

        private void LoadImage(String name)
        {
            Image<Gray, byte> imageGrey = new Image<Gray, byte>
                ("C:\\Users\\Lenvo\\Documents\\Visual Studio 2013\\Projects\\iaproj\\input\\Cards\\" + name + ".jpg");
            imageGrey.Save("C:\\Users\\Lenvo\\Documents\\Visual Studio 2013\\Projects\\iaproj\\output\\" + name + "_1_Grey.png");

            double[] histogram = new double[255];
            histogram = ComputeHistogram(imageGrey);

            Image<Gray, byte> imageBinary = new Image<Gray, byte>(imageGrey.Width, imageGrey.Height);
            imageBinary = IterativeThresholding(histogram, imageGrey, name);

            
            

            textBox1.Text = "Information Extracted:\n\n";
            textBox1.AppendText("\n");
            extractInformation(imageGrey);
            LogoDetector(imageBinary, imageGrey, name);
        }


        private void extractInformation(Image<Gray, Byte> imageGrey) 
        {
            Tesseract _ocr; //initializing ocr engine
            String words=null;
            try
            {
                _ocr = new Tesseract("tessdata", "eng", Tesseract.OcrEngineMode.OEM_DEFAULT);

                
                _ocr.Recognize(imageGrey);
                words = _ocr.GetText();
                Console.Write(words); 
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex);
            }
            for (int i = 0; i < words.Length; i++)
            {
                Console.Write(words[i]);
            }
            Console.Write("\n");
            Console.Write("Name: ");
            
          
            //textBox1.AppendText("Hello2\n");
            //textBox1.AppendText("Hello3");
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i] == '\n')
                    break;
                Console.Write(words[i]);
                textBox1.AppendText(Char.ToString(words[i]));
            }
            textBox1.AppendText("\n"); //printing to emulator 
            Console.Write("\n");
            int index = 0;
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i] == '@')
                {
                    index=i;
                    break;
                }
            }

            int emailStart = 0;
            for (int i = index; i >= 0; i--)
            {
                if (words[i] == ' ' || words[i]=='\n')
                {
                    emailStart=i+1;
                    break;
                }
            }

            int emailEnd = 0;
            for (int i = index; i <= words.Length ; i++)
            {
                if (words[i] == ' ' || words[i] == '\n')
                {
                    emailEnd = i - 1;
                    break;
                }
            }
            Console.Write("Email Id: ");
            for (int i = emailStart; i <= emailEnd; i++)
            {
                Console.Write(words[i]);
                textBox1.AppendText(Char.ToString(words[i]));
            }
            Console.Write("\n");
            textBox1.AppendText("\n");
            String wordsNoSpace = null;
            int k = 0;
            int numStart = 0, numEnd = 0;
            String temp = null;
            for(int i = 0; i<words.Length;i++)
            {
                if (words[i]!=' ' && words[i]!='-')
                {
                    temp = string.Copy(words[i].ToString());
                    wordsNoSpace += temp;
                }
            }
            int maxNumStart=0,maxNumEnd=0;
            int maxNumLength = 0;
            
            for (int i = 0; i < wordsNoSpace.Length; i++)
            {
                temp = null;
                int numLength = 0;
                if (wordsNoSpace[i] == '0' || wordsNoSpace[i] == 'o' || wordsNoSpace[i] == 'O' || wordsNoSpace[i] == '1' || wordsNoSpace[i] == '2' || wordsNoSpace[i] == '3' || wordsNoSpace[i] == '4' || wordsNoSpace[i] == '5' || wordsNoSpace[i] == '6' || wordsNoSpace[i] == '7' || wordsNoSpace[i] == '8' || wordsNoSpace[i] == '9')
                {
                    numStart = i; numEnd = i;
                    i = i + 1;
                    numLength = 1;
                    while (wordsNoSpace[i] == '0' || wordsNoSpace[i] == 'o' || wordsNoSpace[i] == 'O' || wordsNoSpace[i] == '1' || wordsNoSpace[i] == '2' || wordsNoSpace[i] == '3' || wordsNoSpace[i] == '4' || wordsNoSpace[i] == '5' || wordsNoSpace[i] == '6' || wordsNoSpace[i] == '7' || wordsNoSpace[i] == '8' || wordsNoSpace[i] == '9')
                    {
                        i++;
                        numLength++;
                    }
                    numEnd = --i;
                    if (maxNumLength < numLength)
                    {
                        maxNumLength = numLength;
                        maxNumStart = numStart;
                        maxNumEnd = numEnd;
                    }
               }           
            }
            Console.Write("Phone number: ");
            for (int i =  maxNumStart ; i <= maxNumEnd; i++)
            {
                Console.Write(wordsNoSpace[i]);
                textBox1.AppendText(Char.ToString(wordsNoSpace[i]));
            }

            textBox1.AppendText("\n");
        }

        private void LogoDetector(Image<Gray, Byte> binImage, Image<Gray, Byte> origImage, String name)
        {
            Console.Write("Running Logo Detector\n");
            Image<Gray, Byte> binImagePadded = new Image<Gray, Byte>(binImage.Width + 2, binImage.Height + 2);
            Image<Gray, Byte> origImagePadded = new Image<Gray, Byte>(origImage.Width + 2, origImage.Height + 2);

            int[,] visited = new int[binImage.Height + 2, binImage.Width + 2];

            for (int i = 0; i < binImage.Height; i++) //creating padded image (both binary and greyscale)
            {
                for (int j = 0; j < binImage.Width; j++)
                {
                    binImagePadded.Data[i + 1, j + 1, 0] = binImage.Data[i, j, 0];
                    origImagePadded.Data[i + 1, j + 1, 0] = origImage.Data[i, j, 0];
                }
            }

            //Console.Write(binImagePadded.Data[0, 0, 0]);

            List<int[]> candidateLogos = new List<int[]>(); //

            int flag;
            int count;
            int final1,final2;
            int[] tl = new int[2];
            int[] tr = new int[2];
            int[] bl = new int[2];
            int[] br = new int[2];
            
            for (int i = 1; i < binImagePadded.Height - 1; i++)
            {
                for (int j = 1; j < binImagePadded.Width - 1; j++)
                {

                    if (visited[i, j] != 1 && binImagePadded.Data[i, j, 0] == 0)
                    {
                        //Console.Write(String.Format("{0} {1}", i, j));

                        tl[0] = i - 1; tl[1] = j - 1;
                        tr[0] = i - 1; tr[1] = j + 1;
                        bl[0] = i + 1; bl[1] = j - 1;
                        br[0] = i + 1; br[1] = j + 1;

                    Top: //grow feature rectangle for every unvisited pixel
                        flag = 0;
                        while (true)
                        {
                            for (int k = tl[1]; k <= tr[1]; k++)
                            {
                                //Console.Write(String.Format("{0} {1}", tl[0], k));
                                if (binImagePadded.Data[tl[0], k, 0] == 0)
                                {
                                    flag = 1;
                                    break;
                                }
                            }

                            if (flag == 0 || tl[0] == 0)
                            {
                                break;
                            }
                            else
                            {
                                tl[0]--;
                                tr[0]--;
                                goto Top;
                            }
                        }

                        flag = 0;

                        while (true)
                        {
                            for (int k = bl[1]; k <= br[1]; k++)
                            {
                                if (binImagePadded.Data[bl[0], k, 0] == 0)
                                {
                                    flag = 1;
                                    break;
                                }
                            }

                            if (flag == 0 || bl[0] == binImage.Height)
                            {
                                break;
                            }
                            else
                            {
                                bl[0]++;
                                br[0]++;
                                goto Top;
                            }
                        }

                        flag = 0;

                        while (true)
                        {
                            for (int k = tl[0]; k <= bl[0]; k++)
                            {
                                if (binImagePadded.Data[k, tl[1], 0] == 0)
                                {
                                    flag = 1;
                                    break;
                                }
                            }

                            if (flag == 0 || bl[1] == 0)
                            {
                                break;
                            }
                            else
                            {
                                tl[1]--;
                                bl[1]--;
                                goto Top;
                            }
                        }

                        flag = 0;

                        while (true)
                        {
                            for (int k = tr[0]; k <= br[0]; k++)
                            {
                                if (binImagePadded.Data[k, tr[1], 0] == 0)
                                {
                                    flag = 1;
                                    break;
                                }
                            }

                            if (flag == 0 || br[1] == binImage.Width)
                            {
                                break;
                            }
                            else
                            {
                                tr[1]++;
                                br[1]++;
                                goto Top;
                            }
                        }


                        for (int m = tl[0]; m <= bl[0]; m++) //marking every pixel in feature rect as 1 
                        {
                            for (int n = tl[1]; n <= br[1]; n++)
                            {
                                visited[m, n] = 1;
                            }
                        }

                        //tl, tr, bl, br
                        int[] coords = new int[8];
                        coords[0] = tl[0]; coords[1] = tl[1];
                        coords[2] = tr[0]; coords[3] = tr[1];
                        coords[4] = bl[0]; coords[5] = bl[1];
                        coords[6] = br[0]; coords[7] = br[1];
                        candidateLogos.Add(coords);
                    }
                }
            }

            //Image<Gray, Byte> logoCandidatesVisual = binImagePadded;

            Bitmap logoCandidatesColoured = new Bitmap(binImagePadded.Width, binImagePadded.Height);
            Bitmap finalCandidatesColoured = new Bitmap(binImagePadded.Width, binImagePadded.Height);

            for (int i = 1; i < logoCandidatesColoured.Height - 1; i++)
            {
                for (int j = 1; j < logoCandidatesColoured.Width - 1; j++)
                {


                    if (binImagePadded.Data[i, j, 0] == 255)
                    {
                        logoCandidatesColoured.SetPixel(j, i, Color.White);
                        finalCandidatesColoured.SetPixel(j, i, Color.White);
                    }
                    else
                    {
                        logoCandidatesColoured.SetPixel(j, i, Color.Black);
                        finalCandidatesColoured.SetPixel(j, i, Color.Black);
                    }
                }
            }

            //finalCandidatesColoured = logoCandidatesColoured;

            drawRectangles(logoCandidatesColoured, candidateLogos, name + "_3_inititalCandidates", 1);

            List<int[]> finalCandidateLogos = new List<int[]>();
            finalCandidateLogos = EliminateCandidates(candidateLogos, binImage.Height, binImage.Width);


            drawRectangles(finalCandidatesColoured, finalCandidateLogos, name + "_4_finalCandidates", 0);

            //extractCandidates(binImagePadded, finalCandidateLogos, name);

            extractGreyScaleCandidates(origImagePadded, finalCandidateLogos, name);

        }

        private void extractGreyScaleCandidates(Image<Gray, Byte> image, List<int[]> finalCandidates, String name)
        {
            int logoNo = 0;
            String logoNumber = "";

            foreach (int[] coordSet in finalCandidates)
            {
                logoNo++;
                logoNumber = logoNo.ToString();

                int logoWidth = coordSet[3] - coordSet[1];
                int logoHeight = coordSet[4] - coordSet[0];
                //int logoArea = logoWidth * logoHeight;
                //double areaPercent = (double)logoArea / (double)dim;

                //Console.Write(String.Format("{0} {1} {2} {3}", coordSet[1], coordSet[3], coordSet[0], coordSet[4]));

                Image<Gray, Byte> logo = new Image<Gray, Byte>(logoWidth, logoHeight);

                //Console.WriteLine(String.Format("{0} {1} {2}", logoWidth, logoHeight, areaPercent));

                for (int i = coordSet[0]; i < coordSet[4]; i++)
                {
                    for (int j = coordSet[1]; j < coordSet[3]; j++)
                    {
                        logo.Data[i - coordSet[0], j - coordSet[1], 0] = image.Data[i, j, 0];
                    }
                }

                logo.Save
                    ("C:\\Users\\Lenvo\\Documents\\Visual Studio 2013\\Projects\\iaproj\\output\\" + name + "_5_LogoGrey" + logoNumber + ".jpg");

                pictureBox1.Image = logo.ToBitmap();
                //RunSIFT(logo, logoNumber, name);

                doGreyscaleHistogramMatching(logo, logoNumber, name);
            }
        }

        private void doGreyscaleHistogramMatching(Image<Gray, Byte> logo, String logoNo, String name)
        {
            String[] companyNames = new String[20] 
            {"Fedex", "Bright Bobbin", "SBI", "Audi", "Apple", "Exnora", "AMEnglish", "Ford", "Peterson Co.", "Ecotronics",
                "APA", "TubeA", "Picasso", "Vibgyor", "Kodak", "Yahoo", "Unilever", "Captor", "Tumblr", "Twitter"}; 

            List<KeyValuePair<int, double>> scores = new List<KeyValuePair<int, double>>() { };
            double[] histQ = ComputeHistogram(logo);
            //String query = "3_6.jpg";

            for (int i = 1; i <= 8; i++)
            {
                Image<Gray, byte> logoTester = new Image<Gray, byte>
                    ("C:\\Users\\Lenvo\\Documents\\Visual Studio 2013\\Projects\\iaproj\\input\\LogoDB\\" + i + ".jpg");
                double[] histTester = ComputeHistogram(logoTester);
                double score = compareHistograms(histQ, histTester);

                //scores.Insert(0, new KeyValuePair<string, double>(i + "_" + j, matchScore))
                scores.Add(new KeyValuePair<int, double>(i, score));
            }

            scores.Sort((firstPair, nextPair) =>
            {
                return nextPair.Value.CompareTo(firstPair.Value);
            }
            );

            foreach (KeyValuePair<int, double> pair in scores)
            {
                Console.WriteLine(string.Format("Label: {0} Match-Value: {1}", pair.Key, pair.Value));
            }

            //int first = 0; 
            foreach (KeyValuePair<int, double> pair in scores)
            {

                //Console.Write("Hello");
                Console.WriteLine(string.Format("The max match score for the query image {0} is with {1}. \nHence, the label is {2}.", name, pair.Key, companyNames[pair.Key - 1]));
                textBox1.AppendText(companyNames[pair.Key - 1]);
                break;
            }
        }

        private double compareHistograms(double[] h1, double[] h2)
        {
            int count = 0;

            for (int i = 0; i < 256; i++)
            {
                if (Math.Abs(h1[i] - h2[i]) < 0.001)
                {
                    count++;
                }
            }

            double score = (double)count / (double)256;

            return score;
        }


        private void extractCandidates(Image<Gray, Byte> image, List<int[]> finalCandidates, String name)
        {
            int logoNo = 0;
            String logoNumber = "";

            foreach (int[] coordSet in finalCandidates)
            {
                logoNo++;
                logoNumber = logoNo.ToString();

                int logoWidth = coordSet[3] - coordSet[1];
                int logoHeight = coordSet[4] - coordSet[0];
                //int logoArea = logoWidth * logoHeight;
                //double areaPercent = (double)logoArea / (double)dim;

                //Console.Write(String.Format("{0} {1} {2} {3}", coordSet[1], coordSet[3], coordSet[0], coordSet[4]));

                Image<Gray, Byte> logo = new Image<Gray, Byte>(logoWidth, logoHeight);

                //Console.WriteLine(String.Format("{0} {1} {2}", logoWidth, logoHeight, areaPercent));

                for (int i = coordSet[0]; i < coordSet[4]; i++)
                {
                    for (int j = coordSet[1]; j < coordSet[3]; j++)
                    {
                        logo.Data[i - coordSet[0], j - coordSet[1], 0] = image.Data[i, j, 0];
                    }
                }

                logo.Save
                    ("C:\\Users\\Lenvo\\Documents\\Visual Studio 2013\\Projects\\iaproj\\output\\" + name + "Logo" + logoNumber + ".png");

                Image<Gray, Byte> logoMedianSmooth = new Image<Gray, byte>(logo.Size);
                logoMedianSmooth = logo.SmoothMedian(3);

                logoMedianSmooth.Save
                    ("C:\\Users\\Lenvo\\Documents\\Visual Studio 2013\\Projects\\iaproj\\output\\" + name + "Logo" + logoNumber + "_SmoothMedian3.png");
            }
        }

        private List<int[]> EliminateCandidates(List<int[]> candidateLogos, int h, int w)
        {
            int dim = h * w;

            List<int[]> finalCandidateLogos = new List<int[]>();
            Console.Write(dim);
            foreach (int[] coordSet in candidateLogos)
            {
                int logoWidth = coordSet[3] - coordSet[1];
                int logoHeight = coordSet[4] - coordSet[0];
                int logoArea = logoWidth * logoHeight;
                double areaPercent = (double)logoArea / (double)dim;

                Console.WriteLine(String.Format("{0} {1} {2}", logoWidth, logoHeight, areaPercent));

                if (0.02 <= areaPercent && areaPercent <= 0.1) //eliminating very small and very big candidates
                {
                    Console.Write("Adding\n");
                    finalCandidateLogos.Add(coordSet);
                }
            }

            return finalCandidateLogos;
        }

        private void drawRectangles(Bitmap logoCandidatesColoured, List<int[]> candidateLogos, String name, int flag)
        {
            foreach (int[] coordSet in candidateLogos)
            {
                int[] x = coordSet;
                /*for (int i = 0; i <= 7; i++)
                {
                    Console.Write(String.Format("{0} ", x[i]));
                }*/

                for (int i = x[1]; i <= x[3]; i++)
                {
                    //logoCandidatesVisual.Data[x[0], i, 0] = 128;
                    logoCandidatesColoured.SetPixel(i, x[0], Color.Red);
                }

                for (int i = x[5]; i <= x[7]; i++)
                {
                    //logoCandidatesVisual.Data[x[4], i, 0] = 128;
                    logoCandidatesColoured.SetPixel(i, x[4], Color.Red);
                }

                for (int i = x[0]; i <= x[4]; i++)
                {
                    //logoCandidatesVisual.Data[i, x[1], 0] = 128;
                    logoCandidatesColoured.SetPixel(x[1], i, Color.Red);
                }

                for (int i = x[2]; i <= x[6]; i++)
                {
                    //logoCandidatesVisual.Data[i, x[3], 0] = 128;
                    logoCandidatesColoured.SetPixel(x[3], i, Color.Red);
                }
                Console.WriteLine();
            }

            logoCandidatesColoured.Save
                ("C:\\Users\\Lenvo\\Documents\\Visual Studio 2013\\Projects\\iaproj\\output\\" + name + ".jpg");

            if(flag==1)
            {
                pictureBox3.Image = logoCandidatesColoured;
            }
        }



        private Image<Gray, Byte> IterativeThresholding(double[] histogram, Image<Gray, byte> img, String name)
        {
            //Console.Write("Doing iterative thresholding\n");
            Image<Gray, byte> imageBinary = new Image<Gray, byte>(img.Width, img.Height);

            double averageIntensity = 0;

            for (int i = 0; i < 255; i++)
            {
                averageIntensity += (i * histogram[i]);
            }

            double threshold = averageIntensity;
            double newThreshold = averageIntensity;

            int[,] bins = new int[img.Height, img.Width];
            //Console.Write(threshold);

            do
            {
                threshold = newThreshold;
                int bin1Count = 0;
                int bin1CumulativeIntensity = 0;
                int bin2Count = 0;
                int bin2CumulativeIntensity = 0;

                for (int i = 0; i < img.Height; i++)
                {
                    for (int j = 0; j < img.Width; j++)
                    {

                        if (img.Data[i, j, 0] <= threshold)
                        {
                            bins[i, j] = 0;
                            bin1Count++;
                            bin1CumulativeIntensity += img.Data[i, j, 0];
                        }
                        else
                        {
                            bins[i, j] = 1;
                            bin2Count++;
                            bin2CumulativeIntensity += img.Data[i, j, 0];
                        }
                    }
                }

                double mu1;
                double mu2;

                if (bin1Count == 0)
                {
                    mu1 = 0;
                }
                else
                {
                    mu1 = bin1CumulativeIntensity / bin1Count;
                }

                if (bin2Count == 0)
                {
                    mu2 = 0;
                }
                else
                {
                    mu2 = bin2CumulativeIntensity / bin2Count;
                }


                newThreshold = (mu1 + mu2) / 2;
                //Console.Write(newThreshold);

            } while (Math.Abs(newThreshold - threshold) > 5);

            //Console.Write(String.Format("Final Threshold {0} ", newThreshold));
            double finalThreshold = newThreshold;

            for (int i = 0; i < img.Height; i++)
            {
                for (int j = 0; j < img.Width; j++)
                {
                    //Console.Write(String.Format("{0} ", img.Data[i,j,0]));
                    if (img.Data[i, j, 0] <= finalThreshold)
                    {
                        imageBinary.Data[i, j, 0] = 0;
                    }
                    else
                    {
                        imageBinary.Data[i, j, 0] = 255;
                    }
                }
            }

            imageBinary.Save("C:\\Users\\Lenvo\\Documents\\Visual Studio 2015\\Projects\\iaproj\\output\\" + name + "_2_binaryIterative.png");
            pictureBox2.Image = imageBinary.ToBitmap();
            
            return imageBinary;
        }

        private double[] ComputeHistogram(Image<Gray, byte> img)
        {
            //takes input image and returns probabilistic binning (array) of 256 intensity values
            int[] histogram = new int[256];
            double[] probabibilityHist = new double[256];

            for (int i = 0; i < img.Height; i++)
            {
                for (int j = 0; j < img.Width; j++)
                {
                    //Console.Write(String.Format("{0} {1} {2}", i, j, img.Data[i, j, 0]));
                    histogram[img.Data[i, j, 0]]++;
                }
            }

            double totalNoOfPixels = img.Height * img.Width;
            //Console.Write(String.Format("{0} \n", totalNoOfPixels));
            /*for (int i = 0; i < 255; i++)
            {
                Console.Write(String.Format("{0} ",histogram[i]));
            }*/
            //Console.WriteLine();
            for (int i = 0; i <= 255; i++)
            {
                int count = histogram[i];
                probabibilityHist[i] = ((double)(count)) / totalNoOfPixels;
            }

            /*for(int i=0; i<255; i++)
            {
                Console.Write(probabibilityHist[i]);
            }*/

            return probabibilityHist;
        }
        
        

    }
}

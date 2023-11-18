using Emgu.CV;
using Emgu.CV.Structure;
using DlibDotNet;
using System.Drawing;

class Program
{
    private static VideoCapture capture1;
    private static VideoCapture capture2;

    static void Main(string[] args)
    {
        string rtspUrl1 = "rtsp://";
        string rtspUrl2 = "rtsp://";


        capture1 = new VideoCapture(0); //0 is for deafult webcam
        //capture2 = new VideoCapture(rtspUrl2);

        //if (!capture1.IsOpened || !capture2.IsOpened)
        //{
        //    Console.WriteLine("Error: Unable to open one or both video streams.");
        //    return;
        //}

        Task.Run(() => ProcessStream(capture1, "Stream 1"));
        //Task.Run(() => ProcessStream(capture2, "Stream 2"));

        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();

        capture1.Dispose();
        capture2.Dispose();
        CvInvoke.DestroyAllWindows();
    }

    private static void ProcessStream(VideoCapture capture, string windowName)
    {
        using (var faceDetector = Dlib.GetFrontalFaceDetector())
        {
            while (true)
            {
                System.Threading.Timer frameTimer = new Timer(_ => ProcessFrame(capture, faceDetector, windowName), null, 0, 2000);


                Console.WriteLine($"Processing frames for {windowName}...");

                Console.ReadLine(); // Press Enter to exit

                // Release the timer
                frameTimer.Dispose();

            }
        }
    }
    private static void ProcessFrame(VideoCapture capture, FrontalFaceDetector faceDetector, string windowName)
    {
        using (Mat frame = new Mat())
        {
            capture.Read(frame);

            if (frame.IsEmpty)
            {
                Console.WriteLine($"End of video stream for {windowName}.");
                return;
            }

            // Convert the Emgu.CV Mat to Dlib.Image
            var bitmap = frame.ToBitmap();
            var dlibImage = DlibDotNet.Extensions.BitmapExtensions.ToArray2D<RgbPixel>(bitmap);

            // Perform face detection using Dlib
            var faces = faceDetector.Operator(dlibImage);

            foreach (var face in faces)
            {
                // Extract the region of interest (ROI) from the original frame
                System.Drawing.Rectangle roi = new System.Drawing.Rectangle(face.Left, face.Top, (int)face.Width, (int)face.Height);

          
                // Save the face image
                string savePath = $"{windowName}_face_{DateTime.Now:yyyyMMddHHmmssfff}.jpeg";
                CvInvoke.Imwrite(savePath, new Mat(frame, roi));

                // Draw a rectangle around the detected face
                //CvInvoke.Rectangle(frame, roi, new MCvScalar(0, 255, 0), 2);
            }

            // Display the frame with detected faces in a separate window for each stream
            //CvInvoke.Imshow(windowName, frame);
            CvInvoke.WaitKey(1);

            bitmap.Dispose();
            dlibImage.Dispose();
        }
    }
}

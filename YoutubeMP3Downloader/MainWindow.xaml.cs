using MediaToolkit;
using MediaToolkit.Model;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using VideoLibrary;
using System.Diagnostics;
using System.Net;
using System;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Text.RegularExpressions;

namespace YoutubeMP3Downloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string SaveDir;
        public MainWindow()
        {
            InitializeComponent();

        }

        private void Invalid_Link(string errorMessage)
        {
            URLBox.Text = errorMessage;
            var colorAnim = new ColorAnimationUsingKeyFrames()
            {
                KeyFrames = new ColorKeyFrameCollection
                {
                    new DiscreteColorKeyFrame(Colors.Red, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1.5))),
                    new DiscreteColorKeyFrame(Colors.Gray, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2))),
                    new DiscreteColorKeyFrame(Colors.Red, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2.5))),
                    new DiscreteColorKeyFrame(Colors.Gray, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(3))),
                }
            };

            var storyBoard = new Storyboard();

            storyBoard.Children.Add(colorAnim);
            Storyboard.SetTarget(storyBoard, URLBox);
            Storyboard.SetTargetProperty(storyBoard, new PropertyPath("(BorderBrush).(SolidColorBrush.Color)"));

            storyBoard.Begin();

        }
        private static bool IsYouTubeUrl(string testUrl)
        {
            return TestUrl(@"^(http://youtu\.be/([a-zA-Z0-9]|_)+($|\?.*)|https?://www\.youtube\.com/watch\?v=([a-zA-Z0-9]|_)+($|&).*)", testUrl);
        }
        private static bool TestUrl(string pattern, string testUrl)
        {
            Regex l_expression = new Regex(pattern, RegexOptions.IgnoreCase);

            return l_expression.Matches(testUrl).Count > 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {


            string url = URLBox.Text;


            //First we check to see if the link is valid

            Uri uriResult;
            bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            Debug.Print(result.ToString());


            //If link is valid, continue.
            if (result)
            {


                //We then check if it's a valid Youtube link.
                bool result2 = IsYouTubeUrl(url);
                if (result2)
                {




                    var youtube = YouTube.Default;
                    var vid = youtube.GetVideo(url);
                    Stream myStream;
                    SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                    saveFileDialog1.Filter = "File (*.*)|*.*";
                    saveFileDialog1.FilterIndex = 2;
                    saveFileDialog1.RestoreDirectory = true;

                    if (saveFileDialog1.ShowDialog() == true)
                    {
                        if ((myStream = saveFileDialog1.OpenFile()) != null)
                        {
                            SaveDir = saveFileDialog1.FileName;
                            myStream.Close();

                            //We can't convert the mp4 of the video to an mp3 in memory so we delete the mp4 after converting it to an mp3.
                            File.WriteAllBytes(SaveDir, vid.GetBytes());
                            var inputFile = new MediaFile { Filename = SaveDir };
                            var outputFile = new MediaFile { Filename = $"{SaveDir}.mp3" };

                            using (var engine = new Engine())
                            {
                                engine.GetMetadata(inputFile);

                                engine.Convert(inputFile, outputFile);

                            }
                            File.Delete(SaveDir);
                        }
                    }
                }
                else
                {
                    Invalid_Link("Invalid youtube URL!");
                }
            }
            else
            {
                //If link is not valid, call Invalid_Link 
                Invalid_Link("URL is invalid!");
            }
        }
    }
}

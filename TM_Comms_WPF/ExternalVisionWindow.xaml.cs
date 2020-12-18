using InterManage.Adapter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static TM_Comms.ExternalDetection;

namespace TM_Comms_WPF
{

    public partial class ExternalVisionWindow : Window
    {
        HttpListener HttpListener = new HttpListener();
        DetectionResponse DetectionResponse;


        public ExternalVisionWindow(Window owner)
        {
            Owner = owner;
            DataContext = App.Settings;

            InitializeComponent();

            AddNewAnnotation();

            LoadIPCombo();

            BtnStartListener.Content = "Start Listening";
            BtnStartListener.Background = Brushes.LightSalmon;

        }

        private void LoadIPCombo()
        {
            List<AdapterWrapper> adapterWrappers = new AdapterManager().GetFilteredAdapterList(true, true);
            if(adapterWrappers == null || adapterWrappers.Count == 0) return;

            CmbIPAddresses.Items.Clear();
            CmbIPAddresses.Items.Add("*");

            foreach(AdapterWrapper adapter in adapterWrappers)
            {
                AdapterConfigurationWrapper config = new AdapterConfigurationManager().GetAdapter(adapter.InterfaceIndex);

                if(config.IPEnabled)
                {
                    CmbIPAddresses.Items.Add(config.IPAddress[0]);
                }

            }
        }

        private int AnnotationCount = 1;
        private void AddNewAnnotation()
        {
            Annotation annotation = new Annotation
            {
                box_cx = 500,
                box_cy = 500,
                box_h = 1000,
                box_w = 1000,
                label = $"Annotation {AnnotationCount++}",
            };
            annotation.PropertyChanged += Annotation_PropertyChanged;
            StkResponse.Children.Add(new ExternalVisionResultControl(annotation));
            annotation.score = 1.0f;
        }

        private void Annotation_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            lock(GetContext_lockobject)
            {
                DetectionResponse = new DetectionResponse(StkResponse.Children.Count)
                {
                    message = "success"
                };
                for(int i = 0; i < StkResponse.Children.Count; i++)
                {
                    DetectionResponse.annotations[i] = ((ExternalVisionResultControl)StkResponse.Children[i]).Annotation;
                }
            }

        }

        public static BitmapImage ProcessMessageForImage(Stream message)
        {
            MultipartParser parser = new MultipartParser(message);
            if (parser.Success)
            {
                MemoryStream ms = new MemoryStream(parser.FileContents);

                var imageSource = new BitmapImage();
                imageSource.BeginInit();
                imageSource.StreamSource = ms;
                imageSource.EndInit();
                imageSource.Freeze();
                return imageSource;

            }
            else
                return null;

            //Match boundry = Regex.Match(message, @"(?<=boundary=).*");
            //string boundaryStart = $"(?<=--{boundry.Value})(?s).*";

            //Match body = Regex.Match(message, boundaryStart);

            //TextReader textReader = new StreamReader(message);
            //string sLine = textReader.ReadLine();
            //Regex regex = new Regex("(^-+)|(^content-)|(^submit)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

            //StringBuilder sb = new StringBuilder();

            //while(sLine != null)
            //{
            //    if(!regex.Match(sLine).Success)
            //    {
            //        break;
            //    }
            //    sLine = textReader.ReadLine();
            //}
            //FileStream file = File.OpenWrite("test.jpg");
            //byte[] ar = Encoding.ASCII.GetBytes(textReader.ReadToEnd());
            //file.Write(ar,0,ar.Length);
            //file.Close();
            ////byte[] imageBytes = Convert.FromBase64String();
            ////MemoryStream ms = new MemoryStream(imageBytes);



            //return image;
            //}

        }

        object GetContext_lockobject = new object();
        private void GetContext(IAsyncResult state)
        {
            lock(GetContext_lockobject)
            {
                if(!HttpListener.IsListening)

                    return;

                HttpListenerContext cont = HttpListener.EndGetContext(state);

                if(cont.Request.HttpMethod.Equals("POST"))
                {
                    BitmapImage test = ProcessMessageForImage(cont.Request.InputStream);

                    if(test != null)
                    {
                        WriteableBitmap wbit = new WriteableBitmap(test);

                        wbit.DrawRectangle(
                            DetectionResponse.annotations[0].box_cx - (DetectionResponse.annotations[0].box_h / 2),
                            DetectionResponse.annotations[0].box_cy - (DetectionResponse.annotations[0].box_w / 2),
                            DetectionResponse.annotations[0].box_cx + DetectionResponse.annotations[0].box_h,
                            DetectionResponse.annotations[0].box_cy + DetectionResponse.annotations[0].box_w,
                            Colors.Orange);
                        wbit.Freeze();

                        Dispatcher.BeginInvoke(DispatcherPriority.Render,
                            (Action<string, string>)((string addr, string rawUrl) =>
                            {
                                ImgMain.Source = wbit;

                                StackPanel sp = new StackPanel
                                {
                                    Orientation = Orientation.Horizontal
                                };

                                sp.Children.Add(new Label() { Content = "Remote IP:" });
                                sp.Children.Add(new TextBox() { Text = addr, IsReadOnly = true, VerticalContentAlignment= VerticalAlignment.Center, BorderBrush = null });

                                sp.Children.Add(new Label() { Content = "Request URL:" });
                                sp.Children.Add(new TextBox() { Text = rawUrl, IsReadOnly = true, VerticalContentAlignment = VerticalAlignment.Center, BorderBrush = null });

                                sp.Children.Add(new Label() { Content = "Width:" });
                                sp.Children.Add(new TextBox() { Text = wbit.PixelWidth.ToString(), IsReadOnly = true, VerticalContentAlignment = VerticalAlignment.Center, BorderBrush = null });

                                sp.Children.Add(new Label() { Content = "Height:" });
                                sp.Children.Add(new TextBox() { Text = wbit.PixelHeight.ToString(), IsReadOnly = true, VerticalContentAlignment = VerticalAlignment.Center, BorderBrush = null });

                                LblImageSize.Content = sp;

                            }), cont.Request.RemoteEndPoint.Address.ToString(), cont.Request.RawUrl);
                    }

                    string json = JsonConvert.SerializeObject(DetectionResponse, Formatting.None);
                    byte[] buf = Encoding.ASCII.GetBytes(json);

                    cont.Response.ContentLength64 = buf.Length;
                    cont.Response.OutputStream.Write(buf, 0, buf.Length);
                    cont.Response.Close();
                }
                else if(cont.Request.HttpMethod.Equals("GET"))
                    cont.Response.Close();
                else if(cont.Request.HttpMethod.Equals("HEAD"))
                    cont.Response.Close();

                HttpListener.BeginGetContext(GetContext, new object());
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            lock(GetContext_lockobject)
                HttpListener.Stop();
        }

        private void BtnStartListener_Click(object sender, RoutedEventArgs e)
        {
            if(HttpListener.IsListening)
            {
                lock(GetContext_lockobject)
                    HttpListener.Stop();

                BtnStartListener.Content = "Start Listening";
                BtnStartListener.Background = Brushes.LightSalmon;
            }
            else
            {
                HttpListener.Prefixes.Clear();
                HttpListener.Prefixes.Add($"http://{App.Settings.ExternalVisionWindow_ListenAddress}:{App.Settings.ExternalVisionWindow_ListenPort}/");
                HttpListener.Start();
                HttpListener.BeginGetContext(GetContext, new object());

                BtnStartListener.Content = "Stop Listening";
                BtnStartListener.Background = Brushes.LightBlue;
            }

        }

        private void BtnSaveImage_Click(object sender, RoutedEventArgs e)
        {
            SaveImage("image.png");
        }

        private void SaveImage(string fileName)
        {
            WriteableBitmap bit = null;
            lock(GetContext_lockobject)
            {
                 bit = ((WriteableBitmap)ImgMain.Source).Clone();


            }
            if(bit == null) return;

            bit.Freeze();
            using(FileStream stream5 = new FileStream(fileName, FileMode.Create))
            {
                PngBitmapEncoder encoder5 = new PngBitmapEncoder();
                encoder5.Frames.Add(BitmapFrame.Create(bit));
                encoder5.Save(stream5);
            }
        }
        private void BtnAddManualResponse_Click(object sender, RoutedEventArgs e)
        {
            AddNewAnnotation();
        }
    }
}

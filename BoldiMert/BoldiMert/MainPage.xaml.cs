using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Essentials;



namespace BoldiMert
{

    public partial class MainPage : ContentPage
    {
        System.Timers.Timer T30s = new System.Timers.Timer();
        List<double> MeanList = new List<double>();
        List<int> PeakPos = new List<int>();
        List<double> PeakHeight = new List<double>();
        List<double> prominence = new List<double>();
        //List<double> Control_List = new List<double> {0,0,14,0,0,2,0,0,0,0,2,0,0,2,0,14,0,0,14,0,0};
        List<double> needed_peaks = new List<double>();
        double LastX = 0, LastY = 0, LastZ = 0;
        bool open = false;
        //double freqEnd = 0.0;
        //string results = "Result: ";
        //double freq = 0.0;
       


        public MainPage()
        {
            InitializeComponent();

            Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;

            T30s.Interval = 30000;
            T30s.Elapsed += T30s_Elapsed;
            Accelerometer.Start(SensorSpeed.Game);

            Device.BeginInvokeOnMainThread(() => { });
            

        }

        

        private void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
        {
            var data = e.Reading;
            LastX = data.Acceleration.X;
            LastY = data.Acceleration.Y;
            LastZ = data.Acceleration.Z;

            if (open == true)
            {
                MeanList.Add(Math.Sqrt(Math.Pow(LastX, 2) + Math.Pow(LastY, 2) + Math.Pow(LastZ, 2)));


                Device.BeginInvokeOnMainThread(() => { RESULTS.Text = "" + Math.Round(MeanList.Count()/50.0,0); });
            }
        }

       
            

        
       

        private void T30s_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            
            T30s.Stop();
            open = false;
            Analyse_Peaks(MeanList);
        }



        private void Start_Clicked(object sender, EventArgs e)
        {
            MeanList.Clear();
            needed_peaks.Clear();
            PeakHeight.Clear();
            PeakPos.Clear();
            prominence.Clear();
            T30s.Start();
            open = true;
            Start.IsVisible = false;
        }

        
 
        private void Analyse_Peaks(List<double> accelerationValues)
        {
            double max_peak = accelerationValues.Max();
            for (int i = 1; i < accelerationValues.Count - 1; i++)
            {
                if (accelerationValues[i] > accelerationValues[i - 1] && accelerationValues[i] > accelerationValues[i + 1])
                {
                    PeakPos.Add(i);
                    PeakHeight.Add(accelerationValues[i]);
                }
            }
            for(int i = 0; i < PeakHeight.Count(); i++)
             {
                int start = 0; int end = accelerationValues.Count()-1;

                if (i < accelerationValues.Count() - 1)
                {
                    for (int j = i; j < PeakHeight.Count(); j++)
                        if (PeakHeight[j] > PeakHeight[i])
                        {
                            end = PeakPos[j];
                            break;
                        }
                }

                if (i > 0)
                {
                    for (int j = i - 1; j > 0; j--)
                        if (PeakHeight[j] > PeakHeight[i])
                        {
                            start = PeakPos[j];
                            break;
                        }
                }

                double prom1= PeakHeight[i] - accelerationValues.GetRange(start, PeakPos[i]-start).Min();
                double prom2 = PeakHeight[i] - accelerationValues.GetRange(PeakPos[i], end - PeakPos[i] ).Min();

                prominence.Add(Math.Min(prom1, prom2));
            }
            double prompeaks = 0;
            for(int k = 0; k<prominence.Count();k++)
            {
                if(prominence[k]> 0.4 * (accelerationValues.Max() - 1.0))
                    {
                    prompeaks++;
                    needed_peaks.Add(PeakPos[k]);
                    }
            }

            double freq1 = 0, freq2 = 0, meanfreq = 0;
            foreach (double d in needed_peaks)
                if (d < 250)
                    freq1++;
                else if (d > accelerationValues.Count() - 250)
                    freq2++;

            freq1 /= 5.0;
            freq2 /= 5.0;
            meanfreq = prompeaks / 30.0;
            double drop = (freq1 - freq2) / freq1;


                   
                    
            double signaltonoise =  prompeaks / prominence.Count();
            Device.BeginInvokeOnMainThread(() => { Start.IsVisible = true; RESULTS.Text="Mean frequency [Hz]: " + Math.Round(meanfreq,1) + "\r\nFrequency loss [%]: " + Math.Round(drop*100.0,0) + "\r\nSignal2Noise [%]: " + Math.Round(100.0*signaltonoise,0); });

            //{0,0,4,0,0,2,0,0,0,0,2,0,0,2,0,4,0,0,4,0,0};
        }

    }
}

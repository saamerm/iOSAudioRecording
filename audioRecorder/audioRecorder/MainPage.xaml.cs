using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace audioRecorder
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        void Start_Button_Clicked(System.Object sender, System.EventArgs e)
        {
            var stream = DependencyService.Get<IAudioStream>();
            stream.Start();
            stream.OnActiveChanged += Stream_OnActiveChanged;
            stream.OnBroadcast += Stream_OnBroadcast;
            stream.OnException += Stream_OnException;
        }

        private void Stream_OnException(object sender, Xamarin.Forms.Internals.EventArg<Exception> e)
        {
            Console.WriteLine("Exception" + e.Data.Message);
        }

        private void Stream_OnBroadcast(object sender, Xamarin.Forms.Internals.EventArg<byte[]> e)
        {
            Console.WriteLine("Broadcast");
            Console.WriteLine(e.Data);
            Console.WriteLine(BitConverter.ToString(e.Data));
            var stream = DependencyService.Get<IAudioStream>();
            stream.PlayByteArrayAsync(e.Data);
        }

        private void Stream_OnActiveChanged(object sender, Xamarin.Forms.Internals.EventArg<bool> e)
        {
            Console.WriteLine("OnActiveChanged");
            Console.WriteLine(e.Data);
        }

        void Stop_Button_Clicked(System.Object sender, System.EventArgs e)
        {
            var stream = DependencyService.Get<IAudioStream>();
            stream.Stop();
        }
    }
}

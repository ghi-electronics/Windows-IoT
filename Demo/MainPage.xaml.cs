using GHI.Athens.Gadgeteer;
using GHI.Athens.Modules;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace GHI.Athens.Demo {
	public sealed partial class MainPage : Windows.UI.Xaml.Controls.Page {
		private DispatcherTimer timer;
		private TheProfessor mainboard;
		private HubAP5 hub;
		private Button button;
		private LEDStrip ledStrip;
		private TempHumidSI70 temp;
		private LightSense lightSense;

		public MainPage() {
			this.InitializeComponent();

			Task.Run(async () => this.mainboard = await Module.Create<TheProfessor>())
				.ContinueWith(async t => this.hub = await Module.Create<HubAP5>(this.mainboard.GetProvidedSocket(3))).Unwrap()
				.ContinueWith(async t => this.button = await Module.Create<Button>(this.hub.GetProvidedSocket(6))).Unwrap()
				.ContinueWith(async t => this.ledStrip = await Module.Create<LEDStrip>(this.hub.GetProvidedSocket(8))).Unwrap()
				.ContinueWith(async t => this.lightSense = await Module.Create<LightSense>(this.hub.GetProvidedSocket(1))).Unwrap()
				.ContinueWith(async t => this.temp = await Module.Create<TempHumidSI70>(this.hub.GetProvidedSocket(4))).Unwrap()
				.ContinueWith(t => this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, this.ProgramStarted));
		}

		private void ProgramStarted() {
			this.timer = new DispatcherTimer();
			this.timer.Interval = TimeSpan.FromMilliseconds(1000);
			this.timer.Tick += this.Timer_Tick;
			this.timer.Start();
        }

		private void Timer_Tick(object sender, object e) {
			this.timer.Stop();

			//if (this.button.IsPressed())
			//	this.ledStrip.TurnAllOn();
			//else
			//	this.ledStrip.TurnAllOff();
			//
			//Debug.WriteLine($"{this.lightSense.GetReading():N2}");
			Debug.WriteLine($"{this.temp.TakeMeasurement()}");

			this.timer.Start();
		}
	}
}
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
		private TheProfessor hat;
		private HubAP5 hub;
		private Button button;
		private LEDStrip ledStrip;
		private TempHumidSI70 temp;
		private LightSense lightSense;

		public MainPage() {
			this.InitializeComponent();

			Task.Run(async () => {
				this.hat = await Module.CreateAsync<TheProfessor>();
				this.hub = await Module.CreateAsync<HubAP5>(this.hat.GetProvidedSocket(3));
				this.button = await Module.CreateAsync<Button>(this.hub.GetProvidedSocket(6));
				this.ledStrip = await Module.CreateAsync<LEDStrip>(this.hub.GetProvidedSocket(8));
				this.lightSense = await Module.CreateAsync<LightSense>(this.hub.GetProvidedSocket(1));
				this.temp = await Module.CreateAsync<TempHumidSI70>(this.hub.GetProvidedSocket(4));

				await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, this.ProgramStarted);
            });
		}

		private void ProgramStarted() {
			this.timer = new DispatcherTimer();
			this.timer.Interval = TimeSpan.FromMilliseconds(250);
			this.timer.Tick += this.Timer_Tick;
			this.timer.Start();
		}

		private void Timer_Tick(object sender, object e) {
			this.timer.Stop();

			if (this.button.IsPressed())
				this.ledStrip.TurnAllOn();
			else
				this.ledStrip.TurnAllOff();

			Debug.WriteLine($"{this.lightSense.GetReading():N2}");
			Debug.WriteLine($"{this.temp.TakeMeasurement()}");

			this.timer.Start();
		}
	}
}
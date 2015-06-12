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

		public MainPage() {
			this.InitializeComponent();

			Task.Run(async () => {
				this.hat = await Module.CreateAsync<TheProfessor>();
				this.hub = await Module.CreateAsync<HubAP5>(this.hat.GetProvidedSocket(3));

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

			this.timer.Start();
		}
	}
}
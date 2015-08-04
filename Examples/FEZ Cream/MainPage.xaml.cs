using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using GT = GHIElectronics.UAP.Gadgeteer;
using GTM = GHIElectronics.UAP.Gadgeteer.Modules;

namespace GHIElectronics.UAP.Examples.FEZCream {
	public sealed partial class MainPage : Page {
		private GTM.FEZCream mainboard;
		private GTM.LEDStrip ledStrip;
		private GTM.Button button;
		private Timer timer;

		public MainPage() {
			this.InitializeComponent();

			Task.Run(async () => {
				this.mainboard = await GT.Module.CreateAsync<GTM.FEZCream>();
				this.ledStrip = await GT.Module.CreateAsync<GTM.LEDStrip>(this.mainboard.GetProvidedSocket(4));
				this.button = await GT.Module.CreateAsync<GTM.Button>(this.mainboard.GetProvidedSocket(3));

				this.ProgramStarted();
			});
		}

		private void ProgramStarted() {
			this.timer = new Timer(async s => {
				var pressed = this.button.IsPressed();

				this.ledStrip.SetAll(pressed);

				await this.LedsTextBox.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.LedsTextBox.Text = pressed ? "Pressed" : "Not pressed");
			}, null, 100, 100);
		}
	}
}
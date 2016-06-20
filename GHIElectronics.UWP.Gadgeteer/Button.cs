using GHIElectronics.UWP.GadgeteerCore;
using System.Threading.Tasks;
using Windows.Foundation;

using GSI = GHIElectronics.UWP.GadgeteerCore.SocketInterfaces;

namespace GHIElectronics.UWP.Gadgeteer.Modules {
    public class Button : Module {
        public override string Name => "Button";
        public override string Manufacturer => "GHI Electronics, LLC";

        private GSI.DigitalIO inputPin;
        private GSI.DigitalIO outputPin;

        public event TypedEventHandler<Button, object> Pressed;
        public event TypedEventHandler<Button, object> Released;

        protected async override Task Initialize(ISocket parentSocket) {
            this.outputPin = await parentSocket.CreateDigitalIOAsync(SocketPinNumber.Four, false);
            this.inputPin = await parentSocket.CreateDigitalIOAsync(SocketPinNumber.Three);

            this.inputPin.ValueChanged += (s, e) => {
                if (e.Value) {
                    this.Released?.Invoke(this, null);
                }
                else {
                    this.Pressed?.Invoke(this, null);
                }
            };
        }

        public bool IsPressed() {
            return !this.inputPin.Read();
        }

        public void TurnOnLed() {
            this.outputPin.SetHigh();
        }

        public void TurnOffLed() {
            this.outputPin.SetLow();
        }

        public void SetLed(bool state) {
            this.outputPin.Write(state);
        }
    }
}
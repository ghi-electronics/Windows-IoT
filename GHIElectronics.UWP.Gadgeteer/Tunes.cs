using GHIElectronics.UWP.GadgeteerCore;
using System;
using System.Threading.Tasks;

using GSI = GHIElectronics.UWP.GadgeteerCore.SocketInterfaces;

namespace GHIElectronics.UWP.Gadgeteer.Modules {
    public class Tunes : Module {
        public override string Name => "Tunes";
        public override string Manufacturer => "GHI Electronics, LLC";

        private GSI.PwmOutput pwm;

        protected async override Task Initialize(ISocket parentSocket) {
            this.pwm = await parentSocket.CreatePwmOutputAsync(SocketPinNumber.Nine);
        }

        public void Play(int frequency) {
            if (frequency < 0) throw new ArgumentOutOfRangeException(nameof(frequency), "Must not be negative.");

            if (frequency == 0) {
                this.Stop();
            }
            else {
                this.pwm.Set(frequency, 0.5);
                this.pwm.Enabled = true;
            }
        }

        public void Stop() {
            this.pwm.Enabled = false;
        }
    }
}
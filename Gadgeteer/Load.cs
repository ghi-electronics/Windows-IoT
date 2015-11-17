using GHIElectronics.UWP.GadgeteerCore;
using System.Threading.Tasks;

using GSI = GHIElectronics.UWP.GadgeteerCore.SocketInterfaces;

namespace GHIElectronics.UWP.Gadgeteer.Modules {
    public class Load : Module {
        public override string Name => "Load";
        public override string Manufacturer => "GHI Electronics, LLC";

        public GSI.DigitalIO P1 { get; private set; }
        public GSI.DigitalIO P2 { get; private set; }
        public GSI.DigitalIO P3 { get; private set; }
        public GSI.DigitalIO P4 { get; private set; }
        public GSI.DigitalIO P5 { get; private set; }
        public GSI.DigitalIO P6 { get; private set; }
        public GSI.DigitalIO P7 { get; private set; }

        protected async override Task Initialize(ISocket parentSocket) {
            this.P1 = await parentSocket.CreateDigitalIOAsync(SocketPinNumber.Three, false);
            this.P2 = await parentSocket.CreateDigitalIOAsync(SocketPinNumber.Four, false);
            this.P3 = await parentSocket.CreateDigitalIOAsync(SocketPinNumber.Five, false);
            this.P4 = await parentSocket.CreateDigitalIOAsync(SocketPinNumber.Six, false);
            this.P5 = await parentSocket.CreateDigitalIOAsync(SocketPinNumber.Seven, false);
            this.P6 = await parentSocket.CreateDigitalIOAsync(SocketPinNumber.Eight, false);
            this.P7 = await parentSocket.CreateDigitalIOAsync(SocketPinNumber.Nine, false);
        }
    }
}
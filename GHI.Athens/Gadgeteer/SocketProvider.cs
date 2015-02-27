using System.Collections.Generic;

namespace GHI.Athens.Gadgeteer {
	public abstract class SocketProvider {
		private Dictionary<uint, Socket> providedSockets;

		public abstract string Name { get; }
		public abstract string Manufacturer { get; }

		public IReadOnlyDictionary<uint, Socket> ProvidedSockets { get { return this.providedSockets; } }

		protected SocketProvider() {
			this.providedSockets = new Dictionary<uint, Socket>();
		}

		protected Socket CreateSocket(uint socketNumber) {
			var socket = new Socket(socketNumber);

			this.providedSockets.Add(socket.Number, socket);

			return socket;
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace GHI.Athens.Gadgeteer {
	public enum SocketPinNumber {
		Three,
		Four,
		Five,
		Six,
		Seven,
		Eight,
		Nine
	}

	public enum SocketType {
		X,
		Y,
		I,
		A,
		P,
		S,
		U
	}

	public sealed class Socket {
		private Dictionary<SocketPinNumber, GpioPinDefinition> pinDefinitions;
		private HashSet<SocketType> supportedTypes;

		internal Socket(uint socketNumber) {
			this.pinDefinitions = new Dictionary<SocketPinNumber, GpioPinDefinition>();
			this.supportedTypes = new HashSet<SocketType>();

			this.Number = socketNumber;
		}

		public void EnsureTypeIsSupported(SocketType type) {
			if (!this.IsTypeSupported(type))
				throw new Exception();
		}

		public bool IsTypeSupported(SocketType type) {
			return (type == SocketType.X ? this.supportedTypes.Contains(SocketType.Y) : false) || this.supportedTypes.Contains(type);
		}

		public void AddSupportedTypes(params SocketType[] types) {
			foreach (var t in types)
				this.supportedTypes.Add(t);
		}

		public void AddGpioPinDefinition(SocketPinNumber pinNumber, GpioPinDefinition pinDefinition) {
			this.pinDefinitions[pinNumber] = pinDefinition;
		}

		public IReadOnlyDictionary<SocketPinNumber, GpioPinDefinition> GpioPinDefinitions { get { return this.pinDefinitions; } }
		public IReadOnlyList<SocketType> SupportedTypes { get { return this.supportedTypes.ToList(); } }

		public uint Number { get; }

		public string I2CDeviceId { get; set; }
		public string SpiDeviceId { get; set; }
		public string SerialDeviceId { get; set; }
	}
}
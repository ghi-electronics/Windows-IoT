using System;

namespace GHI.Athens.Gadgeteer {
	public class SocketInterfaceCreationException : Exception {
		public SocketInterfaceCreationException() { }
		public SocketInterfaceCreationException(string message) : base(message) { }
		public SocketInterfaceCreationException(string message, Exception inner) : base(message, inner) { }
	}

	public class UnsupportedSocketTypeException : Exception {
		public UnsupportedSocketTypeException() { }
		public UnsupportedSocketTypeException(string message) : base(message) { }
		public UnsupportedSocketTypeException(string message, Exception inner) : base(message, inner) { }
	}
}
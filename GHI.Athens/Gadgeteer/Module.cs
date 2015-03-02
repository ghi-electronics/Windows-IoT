using System;
using System.Threading.Tasks;

namespace GHI.Athens.Gadgeteer {
	public abstract class Module {
		public abstract string Name { get; }
		public abstract string Manufacturer { get; }
		protected virtual int RequiredSockets { get; } = 1;

		protected Module() {

		}

		protected virtual Task Initialize(Socket socket) {
			throw new InvalidOperationException("This module is not defined properly.");
		}

		protected virtual Task Initialize(params Socket[] parentSockets) {
			throw new InvalidOperationException("This module is not defined properly.");
		}

		public static async Task<T> Create<T>(params Socket[] parentSockets) where T : Module, new() {
			var module = new T();

			if (module.RequiredSockets != parentSockets.Length)
				throw new ArgumentException("Invalid number of sockets passed.", nameof(parentSockets));

			if (module.RequiredSockets == 1)
				await module.Initialize(parentSockets[0]);
			else
				await module.Initialize(parentSockets);

			return module;
		}
	}
}
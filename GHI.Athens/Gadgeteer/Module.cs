using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GHI.Athens.Gadgeteer {
	public abstract class Module {
		private Dictionary<uint, Socket> providedSockets;

		public abstract string Name { get; }
		public abstract string Manufacturer { get; }
		public virtual int RequiredSockets { get; } = 1;

		protected Module() {
			this.providedSockets = new Dictionary<uint, Socket>();
		}

		protected virtual Task Initialize() {
			throw new InvalidModuleDefinitionException($"This module does not overload the proper {nameof(this.Initialize)} method.");
		}

		protected virtual Task Initialize(Socket parentSocket) {
			throw new InvalidModuleDefinitionException($"This module does not overload the proper {nameof(this.Initialize)} method.");
		}

		protected virtual Task Initialize(params Socket[] parentSockets) {
			throw new InvalidModuleDefinitionException($"This module does not overload the proper {nameof(this.Initialize)} method.");
		}

		protected Socket AddProvidedSocket(uint socketNumber) {
			var socket = new Socket(socketNumber);

			this.providedSockets.Add(socket.Number, socket);

			return socket;
		}

		public Socket GetProvidedSocket(uint socketNumber) {
			if (!this.providedSockets.ContainsKey(socketNumber))
				throw new ArgumentException("That socket does not exist.", nameof(socketNumber));

			return this.providedSockets[socketNumber];
		}

		public static async Task<T> CreateAsync<T>(params Socket[] parentSockets) where T : Module, new() {
			var module = new T();

			if (module.RequiredSockets != parentSockets.Length)
				throw new ArgumentException($"Invalid number of sockets passed. Expected {module.RequiredSockets}.", nameof(parentSockets));

			if (module.RequiredSockets == 0)
				await module.Initialize();
			else if (module.RequiredSockets == 1)
				await module.Initialize(parentSockets[0]);
			else
				await module.Initialize(parentSockets);

			return module;
		}
	}
}
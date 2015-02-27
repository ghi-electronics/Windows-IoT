namespace GHI.Athens.Gadgeteer {
	public abstract class Module {
		public abstract string Name { get; }
		public abstract string Manufacturer { get; }

		protected Module() {

		}
	}
}
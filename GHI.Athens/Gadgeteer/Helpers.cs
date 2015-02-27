using System;
using System.Threading.Tasks;
using Windows.Foundation;

namespace GHI.Athens.Gadgeteer {
	public static class Helpers {
		public static T WaitForResults<T>(this Task<T> task) {
			task.RunSynchronously();

			return task.Result;
		}

		public static T WaitForResults<T>(this IAsyncOperation<T> operation) {
			return operation.AsTask().WaitForResults();
		}
	}
}
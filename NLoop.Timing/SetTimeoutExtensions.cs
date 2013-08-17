﻿using System;
using System.Threading;
using NLoop.Core;
using NLoop.Core.Utils;

namespace NLoop.Timing
{
	/// <summary>
	/// Contains the timeout extension methods for the <see cref="EventLoop"/>.
	/// </summary>
	public static class SetTimeoutExtensions
	{
		/// <summary>
		/// Invokes the <paramref name="callback"/> after <paramref name="timeout"/> elapsed.
		/// </summary>
		/// <param name="eventLoop">The <see cref="EventLoop"/> on which the timeout event will be executed.</param>
		/// <param name="callback">The callback which to invoke on timeout.</param>
		/// <param name="timeout">The timeout which to wait before <paramref name="callback"/> should be executed.</param>
		/// <returns>Returns a <see cref="IDisposable"/> which cancels the timer.</returns>
		/// <exception cref="ArgumentNullException">Thrown if one of the parameters is null.</exception>
		public static IDisposable SetTimeout(this EventLoop eventLoop, Action callback, TimeSpan timeout)
		{
			// validate arguments
			if (eventLoop == null)
				throw new ArgumentNullException("eventLoop");
			if (callback == null)
				throw new ArgumentNullException("callback");

			// create a resource managed by the event loop
			var cts = eventLoop.TrackResource((token, untrack) => {
				// create the timer which will schedule the callback
				var timer = new Timer(state => {
					// check if the operation was cancelled
					if (token.IsCancellationRequested)
						return;

					// set timeout only works once, so it does not longer have to be a managed resource
					untrack();

					// schedule the callback for execution
					eventLoop.Schedule(callback);
				}, null, timeout, Timeout.InfiniteTimeSpan);

				// dispose the timer if the cts is cancelled
				token.Register(timer.Dispose);

				// return the created timer
				return timer;
			});

			// return an action which cancels the cts and disposes the resource
			return new DisposeAction(() => {
				cts.Cancel();
				cts.Dispose();
			});
		}
		/// <summary>
		/// Invokes the <paramref name="callback"/> after every elapsed <paramref name="timeout"/>.
		/// </summary>
		/// <param name="eventLoop">The <see cref="EventLoop"/> on which the timeout event will be executed.</param>
		/// <param name="callback">The callback which to invoke on timeout.</param>
		/// <param name="timeout">The timeout which to wait before <paramref name="callback"/> should be executed.</param>
		/// <returns>Returns a <see cref="IDisposable"/> which cancels the timer.</returns>
		/// <exception cref="ArgumentNullException">Thrown if one of the parameters is null.</exception>
		public static IDisposable SetInterval(this EventLoop eventLoop, Action callback, TimeSpan timeout)
		{
			// validate arguments
			if (eventLoop == null)
				throw new ArgumentNullException("eventLoop");
			if (callback == null)
				throw new ArgumentNullException("callback");

			// create a resource managed by the event loop
			var cts = eventLoop.TrackResource((token, untrack) => {
				// create the timer which will schedule the callback
				var timer = new Timer(state => {
					// check if the operation was cancelled
					if (token.IsCancellationRequested)
						return;

					// schedule the callback for execution
					eventLoop.Schedule(callback);
				}, null, timeout, timeout);

				// dispose the timer if the cts is cancelled
				token.Register(timer.Dispose);

				// return the created timer
				return timer;
			});

			// return an action which cancels the cts and disposes the resource
			return new DisposeAction(() => {
				cts.Cancel();
				cts.Dispose();
			});
		}
	}
}
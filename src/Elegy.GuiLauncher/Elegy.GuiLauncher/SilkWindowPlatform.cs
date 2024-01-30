
using Silk.NET.Windowing;
using System;
using System.Collections.Generic;

namespace Elegy.GuiLauncher
{
	public class SilkWindowPlatform : IWindowPlatform
	{
		public bool IsViewOnly => false;

		public bool IsApplicable => throw new NotImplementedException();

		public void ClearContexts()
		{

		}

		public IWindow CreateWindow( WindowOptions opts )
		{
			throw new NotSupportedException();
		}

		public IMonitor GetMainMonitor()
		{
			throw new NotSupportedException();
		}

		public IEnumerable<IMonitor> GetMonitors()
		{
			throw new NotSupportedException();
		}

		public IView GetView( ViewOptions? opts = null )
		{
			throw new NotSupportedException();
		}

		public bool IsSourceOfView( IView view )
		{
			return view is SilkWindow;
		}
	}
}

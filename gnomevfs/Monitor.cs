//
// Monitor.cs: Class wrapping the gnome_vfs_monitor_* methods.
//
// Author:
//   Jeroen Zwartepoorte <jeroen@xs4all.nl>
//
// (C) Copyright Jeroen Zwartepoorte 2004
//

using System;
using System.Runtime.InteropServices;

namespace Gnome.Vfs {
	public delegate void MonitorHandler (string monitor, string uri);

	internal delegate void MonitorCallback (string monitorUri, string uri, MonitorEventType eventType);

	internal delegate void MonitorCallbackNative (IntPtr handle, string monitorUri, string uri, MonitorEventType eventType, IntPtr data);

	internal class MonitorCallbackWrapper : GLib.DelegateWrapper {

		public void NativeCallback (IntPtr handle, string monitorUri, string uri, MonitorEventType eventType, IntPtr data)
		{
			_managed (monitorUri, uri, eventType);
		}

		internal MonitorCallbackNative NativeDelegate;
		protected MonitorCallback _managed;

		public MonitorCallbackWrapper (MonitorCallback managed, object o) : base (o)
		{
			NativeDelegate = new MonitorCallbackNative (NativeCallback);
			_managed = managed;
		}
	}

	public class Monitor {
		private IntPtr handle;
		private MonitorCallbackWrapper wrapper;

		public event MonitorHandler Changed;
		public event MonitorHandler Deleted;
		public event MonitorHandler Startexecuting;
		public event MonitorHandler Stopexecuting;
		public event MonitorHandler Created;
		public event MonitorHandler MetadataChanged;

		public Monitor ()
		{
		}
		
		[DllImport ("gnomevfs-2")]
		private static extern Result gnome_vfs_monitor_add (out IntPtr handle, string uri, MonitorType type, MonitorCallbackNative callback, IntPtr user_data);
		
		public Result Add (string uri, MonitorType type)
		{
			handle = IntPtr.Zero;
			if (wrapper == null)
				wrapper = new MonitorCallbackWrapper (new MonitorCallback (OnMonitorEvent), null);
			Result result = gnome_vfs_monitor_add (out handle, uri, type, wrapper.NativeDelegate, IntPtr.Zero);
			return result;
		}
		
		[DllImport ("gnomevfs-2")]
		private static extern Result gnome_vfs_monitor_cancel (IntPtr handle);
		
		public Result Cancel ()
		{
			if (handle == IntPtr.Zero)
				throw new InvalidOperationException ("Nothing to cancel");
			
			return gnome_vfs_monitor_cancel (handle);
		}
		
		private void OnMonitorEvent (string monitorUri, string uri, MonitorEventType eventType)
		{
			MonitorHandler handler = null;

			switch (eventType) {
				case MonitorEventType.Changed:
					handler = Changed;
					break;
				case MonitorEventType.Deleted:
					handler = Deleted;
					break;
				case MonitorEventType.Startexecuting:
					handler = Startexecuting;
					break;
				case MonitorEventType.Stopexecuting:
					handler = Stopexecuting;
					break;
				case MonitorEventType.Created:
					handler = Created;
					break;
				case MonitorEventType.MetadataChanged:
					handler = MetadataChanged;
					break;
			}

			if (handler != null)
				handler (monitorUri, uri);
		}
	}
}

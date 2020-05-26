using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.Windows.Forms;
using System.Security.Principal;

namespace coinapplib
{
	public abstract class Pipes
	{
		public event EventHandler<PipeEventArgs> DataReceived;
		public event EventHandler<EventArgs> PipeClosed;

		protected PipeStream pipeStream;
		protected Action<Pipes> asyncReaderStart;

		public Pipes()
		{
		}

		public void Close()
		{
			pipeStream.WaitForPipeDrain();
			pipeStream.Close();
			pipeStream.Dispose();
			pipeStream = null;
		}

		/// <summary>
		/// Reads an array of bytes, where the first [n] bytes (based on the server's intsize) indicates the number of bytes to read 
		/// to complete the packet, and invokes the DataReceived event with a string converted from UTF8 of the byte array.
		/// </summary>
		public void StartStringReaderAsync()
		{
			StartByteReaderAsync((b) =>
			{
				string str = Encoding.UTF8.GetString(b).TrimEnd('\0');
				DataReceived?.Invoke(this, new PipeEventArgs(str));
			});
		}

		public void Flush()
		{
			pipeStream.Flush();
		}

		public Task WriteString(string str)
		{
			return WriteBytes(Encoding.UTF8.GetBytes(str));
		}
		
		public Task WriteBytes(byte[] bytes)
		{
			var blength = BitConverter.GetBytes(bytes.Length);
			var bfull = blength.Concat(bytes).ToArray();

			return pipeStream.WriteAsync(bfull, 0, bfull.Length);
		}

		protected void StartByteReaderAsync(Action<byte[]> packetReceived)
		{
			int intSize = sizeof(int);
			byte[] bDataLength = new byte[intSize];

			pipeStream.ReadAsync(bDataLength, 0, intSize).ContinueWith(t =>
			{
				int len = t.Result;

				if (len == 0)
				{
					PipeClosed?.Invoke(this, EventArgs.Empty);
				}
				else
				{
					int dataLength = BitConverter.ToInt32(bDataLength, 0);
					byte[] data = new byte[dataLength];

					pipeStream.ReadAsync(data, 0, dataLength).ContinueWith(t2 =>
					{
						len = t2.Result;

						if (len == 0)
						{
							PipeClosed?.Invoke(this, EventArgs.Empty);
						}
						else
						{
							packetReceived(data);
							StartByteReaderAsync(packetReceived);
						}
					});
				}
			});
		}
	}

	public class ClientPipe : Pipes
	{
		protected NamedPipeClientStream clientPipeStream;

		public ClientPipe(string serverName, string pipeName)
		{
			clientPipeStream = new NamedPipeClientStream(serverName, pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
			pipeStream = clientPipeStream;
		}

		public void Connect()
		{
			clientPipeStream.Connect();
			StartStringReaderAsync();
		}
	}

	public class ServerPipe : Pipes
	{
		public event EventHandler<EventArgs> Connected;

		protected NamedPipeServerStream serverPipeStream;
		protected NamedPipeServerStream sPipeServer;
		protected string PipeName { get; set; }

		public ServerPipe(string pipeName, bool setSecurity = false)
		{
			PipeName = pipeName;

			if (setSecurity)
			{
				//PipeSecurity pSecure = new PipeSecurity();
				//pSecure.SetAccessRule(new PipeAccessRule("Everyone",
				//	PipeAccessRights.ReadWrite,
				//	System.Security.AccessControl.AccessControlType.Allow));


				PipeSecurity pSecure = new PipeSecurity();
				pSecure.SetAccessRule(new PipeAccessRule("Everyone",
					PipeAccessRights.FullControl,
					System.Security.AccessControl.AccessControlType.Allow));

				serverPipeStream = new NamedPipeServerStream(
				pipeName,
				PipeDirection.InOut,
				NamedPipeServerStream.MaxAllowedServerInstances,
				PipeTransmissionMode.Message,
				PipeOptions.Asynchronous, 4096, 4096, pSecure);
			}
			else
			{
				serverPipeStream = new NamedPipeServerStream(
				pipeName,
				PipeDirection.InOut,
				NamedPipeServerStream.MaxAllowedServerInstances,
				PipeTransmissionMode.Message,
				PipeOptions.Asynchronous);
			}
			
			pipeStream = serverPipeStream;
			serverPipeStream.BeginWaitForConnection(new AsyncCallback(PipeConnected), null);
		}

		protected void PipeConnected(IAsyncResult ar)
		{
			serverPipeStream.EndWaitForConnection(ar);
			Connected?.Invoke(this, new EventArgs());
			StartStringReaderAsync();
		}
	}

	public class PipeEventArgs
	{
		//public byte[] Data { get; protected set; }
		//public int Len { get; protected set; }
		public string String { get; protected set; }

		public PipeEventArgs(string str)
		{
			String = str;
		}

		//public PipeEventArgs(byte[] data, int len)
		//{
		//    Data = data;
		//    Len = len;
		//}
	}

	public static class WinFormExtensionMethods
	{
		/// <summary>
		/// Asynchronous invoke on application thread.  Will return immediately unless invocation is not required.
		/// </summary>
		public static void BeginInvoke(this Control control, Action action)
		{
			if (control.InvokeRequired)
			{
				// We want a synchronous call here!!!!
				control.BeginInvoke((Delegate)action);
			}
			else
			{
				action();
			}
		}

		/// <summary>
		/// Synchronous invoke on application thread.  Will not return until action is completed.
		/// </summary>
		public static void Invoke(this Control control, Action action)
		{
			if (control.InvokeRequired)
			{
				// We want a synchronous call here!!!!
				control.Invoke((Delegate)action);
			}
			else
			{
				action();
			}
		}
	}

}

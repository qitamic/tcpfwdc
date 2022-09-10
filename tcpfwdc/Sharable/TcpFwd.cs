using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

public class TcpFwd
{
	//var
	private TcpListener tcpListener;
	private IPEndPoint epProxy;
	private IPEndPoint epRemote;
	private byte[] byConnect;
	private Encoding enc = Encoding.UTF8;

	//ctor
	public TcpFwd(IPEndPoint epLocal, IPEndPoint epRemote) : this(epLocal, epRemote, null) { }
	public TcpFwd(IPEndPoint epLocal, IPEndPoint epRemote, IPEndPoint epProxy)
	{
		this.epProxy = epProxy;
		this.epRemote = epRemote;
		byConnect = enc.GetBytes("CONNECT " + epRemote.Address + ":" + epRemote.Port + " HTTP/1.1" + Environment.NewLine + Environment.NewLine);

		wgConnect = new Worker<TcpClient>(100);
		wgConnect.Work += wgConnect_Work;
		wgForwarder = new Worker<Socket, Socket>(200);
		wgForwarder.Work += wgForwarder_Work;

		tcpListener = new TcpListener(epLocal);
		tcpListener.Start();

		wiConnectionListener = new Worker();
		wiConnectionListener.Work += wiConnectionListener_Work;
		wiConnectionListener.Do();
	}

	//main tcp listener
	private Worker wiConnectionListener;
	private void wiConnectionListener_Work()
	{
		while (true)
		{
			try
			{
				TcpClient tcpc = tcpListener.AcceptTcpClient();
				wgConnect.Do(tcpc);
			}
			catch (Exception ex)
			{
				return;
			}
		}
	}
	
	//http CONNECT
	private Worker<TcpClient> wgConnect;
	private void wgConnect_Work(TcpClient tcpc)
	{
		Socket soc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		bool bOk = false;
		try
		{
			if (epProxy != null)
			{
				//http proxy
				byte[] byResponse = new byte[1024];
				soc.Connect(epProxy);
				soc.Send(byConnect);
				soc.Receive(byResponse);
				string sResponse = enc.GetString(byResponse);
				if (sResponse.StartsWith("HTTP") && sResponse.Contains(" 200 "))
					bOk = true;
			}
			else
			{
				//direct connection
				soc.Connect(epRemote);
				bOk = true;
			}
		}
		catch (Exception ex) { }

		//data exchange
		if (bOk)
		{
			wgForwarder.Do(tcpc.Client, soc);
			wgForwarder.Do(soc, tcpc.Client);
		}
		else
		{
			try { tcpc.Client.Close(); }
			catch { }

			try { soc.Close(); }
			catch { }
		}
	}

	//server <-> client
	private Worker<Socket, Socket> wgForwarder;
	private void wgForwarder_Work(Socket a, Socket b)
	{
		try
		{
			while (true)
			{
				byte[] byData = new byte[1024];
				int iReceived = a.Receive(byData);
				b.Send(byData, iReceived, SocketFlags.None);
				if (iReceived == 0)
					throw new Exception("iReceived = 0");
			}
		}
		catch (Exception ex)
		{
			try { a.Close(); }
			catch { }

			try { b.Close(); }
			catch { }

			return;
		}		

	}
}

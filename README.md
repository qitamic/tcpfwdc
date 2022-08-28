# TCP Forwarder CLI

A TCP port forwarder with accessing server-behind-proxy support

Usage: `tcpfwdc.exe LocalIP LocalPort RemoteIP RemotePort [ProxyIP ProxyPort] [d, daemon]`

E.g. 1: `tcpfwdc.exe 127.0.0.1 5959 192.168.1.1 5900`
Creates a localhost VNC "server" which will be connecting to `192.168.1.1:5900`

E.g. 2: `tcpfwdc.exe 127.0.0.1 5959 192.168.1.1 5900 10.10.10.1 8080`
Performs the same as e.g. 1, but `192.168.1.1` is behind the proxy `10.10.10.1:8080`

E.g. 3: `tcpfwdc.exe 127.0.0.1 7777 192.168.1.1 7070 10.10.10.1 8080`
Performs the same as e.g. 2, but servicing AnyDesk instead of VNC
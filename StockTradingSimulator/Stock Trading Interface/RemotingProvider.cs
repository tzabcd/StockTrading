#if INTERNEL
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels;
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.Net;
using Network_Service_Core;
using Stock_Trading_Simulator_Kernel.Result_Notifer;
using Stock_Trading_Simulator_Kernel.RI_UserWealth;

namespace Stock_Trading_Simulator_Kernel
{
    /// <summary>
    /// 
    /// </summary>
    public class ClientIPServerSinkProvider : IServerChannelSinkProvider
    {
        private IServerChannelSinkProvider next = null;
        private Common coreConfig = new Common();

        public ClientIPServerSinkProvider(IDictionary properties, ICollection providerData)
        { }

        public void GetChannelData(IChannelDataStore channelData)
        {
        }
        public IServerChannelSink CreateSink(IChannelReceiver channel)
        {
            IServerChannelSink nextSink = null;
            if (next != null)
            {
                nextSink = next.CreateSink(channel);
            }
            return new ClientIPServerSink(nextSink);
        }
        public IServerChannelSinkProvider Next
        {
            get { return next; }
            set { next = value; }
        }
    }
    public class ClientIPServerSink : BaseChannelObjectWithProperties, IServerChannelSink, IChannelSinkBase
    {
        private IServerChannelSink _next;
        private IPAddress ip = IPAddress.Parse("127.0.0.1");
        public ClientIPServerSink(IServerChannelSink next)
        {
            _next = next;
        }
        public void AsyncProcessResponse(IServerResponseChannelSinkStack sinkStack, object state, IMessage msg, ITransportHeaders headers, Stream stream)
        {
        }
        public Stream GetResponseStream(IServerResponseChannelSinkStack sinkStack, object state, IMessage msg, ITransportHeaders headers)
        {
            return null;
        }

        public ServerProcessing ProcessMessage(
            IServerChannelSinkStack sinkStack,
            IMessage requestMsg,
            ITransportHeaders requestHeaders,
            Stream requestStream,
            out IMessage responseMsg,
            out ITransportHeaders responseHeaders,
            out Stream responseStream)
        {
            try
            {
                if (_next != null)
                {
                    ip = (IPAddress)requestHeaders[CommonTransportKeys.IPAddress];
                    if (ip != null && (ip.ToString().Trim() == "127.0.0.1" || CheckAuthorizedIP(ip.ToString())))
                    {
                        Common.CurrentStatus(ip.ToString().Trim());
                        ServerProcessing spres = _next.ProcessMessage(sinkStack, requestMsg, requestHeaders, requestStream, out responseMsg, out responseHeaders, out responseStream);
                        return spres;
                    }
                    else
                    {
                        Common.Log("Unauthorized Remoting IP: " + ip.ToString().Trim());
                        responseMsg = null;
                        responseHeaders = null;
                        responseStream = null;
                        return new ServerProcessing();
                    }
                }
                else
                {
                    responseMsg = null;
                    responseHeaders = null;
                    responseStream = null;
                    return new ServerProcessing();
                }
            }
            catch (Exception err)
            {
                Common.Log(err);
                responseMsg = null;
                responseHeaders = null;
                responseStream = null;
                return new ServerProcessing();
            }
        }

        public IServerChannelSink NextChannelSink
        {
            get { return _next; }
            set { _next = value; }
        }
        private bool CheckAuthorizedIP(string strIP)
        {
            try
            {
                if (strIP == null)
                    return false;
                for (byte i = 0; i < 32; i++)
                {
                    if (string.Compare(strIP.Trim(),
                        Common.Config("Authorization", "IP_" + i.ToString().Trim())) == 0)
                        return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
#endif
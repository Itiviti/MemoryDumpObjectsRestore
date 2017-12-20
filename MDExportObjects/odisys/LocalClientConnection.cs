/*************************************************************************
 * ULLINK CONFIDENTIAL INFORMATION
 * _______________________________
 *
 * All Rights Reserved.
 *
 * NOTICE: This file and its content are the property of Ullink. The
 * information included has been classified as Confidential and may
 * not be copied, modified, distributed, or otherwise disseminated, in
 * whole or part, without the express written permission of Ullink.
 ************************************************************************/

using System;
using System.Diagnostics.Contracts;
using System.Threading;
using com.ullink.oms.actions;
using com.ullink.oms.apiaccess.client;
using com.ullink.oms.model;
using com.ullink.ultools.prefs;
using java.util.prefs;

using Action = com.ullink.oms.model.Action;

namespace Ullink.desk.uitests.odisys
{
    public class LocalClientConnection : IDisposable
    {
        private readonly Dispatcher _clientDispatcher;
        private readonly ClientApiAccess _api;

        public LocalClientConnection(string user, string password)
        {
            Preferences prefs = new SimplePreferences();
            prefs.put("user", user);
            prefs.put("password", password);
            prefs.put("host", "localhost");
            prefs.put("port", "2098");

            _api = new ClientApiAccess();
            _api.initClientConnection(prefs);
            _api.setApiName("ui-tests");
            _clientDispatcher = new Dispatcher();
            _api.addClientDispatcher(_clientDispatcher);
            _api.startListening();
            _clientDispatcher.WaitForOnReady();
        }

        public void Dispose()
        {
            _api.removeClientDispatcher(_clientDispatcher);
            _api.stopListening();
            _clientDispatcher.Dispose();
        }

        public void SendAction(Action updateAction)
        {
            _api.sendAction(updateAction);
            _clientDispatcher.WaitForReply();
        }

        private class Dispatcher : ClientDispatcher, IDisposable
        {
            private const int MillisecondsTimeout = 30000;
            private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
            private readonly AutoResetEvent _onReadyEvent = new AutoResetEvent(false);
            private Rej _rej = null;

            public void onAck(Ack a)
            {
                _rej = null;
                _autoResetEvent.Set();
            }

            void ClientDispatcher.onRej(Rej r)
            {
                _rej = r;
                _autoResetEvent.Set();
            }

            void RequestReceiver.onRej(Rej r)
            {
                _rej = r;
                _autoResetEvent.Set();
            }

            void ClientDispatcher.onReply(Reply r)
            {
                _autoResetEvent.Set();
            }

            public void onConnected(ClientApiConnection cac)
            {
            }

            public void onReady(ClientApiConnection cac)
            {
                _onReadyEvent.Set();
            }

            public void onLoggedOut(ClientApiConnection cac)
            {
            }

            public void onConnectionLost(ClientApiConnection cac)
            {
            }

            public void onStopped()
            {
            }

            public void onStopped(ClientDispatcherStopReason cdsr, java.lang.Exception e)
            {
            }

            public void onEvent(Event e)
            {
            }

            void RequestReceiver.onReply(Reply r)
            {
            }

            public void WaitForOnReady()
            {
                if (!_onReadyEvent.WaitOne(MillisecondsTimeout))
                    throw new TimeoutException("Timeout waiting for log in.");
            }

            public void WaitForReply()
            {
                if (!_autoResetEvent.WaitOne(MillisecondsTimeout))
                    throw new TimeoutException("Timeout waiting for server reply.");

                if (_rej != null)
                    throw new RejectException(_rej);
            }

            public void Dispose()
            {
                _onReadyEvent.Dispose();
                _autoResetEvent.Dispose();
            }
        }
    }
}
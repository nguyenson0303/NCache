// Copyright (c) 2018 Alachisoft
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Alachisoft.NCache.Common.Logger;
using Alachisoft.NCache.Common.Monitoring;
using Alachisoft.NCache.Common.Util;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Alachisoft.NCache.Common.Monitoring
{
    public class RequestMonitor
    {
        internal static RequestMonitor s_instance;

        ConcurrentDictionary<string, RequestLedger> _clientDictionary;
        Thread _requestMonitor;
        object _lock = new object();
        int _threadSleepTime = 2*1000;
        bool _monitorRequests = true;

        public static RequestMonitor Instance
        {
            get
            {
                if (s_instance == null)
                    s_instance = new RequestMonitor();

                return s_instance;
            }
        }

        public RequestMonitor()
        {
            _clientDictionary = new ConcurrentDictionary<string, RequestLedger>();
        }

        public void RegisterClientLedger(string requestSource,ILogger logger)
        {
            if (requestSource != null)
            {
                if (!string.IsNullOrEmpty (requestSource))
                {
                    _clientDictionary.TryAdd(requestSource, new RequestLedger(requestSource, logger));
                }
            }
        }


        public void RegisterClientrequestsInLedger(string requestSource, ILogger logger, long requestId, ICancellableRequest command)
        {
            if (requestSource != null && !string.IsNullOrEmpty(requestSource))
            {
                RequestLedger ledger = null;
                ledger = new RequestLedger(requestSource, logger);

                ledger = _clientDictionary.GetOrAdd(requestSource, ledger);
                ledger.AddRequet(requestId, command);
            }
        }

        public void UnRegisterClientRequests (string requestSource, long requestId)
        {
            RequestLedger ledger = null;
            
            if (_clientDictionary.TryGetValue(requestSource, out ledger))
            {
                if (ledger != null)
                {
                    ledger.RemoveRequest(requestId);
                }
                    
            }
            
        }

        public void RemoveClientRequests (string requestSource)
        {
            if (requestSource != null && !string.IsNullOrEmpty(requestSource))
            {
                RequestLedger ledger = null;

                if (!_clientDictionary.TryGetValue(requestSource, out ledger))
                {
                    ledger.Dispoe();
                }
            }
        }

        public void Dispose ()
        {
            _monitorRequests = false;
            
            lock (_lock)
            {
                foreach (RequestLedger ledger in _clientDictionary.Values)
                {
                    ledger.Dispoe();
                }
            }
            _clientDictionary.Clear();
            if(_requestMonitor != null && _requestMonitor.IsAlive)
#if !NETCORE
                _requestMonitor.Abort();
#elif NETCORE
                _requestMonitor.Interrupt();
#endif
            _requestMonitor = null;

        }

        private void MontiorRequests()
        {
            if (ServiceConfiguration.EnableRequestCancellation)
            {
                while (_monitorRequests)
                {
                    try
                    {
                        if (_clientDictionary.Count > 0)
                        {
                            try
                            {
                                foreach (KeyValuePair<string, RequestLedger> requestInfo in _clientDictionary)
                                {
                                    RequestLedger ledger = requestInfo.Value as RequestLedger;
                                    if (ledger != null)
                                    {
                                        ledger.CancelTimedoutRequests();
                                    }
                                }
                            }
                            catch (ThreadAbortException)
                            {

                            }
                            catch (ThreadInterruptedException)
                            {

                            }
                        }
                        Thread.Sleep(_threadSleepTime);
                    }
                    catch (ThreadInterruptedException)
                    {

                    }
                    catch
                    {

                    }
                }
            }
        }

        public void Initialize ()
        {
            if (_requestMonitor == null) {
                _requestMonitor = new Thread(new ThreadStart(MontiorRequests));
                _requestMonitor.IsBackground = true;
                _requestMonitor.Name = "RequestMonitor";
                _requestMonitor.Start();
            }
        }
    }
}

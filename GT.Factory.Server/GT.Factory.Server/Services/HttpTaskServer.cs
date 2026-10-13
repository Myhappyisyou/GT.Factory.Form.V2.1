using Newtonsoft.Json;
using Serilog;
using GT.Factory.Server.Queue;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TaskContracts;

namespace GT.Factory.Server.Services
{
    public class HttpTaskReceiver
    {
        private readonly HttpListener _listener = new HttpListener();
        private readonly DbTaskQueue _queue;

        public HttpTaskReceiver(string prefix, DbTaskQueue queue)
        {
            _listener.Prefixes.Add(prefix);
            _queue = queue;
        }

        public void Start()
        {
            
            _listener.Start();
            
            Thread t = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        var ctx = _listener.GetContext();
                        string json;
                        using (var reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding))
                            json = reader.ReadToEnd();

                        var task = Newtonsoft.Json.JsonConvert.DeserializeObject<DbTask>(json);
                        _queue.Enqueue(task);

                        ctx.Response.StatusCode = 200;
                        ctx.Response.Close();
                    }
                    catch (Exception exp)
                    {
                        Log.Error(exp.Message);
                    }
                }
            });
            t.IsBackground = true;
            t.Start();
        }
    }


}

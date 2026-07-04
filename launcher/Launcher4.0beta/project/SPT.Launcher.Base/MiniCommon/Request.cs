/* Request.cs
 * License: NCSA Open Source License
 * 
 * Copyright: SPT
 * AUTHORS:
 */


using ComponentAce.Compression.Libs.zlib;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace SPT.Launcher.MiniCommon
{
    public class Request
    {
        public string Session;
        public string RemoteEndPoint;

        public Request(string session, string remoteEndPoint)
        {
            Session = session;
            RemoteEndPoint = remoteEndPoint;
        }

        public Stream Send(string url, string method = "GET", string data = null, bool compress = true)
        {
            // disable SSL encryption
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            // set session headers
            var request = WebRequest.Create(new Uri(RemoteEndPoint + url));
            request.Timeout = 15000; // 15 segundos — evita travamento após reconexão Tailscale

            if (!string.IsNullOrWhiteSpace(Session))
            {
                request.Headers.Add("Cookie", $"PHPSESSID={Session}");
                request.Headers.Add("SessionId", Session);
            }

            request.Headers.Add("Accept-Encoding", "deflate");
            request.Method = method;

            if (method != "GET" && !string.IsNullOrWhiteSpace(data))
            {
                // set request body
                var bytes = (compress) ? SimpleZlib.CompressToBytes(data, zlibConst.Z_BEST_COMPRESSION) : Encoding.UTF8.GetBytes(data);

                request.ContentType = "application/json";
                request.ContentLength = bytes.Length;

                if (compress)
                {
                    request.Headers.Add("Content-Encoding", "deflate");
                }

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }

            // get response stream
            try
            {
                var response = request.GetResponse();
                return response.GetResponseStream();
            }
            catch (Exception ex)
            {
                SPT.Launcher.Controllers.LogManager.Instance.Error($"[Request] Network Error connecting to {url}: {ex.Message}");
                if (ex is WebException webEx && webEx.Response is HttpWebResponse webResp)
                {
                    SPT.Launcher.Controllers.LogManager.Instance.Error($"[Request] HTTP Status: {webResp.StatusCode}");
                    try {
                        using var reader = new StreamReader(webResp.GetResponseStream());
                        string errorBody = reader.ReadToEnd();
                        SPT.Launcher.Controllers.LogManager.Instance.Error($"[Request] HTTP Body: {errorBody}");
                    } catch {}
                }
            }

            return null;
        }

        public string GetJson(string url, bool compress = true, bool decompressResponse = true)
        {
            using (var stream = Send(url, "GET", null, compress))
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    if (decompressResponse)
                    {
                        return SimpleZlib.Decompress(ms.ToArray(), null);
                    }
                    else
                    {
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
        }

        public string PostJson(string url, string data, bool compress = true, bool decompressResponse = true)
        {
            using (var stream = Send(url, "POST", data, compress))
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    if (decompressResponse)
                    {
                        return SimpleZlib.Decompress(ms.ToArray(), null);
                    }
                    else
                    {
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
        }
    }
}

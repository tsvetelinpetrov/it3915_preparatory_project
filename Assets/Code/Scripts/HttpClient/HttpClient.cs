﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Cache;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using CI.HttpClient.Core;
using UnityEngine;

namespace CI.HttpClient
{
    public class HttpClient
    {
        private const string DISPATCHER_GAMEOBJECT_NAME = "HttpClientDispatcher";
        private const int DEFAULT_BLOCK_SIZE = 10000;
        private const int DEFAULT_TIMEOUT = 100000;
        private const int DEFAULT_READ_WRITE_TIMEOUT = 300000;
        private const bool DEFAULT_KEEP_ALIVE = true;

        /// <summary>
        /// Chunk size when downloading data. Default is 10,000 bytes (10 kilobytes)
        /// </summary>
        public int DownloadBlockSize { get; set; }

        /// <summary>
        /// Chunk size when uploading data. Default is 10,000 bytes (10 kilobytes)
        /// </summary>
        public int UploadBlockSize { get; set; }

        /// <summary>
        /// Timeout value in milliseconds for opening read / write streams to the server. The default value is 100,000 milliseconds (100 seconds)
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// Timeout value in milliseconds when reading or writing data to / from the server. The default value is 300,000 milliseconds (5 minutes)
        /// </summary>
        public int ReadWriteTimeout { get; set; }

        /// <summary>
        /// The cache policy that will be associated with requests
        /// </summary>
        public RequestCachePolicy Cache { get; set; }

        /// <summary>
        /// The collection of security certificates that will be associated with requests
        /// </summary>
        public X509CertificateCollection Certificates { get; set; }

        /// <summary>
        /// Cookies that will be associated with requests
        /// </summary>
        public CookieContainer Cookies { get; set; }

        /// <summary>
        /// Authentication information that will be associated with requests
        /// </summary>
        public ICredentials Credentials { get; set; }

        /// <summary>
        /// Indicates whether to make a persistent connection to the Internet resource. The default is true
        /// </summary>
        public bool KeepAlive { get; set; }

        /// <summary>
        /// Specifies a collection of headers that will be included with all requests from this instance
        /// </summary>
        public IDictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Proxy information that will be associated with requests
        /// </summary>
        public IWebProxy Proxy { get; set; }

        /// <summary>
        /// The type of decompression to use, default is none
        /// </summary>
        public DecompressionMethods AutomaticDecompression { get; set; }

        private readonly List<HttpWebRequest> _requests;
        private readonly object _lock;

        public static IDispatcher _dispatcher;

        /// <summary>
        /// Provides a class for sending HTTP requests and receiving HTTP responses from a resource identified by a URI
        /// </summary>
        public HttpClient()
        {
            DownloadBlockSize = DEFAULT_BLOCK_SIZE;
            UploadBlockSize = DEFAULT_BLOCK_SIZE;
            Timeout = DEFAULT_TIMEOUT;
            ReadWriteTimeout = DEFAULT_READ_WRITE_TIMEOUT;
            KeepAlive = DEFAULT_KEEP_ALIVE;
            Headers = new Dictionary<string, string>();
            _requests = new List<HttpWebRequest>();
            _lock = new object();

            CreateDispatcherGameObject();
        }

        /// <summary>
        /// Aborts all requests on this instance
        /// </summary>
        public void Abort()
        {
            lock (_lock)
            {
                foreach (HttpWebRequest request in _requests)
                {
                    request.Abort();
                }
            }
        }

        /// <summary>
        /// Sends a DELETE request to the specified Uri and returns the response. A completion option specifies if download progress should be reported
        /// </summary>
        /// <param name="uri">The Uri the request is sent to</param>
        /// <param name="completionOption">Determines how the response should be read</param>
        /// <param name="responseCallback">Callback raised once the request completes</param>
        public void Delete(
            Uri uri,
            HttpCompletionOption completionOption,
            Action<HttpResponseMessage> responseCallback
        )
        {
            QueueRequest(uri, HttpAction.Delete, completionOption, responseCallback);
        }

        /// <summary>
        /// Sends a Delete request to the specified Uri and returns the response. An uploadStatusCallback can be specified to report upload progress
        /// and a completion option specifies if download progress should be reported
        /// </summary>
        /// <param name="uri">The Uri the request is sent to</param>
        /// <param name="content">Data to send</param>
        /// <param name="completionOption">Determines how the response should be read</param>
        /// <param name="responseCallback">Callback raised once the request completes</param>
        /// <param name="uploadStatusCallback">Callback that reports upload progress</param>
        public void Delete(
            Uri uri,
            IHttpContent content,
            HttpCompletionOption completionOption,
            Action<HttpResponseMessage> responseCallback,
            Action<UploadStatusMessage> uploadStatusCallback = null
        )
        {
            QueueRequest(
                uri,
                HttpAction.Delete,
                completionOption,
                content,
                responseCallback,
                uploadStatusCallback,
                null
            );
        }

        /// <summary>
        /// Sends a DELETE request to the specified Uri and returns the response
        /// </summary>
        /// <param name="uri">The Uri the request is sent to</param>
        /// <returns>The http response message</returns>
        public Task<HttpResponseMessage> DeleteAsync(Uri uri)
        {
            var task = new TaskCompletionSource<HttpResponseMessage>();

            Delete(
                uri,
                HttpCompletionOption.AllResponseContent,
                r =>
                {
                    task.SetResult(r);
                }
            );

            return task.Task;
        }

        /// <summary>
        /// Sends a DELETE request to the specified Uri and returns the response
        /// </summary>
        /// <param name="uri">The Uri the request is sent to</param>
        /// <param name="content">Data to send</param>
        /// <returns>The http response message</returns>
        public Task<HttpResponseMessage> DeleteAsync(Uri uri, IHttpContent content)
        {
            var task = new TaskCompletionSource<HttpResponseMessage>();

            Delete(
                uri,
                content,
                HttpCompletionOption.AllResponseContent,
                r =>
                {
                    task.SetResult(r);
                }
            );

            return task.Task;
        }

        /// <summary>
        /// Sends a GET request to the specified Uri and returns the response. A completion option specifies if download progress should be reported
        /// </summary>
        /// <param name="uri">The Uri the request is sent to</param>
        /// <param name="completionOption">Determines how the response should be read</param>
        /// <param name="responseCallback">Callback raised once the request completes</param>
        public void Get(
            Uri uri,
            HttpCompletionOption completionOption,
            Action<HttpResponseMessage> responseCallback
        )
        {
            QueueRequest(uri, HttpAction.Get, completionOption, responseCallback);
        }

        /// <summary>
        /// Sends a GET request to the specified Uri and returns the response
        /// </summary>
        /// <param name="uri">The Uri the request is sent to</param>
        /// <returns>The http response message</returns>
        public Task<HttpResponseMessage> GetAsync(Uri uri)
        {
            var task = new TaskCompletionSource<HttpResponseMessage>();

            Get(
                uri,
                HttpCompletionOption.AllResponseContent,
                r =>
                {
                    task.SetResult(r);
                }
            );

            return task.Task;
        }

        /// <summary>
        /// Sends a PATCH request to the specified Uri and returns the response. An uploadStatusCallback can be specified to report upload progress
        /// and a completion option specifies if download progress should be reported
        /// </summary>
        /// <param name="uri">The Uri the request is sent to</param>
        /// <param name="content">Data to send</param>
        /// <param name="completionOption">Determines how the response should be read</param>
        /// <param name="responseCallback">Callback raised once the request completes</param>
        /// <param name="uploadStatusCallback">Callback that reports upload progress</param>
        public void Patch(
            Uri uri,
            IHttpContent content,
            HttpCompletionOption completionOption,
            Action<HttpResponseMessage> responseCallback,
            Action<UploadStatusMessage> uploadStatusCallback = null
        )
        {
            QueueRequest(
                uri,
                HttpAction.Patch,
                completionOption,
                content,
                responseCallback,
                uploadStatusCallback,
                null
            );
        }

        /// <summary>
        /// Sends a PATCH request to the specified Uri and returns the response. A completion option specifies if download progress should be reported
        /// </summary>
        /// <param name="uri">The Uri the request is sent to</param>
        /// <param name="completionOption">Determines how the response should be read</param>
        /// <param name="responseCallback">Callback raised once the request completes</param>
        public void Patch(
            Uri uri,
            HttpCompletionOption completionOption,
            Action<HttpResponseMessage> responseCallback
        )
        {
            QueueRequest(uri, HttpAction.Patch, completionOption, responseCallback);
        }

        /// <summary>
        /// Sends a PATCH request to the specified Uri and returns the response
        /// </summary>
        /// <param name="uri">The Uri the request is sent to</param>
        /// <param name="content">Data to send</param>
        /// <returns>The http response message</returns>
        public Task PatchAsync(Uri uri, IHttpContent content)
        {
            var task = new TaskCompletionSource<HttpResponseMessage>();

            Patch(
                uri,
                content,
                HttpCompletionOption.AllResponseContent,
                r =>
                {
                    task.SetResult(r);
                }
            );

            return task.Task;
        }

        /// <summary>
        /// Sends a POST request to the specified Uri and returns the response. An uploadStatusCallback can be specified to report upload progress
        /// and a completion option specifies if download progress should be reported
        /// </summary>
        /// <param name="uri">The Uri the request is sent to</param>
        /// <param name="content">Data to send</param>
        /// <param name="completionOption">Determines how the response should be read</param>
        /// <param name="responseCallback">Callback raised once the request completes</param>
        /// <param name="uploadStatusCallback">Callback that reports upload progress</param>
        public void Post(
            Uri uri,
            IHttpContent content,
            HttpCompletionOption completionOption,
            Action<HttpResponseMessage> responseCallback,
            Action<UploadStatusMessage> uploadStatusCallback = null
        )
        {
            QueueRequest(
                uri,
                HttpAction.Post,
                completionOption,
                content,
                responseCallback,
                uploadStatusCallback,
                null
            );
        }

        /// <summary>
        /// Sends a POST request to the specified Uri and returns the response. A completion option specifies if download progress should be reported
        /// </summary>
        /// <param name="uri">The Uri the request is sent to</param>
        /// <param name="completionOption">Determines how the response should be read</param>
        /// <param name="responseCallback">Callback raised once the request completes</param>
        public void Post(
            Uri uri,
            HttpCompletionOption completionOption,
            Action<HttpResponseMessage> responseCallback
        )
        {
            QueueRequest(uri, HttpAction.Post, completionOption, responseCallback);
        }

        /// <summary>
        /// Sends a POST request to the specified Uri and returns the response
        /// </summary>
        /// <param name="uri">The Uri the request is sent to</param>
        /// <param name="content">Data to send</param>
        /// <returns>The http response message</returns>
        public Task PostAsync(Uri uri, IHttpContent content)
        {
            var task = new TaskCompletionSource<HttpResponseMessage>();

            Post(
                uri,
                content,
                HttpCompletionOption.AllResponseContent,
                r =>
                {
                    task.SetResult(r);
                }
            );

            return task.Task;
        }

        /// <summary>
        /// Sends a PUT request to the specified Uri and returns the response. An uploadStatusCallback can be specified to report upload progress
        /// and a completion option specifies if download progress should be reported
        /// </summary>
        /// <param name="uri">The Uri the request is sent to</param>
        /// <param name="content">Data to send</param>
        /// <param name="completionOption">Determines how the response should be read</param>
        /// <param name="responseCallback">Callback raised once the request completes</param>
        /// <param name="uploadStatusCallback">Callback that reports upload progress</param>
        public void Put(
            Uri uri,
            IHttpContent content,
            HttpCompletionOption completionOption,
            Action<HttpResponseMessage> responseCallback,
            Action<UploadStatusMessage> uploadStatusCallback = null
        )
        {
            QueueRequest(
                uri,
                HttpAction.Put,
                completionOption,
                content,
                responseCallback,
                uploadStatusCallback,
                null
            );
        }

        /// <summary>
        /// Sends a PUT request to the specified Uri and returns the response. A completion option specifies if download progress should be reported
        /// </summary>
        /// <param name="uri">The Uri the request is sent to</param>
        /// <param name="completionOption">Determines how the response should be read</param>
        /// <param name="responseCallback">Callback raised once the request completes</param>
        public void Put(
            Uri uri,
            HttpCompletionOption completionOption,
            Action<HttpResponseMessage> responseCallback
        )
        {
            QueueRequest(uri, HttpAction.Put, completionOption, responseCallback);
        }

        /// <summary>
        /// Sends a PUT request to the specified Uri and returns the response
        /// </summary>
        /// <param name="uri">The Uri the request is sent to</param>
        /// <param name="content">Data to send</param>
        /// <returns>The http response message</returns>
        public Task PutAsync(Uri uri, IHttpContent content)
        {
            var task = new TaskCompletionSource<HttpResponseMessage>();

            Put(
                uri,
                content,
                HttpCompletionOption.AllResponseContent,
                r =>
                {
                    task.SetResult(r);
                }
            );

            return task.Task;
        }

        /// <summary>
        /// Sends the specified HTTP request and returns the response. An uploadStatusCallback can be specified to report upload progress
        /// and a completion option specifies if download progress should be reported
        /// </summary>
        /// <param name="request">The HTTP request message to send</param>
        /// <param name="completionOption">Determines how the response should be read</param>
        /// <param name="responseCallback">Callback raised once the request completes</param>
        /// <param name="uploadStatusCallback">Callback that reports upload progress</param>
        public void Send(
            HttpRequestMessage request,
            HttpCompletionOption completionOption,
            Action<HttpResponseMessage> responseCallback,
            Action<UploadStatusMessage> uploadStatusCallback = null
        )
        {
            QueueRequest(
                request.Uri,
                request.Method,
                completionOption,
                request.Content,
                responseCallback,
                uploadStatusCallback,
                request.Headers
            );
        }

        /// <summary>
        /// Sends the specified HTTP request and returns the response
        /// </summary>
        /// <param name="request">The HTTP request message to send</param>
        /// <returns>The http response message</returns>
        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            var task = new TaskCompletionSource<HttpResponseMessage>();

            Send(
                request,
                HttpCompletionOption.AllResponseContent,
                r =>
                {
                    task.SetResult(r);
                }
            );

            return task.Task;
        }

        /// <summary>
        /// Ensures the response is successful, otherwise throws an exception
        /// </summary>
        /// <param name="response">The HTTP response to check</param>
        /// <param name="errorCallback">Callback raised if an error occurs</param>
        /// <exception cref="System.Net.Http.HttpRequestException">Thrown when the HTTP response is not successful</exception>
        /// <remarks>
        /// This method is used to check if the HTTP response was successful. If the response was not successful, an exception is thrown. The error callback is also raised.
        /// </remarks>
        public void EnsureSuccess(HttpResponseMessage response, System.Action<string> errorCallback)
        {
            if (!response.IsSuccessStatusCode)
            {
                if (errorCallback != null)
                    errorCallback($"HTTP Error: {response.StatusCode} - {response.ReasonPhrase}");
                throw new System.Net.Http.HttpRequestException(
                    $"HTTP Error: {response.StatusCode} - {response.ReasonPhrase}"
                );
            }
        }

        private void QueueRequest(
            Uri uri,
            HttpAction httpAction,
            HttpCompletionOption httpCompletionOption,
            Action<HttpResponseMessage> responseCallback
        )
        {
            QueueWorkItem(
                (t) =>
                {
                    try
                    {
                        HttpWebRequest request = CreateRequest(uri, null);
                        new HttpRequest(httpAction, request, _dispatcher).Execute(
                            httpCompletionOption,
                            responseCallback,
                            DownloadBlockSize
                        );
                        RemoveRequest(request);
                    }
                    catch (Exception e)
                    {
                        RaiseErrorResponse(responseCallback, e);
                    }
                }
            );
        }

        private void QueueRequest(
            Uri uri,
            HttpAction httpAction,
            HttpCompletionOption httpCompletionOption,
            IHttpContent httpContent,
            Action<HttpResponseMessage> responseCallback,
            Action<UploadStatusMessage> uploadStatusCallback,
            IDictionary<string, string> headers
        )
        {
            QueueWorkItem(
                (t) =>
                {
                    try
                    {
                        HttpWebRequest request = CreateRequest(uri, headers);
                        new HttpRequest(httpAction, request, _dispatcher).Execute(
                            httpContent,
                            httpCompletionOption,
                            responseCallback,
                            uploadStatusCallback,
                            DownloadBlockSize,
                            UploadBlockSize
                        );
                        RemoveRequest(request);
                    }
                    catch (Exception e)
                    {
                        RaiseErrorResponse(responseCallback, e);
                    }
                }
            );
        }

        private void QueueWorkItem(WaitCallback action)
        {
            ThreadPool.QueueUserWorkItem(action);
        }

        private HttpWebRequest CreateRequest(Uri uri, IDictionary<string, string> requestHeaders)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = AutomaticDecompression;
            DisableWriteStreamBuffering(request);
            AddCache(request);
            AddCertificates(request);
            AddCookies(request);
            AddCredentials(request);
            AddKeepAlive(request);
            AddHeaders(request, requestHeaders);
            AddProxy(request);
            AddTimeouts(request);
            AddRequest(request);
            return request;
        }

        private void DisableWriteStreamBuffering(HttpWebRequest request)
        {
            request.AllowWriteStreamBuffering = false;
        }

        private void AddCache(HttpWebRequest request)
        {
            if (Cache != null)
            {
                request.CachePolicy = Cache;
            }
        }

        private void AddCertificates(HttpWebRequest request)
        {
            if (Certificates != null)
            {
                request.ClientCertificates = Certificates;
            }
        }

        private void AddCookies(HttpWebRequest request)
        {
            if (Cookies != null)
            {
                request.CookieContainer = Cookies;
            }
        }

        private void AddCredentials(HttpWebRequest request)
        {
            if (Credentials != null)
            {
                request.Credentials = Credentials;
            }
        }

        private void AddKeepAlive(HttpWebRequest request)
        {
            request.KeepAlive = KeepAlive;
        }

        private void AddHeaders(HttpWebRequest request, IDictionary<string, string> requestHeaders)
        {
            if (Headers != null)
            {
                foreach (var header in Headers)
                {
                    AddHeader(request, header);
                }
            }

            if (requestHeaders != null)
            {
                foreach (var header in requestHeaders)
                {
                    AddHeader(request, header);
                }
            }
        }

        private void AddHeader(HttpWebRequest request, KeyValuePair<string, string> header)
        {
            switch (header.Key.ToLower())
            {
                case "accept":
                    request.Accept = header.Value;
                    break;
                case "connection":
                    request.Connection = header.Value;
                    break;
                case "content-length":
                    throw new NotSupportedException("Content Length is set automatically");
                case "content-type":
                    throw new NotSupportedException("Content Type is set automatically");
                case "expect":
                    request.Expect = header.Value;
                    break;
                case "date":
                    throw new NotSupportedException(
                        "Date is automatically set by the system to the current date"
                    );
                case "host":
                    throw new NotSupportedException(
                        "Host is automatically set by the system to current host information"
                    );
                case "if-modified-since":
                    request.IfModifiedSince = DateTime.Parse(header.Value);
                    break;
                case "range":
                    int range = int.Parse(header.Value);
                    request.AddRange(range);
                    break;
                case "referer":
                    request.Referer = header.Value;
                    break;
                case "transfer-encoding":
                    throw new NotSupportedException("Transfer Encoding is not currently supported");
                case "user-agent":
                    request.UserAgent = header.Value;
                    break;
                default:
                    request.Headers[header.Key] = header.Value;
                    break;
            }
        }

        private void AddProxy(HttpWebRequest request)
        {
            if (Proxy != null)
            {
                request.Proxy = Proxy;
            }
        }

        private void AddTimeouts(HttpWebRequest request)
        {
            request.Timeout = Timeout;
            request.ReadWriteTimeout = ReadWriteTimeout;
        }

        private void AddRequest(HttpWebRequest request)
        {
            lock (_lock)
            {
                _requests.Add(request);
            }
        }

        private void RemoveRequest(HttpWebRequest request)
        {
            lock (_lock)
            {
                _requests.Remove(request);
            }
        }

        private void RaiseErrorResponse(Action<HttpResponseMessage> action, Exception exception)
        {
            if (action != null)
            {
                _dispatcher.Enqueue(() =>
                {
                    action(new HttpResponseMessage() { Exception = exception });
                });
            }
        }

        private void CreateDispatcherGameObject()
        {
            if (_dispatcher == null)
            {
                _dispatcher = new GameObject(DISPATCHER_GAMEOBJECT_NAME).AddComponent<Dispatcher>();
            }
        }
    }
}

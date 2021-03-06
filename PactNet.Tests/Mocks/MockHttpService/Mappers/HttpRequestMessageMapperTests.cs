﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using NSubstitute;
using PactNet.Mocks.MockHttpService.Mappers;
using PactNet.Mocks.MockHttpService.Models;
using Xunit;

namespace PactNet.Tests.Mocks.MockHttpService.Mappers
{
    public class HttpRequestMessageMapperTests
    {
        public IHttpRequestMessageMapper GetSubject()
        {
            return new HttpRequestMessageMapper();
        }

        [Fact]
        public void Convert_WithNullRequest_ReturnsNull()
        {
            var mapper = GetSubject();

            var result = mapper.Convert(null);

            Assert.Null(result);
        }

        [Fact]
        public void Convert_WithRequest_CallsHttpMethodMapper()
        {
            var request = new ProviderServiceRequest
            {
                Method = HttpVerb.Post,
                Path = "/events"
            };

            var mockHttpMethodMapper = Substitute.For<IHttpMethodMapper>();
            var mockHttpContentMapper = Substitute.For<IHttpContentMapper>();
            var mockHttpBodyContentMapper = Substitute.For<IHttpBodyContentMapper>();

            mockHttpMethodMapper.Convert(HttpVerb.Post).Returns(HttpMethod.Post);

            IHttpRequestMessageMapper mapper = new HttpRequestMessageMapper(
                mockHttpMethodMapper, 
                mockHttpContentMapper, 
                mockHttpBodyContentMapper);

            mapper.Convert(request);

            mockHttpMethodMapper.Received(1).Convert(request.Method);
        }

        [Fact]
        public void Convert_WithHeader_HeaderIsAddedToHttpRequestMessage()
        {
            var request = new ProviderServiceRequest
            {
                Method = HttpVerb.Post,
                Path = "/events",
                Headers = new Dictionary<string, string>
                {
                    { "X-Custom", "Tester" }
                }
            };

            var mockHttpMethodMapper = Substitute.For<IHttpMethodMapper>();
            var mockHttpContentMapper = Substitute.For<IHttpContentMapper>();
            var mockHttpBodyContentMapper = Substitute.For<IHttpBodyContentMapper>();

            mockHttpMethodMapper.Convert(HttpVerb.Post).Returns(HttpMethod.Post);
            mockHttpBodyContentMapper.Convert(Arg.Any<object>(), request.Headers).Returns(new HttpBodyContent(String.Empty, null, null));

            IHttpRequestMessageMapper mapper = new HttpRequestMessageMapper(
                mockHttpMethodMapper,
                mockHttpContentMapper,
                mockHttpBodyContentMapper);

            var result = mapper.Convert(request);

            Assert.Equal(request.Headers.First().Key, result.Headers.First().Key);
            Assert.Equal(request.Headers.First().Value, result.Headers.First().Value.First());
        }

        [Fact]
        public void Convert_WithPlainContentTypeHeader_HeaderIsNotAddedToHttpRequestMessageAndHttpContentMapperIsCalledWithContentType()
        {
            const string contentTypeString = "text/plain";
            var request = new ProviderServiceRequest
            {
                Method = HttpVerb.Post,
                Path = "/events",
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", contentTypeString }
                },
                Body = new {}
            };
            var httpBodyContent = new HttpBodyContent(String.Empty, contentTypeString, null);

            var mockHttpMethodMapper = Substitute.For<IHttpMethodMapper>();
            var mockHttpContentMapper = Substitute.For<IHttpContentMapper>();
            var mockHttpBodyContentMapper = Substitute.For<IHttpBodyContentMapper>();

            mockHttpMethodMapper.Convert(HttpVerb.Post).Returns(HttpMethod.Post);
            mockHttpBodyContentMapper.Convert(Arg.Any<object>(), request.Headers).Returns(httpBodyContent);

            IHttpRequestMessageMapper mapper = new HttpRequestMessageMapper(
                mockHttpMethodMapper,
                mockHttpContentMapper,
                mockHttpBodyContentMapper);

            var result = mapper.Convert(request);

            Assert.Empty(result.Headers);
            mockHttpContentMapper.Received(1).Convert(httpBodyContent);
        }

        [Fact]
        public void Convert_WithPlainContentTypeHeaderLowercased_HeaderIsNotAddedToHttpRequestMessageAndHttpContentMapperIsCalledWithContentType()
        {
            const string contentTypeString = "text/plain";
            var request = new ProviderServiceRequest
            {
                Method = HttpVerb.Post,
                Path = "/events",
                Headers = new Dictionary<string, string>
                {
                    { "content-type", contentTypeString }
                },
                Body = new { }
            };
            var httpBodyContent = new HttpBodyContent(String.Empty, contentTypeString, null);

            var mockHttpMethodMapper = Substitute.For<IHttpMethodMapper>();
            var mockHttpContentMapper = Substitute.For<IHttpContentMapper>();
            var mockHttpBodyContentMapper = Substitute.For<IHttpBodyContentMapper>();

            mockHttpMethodMapper.Convert(HttpVerb.Post).Returns(HttpMethod.Post);
            mockHttpBodyContentMapper.Convert(Arg.Any<object>(), request.Headers).Returns(httpBodyContent);

            IHttpRequestMessageMapper mapper = new HttpRequestMessageMapper(
                mockHttpMethodMapper,
                mockHttpContentMapper,
                mockHttpBodyContentMapper);

            var result = mapper.Convert(request);

            Assert.Empty(result.Headers);
            mockHttpContentMapper.Received(1).Convert(httpBodyContent);
        }

        [Fact]
        public void Convert_WithPlainContentTypeAndUtf8CharsetHeader_HeaderIsNotAddedToHttpRequestMessageAndHttpContentMapperIsCalledWithEncodingAndContentType()
        {
            const string contentTypeString = "text/plain";
            const string encodingString = "utf-8";
            var encoding = Encoding.UTF8;
            var request = new ProviderServiceRequest
            {
                Method = HttpVerb.Post,
                Path = "/events",
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", contentTypeString + "; charset=" + encodingString }
                },
                Body = new { }
            };
            var httpBodyContent = new HttpBodyContent(String.Empty, contentTypeString, encoding);

            var mockHttpMethodMapper = Substitute.For<IHttpMethodMapper>();
            var mockHttpContentMapper = Substitute.For<IHttpContentMapper>();
            var mockHttpBodyContentMapper = Substitute.For<IHttpBodyContentMapper>();

            mockHttpMethodMapper.Convert(HttpVerb.Post).Returns(HttpMethod.Post);
            mockHttpBodyContentMapper.Convert(Arg.Any<object>(), request.Headers).Returns(httpBodyContent);

            IHttpRequestMessageMapper mapper = new HttpRequestMessageMapper(
                mockHttpMethodMapper,
                mockHttpContentMapper,
                mockHttpBodyContentMapper);

            var result = mapper.Convert(request);

            Assert.Empty(result.Headers);
            mockHttpBodyContentMapper.Received(1).Convert(request.Body, request.Headers);
            mockHttpContentMapper.Received(1).Convert(httpBodyContent);
        }

        [Fact]
        public void Convert_WithJsonContentTypeAndUnicodeCharsetHeader_HeaderIsNotAddedToHttpRequestMessageAndHttpContentMapperIsCalledWithEncodingAndContentType()
        {
            const string contentTypeString = "application/json";
            const string encodingString = "utf-16";
            var encoding = Encoding.Unicode;
            var request = new ProviderServiceRequest
            {
                Method = HttpVerb.Post,
                Path = "/events",
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", contentTypeString + "; charset=" + encodingString }
                },
                Body = new { }
            };
            var httpBodyContent = new HttpBodyContent(String.Empty, contentTypeString, encoding);

            var mockHttpMethodMapper = Substitute.For<IHttpMethodMapper>();
            var mockHttpContentMapper = Substitute.For<IHttpContentMapper>();
            var mockHttpBodyContentMapper = Substitute.For<IHttpBodyContentMapper>();

            mockHttpMethodMapper.Convert(HttpVerb.Post).Returns(HttpMethod.Post);
            mockHttpBodyContentMapper.Convert(Arg.Any<object>(), request.Headers).Returns(httpBodyContent);

            IHttpRequestMessageMapper mapper = new HttpRequestMessageMapper(
                mockHttpMethodMapper,
                mockHttpContentMapper,
                mockHttpBodyContentMapper);

            var result = mapper.Convert(request);

            Assert.Empty(result.Headers);
            mockHttpBodyContentMapper.Received(1).Convert(request.Body, request.Headers);
            mockHttpContentMapper.Received(1).Convert(httpBodyContent);
        }

        [Fact]
        public void Convert_WithContentTypeAndCustomHeader_OnlyCustomHeadersIsAddedToHttpRequestMessage()
        {
            const string contentTypeString = "text/plain";
            var request = new ProviderServiceRequest
            {
                Method = HttpVerb.Post,
                Path = "/events",
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", contentTypeString },
                    { "X-Custom", "My Custom header" }
                },
                Body = new { }
            };
            var httpBodyContent = new HttpBodyContent(String.Empty, contentTypeString, null);

            var mockHttpMethodMapper = Substitute.For<IHttpMethodMapper>();
            var mockHttpContentMapper = Substitute.For<IHttpContentMapper>();
            var mockHttpBodyContentMapper = Substitute.For<IHttpBodyContentMapper>();

            mockHttpMethodMapper.Convert(HttpVerb.Post).Returns(HttpMethod.Post);
            mockHttpBodyContentMapper.Convert(Arg.Any<object>(), request.Headers).Returns(httpBodyContent);

            IHttpRequestMessageMapper mapper = new HttpRequestMessageMapper(
                mockHttpMethodMapper,
                mockHttpContentMapper,
                mockHttpBodyContentMapper);

            var result = mapper.Convert(request);

            Assert.Equal(request.Headers.Last().Key, result.Headers.First().Key);
            Assert.Equal(request.Headers.Last().Value, result.Headers.First().Value.First());
        }

        [Fact]
        public void Convert_WithBody_HttpContentMapperIsCalled()
        {
            var request = new ProviderServiceRequest
            {
                Method = HttpVerb.Get,
                Path = "/events",
                Body = new
                {
                    Test = "tester"
                }
            };

            var mockHttpMethodMapper = Substitute.For<IHttpMethodMapper>();
            var mockHttpContentMapper = Substitute.For<IHttpContentMapper>();
            var mockHttpBodyContentMapper = Substitute.For<IHttpBodyContentMapper>();

            mockHttpMethodMapper.Convert(HttpVerb.Get).Returns(HttpMethod.Get);

            IHttpRequestMessageMapper mapper = new HttpRequestMessageMapper(
                mockHttpMethodMapper,
                mockHttpContentMapper,
                mockHttpBodyContentMapper);

            mapper.Convert(request);

            mockHttpBodyContentMapper.Received(1).Convert(request.Body, request.Headers);
        }

        [Fact]
        public void Convert_WithTheWorks_CorrectlyMappedHttpRequestMessageIsReturned()
        {
            const string encodingString = "utf-8";
            var encoding = Encoding.UTF8;
            const string contentTypeString = "application/json";
            const string bodyJson = "{\"Test\":\"tester\",\"Testing\":1}";

            var request = new ProviderServiceRequest
            {
                Method = HttpVerb.Get,
                Path = "/events",
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", contentTypeString + "; charset=" + encodingString },
                    { "X-Custom", "My Custom header" },
                    { "Content-Length", "10000" }, //This header is removed and replace with the correct value of 29
                },
                Body = new
                {
                    Test = "tester",
                    Testing = 1
                }
            };
            var httpBodyContent = new HttpBodyContent(bodyJson, contentTypeString, encoding);

            var mockHttpMethodMapper = Substitute.For<IHttpMethodMapper>();
            var mockHttpContentMapper = Substitute.For<IHttpContentMapper>();
            var mockHttpBodyContentMapper = Substitute.For<IHttpBodyContentMapper>();

            mockHttpMethodMapper.Convert(HttpVerb.Get).Returns(HttpMethod.Get);
            mockHttpContentMapper.Convert(httpBodyContent).Returns(new StringContent(bodyJson, encoding, contentTypeString));
            mockHttpBodyContentMapper.Convert(Arg.Any<object>(), request.Headers).Returns(httpBodyContent);

            IHttpRequestMessageMapper mapper = new HttpRequestMessageMapper(
                mockHttpMethodMapper,
                mockHttpContentMapper,
                mockHttpBodyContentMapper);

            var result = mapper.Convert(request);
            var requestContent = result.Content.ReadAsStringAsync().Result;

            var contentTypeHeader = result.Content.Headers.First(x => x.Key.Equals("Content-Type"));
            var customHeader = result.Headers.First(x => x.Key.Equals("X-Custom"));
            var contentLengthHeader = result.Content.Headers.First(x => x.Key.Equals("Content-Length"));

            Assert.Equal(bodyJson, requestContent);

            //Content-Type header
            Assert.Equal(request.Headers.First().Key, contentTypeHeader.Key);
            Assert.Equal(request.Headers.First().Value, contentTypeHeader.Value.First());

            //X-Custom header
            Assert.Equal(request.Headers.Skip(1).First().Key, customHeader.Key);
            Assert.Equal(request.Headers.Skip(1).First().Value, customHeader.Value.First());

            //Content-Length header
            Assert.Equal(request.Headers.Last().Key, contentLengthHeader.Key);
            Assert.Equal("29", contentLengthHeader.Value.First());
        }
    }
}

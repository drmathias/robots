using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;

namespace Robots.Txt.Parser.Tests.Unit.Http;

public static class HttpClientHandlerMockExtensions
{
    public static void SetupToRespondWith(this Mock<HttpClientHandler> httpClientHandlerMock, HttpStatusCode statusCode, HttpContent? content = null)
    {
        httpClientHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = content ?? new StringContent("")
            });
    }
}

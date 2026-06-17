using Microsoft.AspNetCore.Http;
using OrderFlow.Api.Infrastructure;
using OrderFlow.TestCore.Builders;

namespace OrderFlow.Api.TestUtils.Builders.Infrastructure;

public class HttpCorrelationIdAccessorBuilder : BaseServiceBuilder<HttpCorrelationIdAccessor>
{
    public IHttpContextAccessor FreezeHttpContextAccessor()
    {
        return Freeze<IHttpContextAccessor>();
    }
}
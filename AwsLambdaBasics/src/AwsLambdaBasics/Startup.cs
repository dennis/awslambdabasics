using Amazon.Lambda.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace AwsLambdaBasics;

[LambdaStartup]
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
    }
}

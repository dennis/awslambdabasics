using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System.Net;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AwsLambdaBasics;

public class Function
{
    private const string FacebookAuthUrl = "https://www.facebook.com/v12.0/dialog/oauth";
    private string? ClientId { get; set; }
    private const string RedirectUri = "https://localhost/callback"; // Replace with your redirect URI

    private async Task<Dictionary<string, string>> GetSecrets(ILambdaLogger logger)
    {
        var client = new AmazonSecretsManagerClient();
        var request = new GetSecretValueRequest
        {
            SecretId = "awslambdabasics"
        };

        var response = await client.GetSecretValueAsync(request);
        var secretString = response.SecretString;

        logger.LogCritical("Got secret, deserializing it: " + secretString);

        return JsonSerializer.Deserialize<Dictionary<string, string>>(secretString) ?? [];
    }

    [LambdaFunction(PackageType = LambdaPackageType.Zip)]
    [RestApi(LambdaHttpMethod.Get, "/")]
    public async Task<APIGatewayProxyResponse> Get(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogCritical("HELLO WORLD");
        try
        {
            if(ClientId is null)
            {
                /*
                context.Logger.LogCritical("Getting secrets");

                var secrets = await GetSecrets(context.Logger);

                context.Logger.LogCritical("Got secrets");

                ClientId = secrets["FACEBOOK_CLIENT_ID"];
                */
                ClientId = "99c9954dad1c8f991b40537189987b07";


                if (ClientId is null)
                {
                    context.Logger.LogError("FACEBOK_CLIENT_ID secret is NULL");

                    return new APIGatewayProxyResponse
                    {
                        StatusCode = 500,
                        Body = JsonSerializer.Serialize(new { error = "Internal Server Error", message = "ClientId is NULL"})
                    };
                }
            }
            
            var authUrl = BuildUrl();
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.Redirect,
                Headers = new Dictionary<string, string> { { "Location", authUrl } },
            };

        }
        catch (Exception ex)
        {
            context.Logger.LogCritical($"Error initiating Facebook authentication flow: {ex.Message}");
            context.Logger.Log(LogLevel.Critical.ToString(), ex, ex.Message);

            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = JsonSerializer.Serialize(new { error = "Internal Server Error", message = ex.Message, trace = ex.StackTrace })
            };
        }
    }

    private string BuildUrl()
    {
        var queryParams = new Dictionary<string, string>
        {
            { "client_id", ClientId! },
            { "redirect_uri", RedirectUri },
            { "response_type", "code" },
            { "scope", "email,public_profile" },
        };

        var query = string.Join("&", queryParams);
        return $"{FacebookAuthUrl}?{query}";
    }
}

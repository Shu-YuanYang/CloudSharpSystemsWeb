using Amazon.Lambda.Core;
using APIConnector;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace FunctionalLambdaPort;

public class Function
{
    
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public string FunctionHandler(APIConnector.Model.RESTAPIInputModel input, ILambdaContext context)
    {
        return RESTAPIConnector.CallRestAPIAsync(input).Result;
    }

}

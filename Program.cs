using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Microsoft.AspNetCore.Diagnostics;
using nng_api.Config;
using nng_api.DatabaseProviders;
using nng_api.Exceptions;
using nng_api.Helpers;
using nng_api.LoopTasks;
using nng_api.Services;
using Redis.OM;
using Redis.OM.Modeling;

Console.OutputEncoding = OperatingSystem.IsWindows() ? Encoding.Unicode : Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

var encoderSettings = new TextEncoderSettings();
encoderSettings.AllowRange(UnicodeRanges.All);

builder.Services.AddControllers(options => { options.AllowEmptyInputInBodyModelBinding = true; }).AddJsonOptions(
    options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.MaxDepth = 64;
        options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(encoderSettings);
        options.AllowInputFormatterExceptionMessages = false;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());
    });

builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

builder.WebHost.UseSentry(o =>
{
    o.Dsn = "https://66288da100b24ee18d88395503dc9fe9@o555933.ingest.sentry.io/4504730305757184";
    o.TracesSampleRate = 1.0;
});

builder.WebHost.UseUrls("http://*:1230");

builder.WebHost.UseKestrel(o => { o.AddServerHeader = false; });

var connectionString = EnvironmentConfiguration.GetInstance().Configuration.RedisUrl;
builder.Services.AddSingleton(new RedisConnectionProvider(connectionString));

builder.Services.AddSingleton(typeof(VkService));
builder.Services.AddSingleton(typeof(GroupsDatabaseProvider));
builder.Services.AddSingleton(typeof(SettingsDatabaseProvider));
builder.Services.AddSingleton(typeof(GroupStatsDatabaseProvider));
builder.Services.AddSingleton(typeof(GroupArchiveStatsDatabaseProvider));
builder.Services.AddSingleton(typeof(RequestsDatabaseProvider));
builder.Services.AddSingleton(typeof(WatchDogDatabaseProvider));
builder.Services.AddSingleton(typeof(UsersDatabaseProvider));
builder.Services.AddSingleton(typeof(TokensDatabaseProvider));
builder.Services.AddSingleton(typeof(StartupsDatabaseProvider));

builder.Services.AddHostedService<UserUpdater>();
builder.Services.AddHostedService<StatsArchiver>();
builder.Services.AddHostedService<StatsUpdater>();

var app = builder.Build();

app.UseExceptionHandler(new ExceptionHandlerOptions
{
    AllowStatusCode404Response = true,

    ExceptionHandler = async delegate(HttpContext context)
    {
        var exceptionDetails = context.Features.Get<IExceptionHandlerFeature>();
        var ex = exceptionDetails?.Error;

        if (ex is null) return;

        object? error;

        if (ex is NonAuthorized)
        {
            error = OutputResult.NotAuthorized();
            context.Response.StatusCode = 401;
        }
        else
        {
            error = OutputResult.TeaPot(ex).Value;
            context.Response.ContentType = "application/problem+json";
        }

        await context.Response.WriteAsJsonAsync(error);
    }
});

app.UseSentryTracing();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");

app.Use(async (context, next) =>
{
    await next();

    if (context.Response.HasStarted) return;

    switch (context.Response.StatusCode)
    {
        case 404:
        {
            var response = new
            {
                code = 404,
                message = "Not found"
            };

            await context.Response.WriteAsJsonAsync(response);
            break;
        }

        case 405:
        {
            var response = new
            {
                code = 405,
                message = "Method not allowed"
            };

            await context.Response.WriteAsJsonAsync(response);
            break;
        }

        case 415:
            var input = new
            {
                code = 405,
                message = "Invalid input"
            };
            await context.Response.WriteAsJsonAsync(input);
            break;
    }
});
app.Run();

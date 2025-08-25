using FluentValidation;
using FluentValidation.AspNetCore;
using Lebo.Factories;
using Lebo.Services;
using Lebo.Validators;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .AddFactories()
    //.AddNotificationAsyncHandler<MediaSavedNotification, PortfolioMediaSavedNotificationHandler>()
    //.AddNotificationHandler<MediaDeletedNotification, PortfolioMediaDeletedNotificationHandler>()
    //.AddNotificationAsyncHandler<UmbracoApplicationStartedNotification, UmbracoStartupImageWarmingHandler>()
    .Build();

// Portfolio and Contact services
builder.Services.AddScoped<IPortfolioMediaService, PortfolioMediaService>();
builder.Services.AddScoped<IPortfolioCacheService, PortfolioCacheService>();
builder.Services.AddScoped<IContactService, ContactService>();



// Caching services
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();

// Image warming services (using Umbraco's background job system)
//builder.Services.AddRecurringBackgroundJob<ImageWarmingBackgroundTask>();

// Validation services
builder.Services.AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters()
                .AddValidatorsFromAssemblyContaining<ContactMessageValidator>();

WebApplication app = builder.Build();

await app.BootUmbracoAsync();

app.UseDeveloperExceptionPage();

app.UseResponseCaching();

app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();

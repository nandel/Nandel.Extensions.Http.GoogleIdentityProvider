# Nandel.Extensions.Http.GoogleIdentityProvider

This library add a quick way to authenticate you service-to-service communications using Google Authentication

Google Docs: https://cloud.google.com/run/docs/authenticating/service-to-service

## Install

```
dotnet add package Nandel.Extensions.Http.GoogleIdentityProvider
```

##  (Required) Adding options

We need to add the options shared to all our GoogleIdentityProviders, we can do that using the example bellow:

```csharp
services.AddGoogleIdentityProviderOptions("NamingIsHard");
```

In the configuration provider you are using, you will have the chosen configuration section provided bellow with the 
configurations:
- ServiceAccountKeyPath: (Optional) This Configuration should point to your Service Account Key, if is not provided, we will use an approach that does not require a key but must be executed from inside GCP
- AuthorizationHeaderName: (Optional) In case you want to customize the header name in the requests sent

````json
{
    "NamingIsHard": {
        "ServiceAccountKeyPath": "my-service-account-key.json",
        "AuthorizationHeaderName": "X-Serverless-Authorization"
    }
} 
````

## (Option 1) Using a static value for the audience consumed

Using a static value for the Audience

```csharp
services.AddHttpClient("my-google-http-client")
    .AddGoogleIdentityProvider("https://my-google-cloud-run-service-url.us-central1.run.app");
```

## (Option 2) Using a dynamic value for the Audience

```csharp
services.AddHttpClient("my-google-http-client")
    .AddGoogleIdentityProvider(services => 
    {
        var config = services.GetRequiredService<IConfiguration>();
        var myAudience = config.GetValue<string>("NamingIsHard:Audience");
        return myAudience;
    });
```

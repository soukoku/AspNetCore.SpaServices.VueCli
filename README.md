Provides support for [Vue CLI](https://cli.vuejs.org/) in ASP.net Core's SPA scenarios
like the built-in support for react and angular. Only supported aspnet versions will
be supported.

This is mostly copied and modified from ASP.net Core's
implementation for react 
[https://github.com/dotnet/aspnetcore/blob/master/src/Middleware/SpaServices.Extensions/src/ReactDevelopmentServer](https://github.com/dotnet/aspnetcore/blob/master/src/Middleware/SpaServices.Extensions/src/ReactDevelopmentServer).


# Usage

## ASP.NET Project

Install the `Soukoku.AspNetCore.SpaServices.VueCli` NuGet package on the
ASP.NET Core web project, then modify the `Startup.cs` file similar to the following.


```cs
public void ConfigureServices(IServiceCollection services)
{
  // ... other aspnet configuration skipped here

  // new addition here
  services.AddSpaStaticFiles(configuration =>
  {
    configuration.RootPath = "clientapp/dist";
  });
}

public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
  // ... other aspnet configuration skipped here

  app.UseStaticFiles();
  app.UseSpaStaticFiles(); // new addition
  
  // ... more default stuff

  // new addition at end
  app.UseSpa(spa =>
  {
    spa.Options.SourcePath = "clientapp";

    if (env.IsDevelopment())
    {
      spa.UseVueCli("yarn"); // either "yarn" or "npm"
    }
  });
}
```


## Vue Project

The vue project is a typical one created by vue cli such as `vue create clientapp` and
placed inside the ASPNET site's project folder.


## Pubish Support

If publishing the ASPNET Core's project is needed then edit the .csproj file like below.
Change `SpaRoot` value to the actual vue project's folder name. Change yarn to npm if necessary.

```xml

  <PropertyGroup>
    <SpaRoot>clientapp\</SpaRoot>
    <DefaultItemExcludes>$(DefaultItemExcludes);$(SpaRoot)node_modules\**</DefaultItemExcludes>
  </PropertyGroup>

  <ItemGroup>
    <!-- Don't publish the SPA source files, but do show them in the project files list -->
    <Content Remove="$(SpaRoot)**" />
    <None Remove="$(SpaRoot)**" />
    <None Include="$(SpaRoot)**" Exclude="$(SpaRoot)node_modules\**" />
  </ItemGroup>

  <Target Name="DebugEnsureNodeEnv" BeforeTargets="Build" Condition=" '$(Configuration)' == 'Debug' And !Exists('$(SpaRoot)node_modules') ">
    <!-- Ensure Yarn is installed -->
    <Exec Command="yarn --version" ContinueOnError="true">
      <Output TaskParameter="ExitCode" PropertyName="ErrorCode" />
    </Exec>
    <Error Condition="'$(ErrorCode)' != '0'" Text="Yarn is required to build and run this project." />
    <Message Importance="high" Text="Restoring dependencies using 'yarn'. This may take several minutes..." />
    <Exec WorkingDirectory="$(SpaRoot)" Command="yarn install" />
  </Target>

  <Target Name="PublishRunWebpack" AfterTargets="ComputeFilesToPublish">
    <!-- As part of publishing, ensure the JS resources are freshly built in production mode -->
    <Exec WorkingDirectory="$(SpaRoot)" Command="yarn install" />
    <Exec WorkingDirectory="$(SpaRoot)" Command="yarn build" />

    <!-- Include the newly-built files in the publish output -->
    <ItemGroup>
      <DistFiles Include="$(SpaRoot)dist\**" />
      <ResolvedFileToPublish Include="@(DistFiles->'%(FullPath)')" Exclude="@(ResolvedFileToPublish)">
        <RelativePath>%(DistFiles.Identity)</RelativePath>
        <CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>
        <ExcludeFromSingleFile>true</ExcludeFromSingleFile>
      </ResolvedFileToPublish>
    </ItemGroup>
  </Target>

```

# Note

To get hot-module-reloading to work both vue's dev server and aspnet's 
site need to be on the same protocol for (http or https).

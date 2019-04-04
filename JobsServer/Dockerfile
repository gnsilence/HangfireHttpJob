FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY ["CommonUtils/CommonUtils.csproj", "CommonUtils/"]
COPY ["Hangfire.Dashboard.BasicAuthorization/Hangfire.Dashboard.BasicAuthorization.csproj", "Hangfire.Dashboard.BasicAuthorization/"]
COPY ["Hangfire.HttpJob/Hangfire.HttpJob.csproj", "Hangfire.HttpJob/"]
COPY ["Hangfire.Redis.StackExchange/Hangfire.Redis.StackExchange.csproj", "Hangfire.Redis.StackExchange/"]
COPY ["JobsServer/JobsServer.csproj", "JobsServer/"]

RUN dotnet restore "JobsServer/JobsServer.csproj"
COPY . .
WORKDIR "/src/JobsServer"
RUN dotnet build "JobsServer.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "JobsServer.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "JobsServer.dll"]
FROM microsoft/aspnetcore:2.0-stretch AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0-stretch AS build
WORKDIR /src
COPY ["HangFire/HangFire.csproj", "HangFire/"]
RUN dotnet restore "HangFire/HangFire.csproj"
COPY . .
WORKDIR "/src/HangFire"
RUN dotnet build "HangFire.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "HangFire.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "HangFire.dll"]

#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
#设置时间为中国上海
ENV TZ=Asia/Shanghai
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ServerSample/ServerSample.csproj", "ServerSample/"]
RUN dotnet restore "ServerSample/ServerSample.csproj"
COPY . .
WORKDIR "/src/ServerSample"
RUN dotnet build "ServerSample.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ServerSample.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ServerSample.dll"]
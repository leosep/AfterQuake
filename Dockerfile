FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/AfterQuake.Web/AfterQuake.Web.csproj", "src/AfterQuake.Web/"]
COPY ["src/AfterQuake.Infrastructure/AfterQuake.Infrastructure.csproj", "src/AfterQuake.Infrastructure/"]
COPY ["src/AfterQuake.Application/AfterQuake.Application.csproj", "src/AfterQuake.Application/"]
COPY ["src/AfterQuake.Domain/AfterQuake.Domain.csproj", "src/AfterQuake.Domain/"]
RUN dotnet restore "src/AfterQuake.Web/AfterQuake.Web.csproj"
COPY . .
WORKDIR "/src/src/AfterQuake.Web"
RUN dotnet build "AfterQuake.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AfterQuake.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir -p /app/wwwroot/uploads/photos && chmod 777 /app/wwwroot/uploads/photos
ENV ASPNETCORE_URLS=http://+:80
HEALTHCHECK --interval=30s --timeout=10s --retries=3 \
  CMD curl --fail http://localhost/health/live || exit 1
ENTRYPOINT ["dotnet", "AfterQuake.Web.dll"]

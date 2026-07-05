FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY Directory.Build.props NzNetpay.sln ./
COPY src/NzNetpay.Api/NzNetpay.Api.csproj src/NzNetpay.Api/
RUN dotnet restore src/NzNetpay.Api

COPY src/ src/
RUN dotnet publish src/NzNetpay.Api --no-restore -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "NzNetpay.Api.dll"]

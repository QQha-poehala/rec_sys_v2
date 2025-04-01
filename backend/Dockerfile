FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN mkdir -p /app/publish/resources/ml && \
    cp -r ./Rec_backend/resources/ml/* /app/publish/resources/ml/
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
RUN ls -la /app/resources/ml/
ENTRYPOINT ["dotnet", "Rec_backend.dll"]
#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["Learning.GrpcService/Learning.GrpcService.csproj", "Learning.GrpcService/"]
RUN dotnet restore "Learning.GrpcService/Learning.GrpcService.csproj"
COPY . .
WORKDIR "/src/Learning.GrpcService"
RUN dotnet build "Learning.GrpcService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Learning.GrpcService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Learning.GrpcService.dll"]
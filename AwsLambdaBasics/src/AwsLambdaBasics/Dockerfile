FROM public.ecr.aws/lambda/dotnet:8 AS base

FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
WORKDIR /src
COPY ["AwsLambdaBasics.csproj", "AwsLambdaBasics/"]
RUN dotnet restore "AwsLambdaBasics/AwsLambdaBasics.csproj"

WORKDIR "/src/AwsLambdaBasics"
COPY . .
RUN dotnet build "AwsLambdaBasics.csproj" --configuration Release --output /app/build

FROM build AS publish
RUN dotnet publish "AwsLambdaBasics.csproj" \
            --configuration Release \ 
            --runtime linux-x64 \
            --self-contained false \ 
            --output /app/publish \
            -p:PublishReadyToRun=true  

FROM base AS final
WORKDIR /var/task
COPY --from=publish /app/publish .
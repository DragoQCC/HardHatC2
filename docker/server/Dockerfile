FROM mcr.microsoft.com/dotnet/sdk:7.0-bookworm-slim

WORKDIR /app

EXPOSE 5000

STOPSIGNAL SIGKILL

COPY . .
#RUN git clone https://github.com/DragoQCC/HardHatC2.git .

RUN dotnet build /app/TeamServer/TeamServer.csproj

CMD dotnet run --project /app/TeamServer/TeamServer.csproj | tee --append /app/TeamServer/logs/server.log

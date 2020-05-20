This is the old version of the planthydra API
Hosted as a dotnet web app, in Azure
MSSql storage

Exposes a REST api for communication with the mobile app
Exposes a REST api for communication with the Socket.IO server (which is to be replaced with Azure IoT)

dotnet restore

dotnet run

Check out the Swagger API

Code: 
Start from Controllers
    Account - user management, auth
    Command - control the IoT device from the mobile app 
    InternalComms - REST api for signals from the IoT device
    SensorData - mock of the data from the sensors on the device (temp, light, humidity)
    Ws - the websockets controller used to communicate to the IoT device before transitioning to SocketIO

Other components of interest:
ScheduledWateringService - background service used to push events to the IoT device
PushNotificationService - service used to push notifications throught the Expo platform
EmailSender - SendGrid email client
SecretsVault - secrets manager (env variables + Azure KeyVault)


This is a proof of concept and in no way a showcase of coding best practices
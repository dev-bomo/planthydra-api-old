This is the old version of the planthydra API
Hosted as a dotnet web app, in Azure, with
MSSql storage

Exposes a REST api for communication with the mobile app</br>
Exposes a REST api for communication with the Socket.IO server (which is to be replaced with Azure IoT)

dotnet restore</br>
dotnet run

Check out the Swagger API

Code: 
Start from Controllers</br>
    <b>Account</b> - user management, auth</br>
    <b>Command</b> - control the IoT device from the mobile app </br>
    <b>InternalComms</b> - REST api for signals from the IoT device</br>
    <b>SensorData</b> - mock of the data from the sensors on the device (temp, light, humidity)</br>
    <b>Ws</b> - the websockets controller used to communicate to the IoT device before transitioning to SocketIO</br>

Other components of interest:
<b>ScheduledWateringService</b> - background service used to push events to the IoT device</br>
<b>PushNotificationService</b> - service used to push notifications throught the Expo platform</br>
<b>EmailSender</b> - SendGrid email client</br>
<b>SecretsVault</b> - secrets manager (env variables + Azure KeyVault)</br>


This is a proof of concept and in no way a showcase of coding best practices
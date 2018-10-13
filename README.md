This is a small library together with some simple apps used together with the Blynk server. The library is based on BlynkLibrary by
Sverre Frøystein together with standard TCP client in .NET core. The client will only use virtual pins.

The Blynk application for a Iphone or Android device is a very convinient way of handling IOT devices. Read more on https://www.blynk.cc/.

In order to test this library the following requirement must be met:

1. Install Blynk application on a Iphone or Android device.

2. Install and run https://github.com/blynkkk/blynk-server. The Blynk server is a java-based application which has to be accessible to 
   to the device above. For example it can be installed on a Raspberry Pi device.

3. Connect the application (the smart phone application) to the Blynk server and create a new project. Note that there is a authorization token for 
   the project. This token will be used later when the client is connected to blynk server (and hence identifies itself as the device).
   On the device main screen add a button with a virtual pin number.

4. Copy the authorization token for the project. Use dotnet commands to publish BlynkTester application:
   1. Use a IOT or computer device that has dotnet sdk installed. 
   2. Clone this repository.
   3. Run "dotnet build samples/Blynk.Tester/Blynk.Tester.csproj"
   4. Run "dotnet publish samples/Blynk.Tester/Blynk.Tester.csproj -r [rid]". Here [rid] is specified target platform (for example , win10-x64 (for Windows 10 x64) or linux-arm (for Raspberry pi)).
   5. Go to the publish folder and run "./Blynk.Tester -a [token]". Here [token] is the authorization token given by the project above.
   6. Press the start button for the created project on the smart phone. It will check that the Blynk.Tester is connected (saying that it is online).
   7. Press the button created on the smart phone and then the Blynk.Tester program will write out to the console pin id and value.   

With this client example it is possible to use the push or pull technology to read sensors or control any kind of software or hardware.



The BlynkRepeater application is just a UDP server together with a Blynk client. The server listens at a port and if a message of type

id value

is sent (for example, 15 33.2), then the virtual pin id will push the value to a correspoding configured device application. 

1. Create project in smartphone application and write down the authorization token. Create a "Value Display" configured to use virtual pin 15 and of Push type. 

2. Start Blynk.Repeater with 
   1. Go to the Blynk.Repeater folder.
   2. Run "dotnet build samples/Blynk.Repeater/Blynk.Repeater.csproj"
   3. Run "dotnet publish samples/Blynk.Repeater/Blynk.Repeater.csproj -r [rid]". Here [rid] is specified target platform (for example , win10-x64 (for Windows 10 x64) or linux-arm (for Raspberry pi)).
   4. Go to the publish folder and run "./BlynkRepeater -a [token] -s [server] -p [port]". Here [token] is the authorization token given by a project. [server] is the Blynk server application tcp-url. 
      [port] is the listening port for the udp server. 


3. Use Netcat command: 

nc 127.0.0.1 8000 -u

to send message messages like (manually send by pressing enter):

15 1000
15 101.33
15 -1.3
10 123

4. Start the device inside the smart phone application. If everything is ok, then the values will show up in the "Value Display" widget.


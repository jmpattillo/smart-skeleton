//By John M. Pattillo
//Adafruit_BNO055.cpp has been modified from the original by commenting out "Wire.begin()"
//once powered, WiFi network will be called "SmartSkeleton"
//if password is requested, enter "smartskeleton"

#include <ESP8266WiFi.h>
#include <WiFiUdp.h>
#include <WiFiClient.h>
#include <Wire.h>
#include <Adafruit_BNO055.h> 

//timing delays (determined empirically with a logic analyzer) TODO remeasure these for huzzah board
#define SAMPLE_DELAY (5) //in ms
#define BUS_DELAY (160) //in us
#define INIT_DELAY (1) //in ms

//i2c address of devices.  These are 7 bit i2c address.  The 8th bit is determined by Wire library to set read and write.
//addresses of TCA9548 i2c multiplexer boards
#define TCAADDR_1 0x74 //0x74 with LED boards.  A2 on TCA board is connected to VCC.  This prevents address conflict with default ALLCALL address of PCA9632
#define TCAADDR_2 0x71
//addresses for BNO055 Orientation sensor boards
#define BNOADDR_1 0x28 //default address for BNO055 breakout board
#define BNOADDR_2 0x29 //address pin on BNO055 breakout board connected to 3.3V pin. Used for hand and foot.
//addresses for PCA9632 led driver boards
#define PCAADDR_1 0x61 //pin A1 connected to GND, pin A0 connected to VCC (hand, foot, right eye)
#define PCAADDR_2 0x63 //pin A1 connected to VCC, pin A0 connected to VCC (forearm, leg, torso, left eye)
#define PCAADDR_3 0x62 //pin A1 connected to VCC, pin A0 connected to GND (femur, arm)

//bytes sent to i2c multiplexer to select buses
#define TCABUS_0  B1  //0x01
#define TCABUS_1  B10 //0x02
#define TCABUS_2  B100 //0x04
#define TCABUS_3  B1000
#define TCABUS_4  B10000
#define TCABUS_5  B100000
#define TCABUS_6  B1000000
#define TCABUS_7  B10000000

//pins on the huzzah board to control load switch and built in red led
#define sensorPin 14
#define redLedPin 0

//instantiate BNO055 sensor objects, assinging unique id number to each (50-56).  Not currently using the ID numbers.

Adafruit_BNO055 torsoSensor = Adafruit_BNO055(50, BNOADDR_1);
Adafruit_BNO055 armSensor = Adafruit_BNO055(51, BNOADDR_1);
Adafruit_BNO055 forearmSensor = Adafruit_BNO055(52, BNOADDR_1);
Adafruit_BNO055 handSensor = Adafruit_BNO055(53, BNOADDR_2);
Adafruit_BNO055 femurSensor = Adafruit_BNO055(54, BNOADDR_1);
Adafruit_BNO055 legSensor = Adafruit_BNO055(55, BNOADDR_1);
Adafruit_BNO055 footSensor = Adafruit_BNO055(56, BNOADDR_2);

char inByte = 0x00; //stores the request byte sent from Unity.  0 means do nothing
bool streamQuats = false;
int streamDelay = 0;
IPAddress streamIP;
int streamPort;

unsigned long previousMillis = 0;
const long interval = 1000;

byte redColor[] = {255, 0, 0};
byte halfRedColor[] = {128, 0, 0};
byte dimRedColor[] = {50, 0, 0};
byte greenColor[] = {0, 255, 0};
byte blueColor[] = {0, 0, 255};
byte magentaColor[] = {255, 0, 255};
byte yellowColor[] = {255, 179, 0};
byte whiteColor[] = {128, 128, 128};
byte cyanColor[] = {0, 255, 255};
byte noColor[] = {0, 0, 0};
byte dimGreenColor[] = {0, 50, 0};


//byte arrays to store raw Quaternion bytes from each sensor

byte torsoQuats[8];
byte armQuats[8];
byte forearmQuats[8];
byte handQuats[8];
byte femurQuats[8];
byte legQuats[8];
byte footQuats[8];

byte defQuat[8] = {0x00,0x40,0x00,0x00,0x00,0x00,0x00,0x00}; //default quat in Unity w,x,y,z = 1,0,0,0

//byte arrays to store raw Euler angles bytes from each sensor

byte torsoEulers[6];
byte armEulers[6];
byte forearmEulers[6];
byte handEulers[6];
byte femurEulers[6];
byte legEulers[6];
byte footEulers[6];

byte defEulers[6] = {0x00,0x00,0x00,0x00,0x00,0x00}; //default set of Euler angles in unity x,y,z = 0,0,0 degrees

//byte arrays to store sensor offsets for each sensor

byte defOffsets[22] = {0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00};
byte torsoOffsets[22];
byte armOffsets[22];
byte forearmOffsets[22];
byte handOffsets[22];
byte femurOffsets[22];
byte legOffsets[22];
byte footOffsets[22];


//these bools will be set as true if the sensor initializes properly

boolean torsoBegan = false; 
boolean armBegan = false;
boolean forearmBegan = false;
boolean handBegan = false;
boolean femurBegan = false;
boolean legBegan = false;
boolean footBegan = false;

//torso axis configuration bytes

byte torsoConfigs[6] = {0x06, 0x09, 0x12, 0x18, 0x21, 0x25};
byte torsoConfigSigns[8] = {0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07};
byte currentConfig = 0;
byte currentConfigSign = 0;

//set up wireless access point

const char *ssid = "SmartSkeleton";
const char *password = "smartskeleton";

WiFiUDP Udp;
unsigned int localUdpPort = 4210;  // local port to listen on
byte incomingPacket[255];  // buffer for incoming packets
//char  replyPacket[] = "Hi there! Got the message :-)";  // a reply string to send back


void setup()
{
  //Serial.begin(115200);
 // Serial.println();
 
  //Serial.print("Configuring access point...");
  WiFi.softAP(ssid, password);

  Udp.begin(localUdpPort);
  //Serial.printf("Now listening at IP %s, UDP port %d\n", WiFi.softAPIP().toString().c_str(), localUdpPort);
  
  delay (400); //this delay is only needed to help with logic analyzer
  pinMode(sensorPin,OUTPUT); //SCK or 14 is used to control the enable of the TPS high side load switch
  pinMode(redLedPin, OUTPUT); //gpio 0 is the red led.  pulling this low will turn it on
  digitalWrite(sensorPin, HIGH); //turn the TPS2055A on.  This will turn on the sensors.
  digitalWrite(redLedPin, LOW); //make sure led is on

  //initalize all the sensors
  delay(100);
  Wire.begin(12,13); //MUST COMMENT OUT THE Wire.begin() statement in Adafruit_BNO055.cpp before compiling for this to work
  Wire.setClock(400000); //maximum clock speed
  Wire.setClockStretchLimit(1000); //allows for the BNO055 to slow the clock more than is typical.  This is needed for the BNO055 to work reliably with the ESP8266  

  initAll(); //initialize all sensors and led modules.  LED will flash red if sensor fails to initialize.

  delay(1000); 
}


void loop()
{
  int packetSize = Udp.parsePacket(); //parse incoming packet.  If no incoming packet then packetsize = 0
  if (packetSize) //if packetsize is anything other than zero
  {
    // receive incoming UDP packets
    //Serial.printf("Received %d bytes from %s, port %d\n", packetSize, Udp.remoteIP().toString().c_str(), Udp.remotePort());
    int len = Udp.read(incomingPacket, 255);
  //  if (len > 0) //why is this necessary.  After looking at other examples, I don't think it is.  It is just setting the max byte in buffer to 0
   // {
    //  incomingPacket[len] = 0;
   // }
   //Serial.printf("UDP packet contents: %s\n", incomingPacket);

   if ((incomingPacket[0] == 'X') && (incomingPacket[2] == 'X')) {inByte = incomingPacket[1];} //valid incoming message is a single byte flanked by two constant bytes (X in ascii).
  }


 if (inByte != 0x00) { //check for new byte
  
  //get quaternions and send them for each sensor.
  //these are intended for testing with packet sending program.  Not intended for use with applicaiton
  //add a statement here to perhaps stop checking calibration if calibration is complete  
  if ((inByte == 'T') && torsoBegan ) { getTorsoQuats(); sendQuats(torsoQuats);} /*else if (!torsoBegan) {SendError();}*/
  if ((inByte == 'B') && armBegan) { getArmQuats(); sendQuats(armQuats);}
  if ((inByte == 'A') && forearmBegan) { getForearmQuats(); sendQuats(forearmQuats);}
  if ((inByte == 'M') && handBegan) { getHandQuats(); sendQuats(handQuats);}
  if ((inByte == 'F') && femurBegan) { getFemurQuats(); sendQuats(femurQuats);}
  if ((inByte == 'L') && legBegan) { getLegQuats(); sendQuats(legQuats);}
  if ((inByte == 'P') && footBegan) { getFootQuats(); sendQuats(footQuats);}

  //these are intended for use with application
  if (inByte == 'Q') {/*if calibrated then do this, also stop calibrating*/sendAllQuats();} //maybe add another logic test to make sure they are calibrated
  if (inByte == 'U') {sendUpperLimb();}
  if (inByte == 'O') {sendLowerLimb();}
  if (inByte == 'E') {sendAllEulers();} //may eliminate this one.
  if (inByte == 'C') {sendCalibStatus();}
  if (inByte == 'S') {sendOffsets();}
  if (inByte == 'R') {receiveOffsets(packetSize);} //be sure to send back some sort of confirmation to computer
  if (inByte == 'Z') {resetSensors();} //be sure to send back some sort of confirmation to computer
  if (inByte == 'I') {sendInquiry(); streamQuats = false;}
  if (inByte == 'G') {sendGravityVector();}
 // if (inByte == 'D') {streamQuats = true; streamIP = Udp.remoteIP(); streamPort = Udp.remotePort();}
 // if (inByte == ',') {streamDelay = streamDelay - 10; Udp.beginPacket(Udp.remoteIP(), Udp.remotePort()); Udp.write(streamDelay);}
 // if (inByte == '/') {streamDelay = streamDelay + 10; Udp.beginPacket(Udp.remoteIP(), Udp.remotePort()); Udp.write(streamDelay);}

  if (inByte == 'a') {selectForearmHand(); changeColor(blueColor, PCAADDR_1);} //turn wrist (hand) blue
  if (inByte == 'b') {selectForearmHand(); changeColor(yellowColor, PCAADDR_1);} //turn wrist (hand) yellow
  
  if (inByte == 'd') {selectForearmHand(); changeColor(noColor, PCAADDR_1);} //turn wrist (hand) off
  if (inByte == 'e') {selectForearmHand(); changeColor(blueColor, PCAADDR_2);} //turn elbow (forearm) blue
  if (inByte == 'f') {selectForearmHand(); changeColor(yellowColor, PCAADDR_2);} //turn elbow (forearm) yellow
  
  if (inByte == 'h') {selectForearmHand(); changeColor(noColor, PCAADDR_2);} //turn elbow (forearm) off
  if (inByte == 'i') {selectArm(); changeColor(blueColor, PCAADDR_3);} //turn shoulder (arm) blue
  if (inByte == 'j') {selectArm(); changeColor(yellowColor, PCAADDR_3);} //turn shoulder (arm) yellow
  if (inByte == 'k') {selectArm(); changeColor(magentaColor, PCAADDR_3);} //turn shoulder (arm) magenta
  
  if (inByte == 'm') {selectArm(); changeColor(noColor, PCAADDR_3);} //turn shoulder (arm) off
  if (inByte == 'n') {selectLegFoot(); changeColor(blueColor, PCAADDR_1);} //turn ankle (foot) blue
  if (inByte == 'o') {selectLegFoot(); changeColor(yellowColor, PCAADDR_1);} //turn ankle (foot) yellow
  
  if (inByte == 'q') {selectLegFoot(); changeColor(noColor, PCAADDR_1);} //turn ankle (foot) off
  if (inByte == 'r') {selectLegFoot(); changeColor(blueColor, PCAADDR_2);} //turn knee (leg) blue
  if (inByte == 's') {} //turn knee (leg) red
  if (inByte == 't') {selectLegFoot(); changeColor(noColor, PCAADDR_2);} //turn knee (leg) off
  if (inByte == 'u') {selectFemur(); changeColor(blueColor, PCAADDR_3);} //turn hip (femur) blue
  if (inByte == 'v') {selectFemur(); changeColor(yellowColor, PCAADDR_3);} //turn hip (femur) yellow
  if (inByte == 'w') {selectFemur(); changeColor(magentaColor, PCAADDR_3);} //turn hip (femur) magenta
  
  if (inByte == 'y') {selectFemur(); changeColor(noColor, PCAADDR_3);} //turn hip (femur) off
  if (inByte == 'z') {selectTorso(); changeColor(dimGreenColor, PCAADDR_2);} //turn torso green
  if (inByte == '1') {selectTorso(); changeColor(noColor, PCAADDR_2);} //turn torso off
  if (inByte == '2') {selectHead(); changeColor(redColor, PCAADDR_1); changeColor(redColor, PCAADDR_2);} //turn eyes red
  if (inByte == '3') {selectHead(); changeColor(greenColor, PCAADDR_1); changeColor(greenColor, PCAADDR_2);} //turn eyes green
  if (inByte == '4') {selectHead(); changeColor(whiteColor, PCAADDR_1); changeColor(whiteColor, PCAADDR_2);} //turn eyes white
  if (inByte == '5') {selectHead(); changeColor(blueColor, PCAADDR_1); changeColor(blueColor, PCAADDR_2);} //turn eyes blue
  if (inByte == '6') {selectHead(); changeColor(yellowColor, PCAADDR_1); changeColor(yellowColor, PCAADDR_2);} //turn eyes yellow
  if (inByte == '7') {selectHead(); changeColor(magentaColor, PCAADDR_1); changeColor(magentaColor, PCAADDR_2);} //turn eyes magenta
  if (inByte == '8') {selectHead(); changeColor(noColor, PCAADDR_1); changeColor(noColor, PCAADDR_2);} //turn eyes off
  if (inByte == '9') {if (currentConfig == 5) {currentConfig = 0;} else {currentConfig++;} changeTorsoOrientation(torsoConfigs[currentConfig]);sendOrientAndSign(currentConfig, currentConfigSign);} //next torso config
  if (inByte == '0') {if (currentConfig == 0) {currentConfig = 5;} else {currentConfig--;} changeTorsoOrientation(torsoConfigs[currentConfig]);sendOrientAndSign(currentConfig, currentConfigSign);} //previous torso config
  if (inByte == 'c') {if (currentConfigSign == 7) {currentConfigSign = 0;} else {currentConfigSign++;} changeTorsoSign(torsoConfigSigns[currentConfigSign]);sendOrientAndSign(currentConfig, currentConfigSign);} //next torso sign
  if (inByte == 'g') {if (currentConfigSign == 0) {currentConfigSign = 7;} else {currentConfigSign--;} changeTorsoSign(torsoConfigSigns[currentConfigSign]);sendOrientAndSign(currentConfig, currentConfigSign);} //previous torso sign
  if (inByte == 'x') {} //turn hip (femur) red
  if (inByte == 'l') {changeTorsoOrientation(torsoSensor.REMAP_CONFIG_P1);} //torso config default
  if (inByte == 'p') {changeTorsoSign(torsoSensor.REMAP_SIGN_P1);} //torso sign default
  
  inByte = 0x00; //reset the control byte
 }


/*if (streamQuats == true) {
  digitalWrite(redLedPin, HIGH);
  streamAllQuats();
  delay(streamDelay);
  digitalWrite(redLedPin, LOW);} */

delay(SAMPLE_DELAY);

} //end void loop

void receiveOffsets(int numbytes) { //TODO.  Add acknowledgement function sent after each one.  Add nested if statement to each so they will only work if sensor began
   if (numbytes != 26) {return;} //adjust this as I change the size of packet sent from unity.  Maybe add a trailer to be sure? Add check for trailer
   else { 
     char targetSensor = incomingPacket[3]; //The data structure should be XRX? + 22 bytes.  the ? is one of the characters below.  Maybe add trailer after bytes?
     if (targetSensor == 'T') {assignOffsets(torsoOffsets); selectTorso(); torsoSensor.setSensorOffsets(torsoOffsets); /*add send acknowlegement with sensor indicator*/}
     else if (targetSensor == 'B') {assignOffsets(armOffsets); selectArm(); armSensor.setSensorOffsets(armOffsets);}
     else if (targetSensor == 'A') {assignOffsets(forearmOffsets); selectForearmHand(); forearmSensor.setSensorOffsets(forearmOffsets);}
     else if (targetSensor == 'M') {assignOffsets(handOffsets); selectForearmHand(); handSensor.setSensorOffsets(handOffsets);}
     else if (targetSensor == 'F') {assignOffsets(femurOffsets); selectFemur(); femurSensor.setSensorOffsets(femurOffsets);}
     else if (targetSensor == 'L') {assignOffsets(legOffsets); selectLegFoot(); legSensor.setSensorOffsets(legOffsets);}
     else if (targetSensor == 'P') {assignOffsets(footOffsets); selectLegFoot(); footSensor.setSensorOffsets(footOffsets);}
     else { /*sendError() */ }
   }
}

void sendack(char sensor) {
  

}

void assignOffsets (byte * buff) {
  for (int i = 4; i < 26; i++) {int place = i-4; buff[place] = incomingPacket[i];} //
}

void sendAllQuats() { //TODO get rid of trailer?  Not needed due to udp checksum? Will have to change code in Unity app
  char Q = 0x51;
  Udp.beginPacket(Udp.remoteIP(), Udp.remotePort());
  Udp.write(Q);
  if (torsoBegan)   { getTorsoQuats(); writeQuats(torsoQuats); }     else { writeQuats(defQuat);}
  if (armBegan)     { getArmQuats(); writeQuats(armQuats); }         else { writeQuats(defQuat);}
  if (forearmBegan) { getForearmQuats(); writeQuats(forearmQuats); } else { writeQuats(defQuat);}
  if (handBegan)    { getHandQuats(); writeQuats(handQuats); }       else { writeQuats(defQuat);}
  if (femurBegan)   { getFemurQuats(); writeQuats(femurQuats); }     else { writeQuats(defQuat);}
  if (legBegan)     { getLegQuats(); writeQuats(legQuats); }         else { writeQuats(defQuat);}
  if (footBegan)    { getFootQuats(); writeQuats(footQuats); }       else { writeQuats(defQuat);}
  Udp.write(Q);
  Udp.endPacket();
  //Serial.printf("Sent UDP packet %s\n");
}

void streamAllQuats() {
  char Q = 0x51;
  Udp.beginPacket(streamIP, streamPort);
  Udp.write(Q);
  if (torsoBegan)   { getTorsoQuats(); writeQuats(torsoQuats); }     else { writeQuats(defQuat);}
  if (armBegan)     { getArmQuats(); writeQuats(armQuats); }         else { writeQuats(defQuat);}
  if (forearmBegan) { getForearmQuats(); writeQuats(forearmQuats); } else { writeQuats(defQuat);}
  if (handBegan)    { getHandQuats(); writeQuats(handQuats); }       else { writeQuats(defQuat);}
  if (femurBegan)   { getFemurQuats(); writeQuats(femurQuats); }     else { writeQuats(defQuat);}
  if (legBegan)     { getLegQuats(); writeQuats(legQuats); }         else { writeQuats(defQuat);}
  if (footBegan)    { getFootQuats(); writeQuats(footQuats); }       else { writeQuats(defQuat);}
  Udp.write(Q);
  Udp.endPacket();
  
}

void sendUpperLimb() {
  char U = 0x55;  
  Udp.beginPacket(Udp.remoteIP(), Udp.remotePort());
  Udp.write(U);
  if (torsoBegan)   { getTorsoQuats(); writeQuats(torsoQuats); }     else { writeQuats(defQuat);}
  if (armBegan)     { getArmQuats(); writeQuats(armQuats); }         else { writeQuats(defQuat);}
  if (forearmBegan) { getForearmQuats(); writeQuats(forearmQuats); } else { writeQuats(defQuat);}
  if (handBegan)    { getHandQuats(); writeQuats(handQuats); }       else { writeQuats(defQuat);} 
  Udp.write(U); 
  Udp.endPacket();
}

void sendLowerLimb() {
  char O = 0x4F;
  Udp.beginPacket(Udp.remoteIP(), Udp.remotePort());
  Udp.write(O);
  if (torsoBegan)   { getTorsoQuats(); writeQuats(torsoQuats); }     else { writeQuats(defQuat);}
  if (femurBegan)   { getFemurQuats(); writeQuats(femurQuats); }     else { writeQuats(defQuat);}
  if (legBegan)     { getLegQuats(); writeQuats(legQuats); }         else { writeQuats(defQuat);}
  if (footBegan)    { getFootQuats(); writeQuats(footQuats); }       else { writeQuats(defQuat);}
  Udp.write(O);
  Udp.endPacket();
}

void sendAllEulers () {
  char E = 0x45;
  Udp.beginPacket(Udp.remoteIP(), Udp.remotePort());
  Udp.write(E);
  if (torsoBegan)   {getTorsoEulers(); writeEulers(torsoEulers); }     else { writeEulers(defEulers);}
  if (armBegan)     {getArmEulers(); writeEulers(armEulers); }         else { writeEulers(defEulers);}
  if (forearmBegan) {getForearmEulers(); writeEulers(forearmEulers); } else { writeEulers(defEulers);}
  if (handBegan)    {getHandEulers(); writeEulers(handEulers); }       else { writeEulers(defEulers);}
  if (femurBegan)   {getFemurEulers(); writeEulers(femurEulers); }     else { writeEulers(defEulers);}
  if (legBegan)     {getLegEulers(); writeEulers(legEulers); }         else { writeEulers(defEulers);}
  if (footBegan)    {getFootEulers(); writeEulers(footEulers); }       else { writeEulers(defEulers);}
  Udp.write(E);
  Udp.endPacket();
}


void getTorsoQuats () {
  selectTorso();
  getRawQuatBytes(torsoQuats, BNOADDR_1);
}

void getArmQuats () {
  selectArm();
  getRawQuatBytes(armQuats, BNOADDR_1);
}

void getForearmQuats () {
  selectForearmHand();
  getRawQuatBytes(forearmQuats, BNOADDR_1);
}

void getHandQuats () {
  selectForearmHand();
  getRawQuatBytes(handQuats, BNOADDR_2);
}

void getFemurQuats () {
  selectFemur();
  getRawQuatBytes(femurQuats, BNOADDR_1);
}

void getLegQuats () {
  selectLegFoot();
  getRawQuatBytes(legQuats, BNOADDR_1);
}

void getFootQuats () {
  selectLegFoot();
  getRawQuatBytes(footQuats, BNOADDR_2);
}

void getTorsoEulers () {
  selectTorso();
  getRawEulerBytes(torsoEulers, BNOADDR_1);
  
}

void getArmEulers () {
  selectArm();
  getRawEulerBytes(armEulers, BNOADDR_1);
  
}

void getForearmEulers () {
  selectForearmHand();
  getRawEulerBytes(forearmEulers, BNOADDR_1); 
  
}

void getHandEulers () {
  selectForearmHand();
  getRawEulerBytes(handEulers, BNOADDR_2);
  
}

void getFemurEulers () {
  selectFemur();
  getRawEulerBytes(femurEulers, BNOADDR_1);
  
}

void getLegEulers () {
  selectLegFoot();
  getRawEulerBytes(legEulers, BNOADDR_1);
}

void getFootEulers () {
  selectLegFoot();
  getRawEulerBytes(footEulers, BNOADDR_2);
}


//Init functions for sensors

boolean initTorso () {
     selectTorso();
     boolean did_init = torsoSensor.begin();
     delay(INIT_DELAY);
     torsoSensor.setExtCrystalUse(true);
     delayMicroseconds(BUS_DELAY);
     return did_init;
}

boolean initArm () {
     selectArm();
     boolean did_init = armSensor.begin();
     delay(INIT_DELAY);
     armSensor.setExtCrystalUse(true);
     delayMicroseconds(BUS_DELAY);
     return did_init;
}

boolean initForearm () {
     selectForearmHand();
     boolean did_init = forearmSensor.begin();
     delay(INIT_DELAY);
     forearmSensor.setExtCrystalUse(true);
     delayMicroseconds(BUS_DELAY);
     return did_init;
}

boolean initHand () {
     selectForearmHand();
     boolean did_init = handSensor.begin();
     delay(INIT_DELAY);
     handSensor.setExtCrystalUse(true);
     delayMicroseconds(BUS_DELAY);
     return did_init;
}

boolean initFemur () {
     selectFemur();
     boolean did_init = femurSensor.begin();
     delay(INIT_DELAY);
     femurSensor.setExtCrystalUse(true);
     delayMicroseconds(BUS_DELAY);
     return did_init;
}

boolean initLeg () {
     selectLegFoot();
     boolean did_init = legSensor.begin();
     delay(INIT_DELAY);
     legSensor.setExtCrystalUse(true);
     delayMicroseconds(BUS_DELAY);
     return did_init;
}

boolean initFoot () {
     selectLegFoot();
     boolean did_init = footSensor.begin();
     delay(INIT_DELAY);
     footSensor.setExtCrystalUse(true);
     delayMicroseconds(BUS_DELAY);
     return did_init;
}


//select functions for sensors

void selectTorso() {
  tcaBusSelect(TCABUS_7, TCAADDR_1);
  delayMicroseconds(BUS_DELAY);
} 

void selectArm() {
  tcaBusSelect(TCABUS_1, TCAADDR_1);
  delayMicroseconds(BUS_DELAY);
  tcaBusSelect(TCABUS_6, TCAADDR_2);
  delayMicroseconds(BUS_DELAY);
}

void selectForearmHand () {
  tcaBusSelect(TCABUS_1, TCAADDR_1);
  delayMicroseconds(BUS_DELAY);
  tcaBusSelect(TCABUS_3, TCAADDR_2);
  delayMicroseconds(BUS_DELAY); 
}

void selectFemur() {
  tcaBusSelect(TCABUS_6, TCAADDR_1);
  delayMicroseconds(BUS_DELAY);
  tcaBusSelect(TCABUS_6, TCAADDR_2);
  delayMicroseconds(BUS_DELAY); 
}

void selectLegFoot() {
  tcaBusSelect(TCABUS_6, TCAADDR_1);
  delayMicroseconds(BUS_DELAY);
  tcaBusSelect(TCABUS_3, TCAADDR_2);
  delayMicroseconds(BUS_DELAY); 
}

void selectHead() {
  tcaBusSelect(TCABUS_2, TCAADDR_1);
  delayMicroseconds(BUS_DELAY);  
}



void tcaBusSelect(byte bus, byte i2caddress) { 
  Wire.beginTransmission(i2caddress);
  Wire.write(bus);
  Wire.endTransmission();
}

void getRawQuatBytes (byte * buff, int wire_address) {
   Wire.beginTransmission(wire_address);
   Wire.write(byte(0x20)); //0x20 is first of the BNO055 quaternion registers
   Wire.endTransmission();
   Wire.requestFrom(wire_address, 8); //get the eight quarternion bytes from the BNO055
   for (int i = 0; i < 8; i++) {
      buff[i] = Wire.read();
    }
}

void getRawEulerBytes (byte * buff, int wire_address) {
     Wire.beginTransmission(wire_address);
     Wire.write(byte(0x1A)); //0x1A is the first of the BNO055 Euler angle registers
     Wire.endTransmission();
     Wire.requestFrom(wire_address, 6); //get the six euler angle bytes from the BNO055
     for (int i = 0; i < 6; i++) {
        buff[i] = Wire.read();
     } 
}

 void sendQuats (byte * theseQuats) {
  Udp.beginPacket(Udp.remoteIP(), Udp.remotePort()); 
  for (int i = 0; i < 8; i++) {
     Udp.write(theseQuats[i]);
  }
  Udp.endPacket();
 }

 void sendEulers (byte * theseEulers) {
  Udp.beginPacket(Udp.remoteIP(), Udp.remotePort());
  for (int i = 0; i < 6; i++) {
    Udp.write(theseEulers[i]);
  }
  Udp.endPacket();
 }

 void writeQuats (byte * theseQuats) {
  for (int i = 0; i < 8; i++) {
    Udp.write(theseQuats[i]);
  }
 }

 void writeEulers (byte * theseEulers) {
  for (int i = 0; i < 6; i++) {
    Udp.write(theseEulers[i]);
  }
 }

 void writeTrailer () {
   for (int i = 0; i < 3; i++) {
    Udp.write(0x58);  //send three 'X' as a trailer for checking for valid data
   }
   
}

 void sendOrientAndSign(byte orient, byte sign ) {
   char G = 0x47;
   Udp.beginPacket(Udp.remoteIP(), Udp.remotePort());
   Udp.write(G);
   Udp.write(orient);
   Udp.write(sign);
   Udp.write(G);
   Udp.endPacket(); 
  
  }


  void sendCalibStatus () {
   char C = 0x43;
   Udp.beginPacket(Udp.remoteIP(), Udp.remotePort());
   Udp.write(C);
   uint8_t sys, gyro, accel, mag = 0;
   if (torsoBegan) {    
      selectTorso(); torsoSensor.getCalibration(&sys, &gyro, &accel, &mag); writeCalibStatus(sys,gyro,accel,mag);
      if((sys + gyro + accel + mag) < 7) {changeColor(halfRedColor, PCAADDR_2);}
      else if((sys + gyro + accel + mag) < 12) {changeColor(dimRedColor, PCAADDR_2);}
      else if((sys + gyro + accel + mag) == 12) {changeColor(dimGreenColor, PCAADDR_2);}  
   }     
   else {writeFours();}   
   sys, gyro, accel, mag = 0;  
   if (armBegan) {
      selectArm(); armSensor.getCalibration(&sys, &gyro, &accel, &mag); writeCalibStatus(sys,gyro,accel,mag);
      if((sys + gyro + accel + mag) < 7) {changeColor(halfRedColor, PCAADDR_3);}
      else if((sys + gyro + accel + mag) < 12) {changeColor(dimRedColor, PCAADDR_3);}
      else if((sys + gyro + accel + mag) == 12) {changeColor(dimGreenColor, PCAADDR_3);}  
    }     
   else {writeFours();}
   sys, gyro, accel, mag = 0;

   if (forearmBegan) {
      selectForearmHand(); forearmSensor.getCalibration(&sys, &gyro, &accel, &mag); writeCalibStatus(sys,gyro,accel,mag);
      if((sys + gyro + accel + mag) < 7) {changeColor(halfRedColor, PCAADDR_2);}
      else if((sys + gyro + accel + mag) < 12) {changeColor(dimRedColor, PCAADDR_2);}
      else if((sys + gyro + accel + mag) == 12) {changeColor(dimGreenColor, PCAADDR_2);}  
    }     
   else {writeFours();}
   sys, gyro, accel, mag = 0;

   if (handBegan) {
      selectForearmHand(); handSensor.getCalibration(&sys, &gyro, &accel, &mag); writeCalibStatus(sys,gyro,accel,mag);
      if((sys + gyro + accel + mag) < 7) {changeColor(halfRedColor, PCAADDR_1);}
      else if((sys + gyro + accel + mag) < 12) {changeColor(dimRedColor, PCAADDR_1);}
      else if((sys + gyro + accel + mag) == 12) {changeColor(dimGreenColor, PCAADDR_1);}  
    }     
   else {writeFours();}
   sys, gyro, accel, mag = 0;
  
   if (femurBegan) {selectFemur(); femurSensor.getCalibration(&sys, &gyro, &accel, &mag); writeCalibStatus(sys,gyro,accel,mag);} else {writeFours();}
   sys, gyro, accel, mag = 0;  
   if (legBegan) {selectLegFoot(); legSensor.getCalibration(&sys, &gyro, &accel, &mag); writeCalibStatus(sys,gyro,accel,mag);} else {writeFours();}
   sys, gyro, accel, mag = 0;
   if (footBegan) {selectLegFoot(); femurSensor.getCalibration(&sys, &gyro, &accel, &mag); writeCalibStatus(sys,gyro,accel,mag);} else {writeFours();}
   Udp.write(C);
   Udp.endPacket();
  }

  void sendOffsets() {
  char S = 0x53;
  Udp.beginPacket(Udp.remoteIP(), Udp.remotePort());
  Udp.write(S);
  if (torsoBegan) {getTorsoOffsets(); writeOffsets(torsoOffsets);} else {writeOffsets(defOffsets);}
  if (armBegan) {getArmOffsets(); writeOffsets(armOffsets);} else {writeOffsets(defOffsets);}
  if (forearmBegan) {getForearmOffsets(); writeOffsets(forearmOffsets);} else {writeOffsets(defOffsets);}
  if (handBegan) {getHandOffsets(); writeOffsets(handOffsets);} else {writeOffsets(defOffsets);}
  if (femurBegan) {getFemurOffsets(); writeOffsets(femurOffsets);} else {writeOffsets(defOffsets);}
  if (legBegan) {getLegOffsets(); writeOffsets(legOffsets);} else {writeOffsets(defOffsets);}
  if (footBegan) {getFootOffsets(); writeOffsets(footOffsets);} else {writeOffsets(defOffsets);}
  Udp.write(S);
  Udp.endPacket(); 
  }

 

  void resetSensors() {
    digitalWrite(sensorPin, LOW);
    digitalWrite(redLedPin, HIGH);
    delay(3000);
    digitalWrite(sensorPin, HIGH);   
    delay(1000);
    initAll();
    delay(1000);
    digitalWrite(redLedPin, LOW);
    //TODO add routine to check for unstarted sensors and send flash signal
    sendStatus();    
  }

  void writeCalibStatus (uint8_t sys, uint8_t gyro, uint8_t accel, uint8_t mag) {
   Udp.write(sys);
   Udp.write(gyro);
   Udp.write(accel);
   Udp.write(mag);
  }

  void writeFours (){
   for (int i = 0; i < 4; i++) { Udp.write(0x04); }
  }

  void sendInquiry () {
    /*char inquiry[] = "Got a request";
    Udp.beginPacket(Udp.remoteIP(), Udp.remotePort());
    Udp.write(inquiry);
    Udp.endPacket();
    */
    sendStatus();
  }

  void sendGravityVector () {
    
  }

  void getOffsets (byte * buff, int wire_address) {
   //using my own function here to avoid the requirement for FullyCalibrated
   Wire.beginTransmission(wire_address);
   Wire.write(byte(0x55)); //0x55 is first of the BNO055 offset registers
   Wire.endTransmission();
   Wire.requestFrom(wire_address, 22); //get the 22 offset bytes from the BNO055
   for (int i = 0; i < 22; i++) {
      buff[i] = Wire.read();
    }
  }

  void writeOffsets (byte * buff) {
     for (int i = 0; i < 22; i++) {
     Udp.write(buff[i]);
    }  
  }


  void getTorsoOffsets () {
    selectTorso();
    torsoSensor.setMode(torsoSensor.OPERATION_MODE_CONFIG);
    getOffsets(torsoOffsets, BNOADDR_1);
    torsoSensor.setMode(torsoSensor.OPERATION_MODE_NDOF);  
  }

  void getArmOffsets () {
    selectArm();
    armSensor.setMode(armSensor.OPERATION_MODE_CONFIG);
    getOffsets(armOffsets, BNOADDR_1);
    armSensor.setMode(armSensor.OPERATION_MODE_NDOF); 
  }

  void getForearmOffsets () {
    selectForearmHand();
    forearmSensor.setMode(forearmSensor.OPERATION_MODE_CONFIG);
    getOffsets(forearmOffsets, BNOADDR_1);
    forearmSensor.setMode(forearmSensor.OPERATION_MODE_NDOF); 
  }

  void getHandOffsets () {
    selectForearmHand();
    handSensor.setMode(handSensor.OPERATION_MODE_CONFIG);
    getOffsets(handOffsets, BNOADDR_2);
    handSensor.setMode(handSensor.OPERATION_MODE_NDOF);
  }

  void getFemurOffsets () {
    selectFemur();
    femurSensor.setMode(femurSensor.OPERATION_MODE_CONFIG);
    getOffsets(femurOffsets, BNOADDR_1);
    femurSensor.setMode(forearmSensor.OPERATION_MODE_NDOF);
  }

  void getLegOffsets () {
    selectLegFoot();
    legSensor.setMode(legSensor.OPERATION_MODE_CONFIG);
    getOffsets(legOffsets, BNOADDR_1);
    legSensor.setMode(legSensor.OPERATION_MODE_NDOF);
  }

  void getFootOffsets () {
    selectLegFoot();
    footSensor.setMode(footSensor.OPERATION_MODE_CONFIG);
    getOffsets(footOffsets, BNOADDR_2);
    footSensor.setMode(footSensor.OPERATION_MODE_NDOF);
  }

  void sendStatus () {
    char I = 0x49;
    Udp.beginPacket(Udp.remoteIP(), Udp.remotePort());
    Udp.write(I);
    byte t = 0x01;
    byte f = 0x00;
    if(torsoBegan) {Udp.write(t);} else {Udp.write(f);}
    if(armBegan) {Udp.write(t);} else {Udp.write(f);}
    if(forearmBegan) {Udp.write(t);} else {Udp.write(f);}
    if(handBegan) {Udp.write(t);} else {Udp.write(f);}
    if(femurBegan) {Udp.write(t);} else {Udp.write(f);}
    if(legBegan) {Udp.write(t);} else {Udp.write(f);}
    if(footBegan) {Udp.write(t);} else {Udp.write(f);}
    Udp.write(I);
    Udp.endPacket(); 
  }


  void changeTorsoOrientation ( byte orient) {
    selectTorso();
    torsoSensor.setMode(torsoSensor.OPERATION_MODE_CONFIG);
    Wire.beginTransmission(BNOADDR_1);
    Wire.write(0x41); //write to orientation config address
    Wire.write(orient); //write orienation configuration byte
    Wire.endTransmission();
    torsoSensor.setMode(torsoSensor.OPERATION_MODE_NDOF);    
  }

  void changeTorsoSign (byte sign) {
    selectTorso();
    torsoSensor.setMode(torsoSensor.OPERATION_MODE_CONFIG);
    Wire.beginTransmission(BNOADDR_1);
    Wire.write(0x42); //write to orientation sign address
    Wire.write(sign); //write the sign config byte
    Wire.endTransmission();
    torsoSensor.setMode(torsoSensor.OPERATION_MODE_NDOF);
  } 

  void initTorsoLED () {
    selectTorso();
    initLED(PCAADDR_2); 
  }

 void initArmLED () {
    selectArm();
    initLED(PCAADDR_3);  
  }

 void initForearmLED () {
    selectForearmHand();
    initLED(PCAADDR_2);  
  }

 void initHandLED () {
    selectForearmHand();
    initLED(PCAADDR_1);  
  }

  void initFemurLED () {
    selectFemur();
    initLED(PCAADDR_3); 
  }

  void initLegLED () {
    selectLegFoot();
    initLED(PCAADDR_2);    
  }

  void initFootLED () {
    selectLegFoot();
    initLED(PCAADDR_1);   
  }

  void initLeftEyeLED () {
    selectHead();
    initLED(PCAADDR_2);
  }

  void initRightEyeLED () {
    selectHead();
    initLED(PCAADDR_1);
  }

  void initLED (byte address) {
    Wire.beginTransmission(address);
    Wire.write(0x00); //write to first mode register
    Wire.write(0x00); //first mode register all zeros
    Wire.endTransmission();
    Wire.beginTransmission(address);
    Wire.write(0x01); //write to second mode register
    Wire.write(0x00); //second mode register all zeros
    Wire.endTransmission();
    Wire.beginTransmission(address);
    Wire.write(0x08); //write to ledout register
    Wire.write(0x2A); //led out register controlled by indivdual pwm registers
    Wire.endTransmission();
    changeColor(noColor, address);    
  }

  void changeColor(byte * color, byte address) {  //pca9632 pwm bytes range from 0x00 (off) to 0xFF (fully on) in 256 steps
    Wire.beginTransmission(address); 
    Wire.write(0xA2);  //control register points at red pwm register with autoincrement flag turned on
    Wire.write(color[0]);  //red - write red pwm register, autoincrement pointer to blue pwm register
    Wire.write(color[2]);  //blue - write blue pwm register, autoincrement pointer to green pwm register
    Wire.write(color[1]);  //green - write green pwm register
    Wire.endTransmission();
  }

  void checkCalibration() {
    uint8_t sys, gyro, accel, mag = 0;
    if (torsoBegan) {      
      torsoSensor.getCalibration(&sys, &gyro, &accel, &mag);
      if(sys == 0) {selectTorso(); changeColor(redColor, PCAADDR_2);}
      if(sys == 1) {selectTorso(); changeColor(halfRedColor, PCAADDR_2);}
      if(sys == 2) {selectTorso(); changeColor(dimRedColor, PCAADDR_2);}
      if(sys == 3) {selectTorso(); changeColor(greenColor, PCAADDR_2);}
      }
  }

void flashRed (byte address) {

   Wire.beginTransmission(address);
   Wire.write(0x01); //write to second mode register
   Wire.write(0x20); //second mode register to blinking
   Wire.endTransmission();

   Wire.beginTransmission(address);
   Wire.write(0x06); //write to grppwm address
   Wire.write(0x80); //set half-on duty cycle for blink
   Wire.endTransmission();

   Wire.beginTransmission(address);
   Wire.write(0x07); //write to grpfreq address
   Wire.write(0x17); //set one second perior for blink
   Wire.endTransmission();
   
   Wire.beginTransmission(address);
   Wire.write(0x08); //write to ledout register
   Wire.write(0x03); //red led in grpfreq,grpwm mode.  Others off
   Wire.endTransmission();

   Wire.beginTransmission(address);
   Wire.write(0x02);
   Wire.write(0xFF);
   Wire.endTransmission();
  
} 

void initAll() {
  torsoBegan = initTorso();
  armBegan = initArm();
  forearmBegan = initForearm();
  handBegan = initHand();
  femurBegan = initFemur();
  legBegan = initLeg();
  footBegan = initFoot();

  initTorsoLED();
  initArmLED();
  initForearmLED();
  initHandLED();
  initFemurLED();
  initLegLED();
  initFootLED();
  
  //put initialization of PCA9632 here
  if (!torsoBegan) {selectTorso(); flashRed(PCAADDR_2);}/*{initTorsoLED();} else {selectTorso(); flashRed(PCAADDR_2);} */
  if (!armBegan) {selectArm(); flashRed(PCAADDR_3);}
  if (!forearmBegan) {selectForearmHand(); flashRed(PCAADDR_2);}
  if (!handBegan) {selectForearmHand(); flashRed(PCAADDR_1);}
  if (!femurBegan) {selectFemur(); flashRed(PCAADDR_3);}
  if (!legBegan) {selectLegFoot(); flashRed(PCAADDR_2);}
  if (!footBegan) {selectLegFoot(); flashRed(PCAADDR_1);}
  
}

    



  



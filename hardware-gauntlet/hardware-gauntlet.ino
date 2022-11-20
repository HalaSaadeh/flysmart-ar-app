#include <mpu9250.h>
#include "I2Cdev.h"
#include "MPU6050_6Axis_MotionApps20.h"
#include <imuFilter.h>
#include <ArduinoJson.h>
#include <SoftwareSerial.h>
#include <MPU6050.h>

#if I2CDEV_IMPLEMENTATION == I2CDEV_ARDUINO_WIRE
#include "Wire.h"
#endif

#define indexPin A0
#define middlePin A1
#define INTERRUPT_PIN 2  // use pin 2 on Arduino Uno & most boards
#define vibrationPin 3
#define vibrationPin2 4
#define TO_WORLD true  // Project local axis to global reference frame = true
// Project global axis to local reference frame = false
#define VCC 5                    // voltage at Ardunio 5V line
#define R_DIV 10000.0            // resistor used to create a voltage divider
#define flatResistance1 12000.0  // resistance when flat
#define bendResistance1 19000.0  // resistance at 90 deg
#define flatResistance2 10000.0  // resistance when flat
#define bendResistance2 17000.0  // resistance at 90 deg

// Imu sensor
MPU6050 accelgyro(0x69);
bfs::Mpu9250 imu;

int16_t ax, ay, az;
int16_t gx, gy, gz;

struct MyData {
  byte X;
  byte Y;
  byte Z;
};

MyData data;

float correct;
int j = 0;
// MPU control/status vars
bool dmpReady = false;   // set true if DMP init was successful
uint8_t mpuIntStatus;    // holds actual interrupt status byte from MPU
uint8_t devStatus;       // return status after each device operation (0 = success, !0 = error)
uint16_t packetSize;     // expected DMP packet size (default is 42 bytes)
uint16_t fifoCount;      // count of all bytes currently in FIFO
uint8_t fifoBuffer[64];  // FIFO storage buffer
Quaternion q;            // [w, x, y, z]         quaternion container

VectorFloat gravity;  // [x, y, z]            gravity vector
float ypr[3];         // [yaw, pitch, roll]   yaw/pitch/roll container and gravity vector

// ================================================================
// ===               INTERRUPT DETECTION ROUTINE                ===
// ================================================================

volatile bool mpuInterrupt = false;  // indicates whether MPU interrupt pin has gone high
void dmpDataReady() {
  mpuInterrupt = true;
}

void Initialize() {

// join I2C bus (I2Cdev library doesn't do this automatically)
#if I2CDEV_IMPLEMENTATION == I2CDEV_ARDUINO_WIRE
  Wire.begin();
  Wire.setClock(400000);
  Wire.setWireTimeout(3000, true);
#elif I2CDEV_IMPLEMENTATION == I2CDEV_BUILTIN_FASTWIRE
  Fastwire::setup(400, true);
#endif

  // initialize device
  Serial.println("Initializing I2C devices...");
  accelgyro.initialize();
  //pinMode(INTERRUPT_PIN, INPUT);


  // verify connection
  Serial.println("Testing device connections...\n");
  Serial.println(accelgyro.testConnection() ? "MPU6050 connection successful" : "MPU6050 connection failed");

  // load and configure the DMP
  Serial.println(F("Initializing DMP..."));
  devStatus = accelgyro.dmpInitialize();

  Serial.println("PID tuning... Each Dot = 100 readings");

  accelgyro.CalibrateAccel(6);
  accelgyro.CalibrateGyro(6);
  Serial.println("\nat 600 Readings");
  accelgyro.PrintActiveOffsets();
  Serial.println();
  accelgyro.CalibrateAccel(1);
  accelgyro.CalibrateGyro(1);
  Serial.println("700 Total Readings");
  accelgyro.PrintActiveOffsets();
  Serial.println();
  accelgyro.CalibrateAccel(1);
  accelgyro.CalibrateGyro(1);
  Serial.println("800 Total Readings");
  accelgyro.PrintActiveOffsets();
  Serial.println();
  accelgyro.CalibrateAccel(1);
  accelgyro.CalibrateGyro(1);
  Serial.println("900 Total Readings");
  accelgyro.PrintActiveOffsets();
  Serial.println();
  accelgyro.CalibrateAccel(1);
  accelgyro.CalibrateGyro(1);
  Serial.println("1000 Total Readings");
  accelgyro.PrintActiveOffsets();
  Serial.println("\n\n Offset Calculation Complete \n");
  Serial.println("Offsets successfully applied.");

  if (devStatus == 0) {
    Serial.println(F("Enabling DMP..."));
    accelgyro.setDMPEnabled(true);

    // enable Arduino interrupt detection
    Serial.print(F("Enabling interrupt detection (Arduino external interrupt "));
    Serial.print(digitalPinToInterrupt(INTERRUPT_PIN));
    Serial.println(F(")..."));
    attachInterrupt(digitalPinToInterrupt(INTERRUPT_PIN), dmpDataReady, RISING);
    mpuIntStatus = accelgyro.getIntStatus();

    // set our DMP Ready flag so the main loop() function knows it's okay to use it
    Serial.println(F("DMP ready! Waiting for first interrupt..."));
    dmpReady = true;
    // get expected DMP packet size for later comparison
    packetSize = accelgyro.dmpGetFIFOPacketSize();
  } else {
    Serial.print(F("DMP Initialization failed (code "));
    Serial.print(devStatus);
    Serial.println(F(")"));
  }


}  // Initialize

StaticJsonDocument<200> output;  // JSON Output

SoftwareSerial BluetoothSerial(10, 11);  // Rx, Tx

float velocity = 0;
float previous_time = 0;             // for calculating velocity
float velocity_reset_hold_time = 0;  // for resetting velocity to zero
bool start_velocity_reset_hold = false;
bool readingVelocity = true;
float last_velocity_read = 0;

void setup() {
  // Set input pins for flex sensors
  pinMode(indexPin, INPUT);
  pinMode(middlePin, INPUT);
  pinMode(vibrationPin, OUTPUT);
  pinMode(vibrationPin2, OUTPUT);
  digitalWrite(vibrationPin, LOW);
  digitalWrite(vibrationPin2, LOW);
  // Serial to display data
  Serial.begin(9600);
  BluetoothSerial.begin(9600);
  while (!Serial) {}

  // Start the I2C bus
  Wire.begin();
  Wire.setClock(400000);

  // I2C bus,  0x68 address
  imu.Config(&Wire, bfs::Mpu9250::I2C_ADDR_PRIM);

  // Initialize and configure IMU
  if (!imu.Begin()) {
    Serial.println("Error initializing communication with IMU");
    while (1) {}
  }
  // Set the sample rate divider
  if (!imu.ConfigSrd(19)) {
    Serial.println("Error configured SRD");
    while (1) {}
  }

  output["droneStatusText"] = "Steady";

  Initialize();
}

void loop() {

  if (imu.Read() && dmpReady) {

    float accelX = -imu.accel_x_mps2();
    float accelY = -imu.accel_y_mps2();
    float accelZ = -imu.accel_z_mps2();
    float gyroX = imu.gyro_x_radps();
    float gyroY = imu.gyro_y_radps();
    float gyroZ = imu.gyro_z_radps();

    getYPR();
    detectElevation(accelX, accelY, accelZ);
    readFlexSensors();
    vibrateOnCollision();

    serializeJson(output, Serial);
    Serial.println();

    serializeJson(output, BluetoothSerial);
    BluetoothSerial.println();
  }
}

float digitalize(float num) {
  if (num > 0)
    return (int)(num / 0.4) * 0.4;
  else
    return (int)(num / 0.4) * 0.4;
}

void detectElevation(float accelX, float accelY, float accelZ) {
  float accelSum = digitalize(sqrt(square(accelX) + square(accelY) + square(accelZ)) - 10.3);
  float filteredAccelSum = abs(ypr[1] * 180 / M_PI) < 25 && abs(ypr[2] * 180 / M_PI) < 25 ? accelSum : 0;
  velocity += filteredAccelSum * (millis() - previous_time) / 1000;

  // Serial.print("bound1: ");
  // Serial.print(1);
  // Serial.print(",bound2: ");
  // Serial.print(-1);
  // Serial.print(",vel: ");
  // Serial.print(velocity);
  // Serial.print(",acc: ");
  // Serial.print(filteredAccelSum);
  // Serial.print(" ");

  if (readingVelocity && velocity > 0.2 && output["droneStatusText"] == "Steady")
    output["droneStatusText"] = "UpFast";
  else if (readingVelocity && velocity < -0.2 && output["droneStatusText"] == "Steady")
    output["droneStatusText"] = "DownFast";

  if (readingVelocity && ((output["droneStatusText"] == "UpFast" && velocity < 0.2) || (output["droneStatusText"] == "DownFast" && velocity > -0.2))) {
    readingVelocity = false;
    last_velocity_read = millis();
  }

  if (!readingVelocity && (millis() - last_velocity_read) > 900) {
    readingVelocity = true;
    output["droneStatusText"] = "Steady";
  }

  if (!start_velocity_reset_hold && velocity != 0 && filteredAccelSum == 0)
    start_velocity_reset_hold = true;
  if (start_velocity_reset_hold) {
    if (velocity != 0 && filteredAccelSum == 0)
      velocity_reset_hold_time++;
    else {
      start_velocity_reset_hold = false;
      velocity_reset_hold_time = 0;
    }
    if (velocity_reset_hold_time == 2) {
      velocity = 0;
      start_velocity_reset_hold = false;
      velocity_reset_hold_time = 0;
    }
  }

  previous_time = millis();
}

void readFlexSensors() {
  // Read the ADC, and calculate voltage and resistance from it
  int ADCflex = analogRead(indexPin);
  float Vflex = ADCflex * VCC / 1023.0;
  float Rflex = R_DIV * (VCC / Vflex - 1.0);

  // Use the calculated resistance to estimate the sensor's bend angle:
  float angle1 = map(Rflex, flatResistance1, bendResistance1, 0, 90.0);

  // Read the ADC, and calculate voltage and resistance from it
  int ADCflex2 = analogRead(middlePin);
  float Vflex2 = ADCflex2 * VCC / 1023.0;
  float Rflex2 = R_DIV * (VCC / Vflex2 - 1.0);

  // Use the calculated resistance to estimate the sensor's bend angle:
  float angle2 = map(Rflex2, flatResistance2, bendResistance2, 0, 90.0);

  if (angle1 < 20 && angle2 < 20)
    output["currentGesture"] = "Open Hand";
  else if (angle1 > 70 && angle2 > 70)
    output["currentGesture"] = "Closed Grip";
  else if (angle1 < 30 && angle2 > 65)
    output["currentGesture"] = "Finger Raised";
  else if (angle1 > 40 && angle2 < 30)
    output["currentGesture"] = "OK Hand";
  else
    output["currentGesture"] = "Undetected Gesture";
}

// void getYPR() {
//   while (!mpuInterrupt && fifoCount < packetSize) {
//     if (mpuInterrupt && fifoCount < packetSize) {
//       fifoCount = accelgyro.getFIFOCount();
//     }
//   }

//   mpuInterrupt = false;
//   mpuIntStatus = accelgyro.getIntStatus();
//   fifoCount = accelgyro.getFIFOCount();

//   // check for overflow (this should never happen unless our code is too inefficient)
//   if ((mpuIntStatus & _BV(MPU6050_INTERRUPT_FIFO_OFLOW_BIT)) || fifoCount >= 1024) {
//     // reset so we can continue cleanly
//     accelgyro.resetFIFO();
//     fifoCount = accelgyro.getFIFOCount();
//     Serial.println(F("FIFO overflow!"));

//     // otherwise, check for DMP data ready interrupt (this should happen frequently)
//   } else if (mpuIntStatus & _BV(MPU6050_INTERRUPT_DMP_INT_BIT)) {
//     // wait for correct available data length, should be a VERY short wait
//     while (fifoCount < packetSize) fifoCount = accelgyro.getFIFOCount();

//     // read a packet from FIFO
//     accelgyro.getFIFOBytes(fifoBuffer, packetSize);

//     // track FIFO count here in case there is > 1 packet available
//     // (this lets us immediately read more without waiting for an interrupt)
//     fifoCount -= packetSize;

//     // Get Yaw, Pitch and Roll values
// #ifdef OUTPUT_READABLE_YAWPITCHROLL
//     accelgyro.dmpGetQuaternion(&q, fifoBuffer);
//     accelgyro.dmpGetGravity(&gravity, &q);
//     accelgyro.dmpGetYawPitchRoll(ypr, &q, &gravity);

//     // Yaw, Pitch, Roll values - Radians to degrees
//     ypr[0] = ypr[0] * 180 / M_PI;
//     ypr[1] = ypr[1] * 180 / M_PI;
//     ypr[2] = ypr[2] * 180 / M_PI;

//     // output["yaw"] = ypr[0];
//     // output["pitch"] = ypr[1];
//     // output["roll"] = ypr[2];
//     Serial.print(ypr[0]);
//     Serial.print(" ");
//     Serial.print(ypr[1]);
//     Serial.print(" ");
//     Serial.println(ypr[2]);

//     // Skip 300 readings (self-calibration process)
//     if (j <= 300) {
//       correct = ypr[0];  // Yaw starts at random value, so we capture last value after 300 readings
//       j++;
//     }
//     // After 300 readings
//     else {
//       ypr[0] = ypr[0] - correct;  // Set the Yaw to 0 deg - subtract  the last random Yaw value from the currrent value to make the Yaw 0 degrees
//     }
// #endif
//   }
// }

void getYPR() {
  accelgyro.getMotion6(&ax, &ay, &az, &gx, &gy, &gz);
  if (accelgyro.dmpGetCurrentFIFOPacket(fifoBuffer)) {
    // display Euler angles in degrees
    accelgyro.dmpGetQuaternion(&q, fifoBuffer);
    accelgyro.dmpGetGravity(&gravity, &q);
    accelgyro.dmpGetYawPitchRoll(ypr, &q, &gravity);

    output["yaw"] = ypr[0] * 180 / M_PI;
    output["pitch"] = ypr[1] * 180 / M_PI;
    output["roll"] = ypr[2] * 180 / M_PI;
  }
}
void vibrateOnCollision() {

  if (BluetoothSerial.available()) {
    Serial.println(BluetoothSerial.read());
  }
}
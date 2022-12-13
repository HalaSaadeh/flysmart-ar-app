#include <ros.h>
#include <crazys_arduino_controller/GloveData.h>
#include <mpu9250.h>
#include "I2Cdev.h"
#include "MPU6050_6Axis_MotionApps20.h"
#include <MPU6050.h>

#if I2CDEV_IMPLEMENTATION == I2CDEV_ARDUINO_WIRE
#include "Wire.h"
#endif

#define indexPin A0
#define middlePin A1
#define INTERRUPT_PIN 2  // use pin 2 on Arduino Uno & most boards

#define VCC 5                    // voltage at Ardunio 5V line
#define R_DIV 10000.0            // resistor used to create a voltage divider
#define flatResistance1 12000.0  // resistance when flat
#define bendResistance1 19000.0  // resistance at 90 deg
#define flatResistance2 10000.0  // resistance when flat
#define bendResistance2 17000.0  // resistance at 90 deg

// Imu sensor
MPU6050 accelgyro(0x69);
bfs::Mpu9250 imu;

ros::NodeHandle_<ArduinoHardware, 1, 1, 1, 128> nh;
crazys_arduino_controller::GloveData output;
ros::Publisher pub("CrazyS_arduino/glove_data", &output);

int16_t ax, ay, az;
int16_t gx, gy, gz;

// MPU control status vars
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
#elif I2CDEV_IMPLEMENTATION == I2CDEV_BUILTIN_FASTWIRE
  Fastwire::setup(400, true);
#endif

  accelgyro.initialize();
  pinMode(INTERRUPT_PIN, INPUT);

  // load and configure the DMP
  devStatus = accelgyro.dmpInitialize();


  accelgyro.CalibrateAccel(6);
  accelgyro.CalibrateGyro(6);
  accelgyro.PrintActiveOffsets();
  accelgyro.CalibrateAccel(1);
  accelgyro.CalibrateGyro(1);
  accelgyro.PrintActiveOffsets();
  accelgyro.CalibrateAccel(1);
  accelgyro.CalibrateGyro(1);
  accelgyro.PrintActiveOffsets();
  accelgyro.CalibrateAccel(1);
  accelgyro.CalibrateGyro(1);
  accelgyro.PrintActiveOffsets();
  accelgyro.CalibrateAccel(1);
  accelgyro.CalibrateGyro(1);
  accelgyro.PrintActiveOffsets();

  accelgyro.setDMPEnabled(true);

  attachInterrupt(digitalPinToInterrupt(INTERRUPT_PIN), dmpDataReady, RISING);
  mpuIntStatus = accelgyro.getIntStatus();

  // set our DMP Ready flag so the main loop() function knows it's okay to use it
  dmpReady = true;
  // get expected DMP packet size for later comparison
  packetSize = accelgyro.dmpGetFIFOPacketSize();
}  // Initialize

float velocity = 0;
float previous_time = 0;             // for calculating velocity
float velocity_reset_hold_time = 0;  // for resetting velocity to zero
bool start_velocity_reset_hold = false;
bool readingVelocity = true;
float last_velocity_read = 0;

void setup() {
  
  nh.getHardware()->setBaud(9600);
  nh.initNode();
  nh.advertise(pub);
  // Set input pins for flex sensors
  pinMode(indexPin, INPUT);
  pinMode(middlePin, INPUT);

  // Serial to display data
  Serial.begin(9600);
  while (!Serial) {}

  // Start the I2C bus
  Wire.begin();
  Wire.setClock(400000);
  Wire.setWireTimeout(3000, true);

  // I2C bus,  0x68 address
  imu.Config(&Wire, bfs::Mpu9250::I2C_ADDR_PRIM);
  
  // Initialize and configure IMU
  if (!imu.Begin()) {
    while (1) {}
  }
  // Set the sample rate divider
  if (!imu.ConfigSrd(19)) {
    while (1) {}
  }

  output.elevation = "Steady";

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

    delay(75);
    pub.publish(&output);
    nh.spinOnce();
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

  if (readingVelocity && velocity > 0.2 && output.elevation == "Steady")
    output.elevation = "UpFast";
  else if (readingVelocity && velocity < -0.16 && output.elevation == "Steady")
    output.elevation = "DownFast";

  if (readingVelocity && ((output.elevation == "UpFast" && velocity < 0.2) || (output.elevation == "DownFast" && velocity > -0.16))) {
    readingVelocity = false;
    last_velocity_read = millis();
  }

  if (!readingVelocity && (millis() - last_velocity_read) > 900) {
    readingVelocity = true;
    output.elevation = "Steady";
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
    output.gesture = "Open Hand";
  else if (angle1 > 70 && angle2 > 70)
    output.gesture = "Closed Grip";
  else if (angle1 < 30 && angle2 > 65)
    output.gesture = "Finger Raised";
  else if (angle1 > 40 && angle2 < 30)
    output.gesture = "OK Hand";
  else
    output.gesture = "Undetected Gesture";
}

void getYPR() {
  accelgyro.getMotion6(&ax, &ay, &az, &gx, &gy, &gz);
  if (accelgyro.dmpGetCurrentFIFOPacket(fifoBuffer)) {
    // display Euler angles in degrees
    accelgyro.dmpGetQuaternion(&q, fifoBuffer);
    accelgyro.dmpGetGravity(&gravity, &q);
    accelgyro.dmpGetYawPitchRoll(ypr, &q, &gravity);

    output.yaw = ypr[0] * 180 / M_PI;
    output.pitch = ypr[1] * 180 / M_PI;
    output.roll = ypr[2] * 180 / M_PI;
  }
}

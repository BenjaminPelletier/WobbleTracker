#include <Wire.h>
#include <base64.h>

#ifdef ESP32
  const int PIN_MPU_A0[] = {14, 27, 26, 25}; // {14, 27, 26, 25};
#else
  const int PIN_MPU_A0[] = {16, 0}; // {16, 0, 12, 13}; // D0=GPIO16, D3=GPIO0, D6=GPIO12, D7=GPIO13
#endif
const int MPU_COUNT = sizeof(PIN_MPU_A0) / sizeof(int);
const int ACCEL_OFF = LOW;
const int ACCEL_ON = HIGH;

const uint8_t MPU6050_ADDRESS = 0x69; // Device address when ADO = 1

const uint8_t SMPLRT_DIV = 0x19;
const uint8_t CONFIG = 0x1A;
const uint8_t GYRO_CONFIG = 0x1B;
const uint8_t FS_SEL = 3;
const uint8_t ACCEL_CONFIG = 0x1C;
const uint8_t AFS_SEL = 3;
const uint8_t ACCEL_XOUT_H = 0x3B;
const uint8_t SIGNAL_PATH_RESET = 0x68;
const uint8_t PWR_MGMT_1 = 0x6B;
const uint8_t WHO_AM_I = 0x75; // Should return 0x68

void writeRegister(uint8_t slave_register, uint8_t value) {
  Wire.beginTransmission(MPU6050_ADDRESS);
  Wire.write(slave_register);
  Wire.write(value);
  Wire.endTransmission();
}

uint8_t readRegister(uint8_t slave_register) {
  const uint8_t ONE_BYTE = 1;
  Wire.beginTransmission(MPU6050_ADDRESS);
  Wire.write(slave_register);
  Wire.endTransmission(false);
  Wire.requestFrom(MPU6050_ADDRESS, ONE_BYTE);
  return Wire.read();
}

void debugRegister(String register_name, uint8_t register_value) {
  Serial.print("0b");
  uint8_t v = register_value;
  for (uint8_t i = 0; i < 8; i++) {
    Serial.print(v >= 0x80 ? '1' : '0');
    v <<= 1;
  }
  Serial.print(" 0x");
  v = register_value >> 4;
  Serial.print((char)((v <= 9 ? '0' : '7') + v));
  v = register_value & 0x0F;
  Serial.print((char)((v <= 9 ? '0' : '7') + v));
  Serial.print(' ');
  Serial.print(register_value);
  Serial.print(' ');
  Serial.print(register_name);
  Serial.println();
}

void initAccelerometers() {
  for (int attempt = 0; attempt < 5; attempt++) {
    for (int m = 0; m < MPU_COUNT; m++) {
      digitalWrite(PIN_MPU_A0[m], ACCEL_ON);
      writeRegister(SIGNAL_PATH_RESET, 6);
      writeRegister(SMPLRT_DIV, 0x00);
      writeRegister(CONFIG, 0x00);
      writeRegister(GYRO_CONFIG, (3 << FS_SEL));
      writeRegister(ACCEL_CONFIG, (3 << AFS_SEL));
      writeRegister(PWR_MGMT_1, 0x01);
      digitalWrite(PIN_MPU_A0[m], ACCEL_OFF);
    }
  }
}

void setup() {
  delay(1000);
  Serial.begin(115200);
  Serial.println("=====================================");
  
  for (int m = 0; m < MPU_COUNT; m++) {
    pinMode(PIN_MPU_A0[m], OUTPUT);
    digitalWrite(PIN_MPU_A0[m], ACCEL_OFF);
  }

  Wire.setClock(400000);
  #ifdef ESP8266
    Wire.begin(4, 5); // SDA=D2=GPIO4, SCL=D1=GPIO5
  #else
    Wire.begin();
  #endif
  Wire.setClock(400000);

  initAccelerometers();
  
  for (int m = 0; m < MPU_COUNT; m++) {
    digitalWrite(PIN_MPU_A0[m], ACCEL_ON);
    Serial.print("Accelerometer ");
    Serial.println(m);
    debugRegister("WHO_AMI_I", readRegister(WHO_AM_I));
    debugRegister("SMPLRT_DIV", readRegister(SMPLRT_DIV));
    debugRegister("CONFIG", readRegister(CONFIG));
    debugRegister("PWR_MGMT_1", readRegister(PWR_MGMT_1));
    debugRegister("GYRO_CONFIG", readRegister(GYRO_CONFIG));
    debugRegister("ACCEL_CONFIG", readRegister(ACCEL_CONFIG));
    digitalWrite(PIN_MPU_A0[m], ACCEL_OFF);
  }
}

void printNybbles(char v) {
  char n = v >> 4;
  if (n < 10) {
    Serial.print((char)('0' + n));
  } else {
    Serial.print((char)('A' + n - (char)10));
  }
  n = v & 0xF;
  if (n < 10) {
    Serial.print((char)('0' + n));
  } else {
    Serial.print((char)('A' + n - (char)10));
  }
}

//char BASE64[] = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
void print16(int16_t v) {
  printNybbles((char)((v >> 8) & 0xFF));
  printNybbles((char)(v & 0xFF));
}

int collected = 0;
void loop() {
  if (Serial.available() > 0) {
    while (Serial.available() > 0) {
      Serial.read();
    }
    initAccelerometers();
  }
  collected++;
  //if (collected > 5) return;
  /*unsigned long t0 = millis();
  for (int i=0; i<100; i++) {*/
    
  for (int m = 0; m < MPU_COUNT; m++) {
    digitalWrite(PIN_MPU_A0[m], ACCEL_ON);
    const uint8_t BYTES_PER_READING = 14;
    Wire.beginTransmission(MPU6050_ADDRESS);
    Wire.write(ACCEL_XOUT_H);
    Wire.endTransmission(false);
    Wire.requestFrom(MPU6050_ADDRESS, BYTES_PER_READING, (uint8_t)true);
  
    int16_t xa = Wire.read() << 8;
    xa |= Wire.read();
    int16_t ya = Wire.read() << 8;
    ya |= Wire.read();
    int16_t za = Wire.read() << 8;
    za |= Wire.read();

    int16_t t = Wire.read() << 8;
    t |= Wire.read();

    int16_t xr = Wire.read() << 8;
    xr |= Wire.read();
    int16_t yr = Wire.read() << 8;
    yr |= Wire.read();
    int16_t zr = Wire.read() << 8;
    zr |= Wire.read();

    /*Serial.print(m);
    Serial.print(':');
    Serial.print(xa);
    Serial.print(',');
    Serial.print(ya);
    Serial.print(',');
    Serial.print(za);
    Serial.print(',');
    Serial.print(xr);
    Serial.print(',');
    Serial.print(yr);
    Serial.print(',');
    Serial.print(zr);
    Serial.print(',');*/
    print16(xa);
    print16(ya);
    print16(za);
    print16(xr);
    print16(yr);
    print16(zr);

    /*int16_t data[] = {xa, ya, za, xr, yr, zr};
    for (int i=15; i>=0; i--) {
      uint8_t v = 0;
      for (int d=0; d<6; d++) {
        v <<= 1;
        if ((data[d] >> i) & 1) {
          v |= 1;
        }
      }
      Serial.print(BASE64[v]);
    }*/
    
    digitalWrite(PIN_MPU_A0[m], ACCEL_OFF);
  }
  Serial.println();
  
  /*}
  unsigned long t1 = millis();
  Serial.println(t1 - t0);
  delay(1000);*/
}

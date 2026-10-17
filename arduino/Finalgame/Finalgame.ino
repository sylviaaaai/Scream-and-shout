const int moveBtn = 2;
const int jumpBtn = 3;
const int micPin = A0;
const int sliderPin = A1;
const int monsterBtn = 5;
const int obstacleBtn = 6;

const int monsterLed = 7;
const int obstacleLed = 8;

float volSmooth = 0;
int speedValue = 5;  

int monsterSpeed = 5; 

unsigned long windowStart = 0;
int windowMin = 1023;
int windowMax = 0;


bool monsterReady = true;
bool obstacleReady = true;
unsigned long monsterLastTime = 0;
unsigned long obstacleLastTime = 0;

int readMicVolume() {
  int mn = 1023;
  int mx = 0;
  unsigned long start = millis();

  while (millis() - start < 12) {
    int v = analogRead(micPin);
    if (v < mn) mn = v;
    if (v > mx) mx = v;
  }
  return mx - mn;
}

void setup() {
  Serial.begin(230400);

  pinMode(moveBtn, INPUT_PULLUP);
  pinMode(jumpBtn, INPUT_PULLUP);

  pinMode(monsterBtn, INPUT_PULLUP);
  pinMode(obstacleBtn, INPUT_PULLUP);

  pinMode(monsterLed, OUTPUT);
  pinMode(obstacleLed, OUTPUT);

  digitalWrite(monsterLed, HIGH);   
  digitalWrite(obstacleLed, HIGH);

  windowStart = millis();
}
void loop() {
  
  unsigned long currentTime = millis();
  int moveState = digitalRead(moveBtn);
  int jumpState = digitalRead(jumpBtn);
  int vol = readMicVolume();
  volSmooth = 0.85 * volSmooth + 0.15 * vol;
  speedValue = map((int)volSmooth, 5, 120, 3, 15);
  speedValue = constrain(speedValue, 5, 15);
  // Monster speed
  int val = analogRead(sliderPin);
  if (val < windowMin) windowMin = val;
  if (val > windowMax) windowMax = val;
  if (currentTime - windowStart >= 200) {
    int range = windowMax - windowMin;
    if (range >= 1 && range <= 10) {
      monsterSpeed = 5;
    } else if (range > 10 && range <= 200) {
      monsterSpeed = 7;
    } else if (range > 200) {
      monsterSpeed = 10;
    } else {
      monsterSpeed = 5; 
    }
    windowMin = 1023;
    windowMax = 0;
    windowStart = currentTime;
  }
  // CD（15s）
  if (digitalRead(monsterBtn) == LOW && monsterReady) {
    monsterReady = false;
    monsterLastTime = currentTime;
    digitalWrite(monsterLed, LOW);
  }
  if (!monsterReady && currentTime - monsterLastTime >= 15000) {
    monsterReady = true;
    digitalWrite(monsterLed, HIGH);
  }
  //8s
  if (digitalRead(obstacleBtn) == LOW && obstacleReady) {
    obstacleReady = false;
    obstacleLastTime = currentTime;
    digitalWrite(obstacleLed, LOW);
  }
  if (!obstacleReady && currentTime - obstacleLastTime >= 8000) {
    obstacleReady = true;
    digitalWrite(obstacleLed, HIGH);
  }
  // move,jump,playerSpeed,monsterSpeed,monsterReady,obstacleReady
  Serial.print(moveState);
  Serial.print(",");
  Serial.print(jumpState);
  Serial.print(",");
  Serial.print(speedValue);
  Serial.print(",");
  Serial.print(monsterSpeed);
  Serial.print(",");
  Serial.print(monsterReady);
  Serial.print(",");
  Serial.println(obstacleReady);
  delay(20);
}

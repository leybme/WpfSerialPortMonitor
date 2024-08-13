void setup() {
  // Start the Serial communication at 115200 baud rate
  Serial.begin(115200);
}
unsigned long previousMillis = 0;  // Store the last time the ping was sent
const long interval = 50;        // Interval at which to send the ping (1 second)
void loop() {
  // Check if data is available to read
  if (Serial.available() > 0) {
    // Read incoming byte
    char incomingByte = Serial.read();
    
    // Echo the received byte back to the serial output
    Serial.print("Echo: ");
    Serial.println(incomingByte);
  }
    unsigned long currentMillis = millis();  // Get the current time

  // Check if one second has passed
  if (currentMillis - previousMillis >= interval) {
    previousMillis = currentMillis;  // Update the last time the ping was sent

    // Send the ping message
    Serial.println(millis());
  }
}
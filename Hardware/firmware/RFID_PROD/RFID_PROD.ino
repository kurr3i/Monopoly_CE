#include <SPI.h>
#include <MFRC522.h>
#include <Wire.h>
#include <LiquidCrystal_I2C.h>

// Pines del sensor RFID
#define RST_PIN 5
#define SS_PIN 53

// Otros pines
int buzzer = 12;

// Configuracion del sensor y pantalla
MFRC522 mfrc522(SS_PIN, RST_PIN);
LiquidCrystal_I2C lcd(0x27, 16, 2);


// Funcion de espera
void standBy() {
  mfrc522.PCD_AntennaOff(); 
  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.noBacklight();
}

// Funcion para mostrar errores
void showError(String errorMsg) {
  if (errorMsg.length() > 16) {
    errorMsg = errorMsg.substring(0, 16); // Recorta el mensaje
  }

  lcd.clear();
  lcd.backlight();
  lcd.setCursor(0, 0);
  lcd.print(errorMsg);

  // Tono de error
  for (int i = 0; i < 20; i++) {
    tone(buzzer, 4000);
    delay(10);
    tone(buzzer, 2700);
    delay(10);
  }
  noTone(buzzer);
  delay(1500);
}

// Funcion para mostrar mensajes en pantalla
void showMessage(String msg) {
  mfrc522.PCD_AntennaOn();
  lcd.clear();
  lcd.backlight();

  tone(buzzer, 5000);
  delay(180);
  tone(buzzer, 4500);
  delay(70);
  noTone(buzzer);

  // Recortar mensaje si es muy largo
  if (msg.length() > 16) {
    msg = msg.substring(0, 16);
  }

  // Espacios para centrar el mensaje
  int spaces = (16 - msg.length()) / 2;
  lcd.setCursor(spaces < 0 ? 0 : spaces, 0); 
  lcd.print(msg);
}

// Funcion para leer el numero serial como string entera
String readUIDString() {
  String cardUID = "";
  for (byte i = 0; i < mfrc522.uid.size; i++) {
    if (mfrc522.uid.uidByte[i] < 0x10) {
      cardUID += "0";
    }
    cardUID += String(mfrc522.uid.uidByte[i], HEX);
  }
  cardUID.toUpperCase();
  return cardUID;
}

// Configuracion inicial
void setup() {
  Serial.begin(9600);
  SPI.begin();
  mfrc522.PCD_Init();
  lcd.init();
  lcd.backlight();
  standBy();
}

// Bucle principal
void loop() {

  // Esperar instrucciones
  if (Serial.available() > 0) {
    String comMessage = Serial.readStringUntil('\n'); // Leer mensaje desde serial
    comMessage.trim();

    if (comMessage.startsWith("COM_START_READ|")) {
      String rawMessage = comMessage.substring(15); // Recortar primera parte
      
      int sepIndex = rawMessage.indexOf('|'); // Separar argumentos
      String customMessage = "";
      String expectedUID = "0";

      // Revisar si existe un mensaje para la pantalla
      if (sepIndex != -1) {
        customMessage = rawMessage.substring(0, sepIndex);
        expectedUID = rawMessage.substring(sepIndex + 1);
      } else {
        customMessage = rawMessage;
      }

      if (customMessage == "") {
        customMessage = "ESCANEE TARJETA";
      }

      // Mostrar mensaje en pantalla
      showMessage(customMessage); 

      // Contador de timeout
      unsigned long startTime = millis();
      unsigned long timeoutMs = 7000;

      // Bucle hasta lectura válida
      while (true) {
        
        // Contador
        if (millis() - startTime >= timeoutMs) {
          Serial.println("TIMEOUT");
          showError("TIEMPO AGOTADO");
          break;
        }

        // Si no se ha leido nada
        if (!mfrc522.PICC_IsNewCardPresent() || !mfrc522.PICC_ReadCardSerial()) {
          delay(50); 
          continue;
        }

        // Obtener el serial completo
        String readUID = readUIDString();

        // Revisar si se deben comparar UIDs
        if (expectedUID != "0" && !readUID.equalsIgnoreCase(expectedUID)) {
          
          // Enviar el error
          Serial.println("INVALID");
          showError("ID INCORRECTO");

          // Detener las lecturas
          mfrc522.PICC_HaltA();
          mfrc522.PCD_StopCrypto1();
          break;
        }

        // Enviar la lectura
        Serial.println(readUID);

        lcd.clear();
        lcd.setCursor(0, 0);
        lcd.print("APROBADO");
        tone(buzzer, 5000, 150);
        
        // Mostrar la UID leida
        lcd.setCursor(0, 1);
        lcd.print(readUID);

        // Detener las lecturas
        mfrc522.PICC_HaltA();
        mfrc522.PCD_StopCrypto1();

        delay(1500);
        break; 
      }

      // Volver al modo de espera
      standBy();
    }
  }
}
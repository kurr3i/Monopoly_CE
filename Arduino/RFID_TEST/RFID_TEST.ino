#include <SPI.h>
#include <MFRC522.h>
#include <Wire.h>
#include <LiquidCrystal_I2C.h>

// Definición de pines
#define RST_PIN 5
#define SS_PIN 53

int buzzer = 12;

MFRC522 mfrc522(SS_PIN, RST_PIN);
LiquidCrystal_I2C lcd(0x27, 16, 2);

// Función 
void modoRecibir() {
  lcd.clear();

  tone(buzzer, 5000);
  delay(180);
  tone(buzzer, 4500);
  delay(70);
  noTone(buzzer);

  lcd.setCursor(0, 0);
  lcd.print("   LISTO PARA   ");
  lcd.setCursor(0, 1);
  lcd.print("     PAGAR    ");
}


void setup() {
  Serial.begin(9600);

  SPI.begin();
  mfrc522.PCD_Init();

  lcd.init();
  lcd.backlight();

  modoRecibir();
}

void loop() {
  
  // Revisar si hay tarjeta
  if (!mfrc522.PICC_IsNewCardPresent()) {
    return;
  }

  // Intenta leer el número serial
  if (!mfrc522.PICC_ReadCardSerial()) {
    return;
  }

  // Pago completo
  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("PAGO REALIZADO:");
  tone(buzzer, 5000, 150);

  lcd.setCursor(0, 1);
  Serial.print(F("UID: "));

  // Se imprime el número serial directamente
  for (byte i = 0; i < mfrc522.uid.size; i++) {
    if (mfrc522.uid.uidByte[i] < 0x10) {
      lcd.print("0");
      Serial.print("0");
    }
    lcd.print(mfrc522.uid.uidByte[i], HEX);
    Serial.print(mfrc522.uid.uidByte[i], HEX);

    if (i < mfrc522.uid.size - 1) {
      lcd.print("");
      Serial.print("");
    }
  }
  Serial.println();

  // Detener comunicación
  mfrc522.PICC_HaltA();
  mfrc522.PCD_StopCrypto1();

  delay(3000);

  modoRecibir();
}
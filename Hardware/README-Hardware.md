# Componentes
- Arduino MEGA 2560
- RFID RC522 Sensor
- I2C LCD 16x2
- Passive Buzzer


# Diagrama de conexiones

## Desde Arduino:

- Pin 5 -> RST (RFID RC522)
-Pin 12 -> Signal (Buzzer)
- Pin 20 -> SDA (I2C LCD 16x2)
- Pin 21 -> SCL (I2C LCD 16x2)
- Pin 50 -> MISO (RFID RC522)
- Pin 51 -> MOSI (RFID RC522)
- Pin 52 -> SCK (RFID RC522)
- Pin 53 -> SDA/SS (RC522)
- 5V -> VCC (I2C LCD 16x2)
- 3.3V -> 3.3V (RFID RC522)
- GND -> GND (I2C LCD 16x2)
- GND -> GND (RFID RC522)
- GND -> GND (Buzzer)


# Arduino Virtual

## Requisitos

1. Instalar com0com (Archivo RAR)
2. En opciones de instalación, activar COM#←→COM#
3.1. Cuando finalice, tocar la casilla "Setup".
3.2. Si no se abrió el setup: buscar "setup command prompt" en Windows y ejecutarlo
4. Poner estos comandos:
change CNCA0 PortName=COM10
change CNCB0 PortName=COM11
5. El programa requiere de Python 3.14.x+ y Tkinter.
6. Finalmente para activarlo, se debe cambiar la configuración en la clase Config.cs > ArduinoVirtual a true
import serial
import time
import threading
import tkinter as tk
from tkinter import messagebox

# Puerto virtual desde com0com
SERIAL_PORT = "COM10"  
BAUD_RATE = 9600

# Numeros seriales de las tarjetas reales
TARJETAS = {
    1: "865D2507",
    2: "215A436E",
    3: "41054E6E",
    4: "415A1F6E"
}

class RFID_VIRTUAL:
    def __init__(self, root):
        self.root = root
        self.root.title("Arduino RFID Virtual")
        self.root.geometry("450x380")
        self.root.resizable(False, False)
        self.root.configure(bg="#194670")

        self.ser = None
        self.running = True
        self.timeout_active = False
        self.start_time = 0
        self.timeout_ms = 7000
        self.expected_uid = "0"

        # Diseño de la pantalla
        self.frame_lcd = tk.Frame(root, bg="#00024A", bd=4, relief="ridge")
        self.frame_lcd.pack(pady=15, padx=20, fill="x")

        # Diseño de las filas
        self.lbl_lcd_row0 = tk.Label(self.frame_lcd, text="", font=("Courier", 16, "bold"), fg="#dce531", bg="#00024A", anchor="w", width=16)
        self.lbl_lcd_row0.pack(pady=(10, 2), padx=10, fill="x")
        
        # Diseño de las filas
        self.lbl_lcd_row1 = tk.Label(self.frame_lcd, text="", font=("Courier", 16, "bold"), fg="#dce531", bg="#00024A", anchor="w", width=16)
        self.lbl_lcd_row1.pack(pady=(2, 10), padx=10, fill="x")

        # Diseño de los botones
        self.frame_buttons = tk.LabelFrame(root, text=" Escanear Tarjeta ", fg="#ffffff", bg="#194670", font=("Arial", 10, "bold"), padx=10, pady=10)
        self.frame_buttons.pack(pady=15, padx=20, fill="both", expand=True)

        # Botones
        self.buttons = {}
        for i in range(1, 5):
            btn = tk.Button(
                self.frame_buttons, 
                text=f"Tarjeta {i}\n({TARJETAS[i]})", 
                font=("Arial", 10, "bold"),
                bg="#354431", 
                fg="#ffffff", 
                state=tk.DISABLED,
                command=lambda t=i: self.rfid_read(t),
                width=18,
                height=2,
                relief="groove"
            )
            btn.grid(row=(i-1)//2, column=(i-1)%2, padx=10, pady=8)
            self.buttons[i] = btn

        self.stand_by()
        self.open_serial()
        self.check_timeout()
        self.root.protocol("WM_DELETE_WINDOW", self.on_closing)


    # Funcion para abrir el puerto
    def open_serial(self):
        try:
            self.ser = serial.Serial(port=SERIAL_PORT, baudrate=BAUD_RATE, timeout=0.1)
            threading.Thread(target=self.com_read, daemon=True).start()
        except serial.SerialException as e:
            messagebox.showerror("Error Serial", f"No se pudo abrir el puerto {SERIAL_PORT}.\n{e}")


    # Funcion de espera inicial
    def stand_by(self):
        self.timeout_active = False
        self.frame_lcd.config(bg="#00024A")
        self.lbl_lcd_row0.config(bg="#00024A")
        self.lbl_lcd_row1.config(bg="#00024A")
        self.lbl_lcd_row0.config(text="")
        self.lbl_lcd_row1.config(text="")
        self.set_buttons_state(tk.DISABLED)


    # Funcion para mostrar mensajes
    def show_message(self, msg, expected_uid="0"):
        msg_base = msg[:16]
        # Calcular las columnas
        columns = max(0, (16 - len(msg_base)) // 2)
        centered_text = f"{' ' * columns}{msg_base}"
        
        self.frame_lcd.config(bg="#191EBA")
        self.lbl_lcd_row0.config(bg="#191EBA")
        self.lbl_lcd_row1.config(bg="#191EBA")
        
        self.lbl_lcd_row0.config(text=centered_text)
        self.lbl_lcd_row1.config(text="")
        
        self.expected_uid = expected_uid
        self.set_buttons_state(tk.NORMAL)
        
        self.start_time = time.time()
        self.timeout_active = True


    # Funcion para cambiar el estado de los botones
    def set_buttons_state(self, state):
        bg_color = "#ffffff" if state == tk.NORMAL else "#000000"
        fg_color = "#000000" if state == tk.NORMAL else "#000000"
        cursor_type = "hand2" if state == tk.NORMAL else ""
        
        for btn in self.buttons.values():
            btn.config(state=state, bg=bg_color, fg=fg_color, cursor=cursor_type)


    # Funcion para enviar mensajes
    def com_write(self, mensaje):
        if self.ser and self.ser.is_open:
            self.ser.write(f"{mensaje}\n".encode('utf-8'))


    # Funcion para simular el escaneo de una tarjeta
    def rfid_read(self, numero_tarjeta):
        if not self.timeout_active:
            return
        
        self.timeout_active = False
        uid = TARJETAS[numero_tarjeta]
        
        # Compara el UID recibido con el UID esperado
        if self.expected_uid != "0" and uid.upper() != self.expected_uid.upper():
            self.com_write("INVALID")
            self.lbl_lcd_row0.config(text="ID INCORRECTO")
            self.lbl_lcd_row1.config(text="")
        else:
            self.com_write(uid)
            self.lbl_lcd_row0.config(text="APROBADO")
            self.lbl_lcd_row1.config(text=uid)

        self.set_buttons_state(tk.DISABLED)
        self.root.after(1500, self.stand_by)


    # Funcion para verificar el timeout
    def check_timeout(self):
        if self.timeout_active:
            if (time.time() - self.start_time) * 1000 >= self.timeout_ms:
                self.timeout_active = False
                self.set_buttons_state(tk.DISABLED)
                
                self.com_write("TIMEOUT")
                self.lbl_lcd_row0.config(text="TIEMPO AGOTADO")
                self.lbl_lcd_row1.config(text="")
                
                self.root.after(1500, self.stand_by)
        
        if self.running:
            self.root.after(100, self.check_timeout)


    # Funcion para escuchar el puerto
    def com_read(self):
        while self.running:
            if self.ser and self.ser.is_open:
                try:
                    if self.ser.in_waiting > 0:
                        line = self.ser.readline().decode('utf-8', errors='ignore').strip()
                        
                        if line.startswith("COM_START_READ|"):
                            payload = line[15:]
                            custom_message = "ESCANEE TARJETA"
                            expected_uid = "0"
                            
                            # Dividir el mensaje en partes
                            if "|" in payload:
                                parts = payload.split("|", 1)
                                custom_message = parts[0] if parts[0] else custom_message
                                expected_uid = parts[1]
                            elif payload:
                                custom_message = payload

                            self.root.after(0, lambda m=custom_message, e=expected_uid: self.show_message(m, e))
                except Exception:
                    break
            time.sleep(0.05)


    # Funcion para cerrar el programa
    def on_closing(self):
        self.running = False
        if self.ser and self.ser.is_open:
            self.ser.close()
        self.root.destroy()


# Llamada principal
if __name__ == "__main__":
    root = tk.Tk()
    app = RFID_VIRTUAL(root)
    root.mainloop()
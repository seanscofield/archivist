import os
import sys
import pdfkit
import base64
import PyPDF2
import qrcode
import uuid
import fitz  # PyMuPDF
from jsonio import update_data, create_data
from PyPDF2 import PdfReader, PdfWriter
from reportlab.lib.pagesizes import letter
from reportlab.pdfgen import canvas
from PyQt5.QtWidgets import (QApplication, QWidget, QVBoxLayout, QLineEdit, 
                             QPushButton, QLabel, QMessageBox, QFileDialog)

PATH_TO_WKHTMLTOPDF = r'./wkhtmltopdf/bin/wkhtmltopdf.exe'
CONFIG = pdfkit.configuration(wkhtmltopdf=PATH_TO_WKHTMLTOPDF)
OPT = {
    'margin-top': '2in',
    'margin-bottom': '1in',
    'margin-left': '1in',
    'margin-right': '1in'
}
TEMP_PDF_PATH = "./CacheData/temp.pdf"
FINAL_PDF_PATH = "./CacheData/output.pdf"
UID = str(uuid.uuid4())

def count_pdf_pages(temp_pdf_path):
    with open(temp_pdf_path, 'rb') as file:
        pdf_reader = PyPDF2.PdfReader(file)
        num_pages = len(pdf_reader.pages)
    return num_pages

def get_ar_marker_coordinates(pdf_path):
    pdf_document = fitz.open(pdf_path)
    _image_list = pdf_document.get_page_images(pno=0, full=True)
    ar_marker_coordinates = pdf_document[0].get_image_rects(_image_list[0][7], transform=True)[0][0]
    pdf_document.close()
    return ar_marker_coordinates

def making_pdf_qr(path):
    path_to_file = path
    pdfkit.from_url(path_to_file, output_path=TEMP_PDF_PATH, configuration=CONFIG, options=OPT, verbose=True)

    NUM_PAGES = count_pdf_pages(TEMP_PDF_PATH)

    folder_path = "./QR"
    if not os.path.exists(folder_path):
        os.makedirs(folder_path)
        
    uid_folder_path = os.path.join(folder_path, UID)
    if not os.path.exists(uid_folder_path):
        os.makedirs(uid_folder_path)

    for p_no in range(NUM_PAGES):
        text = '''{{
                "id": "{}",
                "page": {}
            }}'''.format(UID, (p_no + 1))
        qr = qrcode.QRCode(
            version=1,
            error_correction=qrcode.constants.ERROR_CORRECT_L,
            box_size=10,
            border=4,
        )

        qr.add_data(text)
        qr.make(fit=True)
        img = qr.make_image(fill='black', back_color='white')
        ext = ".png"
        file_name = os.path.join(uid_folder_path, str(p_no) + ext)
        img.save(file_name)

    pdf_reader = PdfReader(TEMP_PDF_PATH)
    pdf_writer = PdfWriter()

    for i in range(NUM_PAGES):
        page = pdf_reader.pages[i]
        qr_filename = "{}.png".format(i)
        qr_path = os.path.join(uid_folder_path, qr_filename)
        ar_marker_path = './_ARMarker/Markers/MarkerIcons03.png'
        with open(ar_marker_path, 'rb') as marker_file:
            marker_data = marker_file.read()
            marker_base64 = base64.b64encode(marker_data).decode('utf-8')

        if os.path.exists(qr_path):
            with open(qr_path, 'rb') as qr_file:
                qr_data = qr_file.read()
                qr_base64 = base64.b64encode(qr_data).decode('utf-8')
                
        image_pdf_path = 'image_page.pdf'
        c = canvas.Canvas(image_pdf_path, pagesize=letter)
        c.drawImage("data:image/png;base64," + marker_base64, 205, 710, width=80, height=80)
        c.drawImage("data:image/png;base64," + qr_base64, 295, 700, width=100, height=100)
        c.save()

        with open(image_pdf_path, 'rb') as image_pdf_file:
            image_pdf_reader = PdfReader(image_pdf_file)
            image_page = image_pdf_reader.pages[0]
            page.merge_page(image_page)
            pdf_writer.add_page(page)

        os.remove(image_pdf_path)  
        
    with open(FINAL_PDF_PATH, 'wb') as output_pdf:
        pdf_writer.write(output_pdf)

    os.remove(TEMP_PDF_PATH)
    ar_marker_coordinates = get_ar_marker_coordinates(FINAL_PDF_PATH)
    doc = fitz.open(FINAL_PDF_PATH)
    json_data = {}
    json_data['ar_marker_coordinates'] = [ar_marker_coordinates.x0, ar_marker_coordinates.y0,
                                        ar_marker_coordinates.x1, ar_marker_coordinates.y1]
    json_data['pages'] = []
    total_pages = doc.page_count
    item_count = 0
    hyperlink_id = create_data()

    for page_idx in range(total_pages):
        cur_page = doc.load_page(page_idx)
        links = cur_page.get_links()
        hyperlinks = []

        for item in links:
            x0, y0, x1, y1 = item['from']
            coordinates = [round(coord, 5) for coord in [x0, y0, x1, y1]]
            uri = item.get('uri', '')
            hyperlink = {'id': hyperlink_id + "-" + str(item_count), 'uri': uri, 'coordinates': coordinates}
            hyperlinks.append(hyperlink)
            item_count += 1

        json_data['pages'].append({"hyperlinks": hyperlinks})

    update_data(bin_id=hyperlink_id, json_data=json_data)
    print(json_data)

    doc.close()

def process_pdf_file(file_path):
    pdf_reader = PdfReader(file_path)
    pdf_writer = PdfWriter()

    NUM_PAGES = len(pdf_reader.pages)

    folder_path = "./QR"
    if not os.path.exists(folder_path):
        os.makedirs(folder_path)

    uid_folder_path = os.path.join(folder_path, UID)
    if not os.path.exists(uid_folder_path):
        os.makedirs(uid_folder_path)

    for p_no in range(NUM_PAGES):
        text = '''{{
                "id": "{}",
                "page": {}
            }}'''.format(UID, (p_no + 1))
        qr = qrcode.QRCode(
            version=1,
            error_correction=qrcode.constants.ERROR_CORRECT_L,
            box_size=10,
            border=4,
        )
        qr.add_data(text)
        qr.make(fit=True)
        img = qr.make_image(fill='black', back_color='white')
        ext = ".png"
        file_name = os.path.join(uid_folder_path, str(p_no) + ext)
        img.save(file_name)

    for i in range(NUM_PAGES):
        page = pdf_reader.pages[i]
        qr_filename = "{}.png".format(i)
        qr_path = os.path.join(uid_folder_path, qr_filename)
        ar_marker_path = './_ARMarker/Markers/MarkerIcons03.png'
        with open(ar_marker_path, 'rb') as marker_file:
            marker_data = marker_file.read()
            marker_base64 = base64.b64encode(marker_data).decode('utf-8')

        if os.path.exists(qr_path):
            with open(qr_path, 'rb') as qr_file:
                qr_data = qr_file.read()
                qr_base64 = base64.b64encode(qr_data).decode('utf-8')

        image_pdf_path = 'image_page.pdf'
        c = canvas.Canvas(image_pdf_path, pagesize=letter)
        c.drawImage("data:image/png;base64," + marker_base64, 205, 710, width=80, height=80)
        c.drawImage("data:image/png;base64," + qr_base64, 295, 700, width=100, height=100)
        c.save()

        with open(image_pdf_path, 'rb') as image_pdf_file:
            image_pdf_reader = PdfReader(image_pdf_file)
            image_page = image_pdf_reader.pages[0]
            page.merge_page(image_page)
            pdf_writer.add_page(page)

        os.remove(image_pdf_path)

    with open(FINAL_PDF_PATH, 'wb') as output_pdf:
        pdf_writer.write(output_pdf)

    ar_marker_coordinates = get_ar_marker_coordinates(FINAL_PDF_PATH)
    doc = fitz.open(FINAL_PDF_PATH)

    json_data = {}
    json_data['ar_marker_coordinates'] = [ar_marker_coordinates.x0, ar_marker_coordinates.y0,
                                        ar_marker_coordinates.x1, ar_marker_coordinates.y1]
    json_data['pages'] = []
    total_pages = doc.page_count
    item_count = 0
    hyperlink_id = create_data()

    for page_idx in range(total_pages):
        cur_page = doc.load_page(page_idx)
        links = cur_page.get_links()
        hyperlinks = []

        for item in links:
            x0, y0, x1, y1 = item['from']
            coordinates = [round(coord, 5) for coord in [x0, y0, x1, y1]]
            uri = item.get('uri', '')
            hyperlink = {'id': hyperlink_id + "-" + str(item_count), 'uri': uri, 'coordinates': coordinates}
            hyperlinks.append(hyperlink)
            item_count += 1

        json_data['pages'].append({"hyperlinks": hyperlinks})

    update_data(bin_id=hyperlink_id, json_data=json_data)
    doc.close()

class PDFGeneratorApp(QWidget):
    def __init__(self):
        super().__init__()
        self.initUI()

    def initUI(self):
        self.setWindowTitle('PDF Generator with QR Codes')
        self.layout = QVBoxLayout()
        self.url_input = QLineEdit(self)
        self.url_input.setPlaceholderText('Enter URL')
        self.layout.addWidget(self.url_input)
        self.browse_button = QPushButton('Browse PDF', self)
        self.browse_button.clicked.connect(self.browse_pdf)
        self.layout.addWidget(self.browse_button)
        self.generate_button = QPushButton('Generate PDF from URL', self)
        self.generate_button.clicked.connect(self.generate_pdf_from_url)
        self.layout.addWidget(self.generate_button)
        self.status_label = QLabel('', self)
        self.layout.addWidget(self.status_label)
        self.setLayout(self.layout)

    def browse_pdf(self):
        options = QFileDialog.Options()
        options |= QFileDialog.ReadOnly
        file_path, _ = QFileDialog.getOpenFileName(self, "Open PDF File", "", "PDF Files (*.pdf);;All Files (*)", options=options)
        if file_path:
            self.status_label.setText('Processing PDF...')
            try:
                process_pdf_file(file_path)
                self.status_label.setText('PDF processed successfully!')
                QMessageBox.information(self, 'Success', 'PDF processed successfully!', QMessageBox.Ok)
            except Exception as e:
                self.status_label.setText('Error processing PDF.')
                QMessageBox.critical(self, 'Error', f'Error processing PDF: {e}', QMessageBox.Ok)

    def generate_pdf_from_url(self):
        url = self.url_input.text()
        if url:
            self.status_label.setText('Generating PDF...')
            try:
                making_pdf_qr(url)
                self.status_label.setText('PDF generated successfully!')
                QMessageBox.information(self, 'Success', 'PDF generated successfully!', QMessageBox.Ok)
            except Exception as e:
                self.status_label.setText('Error generating PDF.')
                QMessageBox.critical(self, 'Error', f'Error generating PDF: {e}', QMessageBox.Ok)
        else:
            QMessageBox.warning(self, 'Input Error', 'Please enter a valid URL.', QMessageBox.Ok)


if __name__ == '__main__':
    app = QApplication(sys.argv)
    ex = PDFGeneratorApp()
    ex.show()
    sys.exit(app.exec_())
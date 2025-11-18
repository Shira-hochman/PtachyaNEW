import json
import sys
import os
import io
import base64
import subprocess
import shutil
from datetime import datetime
from docx import Document
from docx.shared import Inches, Pt
from docx.enum.table import WD_TABLE_ALIGNMENT

# נתיב קבוע לספרייה זמנית
TEMP_DIR = os.path.join(os.path.expanduser('~'), 'ptachya_temp_forms')
os.makedirs(TEMP_DIR, exist_ok=True)


# ----------------- פונקציות עזר -----------------

def format_date(date_str: str) -> str:
    """ממיר תאריך YYYY-MM-DD ל-DD/MM/YYYY."""
    try:
        if not date_str: return "_______"
        # מטפל גם בפורמט ISO מלא (כמו 2025-01-01T00:00:00)
        date_part = date_str.split('T')[0]
        return datetime.strptime(date_part, '%Y-%m-%d').strftime('%d/%m/%Y')
    except:
        return "_______"


def get_mark(form_value) -> str:
    """מחזיר 'X' אם הערך הוא True, אחרת מחרוזת ריקה."""
    # משמש לסימון תיבות סימון
    return 'X' if form_value == True else ' '


def replace_table_placeholder(doc, placeholder_key: str, data_array: list, headers: list):
    """
    מחפש טבלה שמכילה את ה-Placeholder וממלא אותה בנתונים דינמיים.
    הנתונים נכנסים מתחת לשורת הכותרות הקיימת.
    """
    for table in doc.tables:
        for row_index, row in enumerate(table.rows):
            if placeholder_key in row.cells[0].text:
                # מצאנו את הטבלה
                # מוחקים את שורות ה-Placeholder המקוריות
                row_to_delete = row_index
                # מוחקים את כל שורות ה-Placeholder
                while len(table.rows) > row_to_delete and placeholder_key in table.rows[row_to_delete].cells[0].text:
                    table._element.remove(table.rows[row_to_delete]._element)

                # מוסיפים את הנתונים החדשים
                for item in data_array:
                    new_row = table.add_row()
                    # יש להתאים את סדר הנתונים למבנה הטבלה ב-DOCX
                    new_row.cells[0].text = item.get(headers[0], '')
                    new_row.cells[1].text = item.get(headers[1], '')
                    new_row.cells[2].text = item.get(headers[2], '')
                return


def InsertSignatureImage(doc, placeholder_text, base64_data, table_cell_index=None):
    """מוצא ומחליף טקסט Placeholder בתמונת Base64 בתוך הפסקה או תא בטבלה."""
    if not base64_data:
        replacement_text = '_______ (לא נחתם) _______'
    else:
        replacement_text = ''

    for table in doc.tables:
        for row in table.rows:
            for cell in row.cells:
                if placeholder_text in cell.text:
                    cell.text = cell.text.replace(placeholder_text, replacement_text)
                    if base64_data:
                        base64WithoutPrefix = base64_data.split(',')[-1]
                        image_bytes = base64.b64decode(base64WithoutPrefix)
                        image_stream = io.BytesIO(image_bytes)
                        # רוחב וגובה קבועים לחתימה
                        cell.paragraphs[0].add_run().add_picture(image_stream, width=Inches(1.5), height=Inches(0.4))
                        cell.paragraphs[0].alignment = WD_TABLE_ALIGNMENT.CENTER
                        return

    # אם לא נמצא בטבלה, חפש בפסקאות רגילות
    for p in doc.paragraphs:
        if placeholder_text in p.text:
            p.text = p.text.replace(placeholder_text, replacement_text)
            if base64_data:
                base64WithoutPrefix = base64_data.split(',')[-1]
                image_bytes = base64.b64decode(base64WithoutPrefix)
                image_stream = io.BytesIO(image_bytes)
                p.add_run().add_picture(image_stream, width=Inches(1.5), height=Inches(0.4))
            return


# ----------------- הפונקציה המרכזית: מילוי והמרה -----------------

def fill_and_convert_to_pdf(form_data: dict, output_pdf_path: str, template_path: str, libre_office_path: str):
    """ממלא את התבנית DOCX, מטמיע חתימות, וממיר ל-PDF באמצעות soffice.exe."""

    # ⭐️ שלב 1: מיפוי הנתונים ⭐️

    # מיפוי שדות פשוטים
    # (הערה: ה-MailMerge עובד רק עם מפתחות ברמה העליונה)

    data = {}

    # פרטי מגיש (Declarant)
    data['DeclarantName'] = form_data.get('declarantName', '_______')
    data['DeclarantId'] = form_data.get('declarantId', '_______')
    data['MaritalStatus'] = form_data.get('maritalStatus', '_______')
    data['ChildrenCount'] = str(form_data.get('childrenCount', 0))
    data['Reasoning'] = form_data.get('reasoning', 'לא סופק נימוק.')
    data['Date'] = format_date(form_data.get('formDate', str(datetime.now())))

    # פרטי תלמיד (StudentDetails)
    student = form_data.get('studentDetails', {})
    data['StudentName'] = student.get('studentName', '_______')
    data['StudentId'] = student.get('studentId', '_______')
    data['Kindergarten'] = student.get('kindergarten', '_______')
    data['City'] = student.get('city', '_______')

    # סיבות לבקשה (DiscountReasons) - ממירים לבחירה 'X' או ריק
    reasons = form_data.get('discountReasons', {})
    data['LowIncome'] = get_mark(reasons.get('lowIncome'))
    data['OtherChildSpecialEd'] = get_mark(reasons.get('otherChildSpecialEd'))
    data['SocialWorkerRec'] = get_mark(reasons.get('socialWorkerRec'))
    # הערה: אם היה "ילד נוסף בפתחיה" בתבנית, היינו צריכים להוסיף כאן שדה עבורו

    # פרטי הכנסה (LowIncomeDetails)
    income = form_data.get('lowIncomeDetails', {})
    data['Spouse1Status'] = income.get('spouse1Status', '_______')
    data['Spouse2Status'] = income.get('spouse2Status', '_______')
    data['Spouse1AvgMonthlyIncome'] = str(income.get('spouse1AvgMonthlyIncome', '_______'))
    data['Spouse2AvgMonthlyIncome'] = str(income.get('spouse2AvgMonthlyIncome', '_______'))
    data['Spouse1Total3Months'] = str(income.get('spouse1Total3Months', '_______'))
    data['Spouse2Total3Months'] = str(income.get('spouse2Total3Months', '_______'))

    # מסמכים שהועלו (RequiredDocuments)
    docs = form_data.get('requiredDocuments', {})
    data['LowIncomeDocsUploaded'] = docs.get('lowIncomeDocsUploaded', 'אין קובץ')
    data['OtherSpecialEdDocsUploaded'] = docs.get('otherSpecialEdDocsUploaded', 'אין קובץ')
    data['SocialWorkerDocsUploaded'] = docs.get('socialWorkerDocsUploaded', 'אין קובץ')

    # חתימה (ParentSignature)
    signature_base64 = form_data.get('parentSignature', '')

    # נתוני הילדים הנוספים
    children_in_custody = form_data.get('childrenInCustody', [])

    # 2. הכנה לקבצים זמניים
    temp_docx_base_name = os.path.basename(output_pdf_path).replace('.pdf', '')
    temp_docx_path = os.path.join(TEMP_DIR, f"filled_temp_{temp_docx_base_name}.docx")

    if not os.path.exists(template_path):
        raise FileNotFoundError(f"Template file not found at: {template_path}")

    shutil.copyfile(template_path, temp_docx_path)

    # 3. מילוי שדות פשוטים באמצעות MailMerge
    try:
        with MailMerge(temp_docx_path) as document:
            document.merge(**data)
            document.write(temp_docx_path)
    except Exception as e:
        print(f"MailMerge failed: {e}", file=sys.stderr)
        # אם MailMerge נכשל, נמשיך עם קובץ ה-DOCX כדי שנוכל לטפל בו עם docx
        pass

    # 4. מילוי טבלאות דינמיות והטמעת תמונות באמצעות docx
    doc = Document(temp_docx_path)

    # א. טיפול בטבלת הילדים הנוספים
    # כיוון שלא ניתן להריץ לולאה ב-MailMerge, אנחנו צריכים להשתמש ב-docx.
    # ה-Placeholder נמצא בתא הראשון בטבלה השלישית ב-DOCX שהגדרנו.

    # הנתונים מועברים בצורה: [{'firstName': 'שם1', 'lastName': 'משפחה1', 'id': '1111'}, ...]
    if len(children_in_custody) > 0:
        children_data_formatted = []
        for child in children_in_custody:
            # הנתונים צריכים להיות בסדר שבו הטבלה מצפה להם (שם פרטי, שם משפחה, ת"ז)
            children_data_formatted.append({
                'שם פרטי': child.get('firstName', ''),
                'שם משפחה': child.get('lastName', ''),
                'מס\' זהות': child.get('id', '')
            })

        # מחפשים את הטבלה הראשונה שתואמת את המבנה של הילדים (טבלה זו תתבסס על המיזוג)
        # כיוון שהטבלה אינה מוכנה למיזוג, נטפל בה ישירות ב-docx.
        # אנו מניחים שהטבלה השנייה במסמך היא טבלת הילדים (אחרי פרטי התלמיד הראשי).

        # ⭐️⭐️⭐️ מתאים את שם העמודות למיפוי של טבלת הילדים ⭐️⭐️⭐️
        if len(doc.tables) >= 2:
            child_table = doc.tables[1]

            # מחיקת שורות ה-Placeholder (שורות 2, 3, 4, 5)
            # מתחילים מהשורה השנייה (אינדקס 1) ומוחקים עד הסוף, כיוון שהן ריקות.
            rows_to_delete = len(child_table.rows) - 1
            for _ in range(rows_to_delete):
                child_table._element.remove(child_table.rows[1]._element)

            # מילוי שורות חדשות
            for child_item in children_data_formatted:
                new_row = child_table.add_row()
                # סדר העמודות: שם פרטי, שם משפחה, מס' זהות
                new_row.cells[0].text = child_item['שם פרטי']
                new_row.cells[1].text = child_item['שם משפחה']
                new_row.cells[2].text = child_item['מס\' זהות']

    # ב. הטמעת החתימה
    InsertSignatureImage(doc, '<<ParentSignature>>', signature_base64)

    doc.save(temp_docx_path)

    # 5. המרת DOCX ל-PDF באמצעות SOFFICE (LibreOffice)
    expected_pdf_name = os.path.basename(temp_docx_path).replace('.docx', '.pdf')
    generated_pdf_path = os.path.join(TEMP_DIR, expected_pdf_name)

    try:
        subprocess.run(
            [
                libre_office_path,
                '--headless',
                '--convert-to', 'pdf',
                temp_docx_path,
                '--outdir', TEMP_DIR
            ],
            capture_output=True, text=True, check=True, timeout=60
        )

        # 6. העתקת הקובץ למיקום שה-C# מצפה לו
        shutil.copyfile(generated_pdf_path, output_pdf_path)

    except subprocess.CalledProcessError as e:
        raise Exception(f"PDF Conversion failed (soffice). Error: {e.stderr}")
    except FileNotFoundError:
        raise Exception(
            f"LibreOffice command not found. Please check 'LibreOfficeExecutablePath' in appsettings: {libre_office_path}")

    finally:
        # 7. ניקוי קבצים זמניים
        if os.path.exists(temp_docx_path):
            os.remove(temp_docx_path)
        if os.path.exists(generated_pdf_path):
            os.remove(generated_pdf_path)


# ----------------- הפונקציה הראשית להפעלה (ללא שינוי) -----------------

def main():
    try:
        input_json_data = sys.stdin.read()
        data_wrapper = json.loads(input_json_data)

        output_path = data_wrapper.get('output_pdf_path')
        template_path = data_wrapper.get('template_path')
        form_data = data_wrapper.get('form_data', {})
        libre_office_path = data_wrapper.get('libre_office_path')

        if not output_path or not template_path or not libre_office_path:
            raise ValueError("נתיב שמירה, נתיב תבנית או נתיב LibreOffice חסרים.")

        # כעת הפונקציה המרכזית מקבלת את הנתונים החדשים של טופס ההנחה
        fill_and_convert_to_pdf(form_data, output_path, template_path, libre_office_path)

        print(json.dumps({'status': 'success', 'path': output_path}))

    except Exception as e:
        # הדפסה ל-stderr כדי שה-C# יוכל לקלוט את השגיאה
        print(json.dumps({'status': 'error', 'message': str(e)}), file=sys.stderr)
        sys.exit(1)


if __name__ == '__main__':
    # ⭐️ אנו משתמשים רק בקובץ generate_pdf_from_docx.py, יש לוודא שהשני (למעלה) נמחק ⭐️
    main()
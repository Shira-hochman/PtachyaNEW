import json
import sys
import os
import io
import base64
import subprocess
import shutil
from datetime import datetime
from docx import Document
from docx.shared import Inches
from mailmerge import MailMerge 

# הגדרות קבועות מהקוד הישן שלך
TEMP_DIR = os.path.join(os.path.expanduser('~'), 'ptachya_temp_forms')
os.makedirs(TEMP_DIR, exist_ok=True)
FOUR_SPACES = '    '
NINE_UNDERSCORES = '_________'

# ----------------- ⭐️⭐️⭐️ פונקציות עזר (חובה שיופיעו כאן) ⭐️⭐️⭐️ -----------------

def format_date(date_str: str) -> str:
    """ממיר תאריך YYYY-MM-DD ל-DD/MM/YYYY."""
    try:
        if not date_str: return "_______"
        date_part = date_str.split('T')[0]
        return datetime.strptime(date_part, '%Y-%m-%d').strftime('%d/%m/%Y')
    except:
        return "_______"

def get_mark(form_value) -> str:
    """מחזיר 'X' אם הערך הוא True, אחרת מחרוזת ריקה."""
    # נתונים מגיעים כ-bool/string מ-C#
    return 'X' if form_value == True or str(form_value).lower() == 'true' else ' '

def clean_and_format_value(value) -> str:
    """מטפל בערכי None או ריקים ומחזיר מחרוזת נקייה."""
    if value is None:
        return "_______"
    val = str(value).strip()
    if not val or val == 'null':
        return "_______"
    
    # 🛑 חשוב: הוספת הלוגיקה לטיפול במספרים
    if isinstance(value, (int, float)) or (isinstance(value, str) and val.replace('.', '').isdigit()):
        return val
        
    return val

def InsertSignatureImage(doc, placeholder_text, base64_data):
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
                        try:
                            base64WithoutPrefix = base64_data.split(',')[-1]
                            image_bytes = base64.b64decode(base64WithoutPrefix)
                            image_stream = io.BytesIO(image_bytes)
                            cell.paragraphs[0].add_run().add_picture(image_stream, width=Inches(1.5), height=Inches(0.4))
                        except Exception as e:
                            print(f"Error embedding signature: {e}", file=sys.stderr)
                            cell.paragraphs[0].add_run(' (שגיאת חתימה) ')
                    return
    
    for p in doc.paragraphs:
        if placeholder_text in p.text:
            p.text = p.text.replace(placeholder_text, replacement_text)
            if base64_data:
                try:
                    base64WithoutPrefix = base64_data.split(',')[-1]
                    image_bytes = base64.b64decode(base64WithoutPrefix)
                    image_stream = io.BytesIO(image_bytes)
                    p.add_run().add_picture(image_stream, width=Inches(1.5), height=Inches(0.4))
                except:
                    p.add_run(' (שגיאת חתימה) ')
            return
            
def replace_and_style_stable(paragraph, key, value):
    """משתמשת בשיטת ה-run המיוחדת כדי לשמור על העיצוב המקורי."""
    if key not in paragraph.text:
        return False
    
    original_font = None
    for run in paragraph.runs:
        if key in run.text:
            original_font = run.font
            break
            
    is_mark = (value == 'X' or value == ' ')
    if is_mark:
        content_value = str(value)
        underline_value = False
    elif value:
        content_value = f" {str(value)} "
        underline_value = True
    else:
        content_value = NINE_UNDERSCORES
        underline_value = False

    original_text = paragraph.text
    temp_placeholder = f"@@@TEMP_PH_{key}@@@"
    
    if key in original_text:
        original_text = original_text.replace(key, temp_placeholder)
        paragraph.clear()
        parts = original_text.split(temp_placeholder)

        for i, part in enumerate(parts):
            paragraph.add_run(part)
            
            if i < len(parts) - 1:
                if not underline_value:
                    paragraph.add_run(content_value)
                else:
                    new_run = paragraph.add_run(content_value)
                    if original_font:
                        new_run.font.name = original_font.name
                        new_run.font.size = original_font.size
                        new_run.font.bold = original_font.bold
                    new_run.font.underline = True
    return True
            
# ----------------- ⭐️ הפונקציה המרכזית ⭐️ -----------------

def fill_and_convert_to_pdf(data_wrapper: dict, output_pdf_path: str, template_path: str, libre_office_path: str):
    
    form_data = data_wrapper.get('form_data', {})
    
    student = form_data.get('studentDetails', {})
    reasons = form_data.get('discountReasons', {})
    income = form_data.get('lowIncomeDetails', {})
    docs = form_data.get('requiredDocuments', {})
    children_in_custody = form_data.get('childrenInCustody', [])
    signature_base64 = form_data.get('parentSignature', '')
    
    # 1. מיפוי נתונים שטוח עבור ה-replace_and_style_stable
    replace_map = {
        '<<Date>>': format_date(form_data.get('formDate')),
        '<<DeclarantName>>': form_data.get('declarantName'),
        '<<DeclarantId>>': form_data.get('declarantId'),
        '<<MaritalStatus>>': form_data.get('maritalStatus'),
        '<<ChildrenCount>>': str(form_data.get('childrenCount')),
        '<<Reasoning>>': form_data.get('reasoning'),
        
        # פרטי תלמיד
        '<<StudentName>>': clean_and_format_value(student.get('studentName')),
        '<<StudentId>>': clean_and_format_value(student.get('studentId')),
        '<<Kindergarten>>': clean_and_format_value(student.get('kindergarten')),
        '<<City>>': clean_and_format_value(student.get('city')),
        
        # סימונים (X)
        '<<LowIncome>>': get_mark(reasons.get('lowIncome')),
        '<<OtherChildSpecialEd>>': get_mark(reasons.get('otherChildSpecialEd')),
        '<<SocialWorkerRec>>': get_mark(reasons.get('socialWorkerRec')),
        
        # פרטי הכנסה
        '<<Spouse1Status>>': clean_and_format_value(income.get('spouse1Status')),
        '<<Spouse2Status>>': clean_and_format_value(income.get('spouse2Status')),
        '<<Spouse1AvgMonthlyIncome>>': clean_and_format_value(income.get('spouse1AvgMonthlyIncome')),
        '<<Spouse2AvgMonthlyIncome>>': clean_and_format_value(income.get('spouse2AvgMonthlyIncome')),
        '<<Spouse1Total3Months>>': clean_and_format_value(income.get('spouse1Total3Months')),
        '<<Spouse2Total3Months>>': clean_and_format_value(income.get('spouse2Total3Months')),
        
        # מסמכים
        '<<LowIncomeDocsUploaded>>': clean_and_format_value(docs.get('lowIncomeDocsUploaded')),
        '<<OtherSpecialEdDocsUploaded>>': clean_and_format_value(docs.get('otherSpecialEdDocsUploaded')),
        '<<SocialWorkerDocsUploaded>>': clean_and_format_value(docs.get('socialWorkerDocsUploaded')),
    }
    
    # 1.1: הוספת שדות הילדים הקשיחים למפה (עד 4)
    for i in range(4):
        child_item = children_in_custody[i] if i < len(children_in_custody) else {}
        replace_map[f'<<ChildFirstName_{i}>>'] = clean_and_format_value(child_item.get('firstName'))
        replace_map[f'<<ChildLastName_{i}>>'] = clean_and_format_value(child_item.get('lastName'))
        replace_map[f'<<ChildId_{i}>>'] = clean_and_format_value(child_item.get('id'))


    # 2. הכנה וטעינת המסמך
    temp_docx_base_name = os.path.basename(output_pdf_path).replace('.pdf', '')
    temp_docx_path = os.path.join(TEMP_DIR, f"filled_temp_{temp_docx_base_name}.docx")

    if not os.path.exists(template_path):
        raise FileNotFoundError(f"Template file not found at: {template_path}")

    shutil.copyfile(template_path, temp_docx_path)
    doc = Document(temp_docx_path)
    
    # 3. החלפת כל שדות הטקסט באמצעות השיטה העמידה (כולל טבלאות 3 ו-4)
    def process_element(element):
        if hasattr(element, 'paragraphs'):
            for paragraph in element.paragraphs:
                for key, value in replace_map.items():
                    # 🛑 השתמש ב replace_and_style_stable לכל השדות!
                    replace_and_style_stable(paragraph, key, value)
    
    process_element(doc)
    for table in doc.tables:
        for row in table.rows:
            for cell in row.cells:
                process_element(cell)

    # 4. טיפול בטבלת הילדים הנוספים (טבלה 2)
    if len(doc.tables) >= 2:
        child_table = doc.tables[1] 
        
        # 🛑🛑🛑 ניקוי שורות ה-Placeholder הקשיחות 🛑🛑🛑
        # אנחנו מוחקים את כל השורות פרט לכותרת (אינדקס 0)
        # מכיוון שהנתונים מולאו קשיח באמצעות MailMerge, עלינו למחוק את ה-4 שורות הדוגמה
        
        # מחיקת 4 שורות ה-Placeholder הקשיחות
        rows_to_delete = min(len(child_table.rows) - 1, 4)
        for i in range(rows_to_delete):
            # אנחנו תמיד מוחקים את שורה 1, כי השורות מעל עולות למעלה
             child_table._element.remove(child_table.rows[1]._element)
             
        # מילוי שורות חדשות רק אם יש יותר מ-4 ילדים (ה-4 הראשונים מולאו קשיח)
        if len(children_in_custody) > 4: 
            for i in range(4, len(children_in_custody)):
                child_item = children_in_custody[i]
                new_row = child_table.add_row()
                # סדר העמודות: שם פרטי, שם משפחה, מס' זהות
                new_row.cells[0].text = clean_and_format_value(child_item.get('firstName'))
                new_row.cells[1].text = clean_and_format_value(child_item.get('lastName'))
                new_row.cells[2].text = clean_and_format_value(child_item.get('id'))
    
    # 5. הטמעת החתימה
    signature_base64 = form_data.get('parentSignature', '')
    InsertSignatureImage(doc, '<<ParentSignature>>', signature_base64)
    
    # 6. שמירת קובץ ה-DOCX המלא
    doc.save(temp_docx_path)
    
    # 7. המרת DOCX ל-PDF
    expected_pdf_name = os.path.basename(temp_docx_path).replace('.docx', '.pdf')
    generated_pdf_path = os.path.join(TEMP_DIR, expected_pdf_name)
    
    try:
        subprocess.run(
            [
                data_wrapper.get('libre_office_path'), 
                '--headless', 
                '--convert-to', 'pdf', 
                temp_docx_path, 
                '--outdir', TEMP_DIR
            ],
            capture_output=True, text=True, check=True, timeout=60
        )
        
        # 8. העתקת הקובץ למיקום שה-C# מצפה לו
        shutil.copyfile(generated_pdf_path, output_pdf_path)

    except subprocess.CalledProcessError as e:
        # זה ייתן לנו את הודעת השגיאה המדויקת של LibreOffice
        raise Exception(f"PDF Conversion failed (soffice). Error: {e.stderr}")
    except FileNotFoundError:
        raise Exception(f"LibreOffice command not found. Please check 'LibreOfficeExecutablePath' in appsettings.")

    finally:
        if os.path.exists(temp_docx_path):
            os.remove(temp_docx_path)
        if os.path.exists(generated_pdf_path):
            os.remove(generated_pdf_path)


# ----------------- הפונקציה הראשית להפעלה -----------------

def main():
    try:
        input_json_data = sys.stdin.read()
        data_wrapper = json.loads(input_json_data)

        output_path = data_wrapper.get('output_pdf_path')
        template_path = data_wrapper.get('template_path')
        libre_office_path = data_wrapper.get('libre_office_path') 

        if not output_path or not template_path or not libre_office_path:
            raise ValueError("נתיב שמירה, נתיב תבנית או נתיב LibreOffice חסרים.")

        fill_and_convert_to_pdf(data_wrapper, output_path, template_path, libre_office_path)

        print(json.dumps({'status': 'success', 'path': output_path}))

    except Exception as e:
        print(json.dumps({'status': 'error', 'message': str(e)}), file=sys.stderr)
        sys.exit(1)


if __name__ == '__main__':
    main()
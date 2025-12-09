import json
import sys
import os
import subprocess
import base64
import io
import shutil
from datetime import datetime
from docx import Document
from docx.shared import Inches, Pt
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_ALIGN_VERTICAL
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.shared import Emu # נדרש לתיקון רוחב טבלאות
from docx.enum.style import WD_STYLE_TYPE

# הגדרות סביבה
TEMP_DIR = os.path.join(os.path.expanduser('~'), 'ptachya_temp_forms')
os.makedirs(TEMP_DIR, exist_ok=True)

PLACEHOLDER_LINE = "______________________"
CHECK_MARK = "V"
data = {}

# ----------------------------- עזר -----------------------------

def format_date(date_str: str) -> str:
    """ ⭐️ תיקון תאריך: מנקה את חלק הזמן (T00:00:00) ⭐️ """
    try:
        # חותך את המחרוזת לפני 'T' אם קיימת, וממיר לפורמט נקי
        if not date_str: return PLACEHOLDER_LINE
        date_part = date_str.split('T')[0]
        return datetime.strptime(date_part, '%Y-%m-%d').strftime('%d/%m/%Y')
    except:
        return clean_value(date_str) # אם הפורמט שונה, מחזיר את המקור נקי


def clean_value(value, default=PLACEHOLDER_LINE):
    if not value or str(value).strip() == "" or str(value).lower() == "null":
        return default
    return str(value)


def add_rtl(par):
    par.paragraph_format.rtl = True
    par.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    return par


def add_paragraph(document, text="", style="RTL_Normal", space_after=6):
    p = document.add_paragraph(text, style=style)
    add_rtl(p)
    p.paragraph_format.space_after = Pt(space_after)
    return p


def underline_run(par, text):
    r = par.add_run(text)
    r.font.underline = True
    return r


def rtl_table(document, rows, cols):
    tbl = document.add_table(rows=rows, cols=cols)
    tbl.autofit = False # מונע מטבלה להתפרס על כל רוחב הדף
    
    for row in tbl.rows:
        for cell in row.cells:
            for p in cell.paragraphs:
                add_rtl(p)
    return tbl


# ----------------------------- חתימה -----------------------------

def insert_signature(document, base64_data):
    """ ⭐️ תיקון חתימה: משתמש בטבלה לסידור תאריך וחתימה (כפי שתוקן ב-V13) ⭐️ """
    
    # 1. יצירת טבלה נסתרת ליישור דו-צדדי
    sig_table = document.add_table(rows=1, cols=2)
    sig_table.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    try:
        sig_table.style = 'Table Grid'
    except (KeyError, ValueError):
        # אם הסגנון לא קיים בתבנית, נשתמש ברירת המחדל ונמשיך הלאה בלי לקרוס
        pass
    
    # הסרת גבולות הטבלה (כדי שלא יראו את הריבוע)
    try:
        tbl_pr = sig_table._tbl.tblPr 
        tbl_pr.attrib.pop(qn('w:tblBorders'), None)
    except:
        pass
    
    # 1. יצירת פסקה עבור החתימה (צד ימין)
    cell_sig = sig_table.cell(0, 0)
    p_sig = cell_sig.paragraphs[0]
    p_sig.clear()
    p_sig.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    
    p_sig.add_run("חתימת הורים: ").bold = True
    
    if not base64_data or base64_data == "null":
        p_sig.add_run(" (לא נחתם)").bold = True
    else:
        try:
            img_bytes = base64.b64decode(base64_data.split(",")[-1])
            img = io.BytesIO(img_bytes)
            p_sig.add_run().add_picture(img, width=Inches(1.5))
        except:
            p_sig.add_run("(שגיאה בטעינת חתימה)").bold = True

    # 2. יצירת פסקה עבור התאריך (צד שמאל)
    cell_date = sig_table.cell(0, 1)
    p_date = cell_date.paragraphs[0]
    p_date.clear()
    p_date.alignment = WD_ALIGN_PARAGRAPH.LEFT
    
    p_date.add_run("תאריך: ").bold = True
    underline_run(p_date, format_date(data.get("formDate")))


# ----------------------------- בניית המסמך -----------------------------

def create_doc(data, output_pdf_path, libre_path, template_path):

    if not os.path.exists(template_path):
        raise FileNotFoundError("Template not found.")

    doc = Document(template_path)

    # יצירת סטייל RTL אם לא קיים
    if "RTL_Normal" not in doc.styles:
        st = doc.styles.add_style("RTL_Normal", WD_STYLE_TYPE.PARAGRAPH)
        st.font.name = "Arial"
        st.font.size = Pt(11)

    # מחיקת תוכן אחרי בס"ד
    found = False
    for p in doc.paragraphs:
        if 'בס"ד' in p.text or "בס\"ד" in p.text:
            found = True
            p.clear()
            p.add_run('בס"ד')
        elif found:
            p._element.getparent().remove(p._element)

    # ---------------------------------------------------
    # ⭐️⭐️ דחיסת תוכן (מוריד רווחים ל-0.5pt אם הטופס ארוך) ⭐️⭐️
    num_children = int(data.get("childrenCount", 0))
    low_income = data.get("discountReasons", {}).get("lowIncome", False)
    
    base_spacing = 6
    if num_children > 2 and low_income:
        base_spacing = 3 # דחיסה קלה

    # רווחים אחרי בס״ד
    for _ in range(4):
        add_paragraph(doc, "")

    student = data.get("studentDetails", {})
    reasons = data.get("discountReasons", {})
    income = data.get("lowIncomeDetails", {})
    children = data.get("childrenInCustody", [])

    # ---------------------------------------------------
    # כותרות
    p = add_paragraph(doc, "הצהרת הורה לצורך מתן הנחה", space_after=4)
    p.runs[0].bold = True
    p.runs[0].font.size = Pt(13)
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER

    p = add_paragraph(doc, "במימון טיפולי טב\"מ - שנה\"ל תשפ\"ו", space_after=12)
    p.runs[0].bold = True
    p.runs[0].font.size = Pt(13)
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER

    add_paragraph(doc, "לכבוד", space_after=base_spacing)
    add_paragraph(doc, "הנהלת פתחיה", space_after=base_spacing + 6)

    # ---------------------------------------------------
    # פרטי תלמיד
    p = add_paragraph(doc, space_after=base_spacing)
    p.add_run("שם התלמיד/ה: ").bold = True
    underline_run(p, clean_value(student.get("studentName")))

    p.add_run("  מ.ז.: ").bold = True
    underline_run(p, clean_value(student.get("studentId")))

    p.add_run("  גן: ").bold = True
    underline_run(p, clean_value(student.get("kindergarten")))

    p.add_run("  עיר: ").bold = True
    underline_run(p, clean_value(student.get("city")))

    # ---------------------------------------------------
    # פרטי מצהיר
    p = add_paragraph(doc, space_after=base_spacing)
    p.add_run("אני הח\"מ ").bold = True
    underline_run(p, clean_value(data.get("declarantName")))

    p.add_run("  ת.ז.: ").bold = True
    underline_run(p, clean_value(data.get("declarantId")))

    p.add_run("  מצב משפחתי: ").bold = True
    underline_run(p, clean_value(data.get("maritalStatus")))

    add_paragraph(doc, "מצהיר/ה בזאת כדלקמן:", space_after=12)

    # ---------------------------------------------------
    # ילדים
    p = add_paragraph(doc, space_after=base_spacing)
    p.add_run("א. מספר הילדים מעל גיל 18: ").bold = True
    underline_run(p, str(num_children))
    p.add_run(" (יש לצרף צילום ת.ז. כולל הספח)")

    tbl = rtl_table(doc, num_children + 1, 3)

    # ⭐️⭐️ הדגשת כותרות טבלה ⭐️⭐️
    hdr = tbl.rows[0].cells
    hdr[0].paragraphs[0].add_run("שם פרטי").bold = True
    hdr[1].paragraphs[0].add_run("שם משפחה").bold = True
    hdr[2].paragraphs[0].add_run("מס' זהות").bold = True

    for i in range(num_children):
        row = tbl.rows[i + 1].cells
        c = children[i] if i < len(children) else {}
        # ⭐️ הדגשת תוכן טבלה ⭐️
        row[0].paragraphs[0].add_run(clean_value(c.get("firstName"), "")).bold = True
        row[1].paragraphs[0].add_run(clean_value(c.get("lastName"), "")).bold = True
        row[2].paragraphs[0].add_run(clean_value(c.get("id"), "")).bold = True

    add_paragraph(doc, "", space_after=base_spacing)

    # ---------------------------------------------------
    # סעיף ב
    p = add_paragraph(doc, "ב. אני מבקש/ת הנחה מהסיבה:", space_after=base_spacing)
    p.runs[0].bold = True # ⭐️ הדגשת הכותרת ⭐️

    def add_reason(text, selected):
        if not selected:
            return
        p = add_paragraph(doc, space_after=3)
        p.paragraph_format.left_indent = Inches(0.4)
        p.add_run(f"• [{CHECK_MARK}] ").bold = True
        p.add_run(text).bold = True # ⭐️ הדגשת הסיבה הנבחרת ⭐️

    add_reason("הכנסות נמוכות", low_income)
    add_reason("ילד נוסף במסגרת חינוך מיוחד (עם אישור לימודים)", reasons.get("otherChildSpecialEd"))
    add_reason("המלצה מעובדת סוציאלית", reasons.get("socialWorkerRec"))

    add_paragraph(doc, "", space_after=base_spacing)

    # ---------------------------------------------------
    # סעיף ג – טבלת הכנסות
    if low_income:
        p = add_paragraph(doc, "ג. יש לצרף 3 תלושי משכורת...", space_after=base_spacing)
        p.runs[0].bold = True # ⭐️ הדגשת הכותרת ⭐️

        add_paragraph(doc, "יש למלא את הפרטים:", space_after=base_spacing)

        tbl = rtl_table(doc, 4, 3)

        # ⭐️⭐️ הדגשת כותרות טבלה ⭐️⭐️
        tbl.rows[0].cells[1].paragraphs[0].add_run("בן הזוג").bold = True
        tbl.rows[0].cells[2].paragraphs[0].add_run("בת הזוג").bold = True

        tbl.cell(1, 0).paragraphs[0].add_run("מעמד אישי").bold = True
        tbl.cell(1, 1).paragraphs[0].add_run(clean_value(income.get("spouse1Status"), "שכיר / עצמאי / לא עובד")).bold = True
        tbl.cell(1, 2).paragraphs[0].add_run(clean_value(income.get("spouse2Status"), "שכירה / עצמאית / לא עובדת")).bold = True

        tbl.cell(2, 0).paragraphs[0].add_run("הכנסה חודשית ממוצעת").bold = True
        tbl.cell(2, 1).paragraphs[0].add_run(clean_value(income.get("spouse1AvgMonthlyIncome"))).bold = True
        tbl.cell(2, 2).paragraphs[0].add_run(clean_value(income.get("spouse2AvgMonthlyIncome"))).bold = True

        tbl.cell(3, 0).paragraphs[0].add_run("סה\"כ ב־3 חודשים").bold = True
        tbl.cell(3, 1).paragraphs[0].add_run(clean_value(income.get("spouse1Total3Months"))).bold = True
        tbl.cell(3, 2).paragraphs[0].add_run(clean_value(income.get("spouse2Total3Months"))).bold = True

    add_paragraph(doc, "", space_after=base_spacing)

    # ---------------------------------------------------
    # נימוק
    p = add_paragraph(doc, space_after=base_spacing)
    p.add_run("ד. נימוק לבקשה: ").bold = True # ⭐️ הדגשת הכותרת ⭐️
    underline_run(p, clean_value(data.get("reasoning"), PLACEHOLDER_LINE * 3))

    add_paragraph(doc, "", space_after=base_spacing)

    # ---------------------------------------------------
    # חתימה
    insert_signature(doc, data.get("parentSignature"))

    # ---------------------------------------------------
    # שמירה + PDF
    temp_docx = os.path.join(TEMP_DIR, "temp_output.docx")
    doc.save(temp_docx)

    subprocess.run([
        libre_path, "--headless", "--convert-to", "pdf",
        temp_docx, "--outdir", TEMP_DIR
    ], check=True)

    generated_pdf = temp_docx.replace(".docx", ".pdf")
    shutil.copyfile(generated_pdf, output_pdf_path)

    os.remove(temp_docx)
    os.remove(generated_pdf)


# ----------------------------- main -----------------------------

def main():
    global data
    wrapper = json.loads(sys.stdin.read())

    data = wrapper.get("form_data", {})
    out = wrapper.get("output_pdf_path")
    libre = wrapper.get("libre_office_path")
    template = wrapper.get("template_path")

    if not out or not libre or not template:
        raise Exception("נתונים חסרים.")

    # ⭐️⭐️⭐️ העברת template_path כארגומנט לפונקציה ⭐️⭐️⭐️
    create_doc(data, out, libre, template)

    print(json.dumps({"status": "success", "path": out}))


if __name__ == "__main__":
    main()
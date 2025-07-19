import os
import json
import re
import google.generativeai as genai
from datetime import datetime

GEMINI_API_KEY = "AIzaSyDuQqUAWb1IBLnsfFNj_SNt4D2q2t4EdoY"
HISTORY_DIR = "conversations_history"
os.makedirs(HISTORY_DIR, exist_ok=True)

genai.configure(api_key=GEMINI_API_KEY)

def analyze_with_gemini(mp3_path):
    """處理從 Flask 傳來的音檔與使用者資料，送進 Gemini"""
    model = genai.GenerativeModel('gemini-2.5-flash')

    with open(mp3_path, 'rb') as audio_file:
        audio_data = audio_file.read()

    audio_file_data = {
        "mime_type": "audio/mp3",
        "data": audio_data
    }

    prompt = """
    你是專業的CoffeeChat對話輔助AI，協助用戶在XR眼鏡中獲得即時的提問建議。

    # 核心任務
    分析最新20秒內容，結合完整對話脈絡，產出XR眼鏡適用的提問建議。

    # 處理步驟
    1. 識別最新20秒的核心主題
    2. 分析這段語音的兩個核心關鍵詞
    3. 為每個關鍵詞生成兩個貼合脈絡的簡短問題（每個最多15字）

    
    # 輸出格式（嚴格遵守）
    [points]
    [Keyword_1]
    [Q_1_1]
    [Q_1_2]
    [Keyword_2]
    [Q_2_1]
    [Q_2_2]

    
    # 提問設計原則
    - 必須是開放式問題
    - 引導分享具體經驗或故事
    - 避免是非題或事實查詢
    - 語調真誠好奇
    - 字數限制：15字以內
    """

    response = model.generate_content([prompt, audio_file_data])
    return response.text.strip()

def parse_gemini_response(text, base_filename):
    """將 Gemini 輸出文字轉為 dict 並儲存 JSON 檔"""
    result = {
        "points": "",
        "Keyword_1": "",
        "Keyword_2": "",
        "Q_1_1": "",
        "Q_1_2": "",
        "Q_2_1": "",
        "Q_2_2": "",
    }

    match_focus = re.search(r"\[points\]：(.+)", text)
    if match_focus:
        result["points"] = match_focus.group(1).strip()
    match_kw1 = re.search(r"\[Keyword_1\]：(.*)", text)
    if match_kw1:
        result["Keyword_1"] = match_kw1.group(1).strip()

    match_kw2 = re.search(r"\[Keyword_2\]：(.*)", text)
    if match_kw2:
        result["Keyword_2"] = match_kw2.group(1).strip()

    q_map = {
        "Q_1_1": r"\[Q_1_1\](.*)",
        "Q_1_2": r"\[Q_1_2\](.*)",
        "Q_2_1": r"\[Q_2_1\](.*)",
        "Q_2_2": r"\[Q_2_2\](.*)",
    }

    for key, pattern in q_map.items():
        match = re.search(pattern, text)
        if match:
            result[key] = match.group(1).strip()

    json_path = os.path.join(HISTORY_DIR, f"{base_filename}.json")
    with open(json_path, 'w', encoding='utf-8') as f:
        json.dump(result, f, ensure_ascii=False, indent=2)

    return result  # 回傳給 Flask 用


# ➤ Flask Server 會呼叫此函式
def process_request_from_flask(mp3_path):
    base_filename = os.path.splitext(os.path.basename(mp3_path))[0]

    print(f"🤖 Gemini 分析中...")
    gemini_output = analyze_with_gemini(mp3_path)
    parsed_json = parse_gemini_response(gemini_output, base_filename)
    return parsed_json  # 傳回去給 Flask server

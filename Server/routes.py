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
    3. 為兩個關鍵詞生成兩個貼合脈絡的簡短問題（每個最多15字）

    # 輸出格式（請嚴格遵守，不得省略標籤）
    [points]
    主題摘要一句話

    [Keyword_1]
    第一個關鍵詞

    [Q_1_1]
    針對 Keyword_1 的第一個開放式提問

    [Q_1_2]
    針對 Keyword_1 的第二個提問

    [Keyword_2]
    第二個關鍵詞

    [Q_2_1]
    針對 Keyword_2 的第一個提問

    [Q_2_2]
    針對 Keyword_2 的第二個提問

    # 提問設計原則
    - 必須是開放式問題
    - 引導分享具體經驗或故事
    - 避免是非題或事實查詢
    - 語調真誠好奇
    - 字數限制：15字以內

    """

    response = model.generate_content([prompt, audio_file_data])
    print(f"[📥] Gemini 回應：\n{response.text.strip()}")
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

    def extract_after_tag(tag, block_text):
        pattern = rf"\[{tag}\]\s*\n?(.*)"
        match = re.search(pattern, block_text)
        if match:
            return match.group(1).strip()
        return ""

    result["points"] = extract_after_tag("points", text)
    result["Keyword_1"] = extract_after_tag("Keyword_1", text)
    result["Keyword_2"] = extract_after_tag("Keyword_2", text)

    q_map = {
        "Q_1_1": r"\[Q_1_1\]\s*:?(.+)",
        "Q_1_2": r"\[Q_1_2\]\s*:?(.+)",
        "Q_2_1": r"\[Q_2_1\]\s*:?(.+)",
        "Q_2_2": r"\[Q_2_2\]\s*:?(.+)",
    }

    for key, pattern in q_map.items():
        match = re.search(pattern, text)
        if match:
            result[key] = match.group(1).strip()

    json_path = os.path.join(HISTORY_DIR, f"{base_filename}.json")
    with open(json_path, 'w', encoding='utf-8') as f:
        json.dump(result, f, ensure_ascii=False, indent=2)

    return result

def summarize_whole_conversation():
    """將所有 points + keywords 整合後丟進 Gemini 產生三句摘要"""
    model = genai.GenerativeModel('gemini-2.5-pro')

    # 建構每段輸入 block
    summary_blocks = []
    for filename in os.listdir(HISTORY_DIR):
        if filename.endswith(".json"):
            json_path = os.path.join(HISTORY_DIR, filename)
            with open(json_path, 'r', encoding='utf-8') as f:
                data = json.load(f)
                point = data.get("points", "").strip()
                k1 = data.get("Keyword_1", "").strip()
                k2 = data.get("Keyword_2", "").strip()

                if point:
                    block = f"[points] {point}"
                    if k1:
                        block += f"\n[Keyword_1] {k1}"
                    if k2:
                        block += f"\n[Keyword_2] {k2}"
                    summary_blocks.append(block)

    if not summary_blocks:
        print("⚠️ 沒有可用的摘要與關鍵字資料")
        return

    combined_text = "\n\n".join(summary_blocks)

    prompt = f"""
你是一位溫暖的 XR 對話輔助 AI，擅長從對話中提煉出觸動人心、具有啟發性與情感溫度的主題。

以下是一場 Coffee Chat 對話中每段擷取的摘要與關鍵詞：

{combined_text}

請你根據這些內容，總結出整場對話中最打動人心的三個核心主題。每一條摘要都應該以「心靈雞湯」的風格呈現——溫柔、簡潔、能引發共鳴。請避免中性或制式的主題名稱。

每條摘要請控制在 20 字內，語氣真誠自然，風格溫暖具人味。

請使用以下格式輸出（請嚴格遵守）：
[Summary_1]
第一個摘要（心靈雞湯風格）

[Summary_2]
第二個摘要（心靈雞湯風格）

[Summary_3]
第三個摘要（心靈雞湯風格）
"""

    # 發送至 Gemini
    response = model.generate_content(prompt)
    summary_output = response.text.strip()
    print(f"\n📌 對話總結結果：\n{summary_output}")

    # 解析成 dict
    def parse_summary_to_dict(text):
        summary_dict = {}
        for i in range(1, 4):
            match = re.search(rf"\[Summary_{i}\]\s*\n?(.*)", text)
            summary_dict[f"Summary_{i}"] = match.group(1).strip() if match else ""
        return summary_dict

    summary_dict = parse_summary_to_dict(summary_output)

    # 儲存為 JSON
    summary_json_path = os.path.join(HISTORY_DIR, "final_summary.json")
    with open(summary_json_path, 'w', encoding='utf-8') as f:
        json.dump(summary_dict, f, ensure_ascii=False, indent=2)


# ➤ Flask Server 會呼叫此函式
def process_request_from_flask(mp3_path):
    base_filename = os.path.splitext(os.path.basename(mp3_path))[0]

    print(f"🤖 Gemini 分析中...")
    gemini_output = analyze_with_gemini(mp3_path)
    parsed_json = parse_gemini_response(gemini_output, base_filename)
    return parsed_json  # 傳回去給 Flask server
